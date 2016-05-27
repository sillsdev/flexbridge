// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: license.rtf file)

using System;
using Chorus.Utilities;
using LibFLExBridgeChorusPlugin.DomainServices;
using LibTriboroughBridgeChorusPlugin;
using NUnit.Framework;
using Palaso.IO;
using Palaso.Progress;

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
	}
}