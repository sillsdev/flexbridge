using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHanders;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using Palaso.Progress.LogBox;
using Palaso.Xml;

namespace FwdataTestApp
{
	public partial class NestFwdataFile : Form
	{
		internal static readonly Encoding Utf8 = Encoding.UTF8;
		private string _srcFwdataPathname;
		private string _workingDir;

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

		private MetadataCache GetFreshMdc()
		{
			var mdc = MetadataCache.TestOnlyNewCache;
			var modelData = File.ReadAllText(Path.Combine(_workingDir, SharedConstants.ModelVersionFilename));
			mdc.UpgradeToVersion(Int32.Parse(modelData.Split(new[] { "{", ":", "}" }, StringSplitOptions.RemoveEmptyEntries)[1]));
			mdc.AddCustomPropInfo(Path.Combine(_workingDir, SharedConstants.CustomPropertiesFilename));
			return mdc;
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

			// 1. Set 'Checksum' to zero (0).
			if (className == "WfiWordform")
			{
				var csElement = rtElement.Element("Checksum");
				if (csElement != null)
					csElement.Remove();
			}

			// Theory has it the FW data is sorted.
			//// 2. Sort <rt>
			//DataSortingService.SortMainElement(rtElement);

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
			_srcFwdataPathname = _openFileDialog.FileName;
			_workingDir = Path.GetDirectoryName(_srcFwdataPathname);
			Cursor = Cursors.WaitCursor;
			var sb = new StringBuilder();
			var sbValidation = new StringBuilder();
			var nestTimer = new Stopwatch();
			var breakupTimer = new Stopwatch();
			var restoreTimer = new Stopwatch();
			var verifyTimer = new Stopwatch();
			var checkOwnObjsurTimer = new Stopwatch();
			var validateTimer = new Stopwatch();
			var ownObjsurFound = false;
			try
			{
				if (_cbRoundTripData.Checked)
				{
					RoundTripData(breakupTimer, restoreTimer, validateTimer, verifyTimer, sb, sbValidation);
				}
				if (_cbNestFile.Checked)
				{
					ownObjsurFound = NestFile(nestTimer, checkOwnObjsurTimer, _cbCheckOwnObjsur.Checked);
				}
				if (_restoreDataFile.Checked)
				{
					RestoreManFileFromPeces(restoreTimer);
				}
			}
			catch (Exception err)
			{
				File.WriteAllText(Path.Combine(_workingDir, "StackTrace.log"), err.GetType().Name + Environment.NewLine +  err.StackTrace);
				if (File.Exists(_srcFwdataPathname + ".orig"))
				{
					File.Delete(_srcFwdataPathname);
					File.Move(_srcFwdataPathname + ".orig", _srcFwdataPathname); // Restore it.
				}
			}
			finally
			{
				var compTxt = String.Format(
					"Time to nest file: {1}{0}Time to check nested file: {2}{0}Own objsur Found: {3}{0}Time to breakup file: {4}.{0}Time to restore file: {5}.{0}Time to verify restoration: {6}.{0}Time to validate files: {7}.{0}{0}{8}",
					Environment.NewLine,
					nestTimer.ElapsedMilliseconds > 0 ? nestTimer.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture) : "Not run",
					checkOwnObjsurTimer.ElapsedMilliseconds > 0 ? checkOwnObjsurTimer.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture) : "Not run",
					_cbCheckOwnObjsur.Checked ? (ownObjsurFound ? "********* YES FIX BUG *********" : "No") : "Not run",
					breakupTimer.ElapsedMilliseconds > 0 ? breakupTimer.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture) : "Not run",
					restoreTimer.ElapsedMilliseconds > 0 ? restoreTimer.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture) : "Not run",
					verifyTimer.ElapsedMilliseconds > 0 ? verifyTimer.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture) : "Not run",
					validateTimer.ElapsedMilliseconds > 0 ? validateTimer.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture) : "Not run",
					sb);
				File.WriteAllText(Path.Combine(_workingDir, "Comparison.log"), compTxt);
				var validationErrors = sbValidation.ToString();
				if (validationErrors.Length > 0)
					File.WriteAllText(Path.Combine(_workingDir, "Validation.log"), validationErrors);
				Cursor = Cursors.Default;
				Close();
			}
		}

		private void RestoreManFileFromPeces(Stopwatch restoreTimer)
		{
			restoreTimer.Start();
			FLExProjectUnifier.PutHumptyTogetherAgain(new NullProgress(), _srcFwdataPathname);
			restoreTimer.Stop();
		}

		private bool NestFile(Stopwatch nestTimer, Stopwatch checkOwnObjsurTimer, bool cbCheckOwnObjsurChecked)
		{
			var ownObjsurFound = false;
			nestTimer.Start();
			NestFile(_srcFwdataPathname);
			nestTimer.Stop();
			GC.Collect(2, GCCollectionMode.Forced);
			if (cbCheckOwnObjsurChecked)
			{
				checkOwnObjsurTimer.Start();
				ownObjsurFound = File.ReadAllText(_srcFwdataPathname + ".nested").Contains("t=\"o\"");
				checkOwnObjsurTimer.Stop();
			}
			return ownObjsurFound;
		}

		private void RoundTripData(Stopwatch breakupTimer, Stopwatch restoreTimer, Stopwatch validateTimer, Stopwatch verifyTimer, StringBuilder sb, StringBuilder sbValidation)
		{
			File.Copy(_srcFwdataPathname, _srcFwdataPathname + ".orig", true); // Keep it safe.
			breakupTimer.Start();
			FLExProjectSplitter.PushHumptyOffTheWall(new NullProgress(), _srcFwdataPathname);
			breakupTimer.Stop();
			restoreTimer.Start();
			FLExProjectUnifier.PutHumptyTogetherAgain(new NullProgress(), _srcFwdataPathname);
			restoreTimer.Stop();
			GC.Collect(2, GCCollectionMode.Forced);

			if (_cbVerify.Checked)
				Verify(verifyTimer, sb);
			if (_cbValidate.Checked)
				ValidateSplitData(validateTimer, sb, sbValidation);
		}

		private void ValidateSplitData(Stopwatch validateTimer, StringBuilder sb, StringBuilder sbValidation)
		{
			GetFreshMdc(); // Want it fresh.
			// Validate all files.
			validateTimer.Start();
			var fbHandler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
							 where handler.GetType().Name == "FieldWorksCommonFileHandler"
							 select handler).First();
			// Custom properties file.
			var currentPathname = Path.Combine(_workingDir, SharedConstants.CustomPropertiesFilename);
			var validationError = fbHandler.ValidateFile(currentPathname, new NullProgress());
			if (validationError != null)
			{
				sbValidation.AppendFormat("File '{1}' reported an error:{0}\t{2}", Environment.NewLine, currentPathname, validationError);
				sbValidation.AppendLine();
				sb.AppendFormat("File '{1}' reported an error:{0}\t{2}", Environment.NewLine, currentPathname, validationError);
				sb.AppendLine();
			}
			// Model version file.
			currentPathname = Path.Combine(_workingDir, SharedConstants.ModelVersionFilename);
			validationError = fbHandler.ValidateFile(currentPathname, new NullProgress());
			if (validationError != null)
			{
				sbValidation.AppendFormat("File '{1}' reported an error:{0}\t{2}", Environment.NewLine, currentPathname, validationError);
				sbValidation.AppendLine();
				sb.AppendFormat("File '{1}' reported an error:{0}\t{2}", Environment.NewLine, currentPathname, validationError);
				sb.AppendLine();
			}

			// General
			foreach (var generalPathname in Directory.GetFiles(Path.Combine(_workingDir, "General"), "*.*", SearchOption.AllDirectories))
			{
				validationError = fbHandler.ValidateFile(generalPathname, new NullProgress());
				if (validationError == null)
					continue;
				sbValidation.AppendFormat("File '{0}' reported an error:{1}", generalPathname, validationError);
				sbValidation.AppendLine();
				sb.AppendFormat("File '{0}' reported an error:{1}", generalPathname, validationError);
				sb.AppendLine();
			}

			// Anthropology
			foreach (var anthropologyPathname in Directory.GetFiles(Path.Combine(_workingDir, "Anthropology"), "*.*", SearchOption.AllDirectories))
			{
				validationError = fbHandler.ValidateFile(anthropologyPathname, new NullProgress());
				if (validationError == null)
					continue;
				sbValidation.AppendFormat("File '{0}' reported an error:{1}", anthropologyPathname, validationError);
				sbValidation.AppendLine();
				sb.AppendFormat("File '{0}' reported an error:{1}", anthropologyPathname, validationError);
				sb.AppendLine();
			}

			// Scripture
			var scriptureFolder = Path.Combine(_workingDir, "Other");
			if (Directory.Exists(scriptureFolder))
			{
				foreach (var scripturePathname in Directory.GetFiles(scriptureFolder, "*.*", SearchOption.AllDirectories))
				{
					validationError = fbHandler.ValidateFile(scripturePathname, new NullProgress());
					if (validationError == null)
						continue;
					sbValidation.AppendFormat("File '{0}' reported an error:{1}", scripturePathname, validationError);
					sbValidation.AppendLine();
					sb.AppendFormat("File '{0}' reported an error:{1}", scripturePathname, validationError);
					sb.AppendLine();
				}
			}

			// Linguistics
			foreach (var linguisticsPathname in Directory.GetFiles(Path.Combine(_workingDir, "Linguistics"), "*.*", SearchOption.AllDirectories))
			{
				validationError = fbHandler.ValidateFile(linguisticsPathname, new NullProgress());
				if (validationError == null)
					continue;
				sbValidation.AppendFormat("File '{0}' reported an error:{1}", linguisticsPathname, validationError);
				sbValidation.AppendLine();
				sb.AppendFormat("File '{0}' reported an error:{1}", linguisticsPathname, validationError);
				sb.AppendLine();
			}
			validateTimer.Stop();
		}

		private static string GetGuid(string record)
		{
			using (var reader = XmlReader.Create(new StringReader(record), FileWriterService.CanonicalReaderSettings))
			{
				reader.MoveToContent();
				reader.MoveToAttribute("guid");
				return reader.Value.ToLowerInvariant();
			}
		}

		private void Verify(Stopwatch verifyTimer, StringBuilder sb)
		{
			verifyTimer.Start();
			GetFreshMdc(); // Want it fresh.
			var origData = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
			using (var fastSplitterOrig = new FastXmlElementSplitter(_srcFwdataPathname + ".orig"))
			{
				bool foundOrigOptionalFirstElement;
				foreach (var origRecord in fastSplitterOrig.GetSecondLevelElementStrings(SharedConstants.AdditionalFieldsTag, SharedConstants.RtTag, out foundOrigOptionalFirstElement))
				{
					if (foundOrigOptionalFirstElement)
					{
						origData.Add(SharedConstants.AdditionalFieldsTag, origRecord);
						foundOrigOptionalFirstElement = false;
						continue;
					}
					origData.Add(GetGuid(origRecord), origRecord);
				}
			}
			using (var fastSplitterNew = new FastXmlElementSplitter(_srcFwdataPathname))
			{
				bool foundNewOptionalFirstElement;
				var newRecords = fastSplitterNew.GetSecondLevelElementStrings(SharedConstants.AdditionalFieldsTag, SharedConstants.RtTag, out foundNewOptionalFirstElement);
				// NB: The main input file *does* have to deal with the optional first element.
				foreach (var newRecord in newRecords)
				{
					string srcGuid = null;
					XElement origElement;
					var newElement = XElement.Parse(newRecord);
					if (newElement.Name == SharedConstants.AdditionalFieldsTag)
					{
						origElement = XElement.Parse(origData[SharedConstants.AdditionalFieldsTag]);
						origData.Remove(SharedConstants.AdditionalFieldsTag);
					}
					else
					{
						srcGuid = newElement.Attribute("guid").Value;
						origElement = XElement.Parse(origData[srcGuid]);
						origData.Remove(srcGuid);
						if (origElement.Attribute("class").Value == "WfiWordform")
						{
							var csProp = origElement.Element("Checksum");
							if (csProp != null)
								csProp.Remove();
						}
					}

					if (XmlUtilities.AreXmlElementsEqual(origElement.ToString(), newElement.ToString()))
						continue;

					if (srcGuid == null)
					{
						File.WriteAllText(Path.Combine(_workingDir, "CustomProperties-SRC.txt"), origElement.ToString());
						File.WriteAllText(Path.Combine(_workingDir, "CustomProperties-TRG.txt"), newElement.ToString());
						sb.Append("Main src and trg custom properties are different in the resulting xml.");
					}
					else
					{
						File.WriteAllText(Path.Combine(_workingDir, srcGuid + "-SRC.txt"), origElement.ToString());
						File.WriteAllText(Path.Combine(_workingDir, srcGuid + "-TRG.txt"), newElement.ToString());
						sb.AppendFormat("Main src and trg object with guid '{0}' are different in the resulting xml.", srcGuid);
					}
					sb.AppendLine();
				}
			}
			verifyTimer.Stop();
		}

		private void NestFile(string srcFwdataPathname)
		{
			var mdc = GetFreshMdc(); // Want it fresh.
			var unownedObjects = new Dictionary<string, SortedDictionary<string, XElement>>(200);
			// Outer dictionary has the class name for its key and a sorted (by guid) dictionary as its value.
			// The inner dictionary has a caseless guid as the key and the byte array as the value.
			var classData = new Dictionary<string, SortedDictionary<string, XElement>>(200, StringComparer.OrdinalIgnoreCase);
			var guidToClassMapping = new Dictionary<string, string>();
			TokenizeFile(mdc, srcFwdataPathname, unownedObjects, classData, guidToClassMapping);

			var root = new XElement("root");
			foreach (var unownedElementKvp in unownedObjects)
			{
				var className = unownedElementKvp.Key;
				var classElement = new XElement(className);
				var unownedElementDict = unownedElementKvp.Value;
				foreach (var unownedElement in unownedElementDict.Values)
				{
					classElement.Add(unownedElement);
					CmObjectNestingService.NestObject(false, unownedElement,
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
							var adjustedTypeValue = MetadataCache.AdjustedPropertyType(typeAttr.Value);
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
			GC.Collect(2, GCCollectionMode.Forced);
		}
	}
}
