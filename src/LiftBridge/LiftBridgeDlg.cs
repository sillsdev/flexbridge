using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Chorus.sync;
using Chorus.UI.Sync;
using LiftIO.Migration;
using LiftIO.Parsing;
using LiftIO.Validation;
using SIL.FieldWorks.Common.COMInterfaces;
using SIL.FieldWorks.Common.Controls;
using SIL.FieldWorks.Common.FXT;
using SIL.FieldWorks.Common.Utils;
using SIL.FieldWorks.FDO;
using SIL.FieldWorks.LexText.Controls;
using SIL.LiftBridge.Properties;

namespace SIL.LiftBridge
{
	public sealed partial class LiftBridgeDlg : Form
	{
		private readonly FdoCache _cache;
		private IAdvInd4 _progressDlg;
		private readonly string _currentRootDataPath;
		private  string _liftPathname;

		internal LiftBridgeDlg()
		{
			InitializeComponent();
		}

		public LiftBridgeDlg(FdoCache cache)
			: this()
		{
			_cache = cache;

			Text = Text + cache.DatabaseName;
			/*
me: Hidden is fine, as well. Where do we want to hide it/them?
 hattonjohn@gmail.com: Appdata
 me: I'm pretty much indifferent as to where they go.
8:26 AM hattonjohn@gmail.com: Ok, appdata then.

AppData\LiftBridge\Foo
AppData\LiftBridge\Bar
			*/
			_currentRootDataPath = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				Path.Combine("LiftBridge", cache.DatabaseName));
			if (!Directory.Exists(_currentRootDataPath))
				Directory.CreateDirectory(_currentRootDataPath);

			var hgPath = Path.Combine(_currentRootDataPath, ".hg");
			if (Directory.Exists(hgPath))
			{
				InstallExistingSystemControl();
			}
			else
			{
				// Use StartupNew control.
				SuspendLayout();
				var ctrl = new StartupNew();
				Controls.Add(ctrl);
				ctrl.Dock = DockStyle.Fill;
				ctrl.Startup += Startup;
				ResumeLayout();
			}
		}

		private void InstallExistingSystemControl()
		{
			var ctrl = new ExistingSystem();
			Controls.Add(ctrl);
			ctrl.Dock = DockStyle.Fill;
		}

		void Startup(object sender, StartupNewEventArgs e)
		{
			if (e.MakeNewSystem)
			{
				// Create a new Hg repo for the given LP in the hard-wired location.
			}
			else
			{
				MessageBox.Show("Decide what to do next for the Extant option. That is, how do we know where we clone from?");
			}

			// Dispose the StartupNew control (disconnect this event handler) and add the main control.
			SuspendLayout();
			var oldControl = (StartupNew)Controls[0];
			Controls.Clear();
			oldControl.Startup -= Startup;
			oldControl.Dispose();
			InstallExistingSystemControl();
			ResumeLayout(true);
		}

		/// <summary>
		/// Export the contents of the lexicon to the given file (first and only parameter).
		/// </summary>
		/// <returns>
		/// The name of the exported LIFT file if successful, or null if an error occurs.
		/// </returns>
		private object ExportLexicon(IAdvInd4 progressDialog, params object[] parameters)
		{
			string outPath;
			try
			{
				if (_progressDlg == null)
					_progressDlg = progressDialog;
				outPath = (string)parameters[0];
				progressDialog.Message = string.Format(
					Resources.ksExportingEntries,
					_cache.LangProject.LexDbOA.EntriesOC.Count);
				using (var dumper = new XDumper(_cache))
				{
					dumper.UpdateProgress += OnDumperUpdateProgress;
					dumper.SetProgressMessage += OnDumperSetProgressMessage;
					// Don't bother writing out the range information in the export.
					dumper.SetTestVariable("SkipRanges", true);
					dumper.SkipAuxFileOutput = true;
					progressDialog.SetRange(0, dumper.GetProgressMaximum());
					progressDialog.Position = 0;
					using (TextWriter textWriter = new StreamWriter(outPath))
					{
						var fxtPath = Path.Combine(
							Path.Combine(DirectoryFinder.FWCodeDirectory, @"Language Explorer\Export Templates"),
							"LIFT.fxt.xml");
						dumper.ExportPicturesAndMedia = true;	// useless without Pictures directory...
						dumper.Go(_cache.LangProject as CmObject, fxtPath, textWriter);
					}
				}
			}
			catch
			{
				outPath = null;
			}
			return outPath;
		}

