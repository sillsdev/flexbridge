using System.IO;
using System.Windows.Forms;
using Chorus;
using Chorus.UI.Sync;
using FieldWorksBridge.Model;

namespace FieldWorksBridge.View
{
	internal class SynchronizeProject : ISynchronizeProject
	{
		#region Implementation of ISynchronizeProject

		public void SynchronizeFieldWorksProject(Form parent, ChorusSystem chorusSystem, LanguageProject langProject)
		{
			// Add the 'lock' file to keep FW apps from starting up at such an inopportune moment.
			var lockPathname = Path.Combine(langProject.DirectoryName, langProject.Name + ".lock");
			try
			{
				File.WriteAllText(lockPathname, "");

				using (var syncDlg = (SyncDialog)chorusSystem.WinForms.CreateSynchronizationDialog())
				{
					syncDlg.SyncOptions.DoSendToOthers = true;
					syncDlg.SyncOptions.DoPullFromOthers = true;
					syncDlg.SyncOptions.DoMergeWithOthers = true;
					syncDlg.ShowDialog(parent);
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