// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.IO;
using Chorus.Utilities;
using LibFLExBridgeChorusPlugin.Contexts;
using LibFLExBridgeChorusPlugin.DomainServices;
using LibTriboroughBridgeChorusPlugin;
using NUnit.Framework;
using SIL.IO;
using SIL.Progress;
using SIL.TestUtilities;

namespace LibFLExBridgeChorusPluginTests.Infrastructure
{
	[TestFixture]
	public class FLExProjectSplitterTests
	{
		[Test]
		public void NullPathnameForBreakupShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => FLExProjectSplitter.PushHumptyOffTheWall(new NullProgress(), false, null));
		}

		[Test]
		public void EmptyPathnameForBreakupShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => FLExProjectSplitter.PushHumptyOffTheWall(new NullProgress(), false, ""));
		}

		[Test]
		public void NonExistingFileForBreakupShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => FLExProjectSplitter.PushHumptyOffTheWall(new NullProgress(), false, "Bogus" + LibTriboroughBridgeSharedConstants.FwXmlExtension));
		}

		[Test]
		public void NotFwDataFileForBreakupShouldThrow()
		{
			using (var tempFile = new TempFile(""))
			{
				var pathname = tempFile.Path;
				Assert.Throws<ApplicationException>(() => FLExProjectSplitter.PushHumptyOffTheWall(new NullProgress(), false, pathname));
			}
		}

		[Test]
		public void UserCancelledBreakupShouldThrow()
		{
			using (var tempFile = TempFile.WithFilename("foo" + LibTriboroughBridgeSharedConstants.FwXmlExtension))
			{
				var progress = new NullProgress
				{
					CancelRequested = true
				};
				var pathname = tempFile.Path;
				Assert.Throws<UserCancelledException>(() => FLExProjectSplitter.PushHumptyOffTheWall(progress, false, pathname));
			}
		}

		[Test]
		public void CopySupportingSettingsFilesIntoRepo_WritingSystemsCopiedToCache()
		{
			// Setup a folder with a writing system to copy and an empty cache folder.
			using (var tempProjFolder = new TemporaryFolder("WritingSystemsCopiedToCache"))
			{
				var wsFolder = Path.Combine(tempProjFolder.Path, "WritingSystemStore");
				var wsCacheFolder = Path.Combine(tempProjFolder.Path, "CachedSettings", "WritingSystemStore");
				Directory.CreateDirectory(wsFolder);
				File.WriteAllText(Path.Combine(wsFolder, "en.ldml"), "<ldml/>");
				Assert.IsFalse(Directory.Exists(wsCacheFolder));
				// SUT
				BaseDomainServices.CopySupportingSettingsFilesIntoRepo(new NullProgress(), false, tempProjFolder.Path);
				// Verify that the writing system was copied into the cache.
				Assert.IsTrue(File.Exists(Path.Combine(wsCacheFolder, "en.ldml")));
			}
		}

		[Test]
		public void CopySupportingSettingsFilesIntoRepo_DeletedWritingSystemsRemovedFromCache()
		{
			// Setup an empty project writing system store and a cache containing one writing system.
			using (var tempProjFolder = new TemporaryFolder("DeletedWritingSystemsRemovedFromCache"))
			{
				var wsFolder = Path.Combine(tempProjFolder.Path, "WritingSystemStore");
				var wsCacheFolder = Path.Combine(tempProjFolder.Path, "CachedSettings", "WritingSystemStore");
				Directory.CreateDirectory(wsFolder);
				Directory.CreateDirectory(wsCacheFolder);
				File.WriteAllText(Path.Combine(wsCacheFolder, "en.ldml"), "<ldml/>");
				Assert.IsFalse(File.Exists(Path.Combine(wsFolder, "en.ldml")));
				// SUT
				BaseDomainServices.CopySupportingSettingsFilesIntoRepo(new NullProgress(), false, tempProjFolder.Path);
				// Verify that the writing system was deleted from the cache
				Assert.IsFalse(File.Exists(Path.Combine(wsCacheFolder, "en.ldml")));
			}
		}
	}
}
