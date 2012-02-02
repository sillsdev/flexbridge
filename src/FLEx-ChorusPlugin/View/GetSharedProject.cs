using System;
using System.IO;
using System.Windows.Forms;
using Chorus.UI.Clone;
using Chorus.VcsDrivers.Mercurial;
using FLEx_ChorusPlugin.Properties;
using Palaso.Progress.LogBox;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.View
{
	internal sealed class GetSharedProject : IGetSharedProject
	{
		private static void PossiblyRenameFolder(string newProjPath, string currentRootDataPath)
		{
			if (newProjPath != currentRootDataPath)
				Directory.Move(newProjPath, currentRootDataPath);
		}

		#region Implementation of IGetSharedProject

		/// <summary>
		/// Get a teammate's shared FieldWorks project from the specified source.
		/// </summary>
		bool IGetSharedProject.GetSharedProjectUsing(Form parent, ExtantRepoSource extantRepoSource, string flexProjectFolder)
		{
			// 2. Make clone from some source.
			var currentBaseFieldWorksBridgePath = flexProjectFolder;
			string langProjName = "NOT YET IMPEMENTED FOR THIS SOURCE";
			switch (extantRepoSource)
			{
				case ExtantRepoSource.Internet:
					var cloneModel = new GetCloneFromInternetModel(currentBaseFieldWorksBridgePath) { LocalFolderName = langProjName };
					using (var internetCloneDlg = new GetCloneFromInternetDialog(cloneModel))
					{
						switch (internetCloneDlg.ShowDialog(parent))
						{
							default:
								return false;
							case DialogResult.OK:
								// It made a clone, but maybe in the wrong name.
								MultipleFileServices.RestoreMainFile(currentBaseFieldWorksBridgePath, langProjName);
								PossiblyRenameFolder(internetCloneDlg.PathToNewProject, currentBaseFieldWorksBridgePath);
								break;
						}
					}
					break;
				case ExtantRepoSource.LocalNetwork:
					using (var openFileDlg = new OpenFileDialog())
					{
						openFileDlg.AutoUpgradeEnabled = true;
						openFileDlg.Title = Resources.kLocateFwDataFile;
						openFileDlg.AutoUpgradeEnabled = true;
						openFileDlg.RestoreDirectory = true;
						openFileDlg.DefaultExt = ".fwdata";
						openFileDlg.Filter = Resources.kFwDataFileFilter;
						openFileDlg.Multiselect = false;

						switch (openFileDlg.ShowDialog(parent))
						{
							default:
								return false;
							case DialogResult.OK:
								var fileFromDlg = openFileDlg.FileName;
								var sourcePath = Path.GetDirectoryName(fileFromDlg);
								var x = Path.GetFileNameWithoutExtension(fileFromDlg);
								// Make a clone the hard way.
								var target = Path.Combine(currentBaseFieldWorksBridgePath, x);
								if (Directory.Exists(target))
									throw new ApplicationException(string.Format(Resources.kCloneTrouble, target));
								var repo = new HgRepository(sourcePath, new StatusProgress());
								repo.CloneLocalWithoutUpdate(target);
								repo.Update();
								MultipleFileServices.RestoreMainFile(currentBaseFieldWorksBridgePath, langProjName);
								// It made a clone, but maybe in the wrong name.
								PossiblyRenameFolder(target, currentBaseFieldWorksBridgePath);
								break;
						}
					}
					break;
				case ExtantRepoSource.Usb:
					using (var usbCloneDlg = new GetCloneFromUsbDialog(currentBaseFieldWorksBridgePath))
					{
						switch (usbCloneDlg.ShowDialog(parent))
						{
							default:
								return false;
							case DialogResult.OK:
								// It made a clone, grab the project name.
								langProjName = Path.GetFileName(usbCloneDlg.PathToNewProject);
								string mainFilePathName = Path.Combine(usbCloneDlg.PathToNewProject, langProjName + ".fwdata");
								MultipleFileServices.RestoreMainFile(mainFilePathName, langProjName);
								PossiblyRenameFolder(usbCloneDlg.PathToNewProject, Path.Combine(currentBaseFieldWorksBridgePath, langProjName));
								break;
						}
					}
					break;
			}

			return true;
		}

		#endregion
	}
}