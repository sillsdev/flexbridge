using System;
using System.IO;
using System.Windows.Forms;
using Chorus.UI.Clone;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress;

namespace LiftBridgeTests.MockedViews
{
	internal class MockedGetSharedProject : IGetSharedProject
	{
		internal ExtantRepoSource RepoSource { get; private set; }

		#region Implementation of IGetSharedProject

		public CloneResult GetSharedProjectUsing(Form parent, ExtantRepoSource extantRepoSource, Func<string, bool> projectFilter, string baseLocalProjectDir, string preferredClonedFolderName)
		{
			RepoSource = extantRepoSource;

			// Create a repo.
			var newProLocation = Path.Combine(baseLocalProjectDir, preferredClonedFolderName);
			var repo = new HgRepository(newProLocation, new NullProgress());
			repo.Init();

			return new CloneResult(newProLocation, CloneStatus.Created);
		}

		#endregion
	}
}