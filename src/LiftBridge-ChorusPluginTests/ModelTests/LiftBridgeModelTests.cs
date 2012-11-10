using System.IO;
using NUnit.Framework;
using SIL.LiftBridge.Model;

namespace LiftBridgeTests.ModelTests
{
	[TestFixture]
	public class LiftBridgeModelTests
	{
		private string _pathToProject;

		private LiftProject CreateNewbieProject()
		{
			return CreateNewbieProject("Newbie");
		}

		private LiftProject CreateNewbieProject(string baseDir)
		{
			var liftProject = new LiftProject(baseDir);
			_pathToProject = liftProject.PathToProject;
			Directory.CreateDirectory(_pathToProject);
			return liftProject;
		}

		[TearDown]
		public void TearDown()
		{
			if (_pathToProject != null && Directory.Exists(_pathToProject))
				Directory.Delete(_pathToProject, true);
			_pathToProject = null;
		}

		[Test]
		public void HasCorrectPathForProject()
		{
			CreateNewbieProject();
			Assert.IsTrue(_pathToProject.EndsWith("LIFT"));
			Assert.IsTrue(_pathToProject.Contains("OtherRepositories"));
		}

		[Test]
		[Category("UnknownMonoIssue")] // It insists on failing on mono, for some reason.
		public void ProjectHasLiftFile()
		{
			var liftProject = CreateNewbieProject();
			var liftPathname = Path.Combine(_pathToProject, "Newbie.lift");
			File.WriteAllText(liftPathname, "");
			Assert.AreEqual(liftPathname, liftProject.LiftPathname);
		}

		[Test]
		[Category("UnknownMonoIssue")] // It insists on failing on mono, for some reason.
		public void ProjectReturnsCorrectLiftFile()
		{
			var liftProject = CreateNewbieProject();
			var badLiftPathname = Path.Combine(_pathToProject, "Newbie.0.12.lift");
			File.WriteAllText(badLiftPathname, "");
			var goodLiftPathname = Path.Combine(_pathToProject, "Newbie.lift");
			File.WriteAllText(goodLiftPathname, "");
			Assert.AreEqual(goodLiftPathname, liftProject.LiftPathname);
		}

		[Test]
		[Category("UnknownMonoIssue")] // It insists on failing on mono, for some reason.
		public void ProjectReturnsOnlyLiftFileWhereFullPathnameHasAPeriodInIt()
		{
			var liftProject = CreateNewbieProject("With.Period");
			var goodLiftPathname = Path.Combine(_pathToProject, "Newbie.lift");
			File.WriteAllText(goodLiftPathname, "");
			Assert.AreEqual(goodLiftPathname, liftProject.LiftPathname);
		}

		[Test]
		public void ProjectHasNoLiftFile()
		{
			var liftProject = CreateNewbieProject();
			Assert.IsNull(liftProject.LiftPathname);
		}
	}
}
