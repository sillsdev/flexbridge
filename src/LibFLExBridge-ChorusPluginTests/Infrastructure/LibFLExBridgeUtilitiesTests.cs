// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.IO;
using LibChorus.TestUtilities;
using LibFLExBridgeChorusPlugin.Infrastructure;
using NUnit.Framework;
using SIL.TestUtilities;

namespace LibFLExBridgeChorusPluginTests.Infrastructure
{
	/// <summary>
	/// Test the FB Utilities class.
	/// </summary>
	[TestFixture]
	public class LibFLExBridgeUtilitiesTests
	{
		[Test]
		public void IsNotFlexProjectRepository()
		{
			using (var sueRepo = new RepositoryWithFilesSetup("Sue", "myNonFWData.lift", "<lift version='0.13' producer='test code' />"))
			{
				var fakeProjectDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
				Directory.CreateDirectory(fakeProjectDir);
				using (TemporaryFolder.TrackExisting(fakeProjectDir))
				{
					var extantDir = Path.Combine(fakeProjectDir, "extantmatchingrepo");
					Directory.CreateDirectory(extantDir);
					Directory.CreateDirectory(Path.Combine(fakeProjectDir, "norepo"));
					Assert.That(LibFLExBridgeUtilities.IsFlexProjectRepository(sueRepo.ProjectFolder.Path), Is.False);
				}
			}
		}

		[Test]
		public void IsFlexProjectRepository()
		{
			using (var sueRepo = new RepositoryWithFilesSetup("Sue", FlexBridgeConstants.CustomPropertiesFilename, "<AdditionalFields />"))
			{
				var fakeProjectDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
				Directory.CreateDirectory(fakeProjectDir);
				using (TemporaryFolder.TrackExisting(fakeProjectDir))
				{
					var extantDir = Path.Combine(fakeProjectDir, "extantmatchingrepo");
					Directory.CreateDirectory(extantDir);
					Directory.CreateDirectory(Path.Combine(fakeProjectDir, "norepo"));
					Assert.That(LibFLExBridgeUtilities.IsFlexProjectRepository(sueRepo.ProjectFolder.Path), Is.True);
				}
			}
		}
	}
}
