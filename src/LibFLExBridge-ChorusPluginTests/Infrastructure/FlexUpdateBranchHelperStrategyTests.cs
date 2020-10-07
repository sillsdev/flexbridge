// Copyright (c) 2015-16 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Globalization;
using System.Threading;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibTriboroughBridgeChorusPlugin.Infrastructure;
using NUnit.Framework;

namespace LibFLExBridgeChorusPluginTests.Infrastructure
{
	[TestFixture]
	class FlexUpdateBranchHelperStrategyTests
	{
        [Test]
		[SetCulture("sv-SE")]
		public void GetModelVersionFromBranchName_HandlesScandinavianCulture()
		{
			const string branchName = "75003.74001";
			var flexUpdateBranchHelper = new FlexUpdateBranchHelperStrategy();
			double modelVersion = 0;
            Assert.DoesNotThrow(()=> modelVersion = ((IUpdateBranchHelperStrategy)flexUpdateBranchHelper).GetModelVersionFromBranchName(branchName));
            Assert.That(modelVersion, Is.EqualTo(75003.74001));
		}
	}
}
