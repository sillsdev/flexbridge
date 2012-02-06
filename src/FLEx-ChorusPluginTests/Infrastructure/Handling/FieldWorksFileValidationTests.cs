using System;
using System.IO;
using System.Linq;
using Chorus.FileTypeHanders;
using NUnit.Framework;
using Palaso.IO;
using Palaso.Progress.LogBox;

namespace FLEx_ChorusPluginTests.Infrastructure.Handling
{
	/// <summary>
	/// Make sure only the FieldWorks 7.0 xml file can be validated by the FieldWorksFileHandler class.
	/// </summary>
	[TestFixture]
	public class FieldWorksFileValidationTests
	{
		private IChorusFileTypeHandler _fileHandler;
		private TempFile _goodXmlTempFile;
		private TempFile _illformedXmlTempFile;
		private TempFile _goodXmlButNotFwTempFile;
		private TempFile _nonXmlTempFile;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_fileHandler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
							where handler.GetType().Name == "FieldWorksCommonFileHandler"
						 select handler).First();
			_goodXmlTempFile = TempFile.WithExtension(".ClassData");
			File.WriteAllText(_goodXmlTempFile.Path, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<classdata />");

			_illformedXmlTempFile = TempFile.WithExtension(".ClassData");
			File.WriteAllText(_illformedXmlTempFile.Path, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<classdata>");

			_goodXmlButNotFwTempFile = TempFile.WithExtension(".ClassData");
			File.WriteAllText(_goodXmlButNotFwTempFile.Path, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<nonfwstuff />");

			_nonXmlTempFile = TempFile.WithExtension(".txt");
			File.WriteAllText(_nonXmlTempFile.Path, "This is not an xml file." + Environment.NewLine);
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			_fileHandler = null;

			_goodXmlTempFile.Dispose();
			_goodXmlTempFile = null;
			_illformedXmlTempFile.Dispose();
			_illformedXmlTempFile = null;
			_goodXmlButNotFwTempFile.Dispose();
			_goodXmlButNotFwTempFile = null;
			_nonXmlTempFile.Dispose();
			_nonXmlTempFile = null;
		}

		[Test]
		public void Cannot_Validate_Nonexistant_File()
		{
			Assert.IsFalse(_fileHandler.CanValidateFile("bogusPathname"));
		}

		[Test]
		public void Cannot_Validate_Null_File()
		{
			Assert.IsFalse(_fileHandler.CanValidateFile(null));
		}

		[Test]
		public void Cannot_Validate_Empty_String_File()
		{
			Assert.IsFalse(_fileHandler.CanValidateFile(String.Empty));
		}

		[Test]
		public void Cannot_Validate_Nonxml_File()
		{
			Assert.IsFalse(_fileHandler.CanValidateFile(_nonXmlTempFile.Path));
		}

		[Test]
		public void Can_Validate_Fw_Xml_File()
		{
			Assert.IsTrue(_fileHandler.CanValidateFile(_goodXmlTempFile.Path));
		}

		[Test]
		public void ValidateFile_Returns_Message_For_Empty_Pathname()
		{
			Assert.IsNotNull(_fileHandler.ValidateFile("", new NullProgress()));
		}

		[Test]
		public void ValidateFile_Returns_Message_For_Null_Pathname()
		{
			Assert.IsNotNull(_fileHandler.ValidateFile(null, new NullProgress()));
		}

		[Test]
		public void ValidateFile_Returns_Null_For_Good_File()
		{
			Assert.IsNull(_fileHandler.ValidateFile(_goodXmlTempFile.Path, new NullProgress()));
		}

		[Test]
		public void ValidateFile_Returns_Message_For_Crummy_Xml_File()
		{
			Assert.IsNotNull(_fileHandler.ValidateFile(_illformedXmlTempFile.Path, new NullProgress()));
		}

		[Test]
		public void ValidateFile_Returns_Message_For_Good_But_Not_Fw_Xml_File()
		{
			Assert.IsNotNull(_fileHandler.ValidateFile(_goodXmlButNotFwTempFile.Path, new NullProgress()));
		}
	}
}