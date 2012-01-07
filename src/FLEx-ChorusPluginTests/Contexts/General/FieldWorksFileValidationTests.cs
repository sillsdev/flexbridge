using System;
using System.IO;
using System.Linq;
using Chorus.FileTypeHanders;
using NUnit.Framework;

namespace FLEx_ChorusPluginTests.Contexts.General
{
	/// <summary>
	/// Make sure only the FieldWorks 7.0 xml file can be validated by the FieldWorksFileHandler class.
	/// </summary>
	[TestFixture]
	public class FieldWorksFileValidationTests
	{
		private IChorusFileTypeHandler _fileHandler;
		private string _goodXmlPathname;
		private string _illformedXmlPathname;
		private string _goodXmlButNotFwPathname;
		private string _nonXmlPathname;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_fileHandler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
							where handler.GetType().Name == "FieldWorksCommonFileHandler"
						 select handler).First();
			_goodXmlPathname = Path.ChangeExtension(Path.GetTempFileName(), ".ClassData");
			File.WriteAllText(_goodXmlPathname, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<classdata />");
			_illformedXmlPathname = Path.ChangeExtension(Path.GetTempFileName(), ".ClassData");
			File.WriteAllText(_illformedXmlPathname, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<classdata>");
			_goodXmlButNotFwPathname = Path.ChangeExtension(Path.GetTempFileName(), ".ClassData");
			File.WriteAllText(_goodXmlButNotFwPathname, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<nonfwstuff />");
			_nonXmlPathname = Path.ChangeExtension(Path.GetTempFileName(), ".txt");
			File.WriteAllText(_nonXmlPathname, "This is not an xml file." + Environment.NewLine);
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			_fileHandler = null;
			if (File.Exists(_goodXmlPathname))
				File.Delete(_goodXmlPathname);
			if (File.Exists(_illformedXmlPathname))
				File.Delete(_illformedXmlPathname);
			if (File.Exists(_goodXmlButNotFwPathname))
				File.Delete(_goodXmlButNotFwPathname);
			if (File.Exists(_nonXmlPathname))
				File.Delete(_nonXmlPathname);
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
			Assert.IsFalse(_fileHandler.CanValidateFile(_nonXmlPathname));
		}

		[Test]
		public void Can_Validate_Fw_Xml_File()
		{
			Assert.IsTrue(_fileHandler.CanValidateFile(_goodXmlPathname));
		}

		[Test]
		public void ValidateFile_Returns_Message_For_Empty_Pathname()
		{
			Assert.IsNotNull(_fileHandler.ValidateFile("", null));
		}

		[Test]
		public void ValidateFile_Returns_Message_For_Null_Pathname()
		{
			Assert.IsNotNull(_fileHandler.ValidateFile(null, null));
		}

		[Test]
		public void ValidateFile_Returns_Null_For_Good_File()
		{
			Assert.IsNull(_fileHandler.ValidateFile(_goodXmlPathname, null));
		}

		[Test]
		public void ValidateFile_Returns_Message_For_Crummy_Xml_File()
		{
			Assert.IsNotNull(_fileHandler.ValidateFile(_illformedXmlPathname, null));
		}

		[Test]
		public void ValidateFile_Returns_Message_For_Good_But_Not_Fw_Xml_File()
		{
			Assert.IsNotNull(_fileHandler.ValidateFile(_goodXmlButNotFwPathname, null));
		}
	}
}