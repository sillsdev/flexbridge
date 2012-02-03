using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using Chorus.merge.xml.generic;
using Chorus.merge.xml.generic.xmldiff;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using Palaso.Xml;

namespace FwdataTestApp
{
	public partial class NestFwdataFile : Form
	{
		internal static readonly Encoding Utf8 = Encoding.UTF8;

		public NestFwdataFile()
		{
			InitializeComponent();
		}

		private void BrowseForFile(object sender, EventArgs e)
		{
			_btnNest.Enabled = false;
			_fwdataPathname.Text = null;

			if (_openFileDialog.ShowDialog(this) != DialogResult.OK)
				return;

			_fwdataPathname.Text = _openFileDialog.FileName;
			_btnNest.Enabled = true;
		}

		private static void CacheDataRecord(IDictionary<string, SortedDictionary<string, XElement>> unownedObjects, IDictionary<string, SortedDictionary<string, XElement>> classData, IDictionary<string, string> guidToClassMapping, string record)
		{
			var rtElement = XElement.Parse(record);
			var guid = rtElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
			var className = rtElement.Attribute(SharedConstants.Class).Value;
			if (rtElement.Attribute(SharedConstants.OwnerGuid) == null)
			{
				SortedDictionary<string, XElement> unownedForCurrentClassName;
				if (!unownedObjects.TryGetValue(className, out unownedForCurrentClassName))
				{
					unownedForCurrentClassName = new SortedDictionary<string, XElement>();
					unownedObjects.Add(className, unownedForCurrentClassName);
				}
				unownedForCurrentClassName.Add(guid, rtElement);
			}
			guidToClassMapping.Add(guid.ToLowerInvariant(), className);

			// 1. Remove 'Checksum' from wordforms.
			if (className == "WfiWordform")
			{
				var csElement = rtElement.Element("Checksum");
				if (csElement != null)
					csElement.Remove();
			}

			// 2. Sort <rt>
			DataSortingService.SortMainElement(rtElement);

			// 3. Cache it.
			SortedDictionary<string, XElement> recordData;
			if (!classData.TryGetValue(className, out recordData))
			{
				recordData = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
				classData.Add(className, recordData);
			}
			recordData.Add(guid, rtElement);
		}

