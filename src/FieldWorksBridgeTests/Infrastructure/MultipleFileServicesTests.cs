using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FieldWorksBridge.Infrastructure;
using NUnit.Framework;

namespace FieldWorksBridgeTests.Infrastructure
{
	[TestFixture]
	public class MultipleFileServicesTests
	{
		[Test]
		public void NullPathnameForBreakupShouldThrow()
		{
			Assert.Throws<ArgumentNullException>(() => MultipleFileServices.BreakupMainFile(null));
		}

		[Test]
		public void EmptyPathnameForBreakupShouldThrow()
		{
			Assert.Throws<ArgumentNullException>(() => MultipleFileServices.BreakupMainFile(""));
		}

		[Test]
		public void NonExistingFileForBreakupShouldThrow()
		{
			Assert.Throws<FileNotFoundException>(() => MultipleFileServices.BreakupMainFile("Bogus.fwdata"));
		}

		[Test]
		public void NullPathnameForRestoreShouldThrow()
		{
			Assert.Throws<ArgumentNullException>(() => MultipleFileServices.PutHumptyTogetherAgain(null));
		}

		[Test]
		public void EmptyPathnameForRestoreShouldThrow()
		{
			Assert.Throws<ArgumentNullException>(() => MultipleFileServices.PutHumptyTogetherAgain(""));
		}

		[Test]
		public void NonExistingFileForRestoreShouldThrow()
		{
			Assert.Throws<FileNotFoundException>(() => MultipleFileServices.PutHumptyTogetherAgain("Bogus.fwdata"));
		}
	}
}
