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
using Chorus.merge;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using FLEx_ChorusPlugin.Infrastructure.Handling;
using Palaso.Progress;
using Palaso.Xml;
using TriboroughBridge_ChorusPlugin;

namespace FwdataTestApp
{
	public partial class NestFwdataFile : Form
	{
		private const string NormalUserProjectDir = @"C:\ProgramData\SIL\FieldWorks 7\TestProjects";
		internal static readonly Encoding Utf8 = Encoding.UTF8;
		private string _srcFwdataPathname;
		private string _workingDir;

		public NestFwdataFile()
		{
			InitializeComponent();
		}

		private void BrowseForFile(object sender, EventArgs e)
		{
			_btnRunSelected.Enabled = false;
			_fwdataPathname.Text = null;

			//if (string.IsNullOrEmpty(_openFileDialog.InitialDirectory))
			//	_openFileDialog.InitialDirectory = NormalUserProjectDir;
			//_openFileDialog.RestoreDirectory = false;
			if (_openFileDialog.ShowDialog(this) != DialogResult.OK)
				return;

			var currentFwdataPathname = _openFileDialog.FileName;
			var currentFilename = Path.GetFileName(currentFwdataPathname);
			var projectDirName = Path.GetDirectoryName(currentFwdataPathname);
			if (currentFilename.ToLowerInvariant() == "zpi" + Utilities.FwXmlExtension || projectDirName.ToLowerInvariant() == "zpi")
				return; // Don't even think of wiping out my ZPI folder.

			_fwdataPathname.Text = currentFwdataPathname;
			_btnRunSelected.Enabled = true;
		}

		private MetadataCache GetFreshMdc()
		{
			var mdc = MetadataCache.TestOnlyNewCache;
			var modelData = File.ReadAllText(Path.Combine(_workingDir, SharedConstants.ModelVersionFilename));
			mdc.UpgradeToVersion(Int32.Parse(modelData.Split(new[] { "{", ":", "}" }, StringSplitOptions.RemoveEmptyEntries)[1]));
			var customPropPathname = Path.Combine(_workingDir, SharedConstants.CustomPropertiesFilename);
			mdc.AddCustomPropInfo(new MergeOrder(
					customPropPathname, customPropPathname, customPropPathname,
					new MergeSituation(customPropPathname, "", "", "", "", MergeOrder.ConflictHandlingModeChoices.WeWin)));
			return mdc;
		}

