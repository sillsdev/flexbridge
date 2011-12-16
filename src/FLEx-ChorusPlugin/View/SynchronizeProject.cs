using System.IO;
using System.Windows.Forms;
using Chorus;
using Chorus.UI.Sync;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Model;

namespace FLEx_ChorusPlugin.View
{
	internal sealed class SynchronizeProject : ISynchronizeProject
	{
		#region Implementation of ISynchronizeProject

		void ISynchronizeProject.SynchronizeFieldWorksProject(Form parent, ChorusSystem chorusSystem, LanguageProject langProject)
		{
			// Add the 'lock' file to keep FW apps from starting up at such an inopportune moment.
			var lockPathname = Path.Combine(langProject.DirectoryName, langProject.Name + ".fwdata.lock");
			File.WriteAllText(lockPathname, "");

			var origPathname = Path.Combine(langProject.DirectoryName, langProject.Name + ".fwdata");
			// Break up into smaller files.
			MultipleFileServices.BreakupMainFile(origPathname, langProject.Name);

			// Do the Chorus business.
			try
			{
				using (var syncDlg = (SyncDialog)chorusSystem.WinForms.CreateSynchronizationDialog())
				{
					syncDlg.SyncOptions.DoSendToOthers = true;
					syncDlg.SyncOptions.DoPullFromOthers = true;
					syncDlg.SyncOptions.DoMergeWithOthers = true;
					syncDlg.ShowDialog(parent);

					if (syncDlg.SyncResult.DidGetChangesFromOthers)
					{
						// Put Humpty together again.
						MultipleFileServices.RestoreMainFile(origPathname, langProject.Name);
					}
				}
			}
			finally
			{
				if (File.Exists(lockPathname))
					File.Delete(lockPathname);
			}
		}

		#endregion
	}
}