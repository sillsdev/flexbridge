using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using LiftIO.Migration;
using LiftIO.Parsing;
using LiftIO.Validation;
using SIL.FieldWorks.Common.Controls;
using SIL.FieldWorks.Common.FwUtils;
using SIL.FieldWorks.Common.FXT;
using SIL.FieldWorks.FDO;
using SIL.FieldWorks.FDO.Infrastructure;
using SIL.FieldWorks.FwCoreDlgs;
using SIL.FieldWorks.LexText.Controls;
using SIL.LiftBridge;
using SIL.LiftBridge.Properties;
using SIL.Utils;

namespace LiftBridgeUtility7
{
	/// <summary>
	/// Class that allows FieldWorks and WeSay users to collaborate using LIFT data.
	///
	/// The assumption is that a FieldWorks user has a Mercurial repository with the WeSay material in it,
	/// which is shared by FieldWorks and WeSay users. This utility sees that FieldWorks LIFT data is exported,
	/// committed into the Mercurial repository, and then Chorus is used to move the data to/from other users.
	///
	/// When the system senses that new data has come in from other users,
	/// the LIFT data is merged back into the FieldWorks data set. If any entries have been deleted
	/// by other users, those entries are then deleted from the FieldWorks system.
	///
	/// See: http://projects.palaso.org/projects/show/liftbridge
	/// </summary>
	/// <remarks>
	/// NB: All of the FieldWorks dlls comes from the FW 7 build.
	/// </remarks>
	public sealed class LiftBridgeUtility70 : IUtility, ILiftBridgeImportExport
	{
		private UtilityDlg _utilityDlg;
		private FdoCache _cache;
		private IProgress _progressDlg;

		#region Implementation of IUtility

		/// <summary>
		/// Load any items in list box.
		/// </summary>
		public void LoadUtilities()
		{
			_utilityDlg.Utilities.Items.Add(this);
		}

		/// <summary>
		/// Notify the utility it has been selected in the dlg.
		/// </summary>
		public void OnSelection()
		{
			_utilityDlg.WhenDescription = Resources.kWhenDescription;
			_utilityDlg.WhatDescription = Resources.kWhatDescription;
			_utilityDlg.RedoDescription = Resources.kRedoDescription;
		}

		/// <summary>
		/// Have the utility do what it does.
		/// </summary>
		public void Process()
		{
			// 1. Export Flex Lexical data as LIFT, where the exported file goes into the folder where the Hg Repository is located.
			// 2. Do commit, push, and pull using Chorus' SyncDialog.
			// 3. Re-import current LIFT data, but only if it actually brought in changes from other users.
			//		If nothing was pulled, then skip the bother of re-importing the LIFT file.
			// The Import/Export operations are done via events, which, in turn,
			// use the ILiftBridgeImportExport methods and properties.
			using (var dlg = new LiftBridgeDlg(this, _cache.DatabaseName))
			{
				dlg.ShowDialog(_utilityDlg);
			}
		}

		/// <summary>
		/// Get the main label describing the utility.
		/// </summary>
		public string Label
		{
			get { return Resources.kLabel; }
		}

		/// <summary>
		/// Set the UtilityDlg.
		/// </summary>
		public UtilityDlg Dialog
		{
			set
			{
				_utilityDlg = value;
				_cache = (FdoCache)_utilityDlg.Mediator.PropertyTable.GetValue("cache");
			}
		}

		#endregion

		/// <summary>
		/// Override method to return the Label property.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return Label;
		}

		void OnDumperSetProgressMessage(object sender, XDumper.MessageArgs e)
		{
			if (_progressDlg == null)
				return;
			var message = Resources.ResourceManager.GetString(e.MessageId, Resources.Culture);
			if (!string.IsNullOrEmpty(message))
				_progressDlg.Message = message;
			_progressDlg.Minimum = 0;
			_progressDlg.Maximum = e.Max;
		}

		void OnDumperUpdateProgress(object sender)
		{
			if (_progressDlg == null)
				return;

			var nMax = _progressDlg.Maximum;
			if (_progressDlg.Position >= nMax)
				_progressDlg.Position = 0;
			_progressDlg.Step(1);
			if (_progressDlg.Position > nMax)
				_progressDlg.Position = _progressDlg.Position % nMax;
		}

