using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SIL.FieldWorks.FwCoreDlgs;
using SIL.LiftBridge;
using SIL.LiftBridge.Properties;

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
	public class LiftBridgeUtility70 : IUtility, ILiftBridgeImportExport
	{
		private UtilityDlg _utilityDlg;
		//private FdoCache _cache;

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
				//_cache = (FdoCache)_utilityDlg.Mediator.PropertyTable.GetValue("cache");
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

		#region Implementation of ILiftBridgeImportExport

		/// <summary>
		/// Export the FieldWorks lexicon into the LIFT file.
		/// The file may, or may not, exist.
		/// </summary>
		public void ExportLexicon()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Import the LIFT file into FieldWorks.
		/// </summary>
		public void ImportLexicon()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets or sets the LIFT file's pathname.
		/// </summary>
		public string LiftPathname { get; set; }

		#endregion
	}
}
