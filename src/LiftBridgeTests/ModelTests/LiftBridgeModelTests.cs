using System.IO;
using NUnit.Framework;
using SIL.LiftBridge.Model;

namespace LiftBridgeTests.ModelTests
{
	[TestFixture]
	public class LiftBridgeModelTests
	{
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
			var liftProject = new LiftProject("Newbie");
			Assert.IsTrue(LiftProjectServices.PathToProject(liftProject).EndsWith("Newbie"));
		}

		[Test]
		public void EnsureProjectIsNotShared()
		{
			var liftProject = new LiftProject("Newbie");
			Assert.IsFalse(LiftProjectServices.ProjectIsShared(liftProject));
		}

		[Test]
		public void EnsureProjectIsShared()
		{
			var liftProject = new LiftProject("Newbie");
			var pathToProj = LiftProjectServices.PathToProject(liftProject);
			var dirInfo = Directory.CreateDirectory(Path.Combine(pathToProj, ".hg"));
			try
			{
				Assert.IsTrue(LiftProjectServices.ProjectIsShared(liftProject));
			}
			finally
			{
				Directory.Delete(dirInfo.FullName);
			}
		}
	}
}
