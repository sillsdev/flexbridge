using System;
using System.Windows.Forms;
using Chorus;
using Chorus.UI.Sync;

namespace FieldWorksBridge.View
{
	internal class SynchronizeProject : ISynchronizeProject
	{
		#region Implementation of ISynchronizeProject

		public void SynchronizeFieldWorksProject(Form parent, ChorusSystem chorusSystem)
		{
			// Use SyncDialog to do the S/R stuff.
			// SyncUIDialogBehaviors.Lazy, SyncUIFeatures.NormalRecommended
			using (var syncDlg = (SyncDialog)chorusSystem.WinForms.CreateSynchronizationDialog())
			{
				syncDlg.SyncOptions.DoSendToOthers = true;
				syncDlg.SyncOptions.DoPullFromOthers = true;
				syncDlg.SyncOptions.DoMergeWithOthers = true;
				syncDlg.ShowDialog(parent);
			}
		}

		#endregion
	}
}