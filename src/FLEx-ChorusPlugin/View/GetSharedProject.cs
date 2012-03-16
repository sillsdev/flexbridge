using System.IO;
using System.Windows.Forms;
using Chorus.clone;
using Chorus.UI.Clone;
using FLEx_ChorusPlugin.Properties;
using Palaso.Extensions;
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
			string langProjName = "NOT YET IMPLEMENTED FOR THIS SOURCE";
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
								MultipleFileServices.PutHumptyTogetherAgain(currentBaseFieldWorksBridgePath);
								PossiblyRenameFolder(internetCloneDlg.PathToNewProject, currentBaseFieldWorksBridgePath);
								break;
						}
					}
					break;
				case ExtantRepoSource.LocalNetwork:
					// This is not the right model for this operation.
					// The real 'fix' is to add a new view and a new model to Chorus that does handle this.
					// That new model really needs to make sure it has something like the "ProjectFilter" delegate
					// on the CloneFromUsb model, which filters the folders to only allow selection of one that is for FW data (cf. below).
					var cloner = new CloneFromUsb();
					using (var openFileDlg = new FolderBrowserDialog())
					{
						openFileDlg.Description = Resources.ksFindProjectDirectory;
						openFileDlg.ShowNewFolderButton = false;

						switch (openFileDlg.ShowDialog(parent))
						{
							default:
								return false;
							case DialogResult.OK:
								var fileFromDlg = openFileDlg.SelectedPath;
								langProjName = Path.GetFileNameWithoutExtension(fileFromDlg);
								// Make a clone the hard way.
								var target = Path.Combine(currentBaseFieldWorksBridgePath, langProjName);
								if (Directory.Exists(target))
								{
									MessageBox.Show(parent, Resources.ksTargetDirectoryExistsContent, Resources.ksTargetDirectoryExistsTitle);
									return false;
								}
								cloner.MakeClone(fileFromDlg, currentBaseFieldWorksBridgePath, new StatusProgress());

								var mainFilePathName = Path.Combine(Path.Combine(currentBaseFieldWorksBridgePath, langProjName), langProjName + ".fwdata");
								MultipleFileServices.PutHumptyTogetherAgain(mainFilePathName);
								break;
						}
					}
					break;
				case ExtantRepoSource.Usb:
					using (var usbCloneDlg = new GetCloneFromUsbDialog(currentBaseFieldWorksBridgePath))
					{
						usbCloneDlg.Model.ProjectFilter = path =>
															{
																var hgDataFolder = path.CombineForPath(".hg", "store", "data");
																return Directory.Exists(hgDataFolder) && Directory.GetFiles(hgDataFolder, "*_custom_properties.i").Length > 0;
															};
						switch (usbCloneDlg.ShowDialog(parent))
						{
							default:
								return false;
							case DialogResult.OK:
								// It made a clone, grab the project name.
								langProjName = Path.GetFileName(usbCloneDlg.PathToNewProject);
								string mainFilePathName = Path.Combine(usbCloneDlg.PathToNewProject, langProjName + ".fwdata");
								MultipleFileServices.PutHumptyTogetherAgain(mainFilePathName);
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