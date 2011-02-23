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
			var liftProject = new LiftProject("Newbie");
			_pathToProject = LiftProjectServices.PathToProject(liftProject);
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
		public void ProjectHasCorrectName()
		{
			var liftProject = new LiftProject("Newbie");
			Assert.AreEqual("Newbie", liftProject.LiftProjectName);
		}

		[Test]
		public void HasCorrectBasePathForProject()
		{
			Assert.IsTrue(LiftProjectServices.BasePath.EndsWith("LiftBridge"));
		}

		[Test]
		public void HasCorrectPathForProject()
		{
			CreateNewbieProject();
			Assert.IsTrue(_pathToProject.EndsWith("Newbie"));
		}

		[Test]
		public void EnsureProjectIsNotShared()
		{
			var liftProject = CreateNewbieProject();
			Assert.IsFalse(LiftProjectServices.ProjectIsShared(liftProject));
		}

		[Test]
		public void EnsureProjectIsShared()
		{
			var liftProject = CreateNewbieProject();
			Directory.CreateDirectory(Path.Combine(_pathToProject, ".hg"));
			Assert.IsTrue(LiftProjectServices.ProjectIsShared(liftProject));
		}

		[Test]
		public void ProjectHasLiftFile()
		{
			var liftProject = CreateNewbieProject();
			var liftPathname = Path.Combine(_pathToProject, "Newbie.lift");
			File.WriteAllText(liftPathname, "");
			Assert.AreEqual(liftPathname, liftProject.LiftPathname);
		}

		[Test]
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
		public void ProjectHasNoLiftFile()
		{
			var liftProject = CreateNewbieProject();
			Assert.IsNull(liftProject.LiftPathname);
		}
	}
}
