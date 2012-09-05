﻿using System;
﻿using Chorus.Utilities;
﻿using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using NUnit.Framework;
using Palaso.IO;
using System.IO;
﻿using Palaso.Progress.LogBox;

namespace FLEx_ChorusPluginTests.Infrastructure
{
	[TestFixture]
	public class MultipleFileServicesTests
	{

		[Test]
		public void NullPathnameForBreakupShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => FLExProjectSplitter.PushHumptyOffTheWall(new NullProgress(), null));
		}

		[Test]
		public void EmptyPathnameForBreakupShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => FLExProjectSplitter.PushHumptyOffTheWall(new NullProgress(), ""));
		}

		[Test]
		public void NonExistingFileForBreakupShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => FLExProjectSplitter.PushHumptyOffTheWall(new NullProgress(), "Bogus.fwdata"));
		}

		[Test]
		public void NotFwDataFileForBreakupShouldThrow()
		{
			using (var tempFile = new TempFile(""))
			{
				var pathname = tempFile.Path;
				Assert.Throws<ApplicationException>(() => FLExProjectSplitter.PushHumptyOffTheWall(new NullProgress(), pathname));
			}
		}

		[Test]
		public void UserCancelledBreakupShouldThrow()
		{
			using (var tempFile = TempFile.WithFilename("foo.fwdata"))
			{
				var progress = new NullProgress
					{
						CancelRequested = true
					};
				var pathname = tempFile.Path;
				Assert.Throws<UserCancelledException>(() => FLExProjectSplitter.PushHumptyOffTheWall(progress, pathname));
			}
		}

		[Test]
		public void NullPathnameForRestoreShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => FLExProjectUnifier.PutHumptyTogetherAgain(new NullProgress(), null));
		}

		[Test]
		public void EmptyPathnameForRestoreShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => FLExProjectUnifier.PutHumptyTogetherAgain(new NullProgress(), ""));
		}

		[Test]
		public void NonExistingFileForRestoreShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => FLExProjectUnifier.PutHumptyTogetherAgain(new NullProgress(), "Bogus.fwdata"));
		}

		[Test]
		public void NonExistantPathForRestoreShouldThrow()
		{
			using (var tempFile = new TempFile())
			{
				var pathname = tempFile.Path;
				Assert.Throws<ApplicationException>(() => FLExProjectUnifier.PutHumptyTogetherAgain(new NullProgress(), Path.Combine(pathname, "Itaintthere")));
			}
		}
	}
}