		/// <summary>
		/// Export the contents of the lexicon to the given file (first and only parameter).
		/// </summary>
		/// <returns>the name of the exported LIFT file if successful, or null if an error occurs.</returns>
		private object ExportLexicon(IProgress progressDialog, params object[] parameters)
		{
			try
			{
				var outPath = (string)parameters[0];
				progressDialog.Message = String.Format(Resources.ksExportingEntries,
					_cache.LangProject.LexDbOA.EntriesOC.Count);
				using (var dumper = new XDumper(_cache))
				{
					dumper.UpdateProgress += OnDumperUpdateProgress;
					dumper.SetProgressMessage += OnDumperSetProgressMessage;
					// Don't bother writing out the range information in the export.
					dumper.SetTestVariable("SkipRanges", true);
					dumper.SkipAuxFileOutput = true;
					progressDialog.Minimum = 0;
					progressDialog.Maximum = dumper.GetProgressMaximum();
					progressDialog.Position = 0;
					var basePath = Path.Combine(DirectoryFinder.FWCodeDirectory, @"Language Explorer\Export Templates");
					var fxtPath = Path.Combine(basePath, "LIFT.fxt.xml");
					using (TextWriter w = new StreamWriter(outPath))
					{
						dumper.ExportPicturesAndMedia = true;	// useless without Pictures directory...
						dumper.Go(_cache.LangProject, fxtPath, w);
					}
					return outPath;
				}
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// Import the LIFT file into FieldWorks.
		/// </summary>
		/// <returns>the name of the exported LIFT file if successful, or null if an error occurs.</returns>
		private object ImportLexicon(IProgress progressDialog, params object[] parameters)
		{
			if (_progressDlg == null)
				_progressDlg = progressDialog;
			progressDialog.Minimum = 0;
			progressDialog.Maximum = 100;
			progressDialog.Position = 0;
			var liftPathname = parameters[0].ToString();
			string sLogFile = null;

			NonUndoableUnitOfWorkHelper.Do(_cache.ActionHandlerAccessor, () =>
			{
				try
				{
					string sFilename;
					var fMigrationNeeded = Migrator.IsMigrationNeeded(liftPathname);
					if (fMigrationNeeded)
					{
						var sOldVersion = Validator.GetLiftVersion(liftPathname);
						progressDialog.Message = String.Format(Resources.kLiftVersionMigration,
							sOldVersion, Validator.LiftVersion);
						sFilename = Migrator.MigrateToLatestVersion(liftPathname);
					}
					else
					{
						sFilename = liftPathname;
					}
					progressDialog.Message = Resources.kLoadingListInfo;
					var flexImporter = new FlexLiftMerger(_cache, FlexLiftMerger.MergeStyle.msKeepOnlyNew, true);
					var parser = new LiftParser<LiftObject, LiftEntry, LiftSense, LiftExample>(flexImporter);
					parser.SetTotalNumberSteps += ParserSetTotalNumberSteps;
					parser.SetStepsCompleted += ParserSetStepsCompleted;
					parser.SetProgressMessage += ParserSetProgressMessage;
					flexImporter.LiftFile = liftPathname;

					var cEntries = parser.ReadLiftFile(sFilename);

					if (fMigrationNeeded)
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
					sLogFile = flexImporter.DisplayNewListItems(liftPathname, cEntries);
				}
				catch (Exception error)
				{
					var sMsg = String.Format(Resources.kProblemImportWhileMerging,
						liftPathname);
					try
					{
						var bldr = new StringBuilder();
						bldr.AppendFormat(Resources.kProblem,
							liftPathname);
						bldr.AppendLine();
						bldr.AppendLine(error.Message);
						bldr.AppendLine();
						bldr.AppendLine(error.StackTrace);
						if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
							ClipboardUtils.SetDataObject(bldr.ToString(), true);
					}
					catch
					{
					}
					MessageBox.Show(sMsg, Resources.kProblemMerging,
						MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
			});
			return sLogFile;
		}

		void ParserSetTotalNumberSteps(object sender, LiftParser<LiftObject, LiftEntry, LiftSense, LiftExample>.StepsArgs e)
		{
			_progressDlg.Maximum = e.Steps;
			_progressDlg.Position = 0;
		}

		void ParserSetProgressMessage(object sender, LiftParser<LiftObject, LiftEntry, LiftSense, LiftExample>.MessageArgs e)
		{
			_progressDlg.Position = 0;
			_progressDlg.Message = e.Message;
		}

		void ParserSetStepsCompleted(object sender, LiftParser<LiftObject, LiftEntry, LiftSense, LiftExample>.ProgressEventArgs e)
		{
			var nMax = _progressDlg.Maximum;
			_progressDlg.Position = e.Progress > nMax ? e.Progress % nMax : e.Progress;
		}

		#region Implementation of ILiftBridgeImportExport

		/// <summary>
		/// Export the FieldWorks lexicon into the LIFT file.
		/// The file may, or may not, exist.
		/// </summary>
		/// <returns>True, if successful, otherwise false.</returns>
		public bool ExportLexicon(Form parentForm)
		{
			using (new WaitCursor(parentForm))
			{
				using (var progressDlg = new ProgressDialogWithTask(parentForm))
				{
					_progressDlg = progressDlg;
					progressDlg.ProgressBarStyle = ProgressBarStyle.Continuous;
					try
					{
						progressDlg.Title = Resources.kExportLiftLexicon;

						var outPath = Path.GetTempFileName();
						outPath = (string)progressDlg.RunTask(true, ExportLexicon, outPath);
						if (outPath == null)
							return false;

						// Copy temp file to real LIFT file.
						File.Copy(outPath, LiftPathname, true);
						File.Delete(outPath); // Delete temp file.
						return true;
					}
					catch
					{
						return false;
					}
					finally
					{
						_progressDlg = null;
					}
				}
			}
		}

		/// <summary>
		/// Import the LIFT file into FieldWorks.
		/// </summary>
		/// <returns>True, if successful, otherwise false.</returns>
		public bool ImportLexicon(Form parentForm)
		{
			using (new WaitCursor(parentForm))
			{
				using (var progressDlg = new ProgressDialogWithTask(parentForm))
				{
					_progressDlg = progressDlg;
					progressDlg.ProgressBarStyle = ProgressBarStyle.Continuous;
					try
					{
						progressDlg.Title = Resources.kImportLiftlexicon;

						var logFile = (string)progressDlg.RunTask(true, ImportLexicon, LiftPathname);
						return logFile != null;
					}
					catch
					{
						return false;
					}
					finally
					{
						_progressDlg = null;
					}
				}
			}
		}

		/// <summary>
		/// Gets or sets the LIFT file's pathname.
		/// </summary>
		public string LiftPathname { get; set; }

		#endregion
	}
}
