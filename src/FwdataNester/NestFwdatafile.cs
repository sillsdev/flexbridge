// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

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
using Chorus.FileTypeHandlers;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.merge.xml.generic.xmldiff;
using Chorus.VcsDrivers.Mercurial;
using LibFLExBridgeChorusPlugin.DomainServices;
using LibFLExBridgeChorusPlugin.Infrastructure;
using SIL.Progress;
using SIL.Xml;
using LibTriboroughBridgeChorusPlugin;
using SIL.PlatformUtilities;
using TriboroughBridge_ChorusPlugin;

namespace FwdataTestApp
{
	public sealed partial class NestFwdataFile : Form
	{
		private static string CurrentBaseFolder = @"D:\TestProjects";
		private string _srcFwdataPathname;
		private string _workingDir;

		public NestFwdataFile()
		{
			if (Platform.IsUnix)
			{
				CurrentBaseFolder = Path.Combine(Environment.GetEnvironmentVariable(@"HOME"), @"TestProjects");
			}

			InitializeComponent();

			_fwdataPathname.Text = CurrentBaseFolder;
			_folderBrowserDialog.SelectedPath = CurrentBaseFolder;

			PopulateList();

			_btnRunSelected.Enabled = _listView.Items.Count > 0;
		}

		private void BrowseForFolder(object sender, EventArgs e)
		{
			_btnRunSelected.Enabled = false;
			_fwdataPathname.Text = CurrentBaseFolder;

			if (_folderBrowserDialog.ShowDialog(this) != DialogResult.OK)
				return;

			CurrentBaseFolder = _folderBrowserDialog.SelectedPath;
			_fwdataPathname.Text = CurrentBaseFolder;
			PopulateList();

			_btnRunSelected.Enabled = _listView.Items.Count > 0;
		}

		private void PopulateList()
		{
			_listView.SuspendLayout();
			_listView.Items.Clear();
			foreach (var projectDir in Directory.GetDirectories(CurrentBaseFolder))
			{
				if (projectDir.EndsWith("zpi") || projectDir.Contains("."))
					continue;

				var fwdataFiles = Directory.GetFiles(projectDir, "*.fwdata", SearchOption.TopDirectoryOnly);
				if (fwdataFiles.Length == 0)
					continue;

				var fwdataFileName = Path.GetFileNameWithoutExtension(fwdataFiles[0]);
				var listItem = new ListViewItem(fwdataFileName)
					{
						Tag = projectDir,
						Checked = true
					};
				_listView.Items.Add(listItem);
			}
			_listView.ResumeLayout();
		}

		private void RunSelected(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			var totalRunTimer = new Stopwatch();
			var sbTopLevel = new StringBuilder();
			totalRunTimer.Start();
			foreach (ListViewItem selectedItem in _listView.CheckedItems)
			{
				GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
				RunSelected(Path.Combine((string)selectedItem.Tag, selectedItem.Text + ".fwdata"), sbTopLevel);
			}
			totalRunTimer.Stop();
			sbTopLevel.AppendFormat(@"Time to run everything: {0}", totalRunTimer.Elapsed);
			File.WriteAllText(Path.Combine(CurrentBaseFolder, "TotalTime.log"), sbTopLevel.ToString());
			Cursor = Cursors.Default;
			Close();
		}

