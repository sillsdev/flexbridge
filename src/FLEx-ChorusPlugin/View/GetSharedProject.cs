using System.IO;
using System.Windows.Forms;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using Chorus.clone;
using Chorus.UI.Clone;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Properties;
using FLEx_ChorusPlugin.Model;
using Palaso.Extensions;
using Palaso.Progress.LogBox;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.View
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
					// This is not the right model for this operation.
					// The real 'fix' is to add a new view and a new model to Chorus that does handle this.
					// That new model really needs to make sure it has something like the "ProjectFilter" delegate
					// on the CloneFromUsb model, which filters the folders to only allow selection of one that is for FW data (cf. below).
					// More notes on using the CloneFromUsb model in this context:
					//	All of the code starting at "var actualClonedFolder" and going through the line "repo.SetIsOneDefaultSyncAddresses(address, true);"
					//	Belong in the model, not here. But, the USB clone model doesn't do that, since it doens;t need to write to the hgrc file for its location.
					//	And, even if it did such a write, nobody would care enough to read it, as USB S/R is *much* simpler to figure out,
					//	and its S/R is quite deterministic with no need to store a location.
					// See how this was done in Lift Bridge with a new model and view which I hope end up in Chorus at some point.
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
								var actualClonedFolder = cloner.MakeClone(fileFromDlg, currentBaseFieldWorksBridgePath, new StatusProgress());

								var repo = new HgRepository(actualClonedFolder, new NullProgress());
								var address = RepositoryAddress.Create("Shared NetWork", fileFromDlg);
								// These next two calls are fine in how they treat the hgrc update, as a bootstrap clone has no old stuff to fret about.
								// SetKnownRepositoryAddresses blows away entire 'paths' section, including the "default" one that hg puts in, which we don't really want anyway.
								repo.SetKnownRepositoryAddresses(new[] { address });
								// SetIsOneDefaultSyncAddresses adds 'address' to another section (ChorusDefaultRepositories) in hgrc.
								// 'true' then writes the "address.Name=" (section.Set(address.Name, string.Empty);).
								// I (RandyR) think this then uses that address.Name as the new 'default' for that particular repo source type.
								repo.SetIsOneDefaultSyncAddresses(address, true);

								var mainFilePathName = Path.Combine(actualClonedFolder, langProjName + ".fwdata");
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