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
				File.WriteAllText(newFile, "some hasProject stuff");
				var repo = new HgRepository(hasProject.Path, new NullProgress());
				repo.Init();
				repo.AddAndCheckinFile(newFile);

				using (var hasNonmatchingProject = new TemporaryFolder("hasNonmatchingProjects"))
				{
					var mainProjectDir = Directory.CreateDirectory(Path.Combine(hasNonmatchingProject.Path, "mainfwdir"));
					newFile = Path.Combine(mainProjectDir.FullName, "test.txt");
					File.WriteAllText(newFile, "some mainfwdir stuff");
					repo = new HgRepository(mainProjectDir.FullName, new NullProgress());
					repo.Init();
					repo.AddAndCheckinFile(newFile);

					var nestedLiftDir = Directory.CreateDirectory(Path.Combine(mainProjectDir.FullName, Utilities.OtherRepositories, Utilities.LIFT));
					newFile = Path.Combine(nestedLiftDir.FullName, "test.txt");
					File.WriteAllText(newFile, "some lifty stuff");
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
		public void HasNoExtantRepositories()
		{
			using (var hasProject = new TemporaryFolder("hasRepo"))
			{
				Utilities.SetProjectsPathForTests(hasProject.Path);
				try
				{
					var newFile = Path.Combine(hasProject.Path, "test.txt");
					File.WriteAllText(newFile, "some stuff");
					Assert.AreEqual(0, Utilities.ExtantRepoIdentifiers.Count);
				}
				finally
				{
					Utilities.SetProjectsPathForTests(null);
				}
			}
		}

		[Test]
		public void HasExtantRepositories()
		{
			using (var parentFolder = new TemporaryFolder("parentFolder"))
			using (var childFolder = new TemporaryFolder(parentFolder, "childFolder"))
			{
				Utilities.SetProjectsPathForTests(parentFolder.Path);
				try
				{
					var newFile = Path.Combine(childFolder.Path, "test.txt");
					File.WriteAllText(newFile, "some stuff");
					var repo = new HgRepository(childFolder.Path, new NullProgress());
					repo.Init();
					repo.AddAndCheckinFile(newFile);

					Assert.AreEqual(1, Utilities.ExtantRepoIdentifiers.Count);
					Assert.IsTrue(Utilities.ExtantRepoIdentifiers.ContainsKey(repo.Identifier));
					Assert.That(Utilities.ExtantRepoIdentifiers[repo.Identifier], Is.EqualTo("childFolder"));
				}
				finally
				{
					Utilities.SetProjectsPathForTests(null);
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