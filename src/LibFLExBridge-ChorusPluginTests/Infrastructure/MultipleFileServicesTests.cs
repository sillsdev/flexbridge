// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

﻿using System;
﻿using System.IO;
﻿using Chorus.Utilities;
using NUnit.Framework;
using Palaso.IO;
﻿using Palaso.Progress;
using LibFLExBridgeChorusPlugin;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibTriboroughBridgeChorusPlugin;

namespace LibFLExBridgeChorusPluginTests.Infrastructure
{
	[TestFixture]
	public class MultipleFileServicesTests
	{

		[Test]
		public void NullPathnameForBreakupShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => FLExProjectSplitter.PushHumptyOffTheWall(
				new NullProgress(), null));
		}

		[Test]
		public void EmptyPathnameForBreakupShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => FLExProjectSplitter.PushHumptyOffTheWall(
				new NullProgress(), ""));
		}

		[Test]
		public void NonExistingFileForBreakupShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => FLExProjectSplitter.PushHumptyOffTheWall(
				new NullProgress(), "Bogus" + SharedConstants.FwXmlExtension));
		}

		[Test]
		public void NotFwDataFileForBreakupShouldThrow()
		{
			using (var tempFile = new TempFile(""))
			{
				var pathname = tempFile.Path;
				Assert.Throws<ApplicationException>(() => FLExProjectSplitter.PushHumptyOffTheWall(
					new NullProgress(), pathname));
			}
		}

		[Test]
		public void UserCancelledBreakupShouldThrow()
		{
			using (var tempFile = TempFile.WithFilename("foo" + SharedConstants.FwXmlExtension))
			{
				var progress = new NullProgress
					{
						CancelRequested = true
					};
				var pathname = tempFile.Path;
				Assert.Throws<UserCancelledException>(() => FLExProjectSplitter.PushHumptyOffTheWall(
					progress, pathname));
			}
		}

		[Test]
		public void NullPathnameForRestoreShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => FLExProjectUnifier.PutHumptyTogetherAgain(
				new NullProgress(), null));
		}

		[Test]
		public void EmptyPathnameForRestoreShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => FLExProjectUnifier.PutHumptyTogetherAgain(
				new NullProgress(), ""));
		}

		[Test]
		public void NonExistingFileForRestoreShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => FLExProjectUnifier.PutHumptyTogetherAgain(
				new NullProgress(), "Bogus" + SharedConstants.FwXmlExtension));
		}

		[Test]
		public void NonExistantPathForRestoreShouldThrow()
		{
			using (var tempFile = new TempFile())
			{
				var pathname = tempFile.Path;
				Assert.Throws<ApplicationException>(() => FLExProjectUnifier.PutHumptyTogetherAgain(
					new NullProgress(), Path.Combine(pathname, "Itaintthere")));
			}
		}
	}
}