using System;
using System.IO;
using System.Windows.Forms;
using Chorus.UI.Clone;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using FieldWorksBridge.Properties;

namespace FieldWorksBridge.View
{
	internal class GetSharedProject : IGetSharedProject
	{
		#region Implementation of IGetSharedProject

		/// <summary>
		/// Get a teammate's shared FieldWorks project from the specified source.
		/// </summary>
		public bool GetSharedProjectUsing(Form parent, ExtantRepoSource extantRepoSource)
		{
			string currentRootDataPath;
			// 1. Find out the name of the lang proj: langProjName. This is the folder where the clone will go.
			using (var folderBrowserDlg = new FolderBrowserDialog())
			{
				folderBrowserDlg.RootFolder = Environment.SpecialFolder.LocalApplicationData;
				folderBrowserDlg.ShowNewFolderButton = true;
				folderBrowserDlg.Description = Resources.kSelectClonedDataFolder;
				if (folderBrowserDlg.ShowDialog(parent) == DialogResult.OK)
					currentRootDataPath = folderBrowserDlg.SelectedPath;
				else
					return false;
			}

			// Make sure it is an empty folder.
			if (Directory.GetFiles(currentRootDataPath).Length > 0 || Directory.GetDirectories(currentRootDataPath).Length > 0)
				return false;

			// 2. Make clone from some source.
			var currentBaseFieldWorksBridgePath = Directory.GetParent(currentRootDataPath).FullName;
			var dirInfo = new DirectoryInfo(currentRootDataPath);
			var langProjName = dirInfo.Name;
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
								PossiblyRenameFolder(internetCloneDlg.PathToNewProject, currentRootDataPath);
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
// ReSharper disable AssignNullToNotNullAttribute
								var target = Path.Combine(currentBaseFieldWorksBridgePath, x);
								if (Directory.Exists(target))
									throw new ApplicationException(string.Format(Resources.kCloneTrouble, target));
								var repo = new HgRepository(sourcePath, new StatusProgress());
								repo.CloneLocal(target);
								// It made a clone, but maybe in the wrong name.
								PossiblyRenameFolder(target, currentRootDataPath);
// ReSharper restore AssignNullToNotNullAttribute
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
								// It made a clone, but maybe in the wrong name.
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