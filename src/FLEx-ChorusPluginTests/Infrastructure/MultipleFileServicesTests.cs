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
				Assert.Throws<ApplicationException>(() => new FLExProjectSplitter(tempFile.Path));
			}
		}

		[Test]
		public void NullPathnameForRestoreShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => MultipleFileServices.PutHumptyTogetherAgain(null));
		}

		[Test]
		public void EmptyPathnameForRestoreShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => MultipleFileServices.PutHumptyTogetherAgain(""));
		}

		[Test]
		public void NonExistingFileForRestoreShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => MultipleFileServices.PutHumptyTogetherAgain("Bogus.fwdata"));
		}

		[Test]
		public void NonExistantPathForRestoreShouldThrow()
		{
			using (var tempFile = new TempFile())
			{
				Assert.Throws<ApplicationException>(() => MultipleFileServices.PutHumptyTogetherAgain(Path.Combine(tempFile.Path, "Itaintthere")));
			}
		}
	}
}