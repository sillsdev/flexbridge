using System;
using System.IO;
using System.Windows.Forms;
using Chorus.UI.Clone;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress.LogBox;
using SIL.LiftBridge.Model;
using SIL.LiftBridge.Properties;

namespace SIL.LiftBridge.View
{
	/// <summary>
	/// Get a teammate's shared Lift project from the specified source.
	/// </summary>
	internal class GetSharedProject : IGetSharedProject
	{
		#region Implementation of IGetSharedProject

		/// <summary>
		/// Get a teammate's shared Lift project from the specified source.
		/// </summary>
		/// <returns>
		/// 'true' if the shared project was cloned, otherwise 'false'.
		/// </returns>
		public bool GetSharedProjectUsing(Form parent, ExtantRepoSource extantRepoSource, LiftProject project)
		{
			var currentRootDataPath = LiftProjectServices.PathToProject(project);
			// Make sure it is an empty folder.
			if (Directory.GetFiles(currentRootDataPath).Length > 0 || Directory.GetDirectories(currentRootDataPath).Length > 0)
				return false;

			// Actually, we don't want the folder to exist at all.
			// Just delete it, or Chorus will not be happy.
			Directory.Delete(currentRootDataPath);

			// 2. Make clone from some source.
			switch (extantRepoSource)
			{
				case ExtantRepoSource.Internet:
					var cloneModel = new GetCloneFromInternetModel(LiftProjectServices.BasePath)
					{
						LocalFolderName = project.LiftProjectName
					};
					using (var internetCloneDlg = new GetCloneFromInternetDialog(cloneModel))
					{
						var dlgResult = internetCloneDlg.ShowDialog(parent);
						switch (dlgResult)
						{
							default:
								return false;
							case DialogResult.OK:
								// It made a clone, but maybe in the 'wrong' folder name.
								PossiblyRenameFolder(internetCloneDlg.PathToNewProject, currentRootDataPath);
								break;
						}
					}
					break;
				case ExtantRepoSource.LocalNetwork:
					using (var openFileDlg = new OpenFileDialog())
					{
						openFileDlg.AutoUpgradeEnabled = true;
						openFileDlg.Title = Resources.kLocateLiftFile;
						openFileDlg.AutoUpgradeEnabled = true;
						openFileDlg.RestoreDirectory = true;
						openFileDlg.DefaultExt = ".lift";
						openFileDlg.Filter = Resources.kLiftFileFilter;
						openFileDlg.Multiselect = false;

						var dlgResult = openFileDlg.ShowDialog(parent);
						switch (dlgResult)
						{
							default:
								return false;
							case DialogResult.OK:
								var fileFromDlg = openFileDlg.FileName;
								var sourcePath = Path.GetDirectoryName(fileFromDlg);
								var x = Path.GetFileNameWithoutExtension(fileFromDlg);
								// Make a clone the hard way.
// ReSharper disable AssignNullToNotNullAttribute
								var target = Path.Combine(LiftProjectServices.BasePath, x);
								if (Directory.Exists(target))
									throw new ApplicationException(string.Format(Resources.kCloneTrouble, target));
								var repo = new HgRepository(sourcePath, new StatusProgress());
								repo.CloneLocal(target);
								// It made a clone, but maybe in the 'wrong' folder name.
								PossiblyRenameFolder(target, currentRootDataPath);
// ReSharper restore AssignNullToNotNullAttribute
								break;
						}
					}
					break;
				case ExtantRepoSource.Usb:
					using (var usbCloneDlg = new GetCloneFromUsbDialog(LiftProjectServices.BasePath))
					{
						var dlgResult = usbCloneDlg.ShowDialog(parent);
						switch (dlgResult)
						{
							default:
								return false;
							case DialogResult.OK:
								// It made a clone, but maybe in the 'wrong' folder name.
								PossiblyRenameFolder(usbCloneDlg.PathToNewProject, currentRootDataPath);
								break;
						}
					}
					break;
			}
			return true;
		}

		private static void PossiblyRenameFolder(string newProjPath, string currentRootDataPath)
		{
			if (newProjPath != currentRootDataPath)
				Directory.Move(newProjPath, currentRootDataPath);
		}

		#endregion
	}
}