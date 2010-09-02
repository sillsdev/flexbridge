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
	public partial class LiftBridgeDlg : Form
	{
		private readonly FdoCache _cache;
		private IAdvInd4 _progressDlg;
		private readonly string _currentRootDataPath;
		private  string _liftPathname;

		public LiftBridgeDlg()
		{
			InitializeComponent();
		}

		internal LiftBridgeDlg(FdoCache cache)
			: this()
		{
			_cache = cache;
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
		}

		private void ReviewClick(object sender, EventArgs e)
		{

		}

		private void SetUpClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{

		}

		private void SendReceiveClicked(object sender, EventArgs e)
		{
			using (new WaitCursor(this))
			{
				using (var progressDlg = new ProgressDialogWithTask(this))
				{
					progressDlg.ProgressBarStyle = ProgressBarStyle.Continuous;
					_progressDlg = progressDlg;
					try
					{
						progressDlg.Title = Resources.kLiftBridgeProcessing;
						// 1. Export FLex lexicon
						var outPath = Path.GetTempFileName();
						outPath = (string)progressDlg.RunTask(true, ExportLexicon, outPath);
						if (outPath == null)
						{
							// TODO: some sort of error report?
							return;
						}

						// 2. Commit/Push/Pull/Merge via Chorus.
						// Use/Create a LIFT file at known place on the computer.
						// JH says (31 Aug 2010): "I’m not saying we use some FW directory.
						// 	I’m just saying that, given a unique name (like the language project),
						// 	we can map to somewhere on your disk, one we define.
						// Yes, LinkBridge would be the one making that .hg folder, invisibly."
						// Since the Flex data was successfully exported,
						// now copy it to the Hg folder.
						_liftPathname = Path.Combine(_currentRootDataPath, Path.ChangeExtension(outPath, "lift"));
						File.Copy(outPath, _liftPathname, true);
						if ((bool)progressDlg.RunTask(true, ChorusMerge, null))
						{
							// 3. Re-import lexicon, overwriting current contents.
							// But, only if Chorus reports we got some changes from afar.
							var logFile = (string)progressDlg.RunTask(true, ImportLexicon, null);
							if (logFile == null)
							{
								// TODO: some sort of error report?
								return;
							}
						}
					}
					catch
					{
					}
				}
			}
			DialogResult = DialogResult.OK;
			Close();
		}

		/// <summary>
		/// Export the contents of the lexicon to the given file (first and only parameter).
		/// </summary>
		/// <returns>
		/// The name of the exported LIFT file if successful, or null if an error occurs.
		/// </returns>
		protected object ExportLexicon(IAdvInd4 progressDialog, params object[] parameters)
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
		protected object ChorusMerge(IAdvInd4 progressDialog, params object[] parameters)
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
		protected object ImportLexicon(IAdvInd4 progressDialog, params object[] parameters)
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
			//Debug.WriteLine(String.Format("OnDumperSetProgressMessage(\"{0}\")", e.MessageId));
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
