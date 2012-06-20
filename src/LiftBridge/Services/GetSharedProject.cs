using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Chorus.UI.Clone;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Extensions;
using Palaso.Progress.LogBox;
using SIL.LiftBridge.Model;
using SIL.LiftBridge.Properties;
using SIL.LiftBridge.View;

namespace SIL.LiftBridge.Services
{
	/// <summary>
	/// Get a teammate's shared Lift project from the specified source.
	/// </summary>
	internal class GetSharedProject : IGetSharedProject
	{
		private static bool ProjectFilter(string path)
		{
			var hgDataFolder = path.CombineForPath(".hg", "store", "data");
			return Directory.Exists(hgDataFolder)
				&& Directory.GetFiles(hgDataFolder).Any(pathname => pathname.ToLowerInvariant().EndsWith(".lift.i"));
		}

		#region Implementation of IGetSharedProject

		/// <summary>
		/// Get a teammate's shared Lift project from the specified source.
		/// </summary>
		/// <returns>
		/// One of several of the enum values.
		/// </returns>
		public CloneResult GetSharedProjectUsing(Form parent, ExtantRepoSource extantRepoSource, LiftProject project)
		{
			string proposedRootDataDir;
			var result = CheckFolder(parent, project, out proposedRootDataDir);
			switch (result)
			{
				default:
					// Not returned by CheckFolder.
				// case CloneResult.Created:
				// case CloneResult.NotCreated:
//#if DEBUG
					Debug.Fail("'result' not supported here, or not recognized.");
					return CloneResult.Cancel;
//#endif
				case CloneResult.OkToCreate:
					return MakeClone(parent, extantRepoSource, project);
				case CloneResult.Cancel: // Fall through, as long as it returns 'result'.
				case CloneResult.UseExisting:
					return result;
			}
		}