		/// <summary>
		/// Do the Commit/Push/Pull/Merge to the LIFT file given by the first (and only)
		/// parameter.
		/// </summary>
		/// <returns>
		/// True, Chorus reports that changes were found in other data in the pull/merge.
		/// Otherwise, False.
		/// </returns>
		private object ChorusMerge(IAdvInd4 progressDialog, params object[] parameters)
		{
			var configuration = new ProjectFolderConfiguration(_currentRootDataPath);
			// 'Borrowed' from WeSay, to not have a dependency on it.
			//exclude has precedence, but these are redundant as long as we're using the policy
			//that we explicitly include all the files we understand.  At least someday, when these
			//effect what happens in a more persistent way (e.g. be stored in the hgrc), these would protect
			//us a bit from other apps that might try to do a *.* include
			// Excludes
			configuration.ExcludePatterns.Add("**/cache");
			configuration.ExcludePatterns.Add("**/Cache");
			configuration.ExcludePatterns.Add("autoFonts.css");
			configuration.ExcludePatterns.Add("autoLayout.css");
			configuration.ExcludePatterns.Add("defaultDictionary.css");
			configuration.ExcludePatterns.Add("*.old");
			configuration.ExcludePatterns.Add("*.WeSayUserMemory");
			configuration.ExcludePatterns.Add("*.tmp");
			configuration.ExcludePatterns.Add("*.bak");
			// Includes.
			configuration.IncludePatterns.Add("audio/*.*");
			configuration.IncludePatterns.Add("pictures/*.*");
			configuration.IncludePatterns.Add("**.css"); //stylesheets
			configuration.IncludePatterns.Add("export/*.lpconfig");//lexique pro
			configuration.IncludePatterns.Add("**.lift");
			configuration.IncludePatterns.Add("**.WeSayConfig");
			configuration.IncludePatterns.Add("**.WeSayUserConfig");
			configuration.IncludePatterns.Add("**.xml");
			configuration.IncludePatterns.Add(".hgIgnore");

			using (var dlg = new SyncDialog(configuration,
										   SyncUIDialogBehaviors.Lazy,
										   SyncUIFeatures.NormalRecommended))
			{
				dlg.Text = Resources.kSendReceive;
				dlg.SyncOptions.DoMergeWithOthers = true;
				dlg.SyncOptions.DoPullFromOthers = true;
				dlg.SyncOptions.DoSendToOthers = true;
				// leave it with the default, for now... dlg.SyncOptions.RepositorySourcesToTry.Clear();
				//dlg.SyncOptions.CheckinDescription = CheckinDescriptionBuilder.GetDescription();
				dlg.ShowDialog(this);
				return (dlg.SyncResult != null && dlg.SyncResult.DidGetChangesFromOthers);
			}
		}

