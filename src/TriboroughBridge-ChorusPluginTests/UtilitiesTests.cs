using System;
using System.IO;
using System.Reflection;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;
using Palaso.Progress;
using Palaso.TestUtilities;
using TriboroughBridge_ChorusPlugin;

namespace TriboroughBridge_ChorusPluginTests
{
	[TestFixture]
	public class UtilitiesTests
	{
		[TearDown]
		public void TearDown()
		{
			Utilities.ClearCacheForTests();
		}

		[Test]
		public void EnsureFilePrefixIsRemoved()
		{
			var prefix = Uri.UriSchemeFile + ":";
			var fullPathname = Assembly.GetExecutingAssembly().CodeBase;
			Assert.IsTrue(fullPathname.StartsWith(prefix));
			var reducedPathname = Utilities.StripFilePrefix(fullPathname);
			Assert.IsFalse(reducedPathname.StartsWith(prefix));
		}

		[Test]
		public void MoveRepositoryRemovesSourceFolder()
		{
			var tempFolderForOs = Path.GetTempPath();
			var tempCloneHolder = new TemporaryFolder("TempCloneHolder");
			var tempCloneDir = new TemporaryFolder(tempCloneHolder, "TempClone");
			var repo = new HgRepository(tempCloneDir.Path, new NullProgress());
			repo.Init();
			var tempDataPathname = Path.Combine(tempCloneDir.Path, "dummy" + Utilities.LiftExtension);
			File.WriteAllText(tempDataPathname, "dummy data");
			repo.AddAndCheckinFile(tempDataPathname);

			// Add import failure file, but don't add it to the repo.
			var roadblockPathname = Path.Combine(tempCloneDir.Path, Utilities.FailureFilename);
			File.WriteAllText(roadblockPathname, "standard");

			var tempNewHomeDir = Path.Combine(tempFolderForOs, "FinalCloneHolder");

			try
			{
				Utilities.MakeLocalCloneAndRemoveSourceParentFolder(tempCloneDir.Path, tempNewHomeDir, new NullProgress());
				Assert.IsFalse(Directory.Exists(tempCloneHolder.Path));
				Assert.IsTrue(File.Exists(Path.Combine(tempNewHomeDir, BridgeTrafficCop.hg, "hgrc")));
				Assert.IsTrue(File.Exists(Path.Combine(tempNewHomeDir, "dummy" + Utilities.LiftExtension)));
			}
			finally
			{
				if (Directory.Exists(tempCloneHolder.Path))
					Directory.Delete(tempCloneHolder.Path, true);
				if (Directory.Exists(tempNewHomeDir))
					Directory.Delete(tempNewHomeDir, true);
			}
		}

		[Test]
		public void DoesNotHaveMatchingProjectWhenNoneExist()
		{
			using (var hasProject = new TemporaryFolder("hasProject"))
			using (var hasNoProject = new TemporaryFolder("hasNoProject"))
			{
				var newFile = Path.Combine(hasProject.Path, "test.txt");
				File.WriteAllText(newFile, "some stuff");
				var repo = new HgRepository(hasProject.Path, new NullProgress());
				repo.Init();
				repo.AddAndCheckinFile(newFile);

				Assert.IsFalse(Utilities.AlreadyHasLocalRepository(hasNoProject.Path, hasProject.Path));
			}
		}

		[Test]
		public void DoesNotHaveMatchingProjectWhenTwoExist()
		{
			using (var hasProject = new TemporaryFolder("hasProject"))
			{
				var newFile = Path.Combine(hasProject.Path, "test.txt");
				File.WriteAllText(newFile, "some stuff");
				var repo = new HgRepository(hasProject.Path, new NullProgress());
				repo.Init();
				repo.AddAndCheckinFile(newFile);

				using (var hasNonmatchingProject = new TemporaryFolder("hasNonmatchingProjects"))
				{
					var mainProjectDir = Directory.CreateDirectory(Path.Combine(hasNonmatchingProject.Path, "mainfwdir"));
					newFile = Path.Combine(mainProjectDir.FullName, "test.txt");
					File.WriteAllText(newFile, "some stuff");
					repo = new HgRepository(mainProjectDir.FullName, new NullProgress());
					repo.Init();
					repo.AddAndCheckinFile(newFile);

					var nestedLiftDir = Directory.CreateDirectory(Path.Combine(mainProjectDir.FullName, Utilities.OtherRepositories, Utilities.LIFT));
					newFile = Path.Combine(nestedLiftDir.FullName, "test.txt");
					File.WriteAllText(newFile, "some stuff");
					repo = new HgRepository(nestedLiftDir.FullName, new NullProgress());
					repo.Init();
					repo.AddAndCheckinFile(newFile);

					Assert.IsFalse(Utilities.AlreadyHasLocalRepository(hasNonmatchingProject.Path, hasProject.Path));
				}
			}
		}

		[Test]
		public void HasMatchingMainProject()
		{
			using (var hasProject = new TemporaryFolder("hasProject"))
			{
				var newFile = Path.Combine(hasProject.Path, "test.txt");
				File.WriteAllText(newFile, "some stuff");
				var repo = new HgRepository(hasProject.Path, new NullProgress());
				repo.Init();
				repo.AddAndCheckinFile(newFile);

				using (var hasNonmatchingProject = new TemporaryFolder("hasNonmatchingProjects"))
				{
					var mainProjectDir = Directory.CreateDirectory(Path.Combine(hasNonmatchingProject.Path, "mainfwdir"));
					repo.CloneLocalWithoutUpdate(mainProjectDir.FullName);

					Assert.IsTrue(Utilities.AlreadyHasLocalRepository(hasNonmatchingProject.Path, hasProject.Path));
				}
			}
		}

		[Test]
		public void HasMatchingLiftProject()
		{
			using (var hasProject = new TemporaryFolder("hasProject"))
			{
				var newFile = Path.Combine(hasProject.Path, "test.txt");
				File.WriteAllText(newFile, "some stuff");
				var repo = new HgRepository(hasProject.Path, new NullProgress());
				repo.Init();
				repo.AddAndCheckinFile(newFile);

				using (var hasNonmatchingProject = new TemporaryFolder("hasNonmatchingProjects"))
				{
					var mainProjectDir = Directory.CreateDirectory(Path.Combine(hasNonmatchingProject.Path, "mainfwdir"));
					var nestedLiftDir = Directory.CreateDirectory(Path.Combine(mainProjectDir.FullName, Utilities.OtherRepositories, Utilities.LIFT));
					repo.CloneLocalWithoutUpdate(nestedLiftDir.FullName);

					Assert.IsTrue(Utilities.AlreadyHasLocalRepository(hasNonmatchingProject.Path, hasProject.Path));
				}
			}
		}
	}
}