		private static void CacheDataRecord(IDictionary<string, SortedDictionary<string, string>> unownedObjects, IDictionary<string, SortedDictionary<string, string>> classData, IDictionary<string, string> guidToClassMapping, string record)
		{
			//var rtElement = XElement.Parse(record);
			var attrValues = XmlUtils.GetAttributes(record, new HashSet<string> { SharedConstants.GuidStr, SharedConstants.Class, SharedConstants.OwnerGuid });
			var guid = attrValues[SharedConstants.GuidStr].ToLowerInvariant();
			var className = attrValues[SharedConstants.Class];
			if (attrValues[SharedConstants.OwnerGuid] == null)
			{
				SortedDictionary<string, string> unownedForCurrentClassName;
				if (!unownedObjects.TryGetValue(className, out unownedForCurrentClassName))
				{
					unownedForCurrentClassName = new SortedDictionary<string, string>();
					unownedObjects.Add(className, unownedForCurrentClassName);
				}
				unownedForCurrentClassName.Add(guid, record);
			}
			guidToClassMapping.Add(guid.ToLowerInvariant(), className);

			// 1. Set 'Checksum' to zero (0).
			if (className == "WfiWordform")
			{
				var wfElement = XElement.Parse(record);
				var csElement = wfElement.Element("Checksum");
				if (csElement != null)
				{
					csElement.Remove();
					record = wfElement.ToString();
				}
			}

			// Theory has it the FW data is sorted.
			//// 2. Sort <rt>
			//DataSortingService.SortMainRtElement(rtElement);

			// 3. Cache it.
			SortedDictionary<string, string> recordData;
			if (!classData.TryGetValue(className, out recordData))
			{
				recordData = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				classData.Add(className, recordData);
			}
			recordData.Add(guid, record);
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
			var ambiguousTimer = new Stopwatch();
			var restoreTimer = new Stopwatch();
			var verifyTimer = new Stopwatch();
			var checkOwnObjsurTimer = new Stopwatch();
			var validateTimer = new Stopwatch();
			var ownObjsurFound = false;
			try
			{
				if (_rebuildDataFile.Checked)
				{
					RestoreMainFileFromPieces(restoreTimer);
				}
				else
				{
					RestoreProjectIfNeeded(_srcFwdataPathname);
					if (_cbRoundTripData.Checked)
					{
						RoundTripData(breakupTimer, restoreTimer, ambiguousTimer, sbValidation);
					}
					if (_cbVerify.Checked)
					{
						Verify(verifyTimer, sb);
					}
					if (_cbValidate.Checked)
					{
						ValidateSplitData(validateTimer, sb, sbValidation);
					}
					if (_cbNestFile.Checked)
					{
						ownObjsurFound = NestFile(nestTimer, checkOwnObjsurTimer, _cbCheckOwnObjsur.Checked);
					}
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
					"Time to nest file: {1}{0}Time to check nested file: {2}{0}Own objsur Found: {3}{0}Time to breakup file: {4}.{0}Time to restore file: {5}.{0}Time to verify restoration: {6}.{0}Time to validate files: {7}.{0}Time to check ambiguous data: {8}.{0}{0}{9}",
					Environment.NewLine,
					nestTimer.ElapsedMilliseconds > 0 ? nestTimer.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture) : "Not run",
					checkOwnObjsurTimer.ElapsedMilliseconds > 0 ? checkOwnObjsurTimer.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture) : "Not run",
					_cbCheckOwnObjsur.Checked ? (ownObjsurFound ? "********* YES FIX BUG *********" : "No") : "Not run",
					breakupTimer.ElapsedMilliseconds > 0 ? breakupTimer.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture) : "Not run",
					restoreTimer.ElapsedMilliseconds > 0 ? restoreTimer.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture) : "Not run",
					verifyTimer.ElapsedMilliseconds > 0 ? verifyTimer.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture) : "Not run",
					validateTimer.ElapsedMilliseconds > 0 ? validateTimer.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture) : "Not run",
					ambiguousTimer.ElapsedMilliseconds > 0 ? ambiguousTimer.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture) : "Not run",
					sb);
				File.WriteAllText(Path.Combine(_workingDir, "Comparison.log"), compTxt);
				var validationErrors = sbValidation.ToString();
				if (validationErrors.Length > 0)
					File.WriteAllText(Path.Combine(_workingDir, "Validation.log"), validationErrors);
				Cursor = Cursors.Default;
				Close();
			}
		}

		private void RestoreMainFileFromPieces(Stopwatch restoreTimer)
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

		private void RoundTripData(Stopwatch breakupTimer, Stopwatch restoreTimer, Stopwatch ambiguousTimer, StringBuilder sbValidation)
		{
			File.Copy(_srcFwdataPathname, _srcFwdataPathname + ".orig", true); // Keep it safe.
			breakupTimer.Start();
			FLExProjectSplitter.PushHumptyOffTheWall(new NullProgress(), _srcFwdataPathname);
			breakupTimer.Stop();
			GC.Collect(2, GCCollectionMode.Forced);

			if (_cbCheckAmbiguousElements.Checked)
			{
				var allDataFiles = new HashSet<string>();
				var currentDir = Path.Combine(_workingDir, "Linguistics");
				if (Directory.Exists(currentDir))
				{
					allDataFiles.UnionWith(from pathname in Directory.GetFiles(currentDir, "*.*", SearchOption.AllDirectories)
										   where !pathname.ToLowerInvariant().EndsWith("chorusnotes")
										   select pathname);
				}
				currentDir = Path.Combine(_workingDir, "Anthropology");
				if (Directory.Exists(currentDir))
				{
					allDataFiles.UnionWith(from pathname in Directory.GetFiles(currentDir, "*.*", SearchOption.AllDirectories)
										   where !pathname.ToLowerInvariant().EndsWith("chorusnotes")
										   select pathname);
				}
				currentDir = Path.Combine(_workingDir, "Other");
				if (Directory.Exists(currentDir))
				{
					allDataFiles.UnionWith(
						from pathname in Directory.GetFiles(currentDir, "*.*", SearchOption.AllDirectories)
						where !pathname.ToLowerInvariant().EndsWith("chorusnotes")
						select pathname);
				}
				currentDir = Path.Combine(_workingDir, "General");
				if (Directory.Exists(currentDir))
				{
					allDataFiles.UnionWith(from pathname in Directory.GetFiles(currentDir, "*.*", SearchOption.AllDirectories)
										   where !pathname.ToLowerInvariant().EndsWith("chorusnotes")
										   select pathname);
				}
				var mergeOrder = new MergeOrder(null, null, null, new NullMergeSituation())
				{
					EventListener = new ChangeAndConflictAccumulator()
				};
				var merger = FieldWorksMergeStrategyServices.CreateXmlMergerForFieldWorksData(mergeOrder, MetadataCache.MdCache);
				ambiguousTimer.Start();
				foreach (var dataFile in allDataFiles)
				{
					var extension = Path.GetExtension(dataFile).Substring(1);
					string optionalElementName = null;
					string mainRecordName = null;
					switch (extension)
					{
						case SharedConstants.Style:
							mainRecordName = SharedConstants.StStyle;
							break;
						case SharedConstants.List:
							mainRecordName = SharedConstants.CmPossibilityList;
							break;
						case SharedConstants.langproj:
							mainRecordName = SharedConstants.LangProject;
							break;
						case SharedConstants.Annotation:
							mainRecordName = SharedConstants.CmAnnotation;
							break;
						case SharedConstants.Filter:
							mainRecordName = SharedConstants.CmFilter;
							break;
						case SharedConstants.orderings:
							mainRecordName = SharedConstants.VirtualOrdering;
							break;
						case SharedConstants.pictures:
							mainRecordName = SharedConstants.CmPicture;
							break;
						case SharedConstants.ArchivedDraft:
							mainRecordName = SharedConstants.ScrDraft;
							break;
						case SharedConstants.ImportSetting:
							mainRecordName = SharedConstants.ScrImportSet;
							break;
						case SharedConstants.Srs:
							mainRecordName = SharedConstants.ScrRefSystem;
							break;
						case SharedConstants.Trans:
							mainRecordName = SharedConstants.Scripture;
							break;
						case SharedConstants.bookannotations:
							mainRecordName = SharedConstants.ScrBookAnnotations;
							break;
						case SharedConstants.book:
							mainRecordName = SharedConstants.ScrBook;
							break;
						case SharedConstants.Ntbk:
							optionalElementName = SharedConstants.Header;
							mainRecordName = SharedConstants.RnGenericRec;
							break;
						case SharedConstants.Reversal:
							optionalElementName = SharedConstants.Header;
							mainRecordName = SharedConstants.ReversalIndexEntry;
							break;
						case SharedConstants.Lexdb:
							optionalElementName = SharedConstants.Header;
							mainRecordName = SharedConstants.LexEntry;
							break;
						case SharedConstants.TextInCorpus:
							mainRecordName = SharedConstants.Text;
							break;
						case SharedConstants.Inventory:
							optionalElementName = SharedConstants.Header;
							mainRecordName = SharedConstants.WfiWordform;
							break;
						case SharedConstants.DiscourseExt:
							optionalElementName = SharedConstants.Header;
							mainRecordName = SharedConstants.DsChart;
							break;
						case SharedConstants.Featsys:
							mainRecordName = SharedConstants.FsFeatureSystem;
							break;
						case SharedConstants.Phondata:
							mainRecordName = SharedConstants.PhPhonData;
							break;
						case SharedConstants.Morphdata:
							mainRecordName = SharedConstants.MoMorphData;
							break;
						case SharedConstants.Agents:
							mainRecordName = SharedConstants.CmAgent;
							break;
					}
					using (var fastSplitter = new FastXmlElementSplitter(dataFile))
					{
						bool foundOptionalFirstElement;
						foreach (var parentNode in fastSplitter.GetSecondLevelElementStrings(optionalElementName, mainRecordName, out foundOptionalFirstElement).Select(record => XmlUtilities.GetDocumentNodeFromRawXml(record, new XmlDocument())))
						{
							XmlMergeService.RemoveAmbiguousChildren(merger.EventListener, merger.MergeStrategies, parentNode);
						}
					}
				}
				ambiguousTimer.Stop();
				foreach (var warning in ((ChangeAndConflictAccumulator)merger.EventListener).Warnings)
				{
					sbValidation.AppendLine(warning.Description);
					sbValidation.AppendLine();
				}
				GC.Collect(2, GCCollectionMode.Forced);
			}
			restoreTimer.Start();
			FLExProjectUnifier.PutHumptyTogetherAgain(new NullProgress(), _srcFwdataPathname);
			restoreTimer.Stop();
			GC.Collect(2, GCCollectionMode.Forced);
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

		private void Verify(Stopwatch verifyTimer, StringBuilder sb)
		{
			verifyTimer.Start();
			GetFreshMdc(); // Want it fresh.
			var origData = new Dictionary<string, byte[]>(StringComparer.InvariantCultureIgnoreCase);
			using (var fastSplitterOrig = new FastXmlElementSplitter(_srcFwdataPathname + ".orig"))
			{
				bool foundOrigOptionalFirstElement;
				foreach (var origRecord in fastSplitterOrig.GetSecondLevelElementBytes(SharedConstants.AdditionalFieldsTag, SharedConstants.RtTag, out foundOrigOptionalFirstElement))
				{
					if (foundOrigOptionalFirstElement)
					{
						origData.Add(SharedConstants.AdditionalFieldsTag, origRecord);
						foundOrigOptionalFirstElement = false;
						continue;
					}
					origData.Add(XmlUtils.GetAttributes(origRecord, new HashSet<string> { SharedConstants.GuidStr })[SharedConstants.GuidStr].ToLowerInvariant(), origRecord);
				}
			}
			GC.Collect(2, GCCollectionMode.Forced);
			using (var fastSplitterNew = new FastXmlElementSplitter(_srcFwdataPathname))
			{
				bool foundNewOptionalFirstElement;
				var newRecords = fastSplitterNew.GetSecondLevelElementStrings(SharedConstants.AdditionalFieldsTag, SharedConstants.RtTag, out foundNewOptionalFirstElement);
				// NB: The main input file *does* have to deal with the optional first element.
				var utf8 = Encoding.UTF8;
				var counter = 0;
				foreach (var newRecord in newRecords)
				{
					var newRecCopy = newRecord;
					byte[] origRecString;
					string srcGuid = null;
					if (newRecCopy.Contains(SharedConstants.AdditionalFieldsTag))
					{
						origRecString = origData[SharedConstants.AdditionalFieldsTag];
						origData.Remove(SharedConstants.AdditionalFieldsTag);
					}
					else
					{
						var attrValues = XmlUtils.GetAttributes(newRecord, new HashSet<string> { SharedConstants.GuidStr, SharedConstants.Class });
						srcGuid = attrValues[SharedConstants.GuidStr];
						origRecString = origData[srcGuid];
						origData.Remove(srcGuid);
						if (attrValues[SharedConstants.Class] == "WfiWordform")
						{
							var wfElement = XElement.Parse(utf8.GetString(origRecString));
							var csProp = wfElement.Element("Checksum");
							if (csProp != null)
								csProp.Remove();
							origRecString = utf8.GetBytes(wfElement.ToString());
						}
					}

					if (XmlUtilities.AreXmlElementsEqual(utf8.GetString(origRecString), newRecCopy))
						continue;

					if (srcGuid == null)
					{
						WriteProblemDataFile(Path.Combine(_workingDir, "CustomProperties-SRC.txt"), origRecString);
						WriteProblemDataFile(Path.Combine(_workingDir, srcGuid + "CustomProperties-TRG.txt"), utf8.GetBytes(newRecCopy));
						sb.Append("Main src and trg custom properties are different in the resulting xml.");
					}
					else
					{
						WriteProblemDataFile(Path.Combine(_workingDir, srcGuid + "-SRC.txt"), origRecString);
						WriteProblemDataFile(Path.Combine(_workingDir, srcGuid + "-TRG.txt"), utf8.GetBytes(newRecCopy));
						sb.AppendFormat("Main src and trg object with guid '{0}' are different in the resulting xml.", srcGuid);
					}
					sb.AppendLine();
					if (counter == 1000)
					{
						GC.Collect(2, GCCollectionMode.Forced);
						counter = 0;
					}
					else
					{
						counter++;
					}
				}
			}
			if (origData.Count > 0)
			{
				sb.AppendFormat("Hmm, there are {0} more <rt> elements in the original than in the rebuilt fwdata file.", origData.Count);
				sb.AppendLine();
				foreach (var attrs in origData.Values.Select(byteData => XmlUtils.GetAttributes(byteData, new HashSet<string> { SharedConstants.GuidStr, SharedConstants.Class })))
				{
					sb.AppendFormat("\t\t'{0}' of class '{1}' is not in rebuilt file.", attrs[SharedConstants.GuidStr], attrs[SharedConstants.Class]);
					sb.AppendLine();
				}
			}
			verifyTimer.Stop();
		}

		private static void WriteProblemDataFile(string pathname, byte[] data)
		{
			using (var reader = XmlReader.Create(new MemoryStream(data, false), CanonicalXmlSettings.CreateXmlReaderSettings()))
			using (var writer = XmlWriter.Create(pathname, CanonicalXmlSettings.CreateXmlWriterSettings()))
			{
				writer.WriteNode(reader, true);
			}
		}

		private void NestFile(string srcFwdataPathname)
		{
			var mdc = GetFreshMdc(); // Want it fresh.
			var unownedObjects = new Dictionary<string, SortedDictionary<string, string>>(200);
			// Outer dictionary has the class name for its key and a sorted (by guid) dictionary as its value.
			// The inner dictionary has a caseless guid as the key and the byte array as the value.
			var classData = new Dictionary<string, SortedDictionary<string, string>>(200, StringComparer.OrdinalIgnoreCase);
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
					var element = XElement.Parse(unownedElement);
					classElement.Add(element);
					CmObjectNestingService.NestObject(false, element,
												  classData,
												  guidToClassMapping);
				}
				root.Add(classElement);
			}
			FileWriterService.WriteNestedFile(srcFwdataPathname + ".nested", root);
		}

		private static void TokenizeFile(MetadataCache mdc, string srcFwdataPathname, Dictionary<string, SortedDictionary<string, string>> unownedObjects, Dictionary<string, SortedDictionary<string, string>> classData, Dictionary<string, string> guidToClassMapping)
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

		private void RestoreProjects(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			try
			{
				// Wipe out contents of all test folders in regular FW project location,
				// EXCEPT the real ZPI project.
				// If there is no copy of the fwdata file in the main project folder, then skip it.
				var allProjectDirNamesExceptMine = Directory.GetDirectories(NormalUserProjectDir).Where(projectDirName => Path.GetFileNameWithoutExtension(projectDirName).ToLowerInvariant() != "zpi");
				foreach (var projectDirName in allProjectDirNamesExceptMine)
				{
					RestoreProjectIfNeeded(Directory.GetFiles(projectDirName, "*" + Utilities.FwXmlExtension).FirstOrDefault());
				}
			}
			finally
			{
				Cursor = Cursors.Default;
			}
		}

		private static void RestoreProjectIfNeeded(string currentFwdataPathname)
		{
			if (currentFwdataPathname == null)
				return;
			var currentFilename = Path.GetFileName(currentFwdataPathname);
			var projectDirName = Path.GetDirectoryName(currentFwdataPathname);
			if (currentFilename.ToLowerInvariant() == "zpi" + Utilities.FwXmlExtension || projectDirName.ToLowerInvariant() == "zpi")
				return; // Don't even think of wiping out my ZPI folder.

			var backupDataFilesFullPathnames = Directory.GetFiles(NormalUserProjectDir, "*" + Utilities.FwXmlExtension, SearchOption.TopDirectoryOnly);
			var backupDataFilenames = backupDataFilesFullPathnames.Select(pathname => Path.GetFileName(pathname)).ToList();
			if (!backupDataFilenames.Contains(currentFilename))
				return;

			var backupFileInfo = backupDataFilesFullPathnames
				.Where(pathname => Path.GetFileName(pathname).ToLowerInvariant() == currentFilename.ToLowerInvariant())
				.Select(pathname => new FileInfo(pathname)).First();
			var currentFileInfo = new FileInfo(currentFwdataPathname);
			var subDirs = Directory.GetDirectories(projectDirName, "*.*", SearchOption.TopDirectoryOnly);
			var allFiles = Directory.GetFiles(projectDirName, "*.*", SearchOption.TopDirectoryOnly);
			if (backupFileInfo.LastWriteTimeUtc == currentFileInfo.LastWriteTimeUtc && (subDirs.Length == 0 && allFiles.Length == 1 && currentFwdataPathname == allFiles[0]))
				return;

			// Clear it out.
			foreach (var subDir in subDirs)
				Directory.Delete(subDir, true);
			foreach (var pathname in allFiles)
				File.Delete(pathname);
			File.Copy(Path.Combine(NormalUserProjectDir, currentFilename), Path.Combine(projectDirName, currentFilename));
		}

		private void ClearCheckboxes(object sender, EventArgs e)
		{
			_cbNestFile.CheckState = CheckState.Unchecked;
			_cbRoundTripData.CheckState = CheckState.Unchecked;
			_cbVerify.CheckState = CheckState.Unchecked;
			_cbCheckOwnObjsur.CheckState = CheckState.Unchecked;
			_cbValidate.CheckState = CheckState.Unchecked;
			_rebuildDataFile.CheckState = CheckState.Unchecked;
		}
	}
}
