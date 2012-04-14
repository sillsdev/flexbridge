﻿using System;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using NUnit.Framework;
using Palaso.IO;
using System.IO;

namespace FLEx_ChorusPluginTests.Infrastructure
{
	[TestFixture]
	public class MultipleFileServicesTests
	{

		[Test]
		public void NullPathnameForBreakupShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => new FLExProjectSplitter(null));
		}

		[Test]
		public void EmptyPathnameForBreakupShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => new FLExProjectSplitter(""));
		}

		[Test]
		public void NonExistingFileForBreakupShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => new FLExProjectSplitter("Bogus.fwdata"));
		}

		[Test]
		public void NotFwDataFileForBreakupShouldThrow()
		{
			using (var tempFile = new TempFile(""))
			{
				var pathname = tempFile.Path;
				Assert.Throws<ApplicationException>(() => new FLExProjectSplitter(pathname));
			}
		}

		[Test]
		public void NullPathnameForRestoreShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => new FLExProjectUnifier(null));
		}

		[Test]
		public void EmptyPathnameForRestoreShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => new FLExProjectUnifier(""));
		}

		[Test]
		public void NonExistingFileForRestoreShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => new FLExProjectUnifier("Bogus.fwdata"));
		}

		[Test]
		public void NonExistantPathForRestoreShouldThrow()
		{
			using (var tempFile = new TempFile())
			{
				var pathname = tempFile.Path;
				Assert.Throws<ApplicationException>(() => new FLExProjectUnifier(Path.Combine(pathname, "Itaintthere")));
			}
		}
	}
}