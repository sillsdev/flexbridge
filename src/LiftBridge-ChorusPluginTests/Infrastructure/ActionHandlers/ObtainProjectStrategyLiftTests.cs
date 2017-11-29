// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.IO;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.TestUtilities;
using SIL.LiftBridge.Infrastructure.ActionHandlers;
using LibTriboroughBridgeChorusPlugin;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Infrastructure;

namespace LiftBridgeTests.Infrastructure.ActionHandlers
{
	/// <summary>
	/// Test the LiftObtainProjectStrategy.
	/// </summary>
	[TestFixture]
	public class ObtainProjectStrategyLiftTests
	{
		[Test]
		public void DoNotHaveProjectDoesNotFilterOutRepo()
		{
			using (var sueRepo = new RepositoryWithFilesSetup("SueForLift", "Sue.lift", "<bogusliftstuff />"))
			{
				var fakeProjectDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
				Directory.CreateDirectory(fakeProjectDir);
				using (TemporaryFolder.TrackExisting(fakeProjectDir))
				{
					var extantDir = Path.Combine(fakeProjectDir, "extantmatchingrepo",
						LibTriboroughBridgeSharedConstants.OtherRepositories, TriboroughBridgeUtilities.LIFT);
					Directory.CreateDirectory(extantDir);
					Directory.CreateDirectory(Path.Combine(fakeProjectDir, "norepowithoffset",
						LibTriboroughBridgeSharedConstants.OtherRepositories, TriboroughBridgeUtilities.LIFT));
					Directory.CreateDirectory(Path.Combine(fakeProjectDir, "noreposansoffset"));
					IObtainProjectStrategy strat = new ObtainProjectStrategyLift();
					Assert.IsTrue(strat.ProjectFilter(sueRepo.ProjectFolder.Path));
				}
			}
		}
	}
}