using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Contexts;
using Palaso.Progress;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.Progress;
using Palaso.UI.WindowsForms.Progress.Commands;

namespace FLEx_ChorusPlugin.Infrastructure.DomainServices
{
	/// <summary>
	/// Encapsulates, as a state machine, the joining of split fieldworks project files.
	/// The task is quantized to make use of a palaso progress bar.
	/// Most of the subtasks need to share two dictionaries that are protected
	/// from "public static" access in this class. (ie., main reason to encapsulate)
	/// The subtasks vary greatly in granulatity with most of the time
	/// spent in writing and caching xml "properties" and writing out files.
	/// Two static methods from MultipleFileServices were moved here since
	/// they are only used in this task.
	///
	/// Randy's summary:
	///  2. Put the multiple files back together into the main fwdata file,
	///		but only if a Send/Receive had new information brought back into the local repo.
	///		NB: The client of the service decides if new information was found, and decides to call the service, or not.
	/// </summary>
	internal class FLExProjectUnifier
	{
		private enum JoinTasks
		{
			SetupUnifiedFile,
			UpdateVersion,
			WriteOptionalProperties,
			RestoreDomainData,
			CopyTempToFwdata
		};

		private JoinTasks _nextJoinTask = JoinTasks.SetupUnifiedFile;

		private readonly MetadataCache _mdc = MetadataCache.MdCache;
		private readonly string _mainFilePathname;
		private readonly string _pathRoot;
		private static string _tempPathname;
		private static UnifyFwdataCommand _unifyFwdataCommand;
		private XmlWriter _writer;

		public FLExProjectUnifier(string mainFilePathname)
		{
			FileWriterService.CheckPathname(mainFilePathname);
			_mainFilePathname = mainFilePathname;
			_pathRoot = Path.GetDirectoryName(_mainFilePathname);
			_tempPathname = Path.GetTempFileName();
		}

		public int Steps
		{
			get
			{
				return Enum.GetValues(typeof(JoinTasks)).GetLength(0);
			}
		}

		// ONLY use in tests where you don't want the progress bar to show.
		internal void PutHumptyTogetherAgain()
		{
			try
			{
				for (var subtask = 0; subtask < Steps; subtask++)
				{
					PutHumptyTogetherAgainWatching();
				}
			}
			finally
			{
				CloseTempFile();
			}
		}

		/// <summary>
		/// A state machine needed to quantize the process of unifying the split *.fwdata
		/// files so a progress bar can track it.
		/// </summary>
		internal void PutHumptyTogetherAgainWatching()
		{
			switch (_nextJoinTask)
			{
				case JoinTasks.SetupUnifiedFile:
					SetupUnifiedFile();
					_nextJoinTask = JoinTasks.UpdateVersion;
					break;
				case JoinTasks.UpdateVersion:
					UpgradeToVersion();
					_nextJoinTask = JoinTasks.WriteOptionalProperties;
					break;
				case JoinTasks.WriteOptionalProperties:
					WriteOptionalProperties();
					_nextJoinTask = JoinTasks.RestoreDomainData;
					break;
				case JoinTasks.RestoreDomainData:
					RestoreDomainData();
					_nextJoinTask = JoinTasks.CopyTempToFwdata;
					break;
				case JoinTasks.CopyTempToFwdata:
					CopyTempToFwdata();
					_nextJoinTask = JoinTasks.SetupUnifiedFile;
					break;
				default:
					CloseTempFile();
					var badState = _nextJoinTask;
					_nextJoinTask = JoinTasks.SetupUnifiedFile;
					throw new InvalidOperationException("Invalid state [" + badState + "] while assembling Send/Receive project file.");
			}
		}

		internal void SetupUnifiedFile()
		{
			// There is no particular reason to ensure the order of objects in 'mainFilePathname' is retained,
			// but the optional custom props element must be first.
			// NB: This should follow current FW write settings practice.
			var fwWriterSettings = new XmlWriterSettings
			{
				OmitXmlDeclaration = false,
				CheckCharacters = true,
				ConformanceLevel = ConformanceLevel.Document,
				Encoding = new UTF8Encoding(false),
				Indent = true,
				IndentChars = (""),
				NewLineOnAttributes = false
			};
			// only throws ArgumentNullException, which is prevented by a check when this class is constructed
			_writer = XmlWriter.Create(_tempPathname, fwWriterSettings);
		}

		internal void UpgradeToVersion()
		{
			_writer.WriteStartElement("languageproject");

			// Write out version number from the ModelVersion file.
			var modelVersionData = File.ReadAllText(Path.Combine(_pathRoot, SharedConstants.ModelVersionFilename));
			var splitModelVersionData = modelVersionData.Split(new[] {"{", ":", "}"}, StringSplitOptions.RemoveEmptyEntries);
			var version = splitModelVersionData[1].Trim();
			_writer.WriteAttributeString("version", version);

			var mdc = MetadataCache.MdCache; // This may really need to be a reset
			mdc.UpgradeToVersion(Int32.Parse(version));
		}

