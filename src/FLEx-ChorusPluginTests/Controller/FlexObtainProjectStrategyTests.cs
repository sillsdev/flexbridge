using System;
using System.IO;
using FLEx_ChorusPlugin.Controller;
using FLEx_ChorusPlugin.Infrastructure;
using LibChorus.TestUtilities;
using NUnit.Framework;
using Palaso.TestUtilities;
using TriboroughBridge_ChorusPlugin;

namespace FLEx_ChorusPluginTests.Controller
{
	/// <summary>
	/// Test the FlexObtainProjectStrategy.
	/// </summary>
	[TestFixture]
	public class FlexObtainProjectStrategyTests
	{
		[Test]
		public void AlreadyHaveProjectFiltersOutAttemptToCloneAgain()
		{
			using (var sueRepo = new RepositoryWithFilesSetup("Sue", SharedConstants.CustomPropertiesFilename, "contents"))
			{
				var fakeProjectDir = Path.Combine(Utilities.ProjectsPath, Guid.NewGuid().ToString());
				using (var tempDir = TemporaryFolder.TrackExisting(fakeProjectDir))
				{
					var sue = sueRepo.GetRepository();
					sue.CloneLocalWithoutUpdate(fakeProjectDir);
					var strat = new FlexObtainProjectStrategy();
					Assert.IsFalse(strat.ProjectFilter(sueRepo.ProjectFolder.Path));
				}
			}
		}
	}
}