using System;
using System.IO;
using FieldWorksBridge.Model;
using NUnit.Framework;

namespace FieldWorksBridgeTests.Model
{
	/// <summary>
	/// Test the LanguageProject class.
	/// </summary>
	[TestFixture]
	public class LanguageProjectTests
	{
		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void NullPathnameThrows()
		{
			new LanguageProject(null);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void EmptyPathnameThrows()
		{
			new LanguageProject(string.Empty);
		}

		[Test, ExpectedException(typeof(FileNotFoundException))]
		public void NonExistantFileThrows()
		{
			new LanguageProject("NobodyHome");
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void NonFwFileThrows()
		{
			var tempFile = Path.GetTempFileName();
			try
			{
				new LanguageProject(tempFile);
			}
			finally
			{
				File.Delete(tempFile);
			}
		}

		[Test]
		public void FwFileHasFolderPath()
		{
			var temp = Path.GetTempFileName();
			var tempFile = Path.ChangeExtension(temp, ".fwdata");
			File.Move(temp, tempFile);
			try
			{
				var lp = new LanguageProject(tempFile);
				Assert.AreEqual(Path.GetDirectoryName(tempFile), lp.DirectoryName);
			}
			finally
			{
				File.Delete(tempFile);
			}
		}

		[Test]
		public void ProjectHasCorrectName()
		{
			var temp = Path.GetTempFileName();
			var tempFile = Path.ChangeExtension(temp, ".fwdata");
			File.Move(temp, tempFile);
			try
			{
				var lp = new LanguageProject(tempFile);
				Assert.AreEqual(Path.GetDirectoryName(tempFile), lp.DirectoryName);

				var fileName = Path.GetFileNameWithoutExtension(tempFile);
				Assert.AreEqual(fileName, lp.Name);
			}
			finally
			{
				File.Delete(tempFile);
			}
		}

		[Test]
		public void LockedProjectIsInUse()
		{
			var tempFolder = Path.GetTempPath();
			var tempDir = Directory.CreateDirectory(Path.Combine(tempFolder, "FWBTest"));
			try
			{
				var fwdataFile = Path.Combine(tempDir.FullName, "test.fwdata");
				File.WriteAllText(fwdataFile, "");

				var lp = new LanguageProject(fwdataFile);
				Assert.IsFalse(lp.FieldWorkProjectInUse);
				var lockedFwdataFile = fwdataFile + ".lock";
				File.WriteAllText(lockedFwdataFile, "");
				Assert.IsTrue(lp.FieldWorkProjectInUse);
			}
			finally
			{
				Directory.Delete(tempDir.FullName, true);
			}
		}

		[Test]
		public void NameIsSameAsToString()
		{
			var temp = Path.GetTempFileName();
			var tempFile = Path.ChangeExtension(temp, ".fwdata");
			File.Move(temp, tempFile);
			try
			{
				var lp = new LanguageProject(tempFile);
				Assert.AreEqual(Path.GetFileNameWithoutExtension(tempFile), lp.ToString());

				var fileName = Path.GetFileNameWithoutExtension(tempFile);
				Assert.AreEqual(fileName, lp.ToString());
			}
			finally
			{
				File.Delete(tempFile);
			}
		}
	}
}