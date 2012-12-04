using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
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
			var tempCloneHolder = Path.Combine(tempFolderForOs, "TempCloneHolder");
			Directory.CreateDirectory(tempCloneHolder);
			var tempCloneDir = Path.Combine(tempCloneHolder, "TempClone");
			Directory.CreateDirectory(tempCloneDir);
			var tempCloneHgDir = Path.Combine(tempCloneDir, ".hg");
			Directory.CreateDirectory(tempCloneHgDir);
			var tempHgrcPathname = Path.Combine(tempCloneHgDir, "hgrc");
			File.WriteAllText(tempHgrcPathname, "dummy data");
			var tempDataPathname = Path.Combine(tempCloneDir, "dummy.lift");
			File.WriteAllText(tempDataPathname, "dummy data");

			var tempNewHomeDir = Path.Combine(tempFolderForOs, "FinalCloneHolder");
			Directory.CreateDirectory(tempNewHomeDir);


			try
			{
				Utilities.MoveRepository(tempCloneDir, tempNewHomeDir);
				Assert.IsFalse(Directory.Exists(tempCloneHolder));
				Assert.IsTrue(File.Exists(Path.Combine(tempNewHomeDir, ".hg", "hgrc")));
				Assert.IsTrue(File.Exists(Path.Combine(tempNewHomeDir, "dummy.lift")));
			}
			finally
			{
				if (Directory.Exists(tempCloneHolder))
					Directory.Delete(tempCloneHolder, true);
				if (Directory.Exists(tempNewHomeDir))
					Directory.Delete(tempNewHomeDir, true);
			}
		}
	}
}