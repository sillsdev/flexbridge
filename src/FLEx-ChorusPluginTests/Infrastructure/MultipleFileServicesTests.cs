using System;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using NUnit.Framework;
using Palaso.IO;

namespace FLEx_ChorusPluginTests.Infrastructure
{
	[TestFixture]
	public class MultipleFileServicesTests
	{
		[Test]
		public void NullPathnameForBreakupShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => MultipleFileServices.PushHumptyOffTheWall(null, "ZPI"));
		}

		[Test]
		public void EmptyPathnameForBreakupShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => MultipleFileServices.PushHumptyOffTheWall("", "ZPI"));
		}

		[Test]
		public void NonExistingFileForBreakupShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => MultipleFileServices.PushHumptyOffTheWall("Bogus.fwdata", "ZPI"));
		}

		[Test]
		public void NotFwDataFileForBreakupShouldThrow()
		{
			using (var tempFile = new TempFile(""))
			{
				Assert.Throws<ApplicationException>(() => MultipleFileServices.PushHumptyOffTheWall(tempFile.Path, "ZPI"));
			}
		}

		[Test]
		public void NullPathnameForRestoreShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => MultipleFileServices.PutHumptyTogetherAgain(null, "ZPI"));
		}

		[Test]
		public void EmptyPathnameForRestoreShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => MultipleFileServices.PutHumptyTogetherAgain("", "ZPI"));
		}

		[Test]
		public void NonExistingFileForRestoreShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => MultipleFileServices.PutHumptyTogetherAgain("Bogus.fwdata", "ZPI"));
		}

		[Test]
		public void NotFwDataFileForRestoreShouldThrow()
		{
			using (var tempFile = new TempFile(""))
			{
				Assert.Throws<ApplicationException>(() => MultipleFileServices.PutHumptyTogetherAgain(tempFile.Path, "ZPI"));
			}
		}
	}
}