		private void RunSelected(object sender, EventArgs e)
		{
			var srcFwdataPathname = _openFileDialog.FileName;
			var workingDir = Path.GetDirectoryName(srcFwdataPathname);
			Cursor = Cursors.WaitCursor;
			var sb = new StringBuilder();
			var nestTimer = new Stopwatch();
			var breakupTimer = new Stopwatch();
			var restoreTimer = new Stopwatch();
			var verifyTimer = new Stopwatch();
			try
			{
				if (_cbNestFile.Checked)
				{
					nestTimer.Start();
					NestFile(srcFwdataPathname);
					nestTimer.Stop();
				}
				if (_cbRoundTripData.Checked)
				{
					var projectName = Path.GetFileNameWithoutExtension(srcFwdataPathname);
					File.Copy(srcFwdataPathname, srcFwdataPathname + ".orig", true); // Keep it safe.
					breakupTimer.Start();
					MultipleFileServices.PushHumptyOffTheWall(srcFwdataPathname, projectName);
					breakupTimer.Stop();
					restoreTimer.Start();
					MultipleFileServices.PutHumptyTogetherAgain(srcFwdataPathname, projectName);
					restoreTimer.Stop();

					if (_cbVerify.Checked)
					{
						verifyTimer.Start();
						// Figure out how to do this, but it needs to compare .orig with srcFwdataPathname.
						var mdc = MetadataCache.TestOnlyNewCache; // Want it fresh.
						var unownedObjectsSrc = new Dictionary<string, SortedDictionary<string, XElement>>(200);
						// Outer dictionary has the class name for its key and a sorted (by guid) dictionary as its value.
						// The inner dictionary has a caseless guid as the key and the byte array as the value.
						var classDataSrc = new Dictionary<string, SortedDictionary<string, XElement>>(200, StringComparer.OrdinalIgnoreCase);
						var guidToClassMappingSrc = new Dictionary<string, string>();
						TokenizeFile(mdc, srcFwdataPathname + ".orig", unownedObjectsSrc, classDataSrc, guidToClassMappingSrc);
						sb.AppendFormat("Number of records: {0}", guidToClassMappingSrc.Count);
						sb.AppendLine();

						mdc = MetadataCache.TestOnlyNewCache; // Want it fresh.
						var unownedObjectsTgt = new Dictionary<string, SortedDictionary<string, XElement>>(200);
						// Outer dictionary has the class name for its key and a sorted (by guid) dictionary as its value.
						// The inner dictionary has a caseless guid as the key and the byte array as the value.
						var classDataTgt = new Dictionary<string, SortedDictionary<string, XElement>>(200, StringComparer.OrdinalIgnoreCase);
						var guidToClassMappingTgt = new Dictionary<string, string>();
						TokenizeFile(mdc, srcFwdataPathname, unownedObjectsTgt, classDataTgt, guidToClassMappingTgt);

						// Deal with guid+class mappings.
						if (guidToClassMappingSrc.Count != guidToClassMappingTgt.Count)
						{
							sb.AppendFormat("Mismatched number of guids in guidToClassMapping. guidToClassMappingSrc: '{0}' guidToClassMappingTgt: '{1}'", guidToClassMappingSrc.Count, guidToClassMappingTgt.Count);
							sb.AppendLine();
						}
						foreach (var gcKvp in guidToClassMappingSrc)
						{
							var srcGuid = gcKvp.Key;
							var srcClassName = gcKvp.Value;
							string trgClassName;
							if (!guidToClassMappingTgt.TryGetValue(srcGuid, out trgClassName))
							{
								sb.AppendFormat("Guid '{0}' is not in target map.", srcGuid);
								sb.AppendLine();
								continue;
							}
							if (srcClassName == trgClassName)
								continue;

							sb.AppendFormat("Class names for guid '{0}' do no match. srcClassName: '{1}' trgClassName: '{2}'", srcGuid, srcClassName, trgClassName);
							sb.AppendLine();
						}

						// Deal with unowned objects.
						if (unownedObjectsSrc.Count != unownedObjectsTgt.Count)
						{
							sb.AppendFormat("Mismatched number of classes of unowned elements. unownedObjectsSrc: '{0}' unownedObjectsTgt: '{1}'", unownedObjectsSrc.Count, unownedObjectsTgt.Count);
							sb.AppendLine();
						}
						if (unownedObjectsSrc.Values.Count != unownedObjectsTgt.Values.Count)
						{
							sb.AppendFormat("Mismatched number of unowned elements. unownedObjectsSrc: '{0}' unownedObjectsTgt: '{1}'", unownedObjectsSrc.Values.Count, unownedObjectsTgt.Values.Count);
							sb.AppendLine();
						}
						foreach (var outerSrcKvp in unownedObjectsSrc)
						{
							var outerSrcClassName = outerSrcKvp.Key;
							var outerSrcUnOwnedDict = outerSrcKvp.Value;
							SortedDictionary<string, XElement> outerTrgUnOwnedDict;
							if (!unownedObjectsTgt.TryGetValue(outerSrcClassName, out outerTrgUnOwnedDict))
							{
								sb.AppendFormat("Class name '{0}' is not in target unowned list.", outerSrcClassName);
								sb.AppendLine();
								continue;
							}
							if (outerSrcUnOwnedDict.Count != outerTrgUnOwnedDict.Count)
							{
								sb.AppendFormat("Wrong number of instances of unowned elements in class: '{0}'. Expected: '{1}', but was '{2}'.", outerSrcClassName, outerSrcUnOwnedDict.Count, outerTrgUnOwnedDict.Count);
								sb.AppendLine();
								continue;
							}
							foreach (var innerSrcKvp in outerSrcUnOwnedDict)
							{
								var srcGuid = innerSrcKvp.Key;
								// Not to worry about the XElement Value.
								if (!outerTrgUnOwnedDict.ContainsKey(srcGuid))
								{
									sb.AppendFormat("Unowned object with guid '{0}' is not in target unowned list.", srcGuid);
									sb.AppendLine();
								}
							}
						}

						// Deal with actual instance data.
						if (classDataSrc.Values.Count != classDataTgt.Values.Count)
						{
							sb.AppendFormat("Wrong number of instances of elements. Expected: '{0}', but was '{1}'.", classDataSrc.Values.Count, classDataTgt.Values.Count);
							sb.AppendLine();
						}
						foreach (var outerSrcMainKvp in classDataSrc)
						{
							var outerSrcClassName = outerSrcMainKvp.Key;
							var outerSrcDict = outerSrcMainKvp.Value;
							SortedDictionary<string, XElement> outerTrgDict;
							if (!classDataTgt.TryGetValue(outerSrcClassName, out outerTrgDict))
							{
								sb.AppendFormat("Class name '{0}' is not in target main list.", outerSrcClassName);
								sb.AppendLine();
								continue;
							}
							if (outerSrcDict.Count != outerTrgDict.Count)
							{
								sb.AppendFormat("Wrong number of instances of main elements in class: '{0}'. Expected: '{1}', but was '{2}'.", outerSrcClassName, outerSrcDict.Count, outerTrgDict.Count);
								sb.AppendLine();
								continue;
							}
							foreach (var innerSrcKvp in outerSrcDict)
							{
								var srcGuid = innerSrcKvp.Key;
								XElement trgElement;
								if (!outerTrgDict.TryGetValue(srcGuid, out trgElement))
								{
									sb.AppendFormat("Main object with guid '{0}' is not in target list.", srcGuid);
									sb.AppendLine();
									continue;
								}
								var srcElement = innerSrcKvp.Value;
								if (!XmlUtilities.AreXmlElementsEqual(new XmlInput(trgElement.ToString()), new XmlInput(srcElement.ToString())))
								{
									File.WriteAllText(Path.Combine(workingDir, srcGuid + "-SRC.txt"), srcElement.ToString());
									File.WriteAllText(Path.Combine(workingDir, srcGuid + "-TRG.txt"), trgElement.ToString());
									sb.AppendFormat("Main src and trg object with guid '{0}' are different in the resulting xml.", srcGuid);
									sb.AppendLine();
								}
							}
						}
						verifyTimer.Stop();
					}
				}
			}
			catch (Exception err)
			{
				File.Delete(srcFwdataPathname);
				File.Move(srcFwdataPathname + ".orig", srcFwdataPathname); // Restore it.
				File.WriteAllText(Path.Combine(workingDir, "StackTrace.log"), err.GetType().Name + Environment.NewLine +  err.StackTrace);
			}
			finally
			{
				var compTxt = String.Format(
					"Time to nest file: {0}{5}Time to breakup file: {1}.{5}Time to restore file: {2}.{5}Time to verify restoration: {3}{5}{5}{4}",
					_cbNestFile.Checked ? nestTimer.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture) : "Not run",
					_cbRoundTripData.Checked ? breakupTimer.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture) : "Not run",
					_cbRoundTripData.Checked ? restoreTimer.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture) : "Not run",
					_cbVerify.Checked ? verifyTimer.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture) : "Not run",
					sb,
					Environment.NewLine);
				File.WriteAllText(Path.Combine(workingDir, "Comparison.log"), compTxt);
				Cursor = Cursors.Default;
				Close();
			}
		}

		private static void NestFile(string srcFwdataPathname)
		{
			var mdc = MetadataCache.TestOnlyNewCache; // Want it fresh.
			var unownedObjects = new Dictionary<string, SortedDictionary<string, XElement>>(200);
			// Outer dictionary has the class name for its key and a sorted (by guid) dictionary as its value.
			// The inner dictionary has a caseless guid as the key and the byte array as the value.
			var classData = new Dictionary<string, SortedDictionary<string, XElement>>(200, StringComparer.OrdinalIgnoreCase);
			var guidToClassMapping = new Dictionary<string, string>();
			TokenizeFile(mdc, srcFwdataPathname, unownedObjects, classData, guidToClassMapping);

			var root = new XElement("root");
			var exceptions = new Dictionary<string, HashSet<string>>();
			foreach (var unownedElementKvp in unownedObjects)
			{
				var className = unownedElementKvp.Key;
				var classElement = new XElement(className);
				var unownedElementDict = unownedElementKvp.Value;
				foreach (var unownedElement in unownedElementDict.Values)
				{
					classElement.Add(unownedElement);
					CmObjectNestingService.NestObject(false, unownedElement,
												  exceptions,
												  classData,
												  guidToClassMapping);
				}
				root.Add(classElement);
			}
			FileWriterService.WriteNestedFile(srcFwdataPathname + ".nested", root);
		}

		private static void TokenizeFile(MetadataCache mdc, string srcFwdataPathname, Dictionary<string, SortedDictionary<string, XElement>> unownedObjects, Dictionary<string, SortedDictionary<string, XElement>> classData, Dictionary<string, string> guidToClassMapping)
		{
			using (var fastSplitter = new FastXmlElementSplitter(srcFwdataPathname))
			{
				bool foundOptionalFirstElement;
				// NB: The main input file *does* have to deal with the optional first element.
				foreach (var record in fastSplitter.GetSecondLevelElementStrings(SharedConstants.AdditionalFieldsTag, SharedConstants.RtTag, out foundOptionalFirstElement))
				{
					if (foundOptionalFirstElement)
					{
						// Cache custom prop file for later write.
						var cpElement = DataSortingService.SortCustomPropertiesRecord(record);
						// Add custom property info to MDC, since it may need to be sorted in the data files.
						foreach (var propElement in cpElement.Elements("CustomField"))
						{
							var className = propElement.Attribute(SharedConstants.Class).Value;
							var propName = propElement.Attribute(SharedConstants.Name).Value;
							var typeAttr = propElement.Attribute("type");
							var adjustedTypeValue = FileWriterService.AdjustedPropertyType(className, propName, typeAttr.Value);
							if (adjustedTypeValue != typeAttr.Value)
								typeAttr.Value = adjustedTypeValue;
							var customProp = new FdoPropertyInfo(
								propName,
								typeAttr.Value,
								true);
							mdc.AddCustomPropInfo(
								className,
								customProp);
						}
						mdc.ResetCaches();
						//optionalFirstElement = Utf8.GetBytes(cpElement.ToString());
						foundOptionalFirstElement = false;
					}
					else
					{
						CacheDataRecord(unownedObjects, classData, guidToClassMapping, record);
					}
				}
			}
		}
	}
}
