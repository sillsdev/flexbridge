// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
﻿using System.IO;
﻿using NUnit.Framework;
using SIL.IO;
﻿using SIL.Progress;
﻿using LibFLExBridgeChorusPlugin.DomainServices;
using LibTriboroughBridgeChorusPlugin;

namespace LibFLExBridgeChorusPluginTests.Infrastructure
{
	[TestFixture]
	public class FLExProjectUnifierTests
	{

		[Test]
		public void NullPathnameForRestoreShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => FLExProjectUnifier.PutHumptyTogetherAgain(
				new NullProgress(), false, null));
		}

		[Test]
		public void EmptyPathnameForRestoreShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => FLExProjectUnifier.PutHumptyTogetherAgain(
				new NullProgress(), false, ""));
		}

		[Test]
		public void NonExistingFileForRestoreShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => FLExProjectUnifier.PutHumptyTogetherAgain(
				new NullProgress(), false, "Bogus" + LibTriboroughBridgeSharedConstants.FwXmlExtension));
		}

		[Test]
		public void NonExistantPathForRestoreShouldThrow()
		{
			using (var tempFile = new TempFile())
			{
				var pathname = tempFile.Path;
				Assert.Throws<ApplicationException>(() => FLExProjectUnifier.PutHumptyTogetherAgain(
					new NullProgress(), false, Path.Combine(pathname, "Itaintthere")));
			}
		}
	}
}
