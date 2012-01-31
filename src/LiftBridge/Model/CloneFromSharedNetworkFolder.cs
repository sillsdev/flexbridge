using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Chorus.VcsDrivers.Mercurial;
using Palaso.IO;
using Palaso.Progress.LogBox;
using SIL.LiftBridge.Properties;

namespace SIL.LiftBridge.Model
{
	/// <summary>
	/// Use this class to make an initial clone from a shared network folder.
	/// Note, most clients can instead use the GetCloneFromSharedNetworkFolderDialog.
	/// </summary>
	public class CloneFromSharedNetworkFolder
	{
		public IEnumerable<string> GetDirectoriesWithMecurialRepos(string rootDir)
		{
			var directories = new string[0];
			try
			{
				// this is all complicated because the yield can't be inside the try/catch
				directories = Directory.GetDirectories(rootDir, ".hg", SearchOption.AllDirectories);
			}
			catch (Exception error)
			{
				MessageBox.Show(
					string.Format("Error while looking at shared network folders. The folder root was {0}. The error was: {1}",
								  rootDir, error.Message), Resources.kError, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return directories.Select(hgDir => hgDir.Replace(".hg", null));
		}

		public string MakeClone(string sourcePath, string parentDirectoryToPutCloneIn, IProgress progress)
		{
			var repo = new HgRepository(sourcePath, progress);
			var actualTarget = repo.CloneLocalWithoutUpdate(parentDirectoryToPutCloneIn);
			var newRepo = new HgRepository(actualTarget, progress);
			newRepo.Update(); // Need this for new clone from shared network folder.
			return actualTarget;
		}
	}
}