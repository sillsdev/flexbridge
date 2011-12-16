using System;
using System.IO;
using Chorus.sync;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;
using Palaso.Progress.LogBox;
using Palaso.TestUtilities;

namespace FLEx_ChorusPluginTests
{
	/// <summary>
	/// Provides temporary directories and repositories.
	/// </summary>
	internal class RepositorySetup : IDisposable
	{
		private readonly StringBuilderProgress _stringBuilderProgress = new StringBuilderProgress();
		internal TemporaryFolder RootFolder;
		internal TemporaryFolder ProjectFolder;
		internal ProjectFolderConfiguration ProjectFolderConfig;

		private void Init(string name)
		{
			Progress = new MultiProgress(new IProgress[] { new ConsoleProgress {ShowVerbose=true}, _stringBuilderProgress });
			RootFolder = new TemporaryFolder("ChorusTest-" + name);
		}

		internal RepositorySetup(string userName)
		{
			Init(userName);

			ProjectFolder = new TemporaryFolder(RootFolder, ProjectName);

			MakeRepositoryForTest(ProjectFolder.Path, userName,Progress);
			ProjectFolderConfig = new ProjectFolderConfiguration(ProjectFolder.Path);

		}


		internal RepositorySetup(string cloneName, RepositorySetup sourceToClone)
		{
			Init(cloneName);
			var pathToProject = RootFolder.Combine(ProjectName);
			ProjectFolderConfig = sourceToClone.ProjectFolderConfig.Clone();
			ProjectFolderConfig.FolderPath = pathToProject;

			sourceToClone.MakeClone(pathToProject);
			ProjectFolder = TemporaryFolder.TrackExisting(RootFolder.Combine(ProjectName));

			var hg = new HgRepository(pathToProject, Progress);
			hg.SetUserNameInIni(cloneName, Progress);

		}

		private void MakeClone(string pathToNewRepo)
		{
			HgHighLevel.MakeCloneFromLocalToLocal(ProjectFolder.Path, pathToNewRepo, true, Progress);
		}

		internal string GetProgressString()
		{
			return _stringBuilderProgress.ToString();
		}

		internal Synchronizer CreateSynchronizer()
		{
			return Synchronizer.FromProjectConfiguration(ProjectFolderConfig, Progress);
		}

		internal HgRepository Repository
		{
			get { return new HgRepository(ProjectFolderConfig.FolderPath, Progress); }
		}

		public void Dispose()
		{
			if (Repository != null)
			{
				Assert.IsFalse(Repository.GetHasLocks(), "A lock was left over, after the test.");
			}
			ProjectFolder.Dispose();
			RootFolder.Dispose();
		}

		internal void WriteIniContents(string s)
		{
			File.WriteAllText(PathToHgrc, s);
		}

		private string PathToHgrc
		{
			get { return Path.Combine(Path.Combine(ProjectFolder.Path, ".hg"), "hgrc"); }
		}

		internal void EnsureNoHgrcExists()
		{
			if (File.Exists(PathToHgrc))
				File.Delete(PathToHgrc);
		}

		internal void AddAndCheckinFile(string fileName, string contents)
		{
			var p = ProjectFolder.Combine(fileName);
			File.WriteAllText(p, contents);
			Repository.AddAndCheckinFile(p);
		}

		internal void ChangeFile(string fileName, string contents)
		{
			var p = ProjectFolder.Combine(fileName);
			File.WriteAllText(p, contents);
		}

		internal void ChangeFileAndCommit(string fileName, string contents, string message)
		{
			var p = ProjectFolder.Combine(fileName);
			File.WriteAllText(p, contents);
		   Repository.Commit(false,message);
		}

		internal void AddAndCheckIn()
		{
			var options = new SyncOptions
							{
								DoMergeWithOthers = false,
								DoPullFromOthers = false,
								DoSendToOthers = false
							};

			CreateSynchronizer().SyncNow(options);
		}

		internal SyncResults CheckinAndPullAndMerge()
		{
			return CheckinAndPullAndMerge(null);
		}

		internal SyncResults CheckinAndPullAndMerge(RepositorySetup otherUser)
		{
			var options = new SyncOptions
							{
								DoMergeWithOthers = true,
								DoPullFromOthers = true,
								DoSendToOthers = true
							};

			if(otherUser!=null)
				options.RepositorySourcesToTry.Add(otherUser.GetRepositoryAddress());
			return CreateSynchronizer().SyncNow(options);
		}

		internal RepositoryAddress GetRepositoryAddress()
		{
			var x =   RepositoryAddress.Create("unknownname", ProjectFolder.Path, false);
			x.Enabled = true;
			return x;
		}

		internal void AssertFileExistsRelativeToRoot(string relativePath)
		{
			Assert.IsTrue(File.Exists(RootFolder.Combine(relativePath)));
		}

		internal void AssertFileExistsInRepository(string pathRelativeToRepositoryRoot)
		{
			Assert.IsTrue(Repository.GetFileExistsInRepo(pathRelativeToRepositoryRoot));
		}

		internal void AssertFileDoesNotExistInRepository(string pathRelativeToRepositoryRoot)
		{
			Assert.IsFalse(Repository.GetFileExistsInRepo(pathRelativeToRepositoryRoot));
		}

