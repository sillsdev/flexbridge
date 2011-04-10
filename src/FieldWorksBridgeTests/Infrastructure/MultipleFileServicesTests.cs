using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FieldWorksBridge.Infrastructure;
using NUnit.Framework;
using Palaso.IO;

namespace FieldWorksBridgeTests.Infrastructure
{
	[TestFixture]
	public class MultipleFileServicesTests
	{
		[Test]
		public void NullPathnameForBreakupShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => MultipleFileServices.BreakupMainFile(null));
		}

		[Test]
		public void EmptyPathnameForBreakupShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => MultipleFileServices.BreakupMainFile(""));
		}

		[Test]
		public void NonExistingFileForBreakupShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => MultipleFileServices.BreakupMainFile("Bogus.fwdata"));
		}

		[Test]
		public void NotFwDataFileForBreakupShouldThrow()
		{
			using (var tempFile = new TempFile(""))
			{
				Assert.Throws<ApplicationException>(() => MultipleFileServices.BreakupMainFile(tempFile.Path));
			}
		}

		[Test]
		public void NullPathnameForRestoreShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => MultipleFileServices.RestoreMainFile(null));
		}

		[Test]
		public void EmptyPathnameForRestoreShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => MultipleFileServices.RestoreMainFile(""));
		}

		[Test]
		public void NonExistingFileForRestoreShouldThrow()
		{
			Assert.Throws<ApplicationException>(() => MultipleFileServices.RestoreMainFile("Bogus.fwdata"));
		}

		[Test]
		public void NotFwDataFileForRestoreShouldThrow()
		{
			using (var tempFile = new TempFile(""))
			{
				Assert.Throws<ApplicationException>(() => MultipleFileServices.RestoreMainFile(tempFile.Path));
			}
		}
	}
}