		internal void WriteOptionalProperties()
		{
			// Write out optional custom property data to the fwdata file.
			// The foo.CustomProperties file will exist, even if it has nothing in it, but the "AdditionalFields" root element.
			var optionalCustomPropFile = Path.Combine(_pathRoot, SharedConstants.CustomPropertiesFilename);
			// Remove 'key' attribute from CustomField elements, before writing to main file.
			var doc = XDocument.Load(optionalCustomPropFile);
			var customFieldElements = doc.Root.Elements("CustomField").ToList();
			if (!customFieldElements.Any())
				return;

			foreach (var cf in customFieldElements)
			{
				cf.Attribute("key").Remove();
				// Restore type attr for object values.
				var propType = cf.Attribute("type").Value;
				cf.Attribute("type").Value = RestoreAdjustedTypeValue(propType);

				_mdc.GetClassInfo(cf.Attribute(SharedConstants.Class).Value).AddProperty(
					new FdoPropertyInfo(cf.Attribute(SharedConstants.Name).Value, propType, true));
			}
			_mdc.ResetCaches();
			FileWriterService.WriteElement(_writer, SharedConstants.Utf8.GetBytes(doc.Root.ToString()));
		}

		internal void RestoreDomainData()
		{
				BaseDomainServices.RestoreDomainData(_writer, _pathRoot);
				_writer.WriteEndElement();
		}

		internal void CopyTempToFwdata()
		{
			_writer.Close();
			File.Copy(_tempPathname, _mainFilePathname, true);
		}

		//finally -- must execute even if some other part of process fails
		internal static void CloseTempFile()
		{
			if (File.Exists(_tempPathname))
				File.Delete(_tempPathname);
		}

		private static string RestoreAdjustedTypeValue(string storedType)
		{
			string adjustedType;
			switch (storedType)
			{
				default:
					adjustedType = storedType;
					break;

				case "OwningCollection":
					adjustedType = "OC";
					break;
				case "ReferenceCollection":
					adjustedType = "RC";
					break;

				case "OwningSequence":
					adjustedType = "OS";
					break;
				case "ReferenceSequence":
					adjustedType = "RS";
					break;

				case "OwningAtomic":
					adjustedType = "OA";
					break;
				case "ReferenceAtomic":
					adjustedType = "RA";
					break;
			}
			return adjustedType;
		}

		// method that is the send/receive dialog "shown" delegate which constructs the progress bar dialog
		// that wraps "humpty dumpty" so we can monitor him as he comes apart and make the user feel good about it.
		public static void UnifyFwdataProgress(Form parentForm, string origPathname)
		{
			_unifyFwdataCommand = new UnifyFwdataCommand(origPathname);
			var progressHandler = new ProgressDialogHandler(parentForm, _unifyFwdataCommand, "Restore project file");
			progressHandler.Finished += (sender, args) => parentForm.Close();
			var progress = new ProgressDialogProgressState(progressHandler);
			_unifyFwdataCommand.BeginInvoke(progress);
			while (progress.State != ProgressState.StateValue.Finished)
			{
				Application.DoEvents();
			}
		}

		public class UnifyFwdataCommand : BasicCommand
		{
			public bool WasCancelled;
			private readonly string _pathName;
			private readonly FLExProjectUnifier _fpu;
			private readonly int _steps;

			public UnifyFwdataCommand(string pathName)
			{
				_pathName = pathName;
				_fpu = new FLExProjectUnifier(_pathName);
				_steps = _fpu.Steps;
			}

			protected override void DoWork(InitializeProgressCallback initializeCallback, ProgressCallback progressCallback,
										   StatusCallback primaryStatusTextCallback,
										   StatusCallback secondaryStatusTextCallback)
			{
				try
				{

					var countForWork = 0;
					while (countForWork < _steps)
					{
						if (Canceling)
						{
							WasCancelled = true;
							return;
						}
						_fpu.PutHumptyTogetherAgainWatching();
						countForWork++;
					}
				}
				finally
				{
					CloseTempFile();
				}
			}

			protected override void DoWork2(ProgressState progress)
			{
				try
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
						_fpu.PutHumptyTogetherAgainWatching();
						countForWork++;
						progress.NumberOfStepsCompleted = countForWork;
					}
				}
				finally
				{
					CloseTempFile();
					progress.State = ProgressState.StateValue.Finished;
				}
			}
		}
	}
}
