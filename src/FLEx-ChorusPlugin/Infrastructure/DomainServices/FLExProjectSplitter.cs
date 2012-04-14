﻿﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Contexts;
using Palaso.Progress;
using Palaso.UI.WindowsForms.Progress;
using Palaso.UI.WindowsForms.Progress.Commands;
using Palaso.Xml;

namespace FLEx_ChorusPlugin.Infrastructure.DomainServices
{
	/// <summary>
	/// Encapsulates the splitting of a fieldworks project file as a state machine.
	/// The task is quantized to make use of a palaso progress bar.
	/// Most of the subtasks need to share two dictionaries that are protected
	/// from "public static" access in this class. (ie., main reason to encapsulate)
	/// The subtasks vary greatly in granulatity with most of the time
	/// spent in writing and caching xml "properties" and writing out files.
	/// Two static methods from MultipleFileServices were moved here since
	/// they are only used in this task.
	///
	/// Randy's summary:
	///  1. Break up the main fwdata file into multiple files
	///		A. One file for the custom property declarations (even if there are no custom properties), and
	///		B. One file for the model version
	///		C. Various files for the CmObject data.
	/// </summary>
	class FLExProjectSplitter
	{
		private enum SplitTasks
		{
			DeleteOldFiles,
			UpdateVersion,
			SortClassXml,
			WriteOrCacheXmlProps,
			WriteLinguisticsFile,
			WriteAnthopologyFile,
			WriteScriptureFile,
			WriteGeneralFile
		};

		private SplitTasks _nextSplitTask = SplitTasks.DeleteOldFiles;

		private readonly string _mainFilePathname;
		private readonly MetadataCache _mdc = MetadataCache.MdCache;
		// Outer Dict has the class name for its key and a sorted (by guid) dictionary as its value.
		// The inner dictionary has a caseless guid as the key and the byte array as the value.
		// (Only has current concrete classes.)
		private Dictionary<string, SortedDictionary<string, XElement>> _classData;
		private Dictionary<string, string> _guidToClassMapping;

		public FLExProjectSplitter(string mainFilePathname)
		{
			FileWriterService.CheckFilename(mainFilePathname);
			_mainFilePathname = mainFilePathname;
		}

		public int Steps
		{
			get
			{
				return Enum.GetValues(typeof(SplitTasks)).GetLength(0);
			}
		}

		internal void PushHumptyOffTheWall()
		{
			for (var subtask = 0; subtask < Steps; subtask++)
			{
				PushHumptyOffTheWallWatching();
			}
		}

		/// <summary>
		/// A state machine needed to quantize the process of tearing the *.fwdata
		/// file apart so a progress bar can track it.
		/// </summary>
		internal void PushHumptyOffTheWallWatching()
		{
			switch (_nextSplitTask)
			{
				case SplitTasks.DeleteOldFiles:
					DeleteOldFiles(Path.GetDirectoryName(_mainFilePathname));
					_nextSplitTask = SplitTasks.UpdateVersion;
					break;
				case SplitTasks.UpdateVersion:
					WriteVersionFile(_mainFilePathname);
					_nextSplitTask = SplitTasks.SortClassXml;
					break;
				case SplitTasks.SortClassXml:
					_classData = GenerateBasicClassData(_mdc);
					_nextSplitTask = SplitTasks.WriteOrCacheXmlProps;
					break;
				case SplitTasks.WriteOrCacheXmlProps:
					WriteOrCacheProperties(_mainFilePathname);
					_nextSplitTask = SplitTasks.WriteLinguisticsFile;
					break;
				case SplitTasks.WriteLinguisticsFile:
					BaseDomainServices.WriteLinguisticsData(Path.GetDirectoryName(_mainFilePathname), _classData, _guidToClassMapping);
					_nextSplitTask = SplitTasks.WriteAnthopologyFile;
					break;
				case SplitTasks.WriteAnthopologyFile:
					BaseDomainServices.WriteAnthropologyData(Path.GetDirectoryName(_mainFilePathname), _classData, _guidToClassMapping);
					_nextSplitTask = SplitTasks.WriteScriptureFile;
					break;
				case SplitTasks.WriteScriptureFile:
					BaseDomainServices.WriteScriptureData(Path.GetDirectoryName(_mainFilePathname), _classData, _guidToClassMapping);
					_nextSplitTask = SplitTasks.WriteGeneralFile;
					break;
				case SplitTasks.WriteGeneralFile:
					BaseDomainServices.WriteGeneralData(Path.GetDirectoryName(_mainFilePathname), _classData, _guidToClassMapping);
					_nextSplitTask = SplitTasks.DeleteOldFiles;
					break;
				default:
					var badState = _nextSplitTask;
					_nextSplitTask = SplitTasks.DeleteOldFiles;
					throw new InvalidOperationException("Invalid state [" + badState + "] while splitting Send/Receive project file.");
			}
#if DEBUG
			// Enable ONLY for testing a round trip.
			//PutHumptyTogetherAgain(mainFilePathname, projectName);
#endif
		}

		private void DeleteOldFiles(string pathRoot)
		{
			// Wipe out custom props file, as it will be re-created, even if it only has the root element in it.
			var customPropPathname = Path.Combine(pathRoot, SharedConstants.CustomPropertiesFilename);
			if (File.Exists(customPropPathname))
				File.Delete(customPropPathname);
			// Delete ModelVersion file, but it gets rewritten soon.
			var modelVersionPathname = Path.Combine(pathRoot, SharedConstants.ModelVersionFilename);
			if (File.Exists(modelVersionPathname))
				File.Delete(modelVersionPathname);

			// Deletes stuff in old and new locations. And (for now) makes sure "DataFiles" folder exists.
			// Brutal, but effective. :-) (But, leaves all ChorusNotes files.)
			BaseDomainServices.RemoveDomainData(pathRoot);
		}

