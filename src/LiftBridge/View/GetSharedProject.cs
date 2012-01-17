using System.IO;
using System.Linq;
using System.Windows.Forms;
using Chorus.UI.Clone;
using Chorus.VcsDrivers;
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
								var repo = new HgRepository(internetCloneDlg.PathToNewProject, new NullProgress());
								project.RepositoryIdentifier = repo.Identifier;
								// TODO: Try to put it into expected location for older FLEx versions, if possible.
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
								if (Directory.GetDirectories(sourcePath, ".hg").Count() == 0)
								{
									MessageBox.Show(parent, Resources.kLoneLiftFileWarning, Resources.kUnsupportedLiftFile, MessageBoxButtons.OK, MessageBoxIcon.Warning);
									return false;
								}
								var x = Path.GetFileNameWithoutExtension(fileFromDlg);
								// Make a clone the hard way.
								var expectedTarget = Path.Combine(LiftProjectServices.BasePath, x);
								var repo = new HgRepository(sourcePath, new StatusProgress());
								var actualTarget = repo.CloneLocalWithoutUpdate(expectedTarget);
								var targetRepo = new HgRepository(actualTarget, new StatusProgress());
								var alias = HgRepository.GetAliasFromPath(actualTarget);
								targetRepo.SetTheOnlyAddressOfThisType(RepositoryAddress.Create(alias, actualTarget));
								project.RepositoryIdentifier = targetRepo.Identifier;
								// TODO: Try to put it into expected location for older FLEx versions, if possible.
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
								var repo = new HgRepository(usbCloneDlg.PathToNewProject, new NullProgress());
								project.RepositoryIdentifier = repo.Identifier;
								// TODO: Try to put it into expected location for older FLEx versions, if possible.
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