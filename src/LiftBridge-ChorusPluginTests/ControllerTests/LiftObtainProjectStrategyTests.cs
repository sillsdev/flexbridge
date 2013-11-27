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
using SIL.LiftBridge.Controller;
using TriboroughBridge_ChorusPlugin;

namespace LiftBridgeTests.ControllerTests
{
	/// <summary>
	/// Test the LiftObtainProjectStrategy.
	/// </summary>
	[TestFixture]
	public class LiftObtainProjectStrategyTests
	{
		[Test]
		public void AlreadyHaveProjectFiltersOutAttemptToCloneAgain()
		{
			using (var sueRepo = new RepositoryWithFilesSetup("SueForLift", "Sue.lift", "contents"))
			{
				var fakeProjectDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
				Utilities.SetProjectsPathForTests(fakeProjectDir);
				try
				{
					using (var tempDir = TemporaryFolder.TrackExisting(fakeProjectDir))
					{
						var sue = sueRepo.GetRepository();
						var extantDir = Path.Combine(fakeProjectDir, "extantmatchingrepo", Utilities.OtherRepositories, Utilities.LIFT);
						Directory.CreateDirectory(extantDir);
						Directory.CreateDirectory(Path.Combine(fakeProjectDir, "norepo"));
						sue.CloneLocalWithoutUpdate(extantDir);
						var strat = new LiftObtainProjectStrategy();
						Assert.IsFalse(strat.ProjectFilter(sueRepo.ProjectFolder.Path));
					}
				}
				finally
				{
					Utilities.SetProjectsPathForTests(null);
				}
			}
		}

		[Test]
		public void DoNotHaveProjectDoesNotFilterOutRepo()
		{
			using (var sueRepo = new RepositoryWithFilesSetup("SueForLift", "Sue.lift", "contents"))
			{
				var fakeProjectDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
				Utilities.SetProjectsPathForTests(fakeProjectDir);
				try
				{
					using (var tempDir = TemporaryFolder.TrackExisting(fakeProjectDir))
					{
						var extantDir = Path.Combine(fakeProjectDir, "extantmatchingrepo", Utilities.OtherRepositories, Utilities.LIFT);
						Directory.CreateDirectory(extantDir);
						Directory.CreateDirectory(Path.Combine(fakeProjectDir, "norepowithoffset", Utilities.OtherRepositories, Utilities.LIFT));
						Directory.CreateDirectory(Path.Combine(fakeProjectDir, "noreposansoffset"));
						var strat = new LiftObtainProjectStrategy();
						Assert.IsTrue(strat.ProjectFilter(sueRepo.ProjectFolder.Path));
					}
				}
				finally
				{
					Utilities.SetProjectsPathForTests(null);
				}
			}
		}
	}
}