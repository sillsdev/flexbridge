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
			var tempDataPathname = Path.Combine(tempCloneDir.Path, "dummy.lift");
			File.WriteAllText(tempDataPathname, "dummy data");
			repo.AddAndCheckinFile(tempDataPathname);

			// Add import failure file, but don't add it to the repo.
			var roadblockPathname = Path.Combine(tempCloneDir.Path, Utilities.FailureFilename);
			File.WriteAllText(roadblockPathname, "standard");

			var tempNewHomeDir = Path.Combine(tempFolderForOs, "FinalCloneHolder");

			try
			{
				Utilities.MakeLocalCloneAndRemoveSourceParentFolder(tempCloneDir.Path, tempNewHomeDir);
				Assert.IsFalse(Directory.Exists(tempCloneHolder.Path));
				Assert.IsTrue(File.Exists(Path.Combine(tempNewHomeDir, ".hg", "hgrc")));
				Assert.IsTrue(File.Exists(Path.Combine(tempNewHomeDir, "dummy.lift")));
				Assert.IsTrue(File.Exists(Path.Combine(tempNewHomeDir, Utilities.FailureFilename)));
			}
			finally
			{
				if (Directory.Exists(tempCloneHolder.Path))
					Directory.Delete(tempCloneHolder.Path, true);
				if (Directory.Exists(tempNewHomeDir))
					Directory.Delete(tempNewHomeDir, true);
			}
		}
	}
}