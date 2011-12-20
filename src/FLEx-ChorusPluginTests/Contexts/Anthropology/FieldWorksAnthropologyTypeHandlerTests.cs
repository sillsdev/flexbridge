using System;
using System.IO;
using System.Linq;
using Chorus.FileTypeHanders;
using NUnit.Framework;
using Palaso.IO;

namespace FLEx_ChorusPluginTests.Contexts.Anthropology
{
	[TestFixture]
	public class FieldWorksAnthropologyTypeHandlerTests
	{
		private IChorusFileTypeHandler _fileHandler;
		private ListenerForUnitTests _eventListener;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_fileHandler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
							where handler.GetType().Name == "FieldWorksAnthropologyTypeHandler"
							select handler).First();
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			_fileHandler = null;
		}

		[SetUp]
		public void TestSetup()
		{
			_eventListener = new ListenerForUnitTests();
		}

		[TearDown]
		public void TestTearDown()
		{
			_eventListener = null;
		}

		[Test]
		public void DescribeInitialContentsShouldHaveAddedForLabel()
		{
			var initialContents = _fileHandler.DescribeInitialContents(null, null);
			Assert.AreEqual(1, initialContents.Count());
			var onlyOne = initialContents.First();
			Assert.AreEqual("Added", onlyOne.ActionLabel);
		}

		[Test]
		public void ExtensionOfKnownFileTypesShouldBeReversal()
		{
			var extensions = _fileHandler.GetExtensionsOfKnownTextFileTypes().ToArray();
			Assert.AreEqual(1, extensions.Count(), "Wrong number of extensions.");
			Assert.AreEqual("ntbk", extensions[0]);
		}

		[Test]
		public void ShouldNotBeAbleToValidateIncorrectFormatFile()
		{
			using (var tempModelVersionFile = new TempFile("<classdata />"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, "ntbk");
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsFalse(_fileHandler.CanValidateFile(newpath));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldBeAbleToValidateInProperlyFormatedFile()
		{
			const string data = @"<Anthropology>
</Anthropology>";
			var testPathname = Path.Combine(Path.GetTempPath(), "DataNotebook.ntbk");
			File.WriteAllText(testPathname, data);
			try
			{
				Assert.IsTrue(_fileHandler.CanValidateFile(testPathname));
			}
			finally
			{
				File.Delete(testPathname);
			}
		}

		[Test]
		public void ShouldBeAbleToDoAllCanOperations()
		{
			const string data = @"<Anthropology>
</Anthropology>";
			var testPathname = Path.Combine(Path.GetTempPath(), "DataNotebook.ntbk");
			File.WriteAllText(testPathname, data);
			try
			{
				Assert.IsTrue(_fileHandler.CanValidateFile(testPathname));
				Assert.IsTrue(_fileHandler.CanDiffFile(testPathname));
				Assert.IsTrue(_fileHandler.CanMergeFile(testPathname));
				Assert.IsTrue(_fileHandler.CanPresentFile(testPathname));
			}
			finally
			{
				File.Delete(testPathname);
			}
		}

		[Test]
		public void ShouldNotBeAbleToValidateFile()
		{
			const string data = @"<classdata>
</classdata>";
			var testPathname = Path.Combine(Path.GetTempPath(), "DataNotebook.ntbk");
			File.WriteAllText(testPathname, data);
			try
			{
				Assert.IsNotNull(_fileHandler.ValidateFile(testPathname, null));
			}
			finally
			{
				File.Delete(testPathname);
			}
		}

		[Test]
		public void ShouldBeAbleToValidateFile()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<Anthropology>
</Anthropology>";
			var testPathname = Path.Combine(Path.GetTempPath(), "DataNotebook.ntbk");
			File.WriteAllText(testPathname, data);
			try
			{
				Assert.IsNull(_fileHandler.ValidateFile(testPathname, null));
			}
			finally
			{
				File.Delete(testPathname);
			}
		}
	}
}