using System.IO;
using System.Windows.Forms;
using Chorus.UI.Clone;
using FLEx_ChorusPlugin.Model;
using FLEx_ChorusPlugin.View;
using Palaso.Extensions;
using Palaso.Progress.LogBox;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Infrastructure
{
	internal sealed class GetSharedProject : IGetSharedProject
	{
		private LanguageProject _currentProject;

		private static void PossiblyRenameFolder(string newProjPath, string currentRootDataPath)
		{
			if (newProjPath != currentRootDataPath)
				Directory.Move(newProjPath, currentRootDataPath);
		}

		#region Implementation of IGetSharedProject

		public LanguageProject CurrentProject
		{
			get
			{
				if (_currentProject == null)
					throw new System.FieldAccessException("Call GetSharedProjectUsing() first.");
				return _currentProject;
			}
			set
			{
				_currentProject = value;
			}
		}

		/// <summary>
		/// Get a teammate's shared FieldWorks project from the specified source.
		/// </summary>
		bool IGetSharedProject.GetSharedProjectUsing(Form parent, ExtantRepoSource extantRepoSource, string flexProjectFolder)
		{
			// 2. Make clone from some source.
			var currentBaseFieldWorksBridgePath = flexProjectFolder;
			const string noProject = "NOT YET IMPLEMENTED FOR THIS SOURCE";
			var langProjName = noProject;
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
								// It made a clone, but maybe in the wrong name, grab the project name.
								langProjName = Path.GetFileName(internetCloneDlg.PathToNewProject);
								if (langProjName == noProject)
									return false;
								var mainFilePathName = Path.Combine(internetCloneDlg.PathToNewProject, langProjName + ".fwdata");
								FLExProjectUnifier.PutHumptyTogetherAgain(new NullProgress(), mainFilePathName);
								PossiblyRenameFolder(internetCloneDlg.PathToNewProject, Path.Combine(currentBaseFieldWorksBridgePath, langProjName));
								break;
						}
					}
					break;
				case ExtantRepoSource.LocalNetwork:
					var cloneFromNetworkFolderModel = new GetCloneFromNetworkFolderModel(currentBaseFieldWorksBridgePath);
					// TODO: Set sensible default folder. This one expands to the path of our .exe file:
					cloneFromNetworkFolderModel.FolderPath = "home";
					// Filter copied from usbCloneDlg.Model below:
					cloneFromNetworkFolderModel.ProjectFilter = path =>
					{
						var hgDataFolder = path.CombineForPath(".hg", "store", "data");
						return Directory.Exists(hgDataFolder) && Directory.GetFiles(hgDataFolder, "*_custom_properties.i").Length > 0;
					};

					using (var openFileDlg = new GetCloneFromNetworkFolderDlg())
					{
						// We don't have a GetCloneFromNetworkFolderDlg constructor that takes the model because
						// it would inexplicably mess up Visual Studio's designer view of the dialog:
						openFileDlg.LoadFromModel(cloneFromNetworkFolderModel);

						switch (openFileDlg.ShowDialog(parent))
						{
							default:
								return false;
							case DialogResult.OK:
								var fileFromDlg = cloneFromNetworkFolderModel.UserSelectedRepositoryPath;
								langProjName = Path.GetFileNameWithoutExtension(fileFromDlg);
								var mainFilePathName = Path.Combine(cloneFromNetworkFolderModel.ActualClonedFolder, langProjName + ".fwdata");
								FLExProjectUnifier.PutHumptyTogetherAgain(new NullProgress(), mainFilePathName);
								// TODO: Call this, as is done for other two?
								//PossiblyRenameFolder(actualClonedFolder, Path.Combine(currentBaseFieldWorksBridgePath, langProjName));
								// TODO: Consider renaming the fwdata file (read: 'project name').
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
								var mainFilePathName = Path.Combine(usbCloneDlg.PathToNewProject, langProjName + ".fwdata");
								FLExProjectUnifier.PutHumptyTogetherAgain(new NullProgress(), mainFilePathName);
								PossiblyRenameFolder(usbCloneDlg.PathToNewProject, Path.Combine(currentBaseFieldWorksBridgePath, langProjName));
								break;
						}
					}
					break;
			}
			if (langProjName != noProject)
			{
				var currentRootDataPath = Path.Combine(currentBaseFieldWorksBridgePath, langProjName);
				var fwProjectPath = Path.Combine(currentRootDataPath, langProjName + ".fwdata");
				CurrentProject = new LanguageProject(fwProjectPath);
			}

			return true;
		}

		#endregion
	}
}