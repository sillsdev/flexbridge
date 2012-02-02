using System;
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
			Assert.Throws<ApplicationException>(() => MultipleFileServices.BreakupMainFile(null, "ZPI"));
		}

		[Test]
		public void EmptyPathnameForBreakupShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => MultipleFileServices.BreakupMainFile("", "ZPI"));
		}

		[Test]
		public void NonExistingFileForBreakupShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => MultipleFileServices.BreakupMainFile("Bogus.fwdata", "ZPI"));
		}

		[Test]
		public void NotFwDataFileForBreakupShouldThrow()
		{
			using (var tempFile = new TempFile(""))
			{
				Assert.Throws<ApplicationException>(() => MultipleFileServices.BreakupMainFile(tempFile.Path, "ZPI"));
			}
		}

		[Test]
		public void NullPathnameForRestoreShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => MultipleFileServices.RestoreMainFile(null, "ZPI"));
		}

		[Test]
		public void EmptyPathnameForRestoreShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => MultipleFileServices.RestoreMainFile("", "ZPI"));
		}

		[Test]
		public void NonExistingFileForRestoreShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => MultipleFileServices.RestoreMainFile("Bogus.fwdata", "ZPI"));
		}

		[Test]
		public void NonExistantPathForRestoreShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => MultipleFileServices.RestoreMainFile(Path.Combine(new TempFile().Path, "Itaintthere") , "ZPI"));
		}
	}
}