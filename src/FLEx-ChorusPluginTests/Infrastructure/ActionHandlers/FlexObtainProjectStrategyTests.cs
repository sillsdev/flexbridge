﻿// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using FLEx_ChorusPlugin.Infrastructure;
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
		private RepositoryWithFilesSetup _sueRepo;
		private TemporaryFolder _tempDir;

		[SetUp]
		public void Setup()
		{
			_sueRepo = new RepositoryWithFilesSetup("Sue",
				SharedConstants.CustomPropertiesFilename, "<AdditionalFields></AdditionalFields>");

			var fakeProjectDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			_tempDir = TemporaryFolder.TrackExisting(fakeProjectDir);
		}

		[TearDown]
		public void TearDown()
		{
			_tempDir.Dispose();
			_sueRepo.Dispose();
		}

		[Test]
		public void AlreadyHaveProjectFiltersOutRepo()
		{
			// Setup
			var repo = _sueRepo.GetRepository();
			var extantDir = Path.Combine(_tempDir.Path, "extantmatchingrepo");
			Directory.CreateDirectory(extantDir);
			repo.CloneLocalWithoutUpdate(extantDir);
			var sut = new ObtainProjectStrategyFlex();

			// Exercise/Verify
			Assert.IsTrue(sut.ProjectFilter(extantDir));
		}

		[Test]
		public void DoNotHaveProjectDoesNotFilterOutRepo()
		{
			// Setup
			var extantDir = Path.Combine(_tempDir.Path, "extantmatchingrepo");
			Directory.CreateDirectory(extantDir);
			var sut = new ObtainProjectStrategyFlex();

			// Exercise/Verify
			Assert.IsFalse(sut.ProjectFilter(extantDir));
		}
	}
}