		/// <summary>
		/// Re-import the modified LIFT file given by the first (and only) parameter.
		/// </summary>
		/// <returns>the name of the log file for the import, or null if a major error occurs.</returns>
		private object ImportLexicon(IAdvInd4 progressDialog, params object[] parameters)
		{
			if (_progressDlg == null)
				_progressDlg = progressDialog;
			progressDialog.SetRange(0, 100);
			progressDialog.Position = 0;
			string sLogFile = null;
			var oldPropChg = _cache.PropChangedHandling;
			try
			{
				_cache.PropChangedHandling = PropChangedHandling.SuppressAll;
				string sFilename;
				var migrationNeeded = Migrator.IsMigrationNeeded(_liftPathname);
				if (migrationNeeded)
				{
					var sOldVersion = Validator.GetLiftVersion(_liftPathname);
					progressDialog.Message = String.Format(Resources.kLiftVersionMigration,
						sOldVersion, Validator.LiftVersion);
					sFilename = Migrator.MigrateToLatestVersion(_liftPathname);
				}
				else
				{
					sFilename = _liftPathname;
				}
				// TODO: validate input file?
				progressDialog.Message = Resources.kLoadingListInfo;
				// FlexLiftMerger.MergeStyle.msKeepOnlyNew means:
				// "Throw away any existing entries/senses/... that are not in the LIFT file."
				var flexImporter = new FlexLiftMerger(_cache, FlexLiftMerger.MergeStyle.msKeepOnlyNew, true);
				var parser = new LiftParser<LiftObject, LiftEntry, LiftSense, LiftExample>(flexImporter);
				parser.SetTotalNumberSteps += ParserSetTotalNumberSteps;
				parser.SetStepsCompleted += ParserSetStepsCompleted;
				parser.SetProgressMessage += ParserSetProgressMessage;
				flexImporter.LiftFile = _liftPathname;
				var cEntries = parser.ReadLiftFile(sFilename);

				if (migrationNeeded)
				{
					// Try to move the migrated file to the temp directory, even if a copy of it
					// already exists there.
					var sTempMigrated = Path.Combine(Path.GetTempPath(),
						Path.ChangeExtension(Path.GetFileName(sFilename), "." + Validator.LiftVersion + ".lift"));
					if (File.Exists(sTempMigrated))
						File.Delete(sTempMigrated);
					File.Move(sFilename, sTempMigrated);
				}
				progressDialog.Message = Resources.kFixingRelationLinks;
				flexImporter.ProcessPendingRelations();
				sLogFile = flexImporter.DisplayNewListItems(_liftPathname, cEntries);
			}
			catch (Exception error)
			{
				var sMsg = String.Format(Resources.kProblemImportWhileMerging,
					_liftPathname);
				try
				{
					var bldr = new StringBuilder();
					bldr.AppendFormat(Resources.kProblem, _liftPathname);
					bldr.AppendLine();
					bldr.AppendLine(error.Message);
					bldr.AppendLine();
					bldr.AppendLine(error.StackTrace);
					if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
						Clipboard.SetDataObject(bldr.ToString(), true);
				}
				catch
				{
				}
				MessageBox.Show(sMsg, Resources.kProblemMerging,
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			finally
			{
				_cache.PropChangedHandling = oldPropChg;
			}
			return sLogFile;
		}

		void OnDumperSetProgressMessage(object sender, XDumper.MessageArgs e)
		{
			if (_progressDlg == null)
				return;
			var message = Resources.ResourceManager.GetString(e.MessageId, Resources.Culture);
			if (!string.IsNullOrEmpty(message))
				_progressDlg.Message = message;
			_progressDlg.SetRange(0, e.Max);
		}

		void OnDumperUpdateProgress(object sender)
		{
			if (_progressDlg == null)
				return;

			int nMin, nMax;
			_progressDlg.GetRange(out nMin, out nMax);
			if (_progressDlg.Position >= nMax)
				_progressDlg.Position = 0;
			_progressDlg.Step(1);
			if (_progressDlg.Position > nMax)
				_progressDlg.Position = _progressDlg.Position % nMax;
		}

		void ParserSetTotalNumberSteps(object sender, LiftParser<LiftObject, LiftEntry, LiftSense, LiftExample>.StepsArgs e)
		{
			_progressDlg.SetRange(0, e.Steps);
			_progressDlg.Position = 0;
		}

		void ParserSetProgressMessage(object sender, LiftParser<LiftObject, LiftEntry, LiftSense, LiftExample>.MessageArgs e)
		{
			_progressDlg.Position = 0;
			_progressDlg.Message = e.Message;
		}

		void ParserSetStepsCompleted(object sender, LiftParser<LiftObject, LiftEntry, LiftSense, LiftExample>.ProgressEventArgs e)
		{
			int nMin, nMax;
			_progressDlg.GetRange(out nMin, out nMax);
			_progressDlg.Position = e.Progress > nMax ? e.Progress%nMax : e.Progress;
		}
	}
}