		internal static void MakeRepositoryForTest(string newRepositoryPath, string userId, IProgress progress)
		{
			HgRepository.CreateRepositoryInExistingDir(newRepositoryPath,progress);
			var hg = new HgRepository(newRepositoryPath, progress);
			hg.SetUserNameInIni(userId,  progress);
		}

		internal static string ProjectName
		{
			get { return "foo project"; }//nb: important that it have a space, as this helps catch failure to enclose in quotes
		}

		internal IProgress Progress { get; set; }

		internal IDisposable GetFileLockForReading(string localPath)
		{
			return new StreamWriter(ProjectFolder.Combine(localPath));
		}

		internal IDisposable GetFileLockForWriting(string localPath)
		{
#if MONO
			// This doesn't work.  A mono bug perhaps? (CP)
			FileStream f = new FileStream(ProjectFolder.Combine(localPath), FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
			// This didn't work either
			//f.Lock(0, f.Length - 1);
			//FileStream f = new FileStream(ProjectFolder.Combine(localPath), FileMode.Open, FileAccess.Write, FileShare.None);
			// This locked the file, but also deleted it (as expected) which isn't what the test expects
			//FileStream f = new FileStream(ProjectFolder.Combine(localPath), FileMode.Create, FileAccess.Write, FileShare.None);
			return f;
#else
			return new StreamReader(ProjectFolder.Combine(localPath));
#endif
		}

		internal void AssertSingleHead()
		{
			var actual = Repository.GetHeads().Count;
			Assert.AreEqual(1, actual, "There should be on only one head, but there are " + actual);
		}

		internal void AssertHeadCount(int count)
		{
			var actual = Repository.GetHeads().Count;
			Assert.AreEqual(count, actual, "Wrong number of heads");
		}

		internal void AssertFileExists(string relativePath)
		{
			Assert.IsTrue(File.Exists(ProjectFolder.Combine(relativePath)));
		}

		internal void AssertFileContents(string relativePath, string expectedContents)
		{
			Assert.AreEqual(expectedContents, File.ReadAllText(ProjectFolder.Combine(relativePath)));
		}

		/// <summary>
		/// Obviously, don't leave this in a unit test... it's only for debugging
		/// </summary>
		internal void ShowInTortoise()
		{
			var start = new System.Diagnostics.ProcessStartInfo("hgtk", "log")
							{
								WorkingDirectory = ProjectFolder.Path
							};
			System.Diagnostics.Process.Start(start);
		}

		/// <summary>
		/// not called "CreateReject*Branch* because we're not naming it (but it is, technically, a branch)
		/// </summary>
		internal void CreateRejectForkAndComeBack()
		{
			var originalTip = Repository.GetTip();
			ChangeFile("test.txt", "bad");
			var options = new SyncOptions
							{
								DoMergeWithOthers = true,
								DoPullFromOthers = true,
								DoSendToOthers = true
							};
			var synchronizer = CreateSynchronizer();
			synchronizer.SyncNow(options);
			var badRev = Repository.GetTip();

			//notice that we're putting changeset which does the tagging over on the original branch
			Repository.RollbackWorkingDirectoryToRevision(originalTip.Number.Hash);
			Repository.TagRevision(badRev.Number.Hash, Synchronizer.RejectTagSubstring);// this adds a new changeset
			synchronizer.SyncNow(options);

			var revision = Repository.GetRevisionWorkingSetIsBasedOn();
			revision.EnsureParentRevisionInfo();
			 Assert.AreEqual(originalTip.Number.LocalRevisionNumber, revision.Parents[0].LocalRevisionNumber, "Should have moved back to original tip.");
		}

		internal void AssertLocalRevisionNumber(int localNumber)
		{
			Assert.AreEqual(localNumber.ToString(), Repository.GetRevisionWorkingSetIsBasedOn().Number.LocalRevisionNumber);
		}

		internal void AssertRevisionHasTag(int localRevisionNumber, string tag)
		{
			Assert.AreEqual(tag, Repository.GetRevision(localRevisionNumber.ToString()).Tag);
		}

		internal void ChangeFileOnNamedBranchAndComeBack(string fileName, string contents, string branchName)
		{
			string previousRevisionNumber = Repository.GetRevisionWorkingSetIsBasedOn().Number.LocalRevisionNumber;
			Repository.Branch(branchName);
			ChangeFileAndCommit(fileName, contents, "Created by ChangeFileOnNamedBranchAndComeBack()");
			Repository.Update(previousRevisionNumber);//go back
		}

		internal BookMark CreateBookmarkHere()
		{
			return new BookMark(Repository);
		}
	}

	internal class BookMark
	{
		private readonly HgRepository _repository;
		private readonly Revision _revision;

		internal BookMark(HgRepository repository)
		{
			_repository = repository;
			_revision = _repository.GetRevisionWorkingSetIsBasedOn();
		}

		internal void Go()
		{
			_repository.Update(_revision.Number.Hash);
		}

		internal void AssertRepoIsAtThisPoint()
		{
			Assert.AreEqual(_revision.Number.Hash, _repository.GetRevisionWorkingSetIsBasedOn().Number.Hash);
		}
	}
}