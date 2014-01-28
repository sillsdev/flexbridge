// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using LibChorus.TestUtilities;
using NUnit.Framework;
using Palaso.TestUtilities;
using SIL.LiftBridge.Infrastructure.ActionHandlers;
using TriboroughBridge_ChorusPlugin;

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
			using (var sueRepo = new RepositoryWithFilesSetup("SueForLift", "Sue.lift", "contents"))
			{
				var fakeProjectDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
				Directory.CreateDirectory(fakeProjectDir);
				using (var tempDir = TemporaryFolder.TrackExisting(fakeProjectDir))
				{
					var extantDir = Path.Combine(fakeProjectDir, "extantmatchingrepo", Utilities.OtherRepositories, Utilities.LIFT);
					Directory.CreateDirectory(extantDir);
					Directory.CreateDirectory(Path.Combine(fakeProjectDir, "norepowithoffset", Utilities.OtherRepositories, Utilities.LIFT));
					Directory.CreateDirectory(Path.Combine(fakeProjectDir, "noreposansoffset"));
					var strat = new ObtainProjectStrategyLift();
					Assert.IsTrue(strat.ProjectFilter(sueRepo.ProjectFolder.Path));
				}
			}
		}
	}
}