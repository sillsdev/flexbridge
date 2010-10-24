using FieldWorksBridge.Infrastructure;
using NUnit.Framework;

namespace FieldWorksBridgeTests.Infrastructure
{
	/// <summary>
	/// Test the DeveloperSystemProjectPathLocator implementation of IProjectPathLocator
	/// to make sure it includes the DistFiles Project file and the default installation (Vista & Windows 7) path.
	/// </summary>
	[TestFixture]
	public class DeveloperSystemProjectPathLocatorTests
	{
		[Test]
		public void EnsureTwoMainPathsReturned()
		{
			var devPathLocator = new DeveloperSystemProjectPathLocator();
			var paths = devPathLocator.BaseFolderPaths;
			Assert.AreEqual(2, paths.Count);
			Assert.IsTrue(paths.Contains(FieldWorksProjectServices.StandardInstallDir));
			Assert.IsTrue(paths.Contains(FieldWorksProjectServices.ProjectsPath));
		}
	}
}
