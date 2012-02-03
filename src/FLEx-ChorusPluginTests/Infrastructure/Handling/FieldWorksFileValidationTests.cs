using System;
using System.IO;
using System.Linq;
using Chorus.FileTypeHanders;
using NUnit.Framework;
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
		private string _goodXmlPathname;
		private string _illformedXmlPathname;
		private string _goodXmlButNotFwPathname;
		private string _nonXmlPathname;
		private string _tempfilePathname1;
		private string _tempfilePathname2;
		private string _tempfilePathname3;
		private string _tempfilePathname4;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_fileHandler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
							where handler.GetType().Name == "FieldWorksCommonFileHandler"
						 select handler).First();
			_tempfilePathname1 = Path.GetTempFileName();
			_goodXmlPathname = Path.ChangeExtension(_tempfilePathname1, ".ClassData");
			File.WriteAllText(_goodXmlPathname, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<classdata />");

			_tempfilePathname2 = Path.GetTempFileName();
			_illformedXmlPathname = Path.ChangeExtension(_tempfilePathname2, ".ClassData");
			File.WriteAllText(_illformedXmlPathname, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<classdata>");

			_tempfilePathname3 = Path.GetTempFileName();
			_goodXmlButNotFwPathname = Path.ChangeExtension(_tempfilePathname3, ".ClassData");
			File.WriteAllText(_goodXmlButNotFwPathname, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<nonfwstuff />");

			_tempfilePathname4 = Path.GetTempFileName();
			_nonXmlPathname = Path.ChangeExtension(_tempfilePathname4, ".txt");
			File.WriteAllText(_nonXmlPathname, "This is not an xml file." + Environment.NewLine);
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			_fileHandler = null;

			if (File.Exists(_tempfilePathname1))
				File.Delete(_tempfilePathname1);
			if (File.Exists(_tempfilePathname2))
				File.Delete(_tempfilePathname2);
			if (File.Exists(_tempfilePathname3))
				File.Delete(_tempfilePathname3);
			if (File.Exists(_tempfilePathname4))
				File.Delete(_tempfilePathname4);

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
			Assert.IsNull(_fileHandler.ValidateFile(_goodXmlPathname, new NullProgress()));
		}

		[Test]
		public void ValidateFile_Returns_Message_For_Crummy_Xml_File()
		{
			Assert.IsNotNull(_fileHandler.ValidateFile(_illformedXmlPathname, new NullProgress()));
		}

		[Test]
		public void ValidateFile_Returns_Message_For_Good_But_Not_Fw_Xml_File()
		{
			Assert.IsNotNull(_fileHandler.ValidateFile(_goodXmlButNotFwPathname, new NullProgress()));
		}
	}
}