		private void WriteVersionFile(string mainFilePathname)
		{
			var pathRoot = Path.GetDirectoryName(mainFilePathname);
			// 1. Write version number file.
			using (var reader = XmlReader.Create(mainFilePathname, FileWriterService.CanonicalReaderSettings))
			{
				reader.MoveToContent();
				reader.MoveToAttribute("version");
				var version = reader.Value;
				FileWriterService.WriteVersionNumberFile(pathRoot, version);
				_mdc.UpgradeToVersion(Int32.Parse(version));
			}
		}

		private void WriteOrCacheProperties(string mainFilePathname)
		{
			var pathRoot = Path.GetDirectoryName(mainFilePathname);

			// Outer Dict has the class name for its key and a sorted (by guid) dictionary as its value.
			// The inner dictionary has a caseless guid as the key and the byte array as the value.
			// (Only has current concrete classes.)
			_guidToClassMapping = new Dictionary<string, string>();
			using (var fastSplitter = new FastXmlElementSplitter(mainFilePathname))
			{
				var haveWrittenCustomFile = false;
				bool foundOptionalFirstElement;
				// NB: The main input file *does* have to deal with the optional first element.
				foreach (var record in fastSplitter.GetSecondLevelElementBytes(SharedConstants.AdditionalFieldsTag, SharedConstants.RtTag, out foundOptionalFirstElement))
				{
					if (foundOptionalFirstElement)
					{
						// 2. Write custom properties file with custom properties.
						FileWriterService.WriteCustomPropertyFile(_mdc, pathRoot, record);
						foundOptionalFirstElement = false;
						haveWrittenCustomFile = true;
					}
					else
					{
						CacheDataRecord(_classData, _guidToClassMapping, record);
					}
				}
				if (!haveWrittenCustomFile)
				{
					// Write empty custom properties file.
					FileWriterService.WriteCustomPropertyFile(Path.Combine(pathRoot, SharedConstants.CustomPropertiesFilename), null);
				}
			}
		}

		private static Dictionary<string, SortedDictionary<string, XElement>> GenerateBasicClassData(MetadataCache mdc)
		{
			return mdc.AllConcreteClasses.ToDictionary(fdoClassInfo => fdoClassInfo.ClassName, fdoClassInfo => new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase));
		}

		private static void CacheDataRecord(
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			IDictionary<string, string> guidToClassMapping,
			byte[] record)
		{
			var rtElement = XElement.Parse(SharedConstants.Utf8.GetString(record));
			var className = rtElement.Attribute(SharedConstants.Class).Value;
			var guid = rtElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
			guidToClassMapping.Add(guid, className);

			// 1. Remove 'Checksum' from wordforms.
			if (className == "WfiWordform")
			{
				// Always set it to zero (0), and force re-parse.
				var csElement = rtElement.Element("Checksum");
				if (csElement != null)
					csElement.Attribute(SharedConstants.Val).Value = "0";
			}

			// Theory has it the FW data is sorted.
			//// 2. Sort <rt>
			//DataSortingService.SortMainElement(rtElement);

			// 3. Cache it.
			classData[className].Add(guid, rtElement);
		}

		// method that is the send/receive dialog "shown" delegate which constructs the progress bar dialog
		// that wraps "humpty dumpty" so we can monitor him as he comes apart and make the user feel good about it.

		public static void SplitFwdataDelegate(Form parentForm, string origPathname)
		{
			var splitFwdataCommand = new SplitFwdataCommand(origPathname);
			var progressHandler = new ProgressDialogHandler(parentForm, splitFwdataCommand, "Split up project file");
			var progress = new ProgressDialogProgressState(progressHandler);
			splitFwdataCommand.BeginInvoke(progress);
			while (progress.State != ProgressState.StateValue.Finished)
			{
				Application.DoEvents();
			}
		}

		public class SplitFwdataCommand : BasicCommand
		{
			public bool WasCancelled;

			private readonly string _pathName;
			private readonly FLExProjectSplitter _fps;
			private readonly int _steps;

			public SplitFwdataCommand(string pathName)
			{
				_pathName = pathName;
				_fps = new FLExProjectSplitter(_pathName);
				_steps = _fps.Steps;
			}

			protected override void DoWork(InitializeProgressCallback initializeCallback, ProgressCallback progressCallback,
										   StatusCallback primaryStatusTextCallback,
										   StatusCallback secondaryStatusTextCallback)
			{
				var countForWork = 0;
				while (countForWork < _steps)
				{
					if (Canceling)
					{
						WasCancelled = true;
						return;
					}
					_fps.PushHumptyOffTheWallWatching();
					countForWork++;
				}
			}

			protected override void DoWork2(ProgressState progress)
			{
				var countForWork = 0;
				progress.TotalNumberOfSteps = _steps;
				while (countForWork < _steps)
				{
					if (Canceling)
					{
						WasCancelled = true;
						return;
					}
					_fps.PushHumptyOffTheWallWatching();
					countForWork++;
					progress.NumberOfStepsCompleted = countForWork;
				}
				progress.State = ProgressState.StateValue.Finished;
			}
		}
	}
}
