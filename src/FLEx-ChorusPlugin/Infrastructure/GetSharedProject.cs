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

		private static bool RenameFolderIfPossible(string actualCloneLocation, string possibleNewLocation)
		{
			if (actualCloneLocation != possibleNewLocation && !Directory.Exists(possibleNewLocation))
			{
				Directory.Move(actualCloneLocation, possibleNewLocation);
				return true;
			}
			return false;
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
			var currentBaseFlexBridgePath = flexProjectFolder;
			string langProjName = null;
			string actualCloneLocation = null;
			switch (extantRepoSource)
			{
				case ExtantRepoSource.Internet:
					var cloneModel = new GetCloneFromInternetModel(currentBaseFlexBridgePath) { LocalFolderName = langProjName };
					using (var internetCloneDlg = new GetCloneFromInternetDialog(cloneModel))
					{
						switch (internetCloneDlg.ShowDialog(parent))
						{
							default:
								return false;
							case DialogResult.OK:
								// It made a clone, but maybe in the wrong name, grab the project name.
								actualCloneLocation = internetCloneDlg.PathToNewProject;
								langProjName = Path.GetFileName(actualCloneLocation); // internetCloneDlg.PathToNewProject is a folder name, not file name.
								break;
						}
					}
					break;
				case ExtantRepoSource.LocalNetwork:
					var cloneFromNetworkFolderModel = new GetCloneFromNetworkFolderModel(currentBaseFlexBridgePath)
														{
															ProjectFilter = ProjectFilter
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
								actualCloneLocation = cloneFromNetworkFolderModel.ActualClonedFolder;
								langProjName = Path.GetFileName(cloneFromNetworkFolderModel.UserSelectedRepositoryPath);
								break;
						}
					}
					break;
				case ExtantRepoSource.Usb:
					using (var usbCloneDlg = new GetCloneFromUsbDialog(currentBaseFlexBridgePath))
					{
						usbCloneDlg.Model.ProjectFilter = ProjectFilter;
						switch (usbCloneDlg.ShowDialog(parent))
						{
							default:
								return false;
							case DialogResult.OK:
								// It made a clone, grab the project name.
								actualCloneLocation = usbCloneDlg.PathToNewProject;
								langProjName = Path.GetFileName(actualCloneLocation); // folder name, not file name.
								break;
						}
					}
					break;
			}

			var newProjectFileName = langProjName + ".fwdata";
			FLExProjectUnifier.PutHumptyTogetherAgain(new NullProgress(), Path.Combine(actualCloneLocation, newProjectFileName));
			var possibleNewLocation = Path.Combine(currentBaseFlexBridgePath, langProjName);
			var finalCloneLocation = RenameFolderIfPossible(actualCloneLocation, possibleNewLocation) ? possibleNewLocation : actualCloneLocation;
			CurrentProject = new LanguageProject(Path.Combine(finalCloneLocation, newProjectFileName));

			return true;
		}

		#endregion

		private static bool ProjectFilter(string path)
		{
			var hgDataFolder = path.CombineForPath(".hg", "store", "data");
			return Directory.Exists(hgDataFolder) && Directory.GetFiles(hgDataFolder, "*_custom_properties.i").Length > 0;
		}
	}
}