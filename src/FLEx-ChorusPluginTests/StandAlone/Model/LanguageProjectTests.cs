using System;
using System.IO;
using FLEx_ChorusPlugin.StandAlone.Model;
using NUnit.Framework;
using Palaso.IO;
using TriboroughBridge_ChorusPlugin;

namespace FLEx_ChorusPluginTests.StandAlone.Model
{
	/// <summary>
	/// Test the LanguageProject class.
	/// </summary>
	[TestFixture]
	public class LanguageProjectTests
	{
		[Test]
		public void NullPathnameThrows()
		{
			Assert.Throws<ArgumentNullException>(() => new LanguageProject(null));
		}

		[Test]
		public void EmptyPathnameThrows()
		{
			Assert.Throws<ArgumentNullException>(() => new LanguageProject(string.Empty));
		}

		[Test]
		public void NonExistantFileThrows()
		{
			Assert.Throws<FileNotFoundException>(() => new LanguageProject("NobodyHome"));
		}

		[Test]
		public void NonFwFileThrows()
		{
			using (var tempFile = new TempFile())
			{
				Assert.Throws<ArgumentException>(() => new LanguageProject(tempFile.Path));
			}
		}

		[Test]
		public void FwFileHasFolderPath()
		{
			using (var tempFile = TempFile.WithExtension(Utilities.FwXmlExtension))
			{
				var lp = new LanguageProject(tempFile.Path);
				Assert.AreEqual(Path.GetDirectoryName(tempFile.Path), lp.DirectoryName);
			}
		}

		[Test]
		public void ProjectHasCorrectName()
		{
			using (var tempFile = TempFile.WithExtension(Utilities.FwXmlExtension))
			{
				var lp = new LanguageProject(tempFile.Path);
				Assert.AreEqual(Path.GetDirectoryName(tempFile.Path), lp.DirectoryName);

				var fileName = Path.GetFileNameWithoutExtension(tempFile.Path);
				Assert.AreEqual(fileName, lp.Name);
			}
		}

		[Test]
		public void LockedProjectIsInUse()
		{
			var tempFolder = Path.GetTempPath();
			var tempDir = Directory.CreateDirectory(Path.Combine(tempFolder, "FWBTest"));
			try
			{
				var fwdataFile = Path.Combine(tempDir.FullName, "test" + Utilities.FwXmlExtension);
				File.WriteAllText(fwdataFile, "");

				var lp = new LanguageProject(fwdataFile);
				Assert.IsFalse(lp.FieldWorkProjectInUse);
				var lockedFwdataFile = fwdataFile + Utilities.FwLockExtension;
				File.WriteAllText(lockedFwdataFile, "");
				Assert.IsTrue(lp.FieldWorkProjectInUse);
			}
			finally
			{
				// Bad idea!! Directory.Delete(tempFolder, true);
				Directory.Delete(tempDir.FullName, true);
			}
		}

		[Test]
		public void NameIsSameAsToString()
		{
			using (var tempFile = TempFile.WithExtension(Utilities.FwXmlExtension))
			{
				var lp = new LanguageProject(tempFile.Path);
				Assert.AreEqual(Path.GetFileNameWithoutExtension(tempFile.Path), lp.ToString());

				var fileName = Path.GetFileNameWithoutExtension(tempFile.Path);
				Assert.AreEqual(fileName, lp.ToString());
			}
		}
	}
}