		private static CloneResult MakeClone(Form parent, ExtantRepoSource extantRepoSource, LiftProject project)
		{
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
							default: // DialogResult.Cancel
								return CloneResult.Cancel;
							case DialogResult.OK:
								var repo = new HgRepository(internetCloneDlg.PathToNewProject, new NullProgress());
								project.RepositoryIdentifier = repo.Identifier;
								break;
						}
					}
					break;
				case ExtantRepoSource.LocalNetwork:
					var cloneFromNetworkFolderModel = new GetCloneFromNetworkFolderModel(LiftProjectServices.BasePath)
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
								return CloneResult.Cancel;
							case DialogResult.OK:
								var repo = new HgRepository(cloneFromNetworkFolderModel.ActualClonedFolder, new NullProgress());
								project.RepositoryIdentifier = repo.Identifier;
								break;
						}
					}
					break;
				case ExtantRepoSource.Usb:
					using (var usbCloneDlg = new GetCloneFromUsbDialog(LiftProjectServices.BasePath))
					{
						usbCloneDlg.Model.ProjectFilter = ProjectFilter;
						var dlgResult = usbCloneDlg.ShowDialog(parent);
						switch (dlgResult)
						{
							default: // DialogResult.Cancel
								return CloneResult.Cancel;
							case DialogResult.OK:
								var repo = new HgRepository(usbCloneDlg.PathToNewProject, new NullProgress());
								project.RepositoryIdentifier = repo.Identifier;
								break;
						}
					}
					break;
			}
			return CloneResult.Created;
		}

		#endregion

		private static CloneResult CheckFolder(Form parent, LiftProject project, out string proposedRootDataDir)
		{
			var cloneResult = CloneResult.Cancel;
			// proposedRootDataDir is guaranteed to exist after the call to PathToProject.
			// It may be newly created and thus empty, or it may have stuff in it, for for testers, such as Marlon or Ken.
			proposedRootDataDir = LiftProjectServices.PathToProject(project);

			if (project.Id == Guid.Empty)
			{
				var blowItAway = false;
				// Old interface use.
				if (Directory.GetFiles(proposedRootDataDir).Length > 0 || Directory.GetDirectories(proposedRootDataDir).Length > 0)
				{
					// Ask if they feel lucky, and blow it away, if they do.
					// It may have a repo in it, so that is another way to feel lucky by just using it, which for the old system is probably fairly safe.
					var hasRepo = Directory.Exists(Path.ChangeExtension(proposedRootDataDir, ".hg"));
					if (hasRepo)
					{
						var useExtantRepoResult = MessageBox.Show(
							parent,
							Resources.kExtantSharedSystem + Environment.NewLine
								+ Resources.kUseExtantRepo + Environment.NewLine
								+ Resources.kWIpeOutExtantRepoAndReuseFolder + Environment.NewLine
								+ Resources.kCancelGetMeOutOfHere,
							Resources.kExistingSharedStstemTitle,
							MessageBoxButtons.YesNoCancel,
							MessageBoxIcon.Warning);
						switch (useExtantRepoResult)
						{
							default:
							//case DialogResult.Cancel:
								cloneResult = CloneResult.Cancel;
								break;
							case DialogResult.No:
								// Reset below. cloneResult = CloneResult.OkToCreate;
								blowItAway = true;
								break;
							case DialogResult.Yes:
								cloneResult = CloneResult.UseExisting;
								break;
						}
					}
					else
					{
						// Non empty folder, but no repo.
						var useExtantFolderResult = MessageBox.Show(
							parent,
							Resources.kExtantNonEmptyFolder + Environment.NewLine
								+ Resources.kUseExtantNonEmptyFolder + Environment.NewLine
								+ Resources.kCancelGetMeOutOfHere,
							Resources.kNonEmptyFolderTitle,
							MessageBoxButtons.OKCancel,
							MessageBoxIcon.Warning);
						switch (useExtantFolderResult)
						{
							default:
								//case DialogResult.Cancel:
								cloneResult = CloneResult.Cancel;
								break;
							case DialogResult.OK:
								// Reset below. cloneResult = CloneResult.OkToCreate;
								blowItAway = true;
								break;
						}
					}
				}
				else
				{
					// Empty, so zap it.
					blowItAway = true;
				}
				if (blowItAway)
				{
					Directory.Delete(proposedRootDataDir, true);
					cloneResult = CloneResult.OkToCreate;
				}
			}
			else
			{
				// New interface, which can figure out how to find its repo.
				// It may actually end up in another folder, but no biggie for the new system.
				// Q: What happens if the map file has the guid, and worse, the repo id, but the repo has been zapped?
				if (Directory.Exists(Path.Combine(proposedRootDataDir, ".hg")))
				{
					// A1: 'proposedRootDataDir' has a matching repo, so use it.
					cloneResult = CloneResult.UseExisting;
				}
				else
				{
					// No matching repo for the LP+RepoId.
					var mapDoc = LiftProjectServices.GetMappingDoc();
					var mapForProject = LiftProjectServices.GetMapForProject(project.Id, mapDoc.Element(LiftProjectServices.MappingsRootTag));
					if (mapForProject != null)
					{
						// A2: Remove repo id from file, if it is there, or LB will throw an exception, when it tries to set it to some other value.
						var repoIdAttr = mapForProject.Attribute(LiftProjectServices.RepositoryidentifierAttrTag);
						if (repoIdAttr != null)
						{
							repoIdAttr.Remove();
							mapDoc.Save(LiftProjectServices.MappingPathname);
						}
					}
					// A3. Go ahead and create a new one, but it may not actually end up in proposedRootDataDir.
					cloneResult = CloneResult.OkToCreate;
				}
			}

			return cloneResult;
		}
	}

	internal enum CloneResult
	{
		Created, // Used by GetSharedProject
		NotCreated, // Used by GetSharedProject
		Cancel, // Used by GetSharedProject
		OkToCreate,
		UseExisting // Used by GetSharedProject
	}
}