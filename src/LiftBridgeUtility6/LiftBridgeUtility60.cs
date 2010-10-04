using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using LiftIO.Migration;
using LiftIO.Parsing;
using LiftIO.Validation;
using SIL.FieldWorks.Common.COMInterfaces;
using SIL.FieldWorks.Common.Controls;
using SIL.FieldWorks.Common.FXT;
using SIL.FieldWorks.Common.Utils;
using SIL.FieldWorks.FDO;
using SIL.FieldWorks.LexText.Controls;
using SIL.LiftBridge;
using SIL.LiftBridge.Properties;

namespace LiftBridgeUtility6
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
	/// NB: All of the FieldWorks dlls comes from the FW 6.0.4 install.
	/// </remarks>
	public class LiftBridgeUtility60 : IUtility, ILiftBridgeImportExport
	{
		private UtilityDlg _utilityDlg;
		private FdoCache _cache;
		private IAdvInd4 _progressDlg;

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
			throw new NotImplementedException();
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
			_progressDlg.Position = e.Progress > nMax ? e.Progress % nMax : e.Progress;
		}

		#region Implementation of ILiftBridgeImportExport

		/// <summary>
		/// Export the FieldWorks lexicon into the LIFT file.
		/// The file may, or may not, exist.
		/// </summary>
		public void ExportLexicon()
		{
			// TODO: Get an instance of IAdvInd4 from somewhere.
			//if (_progressDlg == null)
			//    _progressDlg = progressDialog;
			//progressDialog.Message = string.Format(
			//    Resources.ksExportingEntries,
			//    _cache.LangProject.LexDbOA.EntriesOC.Count);
			//using (var dumper = new XDumper(_cache))
			//{
			//    dumper.UpdateProgress += OnDumperUpdateProgress;
			//    dumper.SetProgressMessage += OnDumperSetProgressMessage;
			//    // Don't bother writing out the range information in the export.
			//    dumper.SetTestVariable("SkipRanges", true);
			//    dumper.SkipAuxFileOutput = true;
			//    progressDialog.SetRange(0, dumper.GetProgressMaximum());
			//    progressDialog.Position = 0;
			//    using (TextWriter textWriter = new StreamWriter(_liftPathname))
			//    {
			//        var fxtPath = Path.Combine(
			//            Path.Combine(DirectoryFinder.FWCodeDirectory, @"Language Explorer\Export Templates"),
			//            "LIFT.fxt.xml");
			//        dumper.ExportPicturesAndMedia = true; // useless without Pictures directory...
			//        dumper.Go(_cache.LangProject as CmObject, fxtPath, textWriter);
			//    }
			//}
		}

		/// <summary>
		/// Import the LIFT file into FieldWorks.
		/// </summary>
		public void ImportLexicon()
		{
			// TODO: Get an instance of IAdvInd4 from somewhere.
			//if (_progressDlg == null)
			//    _progressDlg = progressDialog;
			//progressDialog.SetRange(0, 100);
			//progressDialog.Position = 0;
			//string sLogFile = null;
			//var oldPropChg = _cache.PropChangedHandling;
			//try
			//{
			//    _cache.PropChangedHandling = PropChangedHandling.SuppressAll;
			//    string sFilename;
			//    var migrationNeeded = Migrator.IsMigrationNeeded(_liftPathname);
			//    if (migrationNeeded)
			//    {
			//        var sOldVersion = Validator.GetLiftVersion(_liftPathname);
			//        progressDialog.Message = String.Format(Resources.kLiftVersionMigration,
			//            sOldVersion, Validator.LiftVersion);
			//        sFilename = Migrator.MigrateToLatestVersion(_liftPathname);
			//    }
			//    else
			//    {
			//        sFilename = _liftPathname;
			//    }
			//    // TODO: validate input file?
			//    progressDialog.Message = Resources.kLoadingListInfo;
			//    // FlexLiftMerger.MergeStyle.msKeepOnlyNew means:
			//    // "Throw away any existing entries/senses/... that are not in the LIFT file."
			//    var flexImporter = new FlexLiftMerger(_cache, FlexLiftMerger.MergeStyle.msKeepOnlyNew, true);
			//    var parser = new LiftParser<LiftObject, LiftEntry, LiftSense, LiftExample>(flexImporter);
			//    parser.SetTotalNumberSteps += ParserSetTotalNumberSteps;
			//    parser.SetStepsCompleted += ParserSetStepsCompleted;
			//    parser.SetProgressMessage += ParserSetProgressMessage;
			//    flexImporter.LiftFile = _liftPathname;
			//    var cEntries = parser.ReadLiftFile(sFilename);

			//    if (migrationNeeded)
			//    {
			//        // Try to move the migrated file to the temp directory, even if a copy of it
			//        // already exists there.
			//        var sTempMigrated = Path.Combine(Path.GetTempPath(),
			//            Path.ChangeExtension(Path.GetFileName(sFilename), "." + Validator.LiftVersion + ".lift"));
			//        if (File.Exists(sTempMigrated))
			//            File.Delete(sTempMigrated);
			//        File.Move(sFilename, sTempMigrated);
			//    }
			//    progressDialog.Message = Resources.kFixingRelationLinks;
			//    flexImporter.ProcessPendingRelations();
			//    sLogFile = flexImporter.DisplayNewListItems(_liftPathname, cEntries);
			//}
			//catch (Exception error)
			//{
			//    var sMsg = String.Format(Resources.kProblemImportWhileMerging,
			//        _liftPathname);
			//    try
			//    {
			//        var bldr = new StringBuilder();
			//        bldr.AppendFormat(Resources.kProblem, _liftPathname);
			//        bldr.AppendLine();
			//        bldr.AppendLine(error.Message);
			//        bldr.AppendLine();
			//        bldr.AppendLine(error.StackTrace);
			//        if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
			//            Clipboard.SetDataObject(bldr.ToString(), true);
			//    }
			//    catch
			//    {
			//    }
			//    MessageBox.Show(sMsg, Resources.kProblemMerging,
			//        MessageBoxButtons.OK, MessageBoxIcon.Warning);
			//}
			//finally
			//{
			//    _cache.PropChangedHandling = oldPropChg;
			//}
		}

		/// <summary>
		/// Gets or sets the LIFT file's pathname.
		/// </summary>
		public string LiftPathname { get; set; }

		#endregion
	}
}