		private void RunSelected(string currentFwdataPathname, StringBuilder sbTopLevel)
		{
			_srcFwdataPathname = currentFwdataPathname;
			_workingDir = Path.GetDirectoryName(_srcFwdataPathname);
			var sb = new StringBuilder();
			var sbValidation = new StringBuilder();
			var nestTimer = new Stopwatch();
			var breakupTimer = new Stopwatch();
			var ambiguousTimer = new Stopwatch();
			var restoreTimer = new Stopwatch();
			var verifyTimer = new Stopwatch();
			var checkOwnObjsurTimer = new Stopwatch();
			var validateTimer = new Stopwatch();
			var danglingRefsTimer = new Stopwatch();
			var ownObjsurFound = false;
			try
			{
				if (_cbFindDanglingRefs.Checked)
				{
					CheckForDanglingReferencesInMainFile(danglingRefsTimer, sb);
				}
				else if (_rebuildDataFile.Checked)
				{
					if (!String.IsNullOrWhiteSpace(revisionBox.Text))
					{
						HgRunner.Run("hg update -r " + revisionBox.Text, _workingDir, 300, new NullProgress());
					}
					RestoreMainFileFromPieces(restoreTimer);
				}
				else
				{
					RestoreProjectIfNeeded(_srcFwdataPathname);
					if (_cbRoundTripData.Checked)
					{
						RoundTripData(breakupTimer, restoreTimer, ambiguousTimer, sbValidation);
					}
					if (_cbValidate.Checked)
					{
						ValidateSplitData(validateTimer, sb, sbValidation);
					}
					if (_cbVerify.Checked)
					{
						Verify(verifyTimer, sb);
					}
					if (_cbNestFile.Checked)
					{
						ownObjsurFound = NestFile(nestTimer, checkOwnObjsurTimer, _cbCheckOwnObjsur.Checked);
					}
				}
			}
			catch (Exception err)
			{
				GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
				File.WriteAllText(Path.Combine(_workingDir, "StackTrace.log"),
								  err.GetType().Name + Environment.NewLine + err.StackTrace);
				if (File.Exists(_srcFwdataPathname + ".orig"))
				{
					File.Delete(_srcFwdataPathname);
					File.Move(_srcFwdataPathname + ".orig", _srcFwdataPathname); // Restore it.
				}
			}
			finally
			{
				var comparisonNotes = sb.ToString();
				var compTxt = string.Format(
					"Time to nest file: {1}{0}Time to check nested file: {2}{0}Own objsur Found: {3}{0}Time to breakup file: {4}.{0}Time to restore file: {5}.{0}Time to verify restoration: {6}.{0}Time to validate files: {7}.{0}Time to check ambiguous data: {8}.{0}Time to check dangling refs in main file: {9}.{0}{0}{10}",
					Environment.NewLine,
					nestTimer.ElapsedMilliseconds > 0 ? nestTimer.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture) : "Not run",
					checkOwnObjsurTimer.ElapsedMilliseconds > 0
						? checkOwnObjsurTimer.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture)
						: "Not run",
					_cbCheckOwnObjsur.Checked ? (ownObjsurFound ? "********* YES FIX BUG *********" : "No") : "Not run",
					breakupTimer.ElapsedMilliseconds > 0
						? breakupTimer.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture)
						: "Not run",
					restoreTimer.ElapsedMilliseconds > 0
						? restoreTimer.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture)
						: "Not run",
					verifyTimer.ElapsedMilliseconds > 0
						? verifyTimer.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture)
						: "Not run",
					validateTimer.ElapsedMilliseconds > 0
						? validateTimer.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture)
						: "Not run",
					ambiguousTimer.ElapsedMilliseconds > 0
						? ambiguousTimer.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture)
						: "Not run",
					danglingRefsTimer.ElapsedMilliseconds > 0
						? danglingRefsTimer.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture)
						: "Not run",
						comparisonNotes);
				var projectComparisonLogPathname = Path.Combine(_workingDir, "Comparison.log");
				File.WriteAllText(projectComparisonLogPathname, compTxt);
				if (comparisonNotes.Length > 0)
				{
					sbTopLevel.AppendFormat("See comparison log: '{0}'", projectComparisonLogPathname);
					File.WriteAllText(projectComparisonLogPathname, comparisonNotes);
				}
				var validationErrors = sbValidation.ToString();
				if (validationErrors.Length > 0)
				{
					var projectValidationLogPathname = Path.Combine(_workingDir, "Validation.log");
					sbTopLevel.AppendFormat("See validation log: '{0}'", projectValidationLogPathname);
					File.WriteAllText(projectValidationLogPathname, validationErrors);
				}
			}
		}

		private void CheckForDanglingReferencesInMainFile(Stopwatch danglingRefsTimer, StringBuilder sb)
		{
			var mdc = GetFreshMdc(); // Want it fresh.
			var unownedObjects = new Dictionary<string, SortedDictionary<string, byte[]>>(200);
			// Outer dictionary has the class name for its key and a sorted (by guid) dictionary as its value.
			// The inner dictionary has a caseless guid as the key and the byte array as the value.
			var classData = new Dictionary<string, SortedDictionary<string, byte[]>>(200, StringComparer.OrdinalIgnoreCase);
			var guidToClassMapping = new Dictionary<string, string>();
			TokenizeFile(mdc, _srcFwdataPathname, unownedObjects, classData, guidToClassMapping);

			danglingRefsTimer.Start();
			var danglingRefGuids = new Dictionary<string, HashSet<string>>();
			foreach (var kvp in classData.Values.SelectMany(innerDict => innerDict)
				.ToDictionary(innerKvp => innerKvp.Key,
					innerKvp => TriboroughBridgeUtilities.CreateFromBytes(innerKvp.Value)))
			{
				var haveWrittenMainObjInfo = false;
				var currentMainGuid = kvp.Key;
				var currentMainObject = kvp.Value;
				foreach (var objsurRefElement in currentMainObject.Descendants("objsur").Where(objsurElement => objsurElement.Attribute("t").Value == "r"))
				{
					var danglingRefGuid = objsurRefElement.Attribute(FlexBridgeConstants.GuidStr).Value;
					if (guidToClassMapping.ContainsKey(danglingRefGuid))
						continue;
					// Dangling reference.
					HashSet<string> danglingRefsInObject;
					if (!danglingRefGuids.TryGetValue(currentMainGuid, out danglingRefsInObject))
					{
						danglingRefsInObject = new HashSet<string>();
						danglingRefGuids.Add(currentMainGuid, danglingRefsInObject);
					}
					danglingRefsInObject.Add(danglingRefGuid);
					if (!haveWrittenMainObjInfo)
					{
						haveWrittenMainObjInfo = true;
						sb.AppendLine();
						sb.AppendLine(currentMainObject.ToString());
					}
					sb.AppendFormat("Dangling ref: {0}{1}", danglingRefGuid, Environment.NewLine);
				}
			}

			danglingRefsTimer.Stop();
		}

		private MetadataCache GetFreshMdc()
		{
			var mdc = MetadataCache.TestOnlyNewCache;
			var modelVersionPathname = Path.Combine(_workingDir, FlexBridgeConstants.ModelVersionFilename);
			if (!File.Exists(modelVersionPathname))
			{
				FLExProjectSplitter.WriteVersionFile(_srcFwdataPathname);
				using (var fastSplitter = new FastXmlElementSplitter(_srcFwdataPathname))
				{
					bool foundOptionalFirstElement;
					// NB: The main input file *does* have to deal with the optional first element.
					foreach (var record in fastSplitter.GetSecondLevelElementBytes(FlexBridgeConstants.AdditionalFieldsTag, FlexBridgeConstants.RtTag, out foundOptionalFirstElement))
					{
						if (foundOptionalFirstElement)
						{
							// 2. Write custom properties file with custom properties.
							FileWriterService.WriteCustomPropertyFile(mdc, _workingDir, record);
						}
						else
						{
							// Write empty custom properties file.
							FileWriterService.WriteCustomPropertyFile(Path.Combine(_workingDir, FlexBridgeConstants.CustomPropertiesFilename), null);
						}
						break;
					}
				}
			}
			var modelData = File.ReadAllText(modelVersionPathname);
			mdc.UpgradeToVersion(Int32.Parse(modelData.Split(new[] { "{", ":", "}" }, StringSplitOptions.RemoveEmptyEntries)[1]));
			var customPropPathname = Path.Combine(_workingDir, FlexBridgeConstants.CustomPropertiesFilename);
			mdc.AddCustomPropInfo(new MergeOrder(
					customPropPathname, customPropPathname, customPropPathname,
					new MergeSituation(customPropPathname, "", "", "", "", MergeOrder.ConflictHandlingModeChoices.WeWin)));
			return mdc;
		}

		private static void CacheDataRecord(IDictionary<string, SortedDictionary<string, byte[]>> unownedObjects, IDictionary<string, SortedDictionary<string, byte[]>> classData, IDictionary<string, string> guidToClassMapping, byte[] record)
		{
			var attrValues = XmlUtils.GetAttributes(record, new HashSet<string>
				{
					FlexBridgeConstants.GuidStr,
					FlexBridgeConstants.Class,
					FlexBridgeConstants.OwnerGuid
				});
			var guid = attrValues[FlexBridgeConstants.GuidStr].ToLowerInvariant();
			var className = attrValues[FlexBridgeConstants.Class];
			if (attrValues[FlexBridgeConstants.OwnerGuid] == null)
			{
				SortedDictionary<string, byte[]> unownedForCurrentClassName;
				if (!unownedObjects.TryGetValue(className, out unownedForCurrentClassName))
				{
					unownedForCurrentClassName = new SortedDictionary<string, byte[]>();
					unownedObjects.Add(className, unownedForCurrentClassName);
				}
				unownedForCurrentClassName.Add(guid, record);
			}
			guidToClassMapping.Add(guid.ToLowerInvariant(), className);

			// 1. Set 'Checksum' to zero (0).
			if (className == "WfiWordform")
			{
				var wfElement = TriboroughBridgeUtilities.CreateFromBytes(record);
				var csElement = wfElement.Element("Checksum");
				if (csElement != null)
				{
					csElement.Attribute(FlexBridgeConstants.Val).Value = "0";
					record = LibTriboroughBridgeSharedConstants.Utf8.GetBytes(wfElement.ToString());
				}
			}

			// Theory has it the FW data is sorted.
			//// 2. Sort <rt>
			//DataSortingService.SortMainRtElement(rtElement);

			// 3. Cache it.
			SortedDictionary<string, byte[]> recordData;
			if (!classData.TryGetValue(className, out recordData))
			{
				recordData = new SortedDictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
				classData.Add(className, recordData);
			}
			recordData.Add(guid, record);
		}

		private void RestoreMainFileFromPieces(Stopwatch restoreTimer)
		{
			GetFreshMdc(); // Want it fresh.
			restoreTimer.Start();
			FLExProjectUnifier.PutHumptyTogetherAgain(new NullProgress(), true, _srcFwdataPathname);
			restoreTimer.Stop();
		}

		private bool NestFile(Stopwatch nestTimer, Stopwatch checkOwnObjsurTimer, bool cbCheckOwnObjsurChecked)
		{
			var ownObjsurFound = false;
			nestTimer.Start();
			NestFile(_srcFwdataPathname);
			nestTimer.Stop();
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
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
			GetFreshMdc(); // Want it fresh.
			breakupTimer.Start();
			FLExProjectSplitter.PushHumptyOffTheWall(new NullProgress(), false, _srcFwdataPathname);
			breakupTimer.Stop();
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

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
				var merger = FieldWorksMergeServices.CreateXmlMergerForFieldWorksData(mergeOrder, MetadataCache.MdCache);
				ambiguousTimer.Start();
				foreach (var dataFile in allDataFiles)
				{
					var extension = Path.GetExtension(dataFile).Substring(1);
					string optionalElementName = null;
					string mainRecordName = null;
					switch (extension)
					{
						case FlexBridgeConstants.Style:
							mainRecordName = FlexBridgeConstants.StStyle;
							break;
						case FlexBridgeConstants.List:
							mainRecordName = FlexBridgeConstants.CmPossibilityList;
							break;
						case FlexBridgeConstants.langproj:
							mainRecordName = FlexBridgeConstants.LangProject;
							break;
						case FlexBridgeConstants.Annotation:
							mainRecordName = FlexBridgeConstants.CmAnnotation;
							break;
						case FlexBridgeConstants.Filter:
							mainRecordName = FlexBridgeConstants.CmFilter;
							break;
						case FlexBridgeConstants.orderings:
							mainRecordName = FlexBridgeConstants.VirtualOrdering;
							break;
						case FlexBridgeConstants.pictures:
							mainRecordName = FlexBridgeConstants.CmPicture;
							break;
						case FlexBridgeConstants.ArchivedDraft:
							mainRecordName = FlexBridgeConstants.ScrDraft;
							break;
						case FlexBridgeConstants.ImportSetting:
							mainRecordName = FlexBridgeConstants.ScrImportSet;
							break;
						case FlexBridgeConstants.Srs:
							mainRecordName = FlexBridgeConstants.ScrRefSystem;
							break;
						case FlexBridgeConstants.Trans:
							mainRecordName = FlexBridgeConstants.Scripture;
							break;
						case FlexBridgeConstants.bookannotations:
							mainRecordName = FlexBridgeConstants.ScrBookAnnotations;
							break;
						case FlexBridgeConstants.book:
							mainRecordName = FlexBridgeConstants.ScrBook;
							break;
						case FlexBridgeConstants.Ntbk:
							optionalElementName = FlexBridgeConstants.Header;
							mainRecordName = FlexBridgeConstants.RnGenericRec;
							break;
						case FlexBridgeConstants.Reversal:
							optionalElementName = FlexBridgeConstants.Header;
							mainRecordName = FlexBridgeConstants.ReversalIndexEntry;
							break;
						case FlexBridgeConstants.Lexdb:
							optionalElementName = FlexBridgeConstants.Header;
							mainRecordName = FlexBridgeConstants.LexEntry;
							break;
						case FlexBridgeConstants.TextInCorpus:
							mainRecordName = FlexBridgeConstants.Text;
							break;
						case FlexBridgeConstants.Inventory:
							optionalElementName = FlexBridgeConstants.Header;
							mainRecordName = FlexBridgeConstants.WfiWordform;
							break;
						case FlexBridgeConstants.DiscourseExt:
							optionalElementName = FlexBridgeConstants.Header;
							mainRecordName = FlexBridgeConstants.DsChart;
							break;
						case FlexBridgeConstants.Featsys:
							mainRecordName = FlexBridgeConstants.FsFeatureSystem;
							break;
						case FlexBridgeConstants.Phondata:
							mainRecordName = FlexBridgeConstants.PhPhonData;
							break;
						case FlexBridgeConstants.Morphdata:
							mainRecordName = FlexBridgeConstants.MoMorphData;
							break;
						case FlexBridgeConstants.Agents:
							mainRecordName = FlexBridgeConstants.CmAgent;
							break;
					}
					using (var fastSplitter = new FastXmlElementSplitter(dataFile))
					{
						bool foundOptionalFirstElement;
						foreach (var record in fastSplitter.GetSecondLevelElementBytes(optionalElementName, mainRecordName, out foundOptionalFirstElement))
						{
							XmlMergeService.RemoveAmbiguousChildren(merger.EventListener, merger.MergeStrategies, CreateXmlNodeFromBytes(record));
						}
					}
				}
				ambiguousTimer.Stop();
				foreach (var warning in ((ChangeAndConflictAccumulator)merger.EventListener).Warnings)
				{
					sbValidation.AppendLine(warning.Description);
					sbValidation.AppendLine();
					sbValidation.AppendLine(warning.HtmlDetails);
					sbValidation.AppendLine();
				}
				GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
			}
			restoreTimer.Start();
			FLExProjectUnifier.PutHumptyTogetherAgain(new NullProgress(), true, _srcFwdataPathname);
			restoreTimer.Stop();
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
		}

		private static XmlNode CreateXmlNodeFromBytes(byte[] xmlData)
		{
			using (var memoryStream = new MemoryStream(xmlData))
			{
				// This loads the MemoryStream as Utf8 xml. (I checked.)
				var document = new XmlDocument();
				document.Load(memoryStream);
				return document.DocumentElement;
			}
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
			var currentPathname = Path.Combine(_workingDir, FlexBridgeConstants.CustomPropertiesFilename);
			var validationError = fbHandler.ValidateFile(currentPathname, new NullProgress());
			if (validationError != null)
			{
				sbValidation.AppendFormat("File '{1}' reported an error:{0}\t{2}", Environment.NewLine, currentPathname, validationError);
				sbValidation.AppendLine();
				sb.AppendFormat("File '{1}' reported an error:{0}\t{2}", Environment.NewLine, currentPathname, validationError);
				sb.AppendLine();
			}
			// Model version file.
			currentPathname = Path.Combine(_workingDir, FlexBridgeConstants.ModelVersionFilename);
			validationError = fbHandler.ValidateFile(currentPathname, new NullProgress());
			if (validationError != null)
			{
				sbValidation.AppendFormat("File '{1}' reported an error:{0}\t{2}", Environment.NewLine, currentPathname, validationError);
				sbValidation.AppendLine();
				sb.AppendFormat("File '{1}' reported an error:{0}\t{2}", Environment.NewLine, currentPathname, validationError);
				sb.AppendLine();
			}

			// General
			foreach (var generalPathname in Directory.GetFiles(Path.Combine(_workingDir, "General"), "*.*", SearchOption.AllDirectories)
					.Where(pathname => !pathname.ToLowerInvariant().EndsWith("chorusnotes")))
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
			foreach (var anthropologyPathname in Directory.GetFiles(Path.Combine(_workingDir, "Anthropology"), "*.*", SearchOption.AllDirectories)
					.Where(pathname => !pathname.ToLowerInvariant().EndsWith("chorusnotes")))
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
				foreach (var scripturePathname in Directory.GetFiles(scriptureFolder, "*.*", SearchOption.AllDirectories)
						.Where(pathname => !pathname.ToLowerInvariant().EndsWith("chorusnotes")))
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
			foreach (var linguisticsPathname in Directory.GetFiles(Path.Combine(_workingDir, "Linguistics"), "*.*", SearchOption.AllDirectories)
					.Where(pathname => !pathname.ToLowerInvariant().EndsWith("chorusnotes")))
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
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
			verifyTimer.Start();
			GetFreshMdc(); // Want it fresh.
			var origData = new Dictionary<string, byte[]>(StringComparer.InvariantCultureIgnoreCase);
			using (var fastSplitterOrig = new FastXmlElementSplitter(_srcFwdataPathname + ".orig"))
			{
				var foundOrigOptionalFirstElement = false;
				var testedforExistanceOfOrigOptionalFirstElement = false;
				foreach (var origRecord in fastSplitterOrig.GetSecondLevelElementBytes(FlexBridgeConstants.AdditionalFieldsTag, FlexBridgeConstants.RtTag))
				{
					if (!testedforExistanceOfOrigOptionalFirstElement)
					{
						foundOrigOptionalFirstElement = FLExProjectSplitter.IsOptionalFirstElement(origRecord);
						testedforExistanceOfOrigOptionalFirstElement = true;
					}
					if (foundOrigOptionalFirstElement)
					{
						origData.Add(FlexBridgeConstants.AdditionalFieldsTag, origRecord);
						foundOrigOptionalFirstElement = false;
						continue;
					}
					origData.Add(XmlUtils.GetAttributes(origRecord, new HashSet<string> { FlexBridgeConstants.GuidStr })[FlexBridgeConstants.GuidStr].ToLowerInvariant(), origRecord);
				}
			}
			verifyTimer.Stop();
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
			verifyTimer.Start();
			using (var fastSplitterNew = new FastXmlElementSplitter(_srcFwdataPathname))
			{
				// NB: The main input file *does* have to deal with the optional first element.
				//var counter = 0;
				var foundNewOptionalFirstElement = false;
				var testedforExistanceOfNewOptionalFirstElement = false;
				foreach (var newRecordAsBytes in fastSplitterNew.GetSecondLevelElementBytes(FlexBridgeConstants.AdditionalFieldsTag, FlexBridgeConstants.RtTag))
				{
					if (!testedforExistanceOfNewOptionalFirstElement)
					{
						foundNewOptionalFirstElement = FLExProjectSplitter.IsOptionalFirstElement(newRecordAsBytes);
						testedforExistanceOfNewOptionalFirstElement = true;
					}
					var newRecCopyAsBytes = newRecordAsBytes;
					byte[] origRecAsBytes;
					string srcGuid = null;
					if (foundNewOptionalFirstElement)
					{
						origRecAsBytes = origData[FlexBridgeConstants.AdditionalFieldsTag];
						origData.Remove(FlexBridgeConstants.AdditionalFieldsTag);
						foundNewOptionalFirstElement = false;
					}
					else
					{
						var attrValues = XmlUtils.GetAttributes(newRecordAsBytes, new HashSet<string> { FlexBridgeConstants.GuidStr, FlexBridgeConstants.Class });
						srcGuid = attrValues[FlexBridgeConstants.GuidStr];
						origRecAsBytes = origData[srcGuid];
						origData.Remove(srcGuid);
						if (attrValues[FlexBridgeConstants.Class] == "WfiWordform")
						{
							var wfElement = TriboroughBridgeUtilities.CreateFromBytes(origRecAsBytes);
							var csProp = wfElement.Element("Checksum");
							if (csProp != null)
							{
								csProp.Attribute(FlexBridgeConstants.Val).Value = "0";
								origRecAsBytes = LibTriboroughBridgeSharedConstants.Utf8.GetBytes(wfElement.ToString());
							}
						}
					}

					//if (counter == 1000)
					//{
					//    verifyTimer.Stop();
					//    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
					//    verifyTimer.Start();
					//    counter = 0;
					//}
					//else
					//{
					//    counter++;
					//}
					// Way too slow, since it has to always make the XmlNodes.
					// Just feeding byte arrays to XmlUtilities.AreXmlElementsEqual is faster,
					// since it skips making them, if the byte arrays are the same.
					//var origNode = CreateXmlNodeFromBytes(origRecAsBytes);
					//var newNode = CreateXmlNodeFromBytes(newRecCopyAsBytes);
					//if (XmlUtilities.AreXmlElementsEqual(origNode, newNode))
					//    continue;
					//if (srcGuid == null)
					//{
					//    WriteProblemDataFile(Path.Combine(_workingDir, "CustomProperties-SRC.txt"), origNode);
					//    WriteProblemDataFile(Path.Combine(_workingDir, srcGuid + "CustomProperties-TRG.txt"), newNode);
					//    sb.Append("Main src and trg custom properties are different in the resulting xml.");
					//}
					//else
					//{
					//    WriteProblemDataFile(Path.Combine(_workingDir, srcGuid + "-SRC.txt"), origNode);
					//    WriteProblemDataFile(Path.Combine(_workingDir, srcGuid + "-TRG.txt"), newNode);
					//    sb.AppendFormat("Main src and trg object with guid '{0}' are different in the resulting xml.", srcGuid);
					//}
					//if (XmlUtilities.AreXmlElementsEqual(SharedConstants.Utf8.GetString(origRecAsBytes), SharedConstants.Utf8.GetString(newRecCopyAsBytes)))
					//	continue;
					if (XmlUtilities.AreXmlElementsEqual(origRecAsBytes, newRecCopyAsBytes))
						continue;
					if (srcGuid == null)
					{
						WriteProblemDataFile(Path.Combine(_workingDir, "CustomProperties-SRC.txt"), origRecAsBytes);
						WriteProblemDataFile(Path.Combine(_workingDir, srcGuid + "CustomProperties-TRG.txt"), newRecCopyAsBytes);
						sb.Append("Main src and trg custom properties are different in the resulting xml.");
					}
					else
					{
						WriteProblemDataFile(Path.Combine(_workingDir, srcGuid + "-SRC.txt"), origRecAsBytes);
						WriteProblemDataFile(Path.Combine(_workingDir, srcGuid + "-TRG.txt"), newRecCopyAsBytes);
						sb.AppendFormat("Main src and trg object with guid '{0}' are different in the resulting xml.", srcGuid);
					}
					sb.AppendLine();
				}
			}
			if (origData.Count > 0)
			{
				sb.AppendFormat("Hmm, there are {0} more <rt> elements in the original than in the rebuilt fwdata file.", origData.Count);
				sb.AppendLine();
				foreach (var attrs in origData.Values.Select(byteData => XmlUtils.GetAttributes(byteData, new HashSet<string> { FlexBridgeConstants.GuidStr, FlexBridgeConstants.Class })))
				{
					sb.AppendFormat("\t\t'{0}' of class '{1}' is not in rebuilt file.", attrs[FlexBridgeConstants.GuidStr], attrs[FlexBridgeConstants.Class]);
					sb.AppendLine();
				}
			}
			verifyTimer.Stop();
		}

		//private static void WriteProblemDataFile(string pathname, XmlNode data)
		//{
		//    var doc = data.OwnerDocument;
		//    doc.Save(pathname);
		//}
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
			var unownedObjects = new Dictionary<string, SortedDictionary<string, byte[]>>(200);
			// Outer dictionary has the class name for its key and a sorted (by guid) dictionary as its value.
			// The inner dictionary has a caseless guid as the key and the byte array as the value.
			var classData = new Dictionary<string, SortedDictionary<string, byte[]>>(200, StringComparer.OrdinalIgnoreCase);
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
					var element = TriboroughBridgeUtilities.CreateFromBytes(unownedElement);
					classElement.Add(element);
					CmObjectNestingService.NestObject(false, element,
												  classData,
												  guidToClassMapping);
				}
				root.Add(classElement);
			}
			FileWriterService.WriteNestedFile(srcFwdataPathname + ".nested", root);
		}

		private static void TokenizeFile(MetadataCache mdc, string srcFwdataPathname, Dictionary<string, SortedDictionary<string, byte[]>> unownedObjects, Dictionary<string, SortedDictionary<string, byte[]>> classData, Dictionary<string, string> guidToClassMapping)
		{
			using (var fastSplitter = new FastXmlElementSplitter(srcFwdataPathname))
			{
				bool foundOptionalFirstElement;
				// NB: The main input file *does* have to deal with the optional first element.
				foreach (var record in fastSplitter.GetSecondLevelElementBytes(FlexBridgeConstants.AdditionalFieldsTag, FlexBridgeConstants.RtTag, out foundOptionalFirstElement))
					{
					if (foundOptionalFirstElement)
						{
							// Cache custom prop file for later write.
							var cpElement = DataSortingService.SortCustomPropertiesRecord(LibTriboroughBridgeSharedConstants.Utf8.GetString(record));
							// Add custom property info to MDC, since it may need to be sorted in the data files.
							foreach (var propElement in cpElement.Elements(FlexBridgeConstants.CustomField))
							{
								var className = propElement.Attribute(FlexBridgeConstants.Class).Value;
								var propName = propElement.Attribute(FlexBridgeConstants.Name).Value;
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
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
		}

		private void RestoreProjects(object sender, EventArgs e)
		{
			RestoreAllProjects();
		}

		private void RestoreAllProjects()
		{
			Cursor = Cursors.WaitCursor;
			try
			{
				// Wipe out contents of all test folders in regular FW project location,
				// EXCEPT the real ZPI project.
				// If there is no copy of the fwdata file in the main project folder, then skip it.
				var allProjectDirNamesExceptMine =
					Directory.GetDirectories(CurrentBaseFolder)
							 .Where(projectDirName => Path.GetFileNameWithoutExtension(projectDirName).ToLowerInvariant() != "zpi");
				foreach (var projectDirName in allProjectDirNamesExceptMine)
				{
					RestoreProjectIfNeeded(Directory.GetFiles(projectDirName, "*" +
						LibTriboroughBridgeSharedConstants.FwXmlExtension).FirstOrDefault());
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
			if (currentFilename.ToLowerInvariant() == "zpi" + LibTriboroughBridgeSharedConstants.FwXmlExtension ||
				projectDirName.ToLowerInvariant() == "zpi")
			{
				return; // Don't even think of wiping out my ZPI folder.
			}

			var backupDataFilesFullPathnames = Directory.GetFiles(CurrentBaseFolder, "*" + LibTriboroughBridgeSharedConstants.FwXmlExtension,
				SearchOption.TopDirectoryOnly);
			var backupDataFilenames = backupDataFilesFullPathnames.Select(Path.GetFileName).ToList();
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
			File.Copy(Path.Combine(CurrentBaseFolder, currentFilename), Path.Combine(projectDirName, currentFilename));
		}

		private void ClearCheckboxes(object sender, EventArgs e)
		{
			_cbNestFile.CheckState = CheckState.Unchecked;
			_cbRoundTripData.CheckState = CheckState.Unchecked;
			_cbVerify.CheckState = CheckState.Unchecked;
			_cbCheckOwnObjsur.CheckState = CheckState.Unchecked;
			_cbValidate.CheckState = CheckState.Unchecked;
			_rebuildDataFile.CheckState = CheckState.Unchecked;
			_cbCheckAmbiguousElements.CheckState = CheckState.Unchecked;
			_cbFindDanglingRefs.CheckState = CheckState.Unchecked;
		}

		private void RunLoopClicked(object sender, EventArgs e)
		{
			var sb = new StringBuilder();
			const string data = "<element />";

			var bytesTimer = new Stopwatch();
			var ourBytes = Encoding.UTF8.GetBytes(data);
			var theirBytes = Encoding.UTF8.GetBytes(data);
			bytesTimer.Start();
			for (var i = 0; i < 100000; ++i)
			{
				XmlUtilities.AreXmlElementsEqual(ourBytes, theirBytes);
			}
			bytesTimer.Stop();
			sb.AppendFormat("Time to check (as bytes): {0}ms; {1}ticks.", bytesTimer.ElapsedMilliseconds, bytesTimer.ElapsedTicks);
			sb.AppendLine();

			var stringTimer = new Stopwatch();
			stringTimer.Start();
			for (var i = 0; i < 100000; ++i)
			{
				XmlUtilities.AreXmlElementsEqual(data, data);
			}
			stringTimer.Stop();
			sb.AppendFormat("Time to check (as string): {0}ms; {1}ticks.", stringTimer.ElapsedMilliseconds, stringTimer.ElapsedTicks);
			sb.AppendLine();

			var doc = new XmlDocument();
			var ourNode = XmlUtilities.GetDocumentNodeFromRawXml(data, doc);
			var theirNode = XmlUtilities.GetDocumentNodeFromRawXml(data, doc);
			var xmlNodeTimer = new Stopwatch();
			xmlNodeTimer.Start();
			for (var i = 0; i < 100000; ++i)
			{
				XmlUtilities.AreXmlElementsEqual(ourNode, theirNode);
			}
			xmlNodeTimer.Stop();
			sb.AppendFormat("Time to check (as XmlNode): {0}ms; {1}ticks.", xmlNodeTimer.ElapsedMilliseconds, xmlNodeTimer.ElapsedTicks);
			sb.AppendLine();

			var ourInput = new XmlInput(data);
			var theirInput = new XmlInput(data);
			var xmlInputTimer = new Stopwatch();
			xmlInputTimer.Start();
			for (var i = 0; i < 100000; ++i)
			{
				XmlUtilities.AreXmlElementsEqual(ourInput, theirInput);
			}
			xmlInputTimer.Stop();
			sb.AppendFormat("Time to check (as XmlInput): {0}ms; {1}ticks.", xmlInputTimer.ElapsedMilliseconds, xmlInputTimer.ElapsedTicks);

			MessageBox.Show(sb.ToString());
		}

		private void ToggleCheckBoxes(object sender, EventArgs e)
		{
			foreach (ListViewItem item in _listView.Items)
			{
				item.Checked = !item.Checked;
			}
		}
	}
}
