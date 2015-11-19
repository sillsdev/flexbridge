// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using LibFLExBridgeChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.ActionHandlers;
using LibChorus.TestUtilities;
using NUnit.Framework;
using Palaso.TestUtilities;

namespace FLEx_ChorusPluginTests.Infrastructure.ActionHandlers
{
	/// <summary>
	/// Test the ObtainProjectStrategyFlex.
	/// </summary>
	[TestFixture]
	public class FlexObtainProjectStrategyTests
	{
		[Test, Ignore("Not doing it with the filter for now.")]
		public void AlreadyHaveProjectFiltersOutAttemptToCloneAgain()
		{
			using (var sueRepo = new RepositoryWithFilesSetup("Sue", FlexBridgeConstants.CustomPropertiesFilename, "contents"))
			{
				var fakeProjectDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
				using (var tempDir = TemporaryFolder.TrackExisting(fakeProjectDir))
				{
					var sue = sueRepo.GetRepository();
					var extantDir = Path.Combine(fakeProjectDir, "extantmatchingrepo");
					Directory.CreateDirectory(extantDir);
					Directory.CreateDirectory(Path.Combine(fakeProjectDir, "norepo"));
					sue.CloneLocalWithoutUpdate(extantDir);
					var strat = new ObtainProjectStrategyFlex();
					Assert.IsFalse(strat.ProjectFilter(sueRepo.ProjectFolder.Path));
				}
			}
		}

		[Test]
		public void DoNotHaveProjectDoesNotFilterOutRepo()
		{
			using (var sueRepo = new RepositoryWithFilesSetup("Sue", FlexBridgeConstants.CustomPropertiesFilename, "contents"))
			{
				var fakeProjectDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
				Directory.CreateDirectory(fakeProjectDir);
				using (var tempDir = TemporaryFolder.TrackExisting(fakeProjectDir))
				{
					var extantDir = Path.Combine(fakeProjectDir, "extantmatchingrepo");
					Directory.CreateDirectory(extantDir);
					Directory.CreateDirectory(Path.Combine(fakeProjectDir, "norepo"));
					var strat = new ObtainProjectStrategyFlex();
					Assert.IsTrue(strat.ProjectFilter(sueRepo.ProjectFolder.Path));
				}
			}
		}
	}
}