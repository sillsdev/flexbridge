// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.IO;
using NUnit.Framework;
using SIL.LiftBridge.Infrastructure;
using SIL.LiftBridge.Services;
using TriboroughBridge_ChorusPlugin;

namespace LiftBridgeTests.ServicesTests
{
	[TestFixture]
	public class FileAndDirectoryServicesTests
	{
		// FileAndDirectoryServices
		// EnumerateExtantFiles
		// WipeOutNewStuff
		private string _baseDir;
		private string _newBaseFolderPathname;
		private string _newAudioDirName;
		private string _newWsDirName;
		private string _newLdmlPathname;

		[OneTimeSetUp]
		public void FixtureSetup()
		{
			// Create some permanent data for the fixture.
			var tempSysFolder = Path.GetTempPath();
			_baseDir = Path.Combine(tempSysFolder, "FileServicesTestBase");
			Directory.CreateDirectory(_baseDir);
			// Add .hg folder and one file in it.
			Directory.CreateDirectory(Path.Combine(_baseDir, TriboroughBridgeUtilities.hg));
			// Add .git folder and one file in it.
			Directory.CreateDirectory(Path.Combine(_baseDir, LiftUtilties.git));

			// Create 'old' files and folders.
			_newBaseFolderPathname = Path.Combine(_baseDir, "somefile" + LiftUtilties.LiftExtension);
			File.Create(_newBaseFolderPathname).Close();
			_newAudioDirName = Path.Combine(_baseDir, "audio");
			Directory.CreateDirectory(_newAudioDirName);
			_newWsDirName = Path.Combine(_baseDir, "WritingSystems");
			Directory.CreateDirectory(_newWsDirName);
			_newLdmlPathname = Path.Combine(_newWsDirName, "new.ldml");
			File.Create(_newLdmlPathname).Close();
		}

		[OneTimeTearDown]
		public void FixtureTearDown()
		{
			// Wipe out fixture data, and everthing in it.
			Directory.Delete(_baseDir, true);
		}

		[Test]
		public void EnsureAllFileAndFoldersAreListed()
		{
			var allFilesAndDirs = FileAndDirectoryServices.EnumerateExtantFiles(_baseDir);

			Assert.IsFalse(allFilesAndDirs.Contains(Path.Combine(_baseDir, TriboroughBridgeUtilities.hg)));
			Assert.IsFalse(allFilesAndDirs.Contains(Path.Combine(_baseDir, LiftUtilties.git)));

			Assert.IsTrue(allFilesAndDirs.Contains(_newBaseFolderPathname));
			Assert.IsTrue(allFilesAndDirs.Contains(_newAudioDirName));
			Assert.IsTrue(allFilesAndDirs.Contains(_newLdmlPathname));
			Assert.IsTrue(allFilesAndDirs.Contains(_newWsDirName));

			EnsureOldStuffExists();
		}

		[Test]
		public void EnsureNewStuffIsGone()
		{
			var allBeforeFilesAndDirs = FileAndDirectoryServices.EnumerateExtantFiles(_baseDir);

			// Add some new stuff that is to get blown away
			var newFile1 = Path.Combine(_baseDir, "newFile.txt");
			File.Create(newFile1).Close();
			var newDir = Path.Combine(_baseDir, "NewDir");
			Directory.CreateDirectory(newDir);
			var newFileInNewDir = Path.Combine(newDir, "newFileInNewDir.txt");
			File.Create(newFileInNewDir).Close();
			var newLdmlFile = Path.Combine(_newWsDirName, "reallyNew.ldml");
			File.Create(newLdmlFile).Close();

			var allAfterFilesAndDirs = FileAndDirectoryServices.EnumerateExtantFiles(_baseDir);

			FileAndDirectoryServices.WipeOutNewStuff(allBeforeFilesAndDirs, allAfterFilesAndDirs);

			// Make sure new stuff is gone.
			Assert.IsFalse(File.Exists(newFile1));
			Assert.IsFalse(File.Exists(newLdmlFile));
			Assert.IsFalse(File.Exists(newFileInNewDir));
			Assert.IsFalse(Directory.Exists(newDir));

			// Make sure old stuff is still there.
			EnsureOldStuffExists();
		}

		private void EnsureOldStuffExists()
		{
			Assert.IsTrue(File.Exists(_newBaseFolderPathname));
			Assert.IsTrue(File.Exists(_newLdmlPathname));

			Assert.IsTrue(Directory.Exists(Path.Combine(_baseDir, TriboroughBridgeUtilities.hg)));
			Assert.IsTrue(Directory.Exists(Path.Combine(_baseDir, LiftUtilties.git)));
			Assert.IsTrue(Directory.Exists(_newAudioDirName));
			Assert.IsTrue(Directory.Exists(Path.Combine(_baseDir, TriboroughBridgeUtilities.hg)));
			Assert.IsTrue(Directory.Exists(Path.Combine(_baseDir, TriboroughBridgeUtilities.hg)));
			Assert.IsTrue(Directory.Exists(_newWsDirName));
		}
	}
}
