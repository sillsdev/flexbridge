// --------------------------------------------------------------------------------------------
// Copyright (C) 2016 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Reflection;
using FLEx_ChorusPlugin.Infrastructure;
using NUnit.Framework;
using Palaso.IO;
using Palaso.Progress;
using Palaso.TestUtilities;
using TriboroughBridge_ChorusPlugin;

namespace FLEx_ChorusPluginTests.Infrastructure.Handling.ConfigLayout
{
	[TestFixture]
	public class DictionaryConfigurationHandlerStrategyTests : BaseFieldWorksTypeHandlerTests
	{
		private TempFile _configFile;
		private string _xsdSourcePath;

		[TestFixtureSetUp]
		public override void FixtureSetup()
		{
			// Copy the schema file to where the Strategy looks
			var appsDir = Path.GetDirectoryName(Utilities.StripFilePrefix(Assembly.GetExecutingAssembly().CodeBase));
			_xsdSourcePath = Path.Combine(appsDir, "TestData", "Language Explorer", "Configuration", SharedConstants.DictConfigSchemaFilename);
		}

		[TestFixtureTearDown]
		public override void FixtureTearDown() {} // mask base.FixtureTearDown (it is called by this.TestTearDown)

		[SetUp]
		public override void TestSetup()
		{
			// set up the fixture for each test, so each starts with a fresh HandlerStrategy, because DictionaryConfigurationHandlerStrategy
			// caches the path to the schema, but some tests require the schema's presence and others require its absence.
			base.FixtureSetup();
			base.TestSetup();
			_configFile = TempFile.WithExtension("." + SharedConstants.fwdictconfig);
		}

		[TearDown]
		public override void TestTearDown()
		{
			_configFile.Dispose();
			_configFile = null;
			base.TestTearDown();
			base.FixtureTearDown();
		}
		
		[Test]
		public void CanValidateFiles() // Config File and Schema both exist
		{
			using (TempFolderWithSchema())
				Assert.That(FileHandler.CanValidateFile(_configFile.Path), "Should be able to validate against the schema");
		}

		[Test]
		public void CantValidateOtherFiles() // Config File is of the wrong type
		{
			using (TempFolderWithSchema())
			using (var configFile = TempFile.WithExtension(".incorrect"))
				Assert.False(FileHandler.CanValidateFile(configFile.Path));
		}

		[Test]
		public void CantValidateNonexistentFiles() // Config File does not exist
		{
			string path;
			using (var configFile = TempFile.WithExtension(SharedConstants.fwdictconfig))
				path = configFile.Path;
			using (TempFolderWithSchema())
				Assert.False(FileHandler.CanValidateFile(path));
		}

		[Test]
		public void CantValidateWithoutSchema() // Schema does not exist
		{
			Assert.False(FileHandler.CanValidateFile(_configFile.Path), "Should not be able to validate without the schema");
		}

		private const string ValidConfigXml = @"<?xml version='1.0' encoding='utf-8'?>
<DictionaryConfiguration name='Root-based (complex forms as subentries)' allPublications='true' version='1' lastModified='2014-10-07'>
  <ConfigurationItem name='Main Entry' style='Dictionary-Normal' isEnabled='true' field='LexEntry' cssClassNameOverride='entry'>
  <ParagraphOptions paragraphStyle='Dictionary-Normal' continuationParagraphStyle='Dictionary-Continuation' />
	<ConfigurationItem name='Headword' between=' ' after='  ' style='Dictionary-Headword' isEnabled='true' field='MLHeadWord' cssClassNameOverride='mainheadword'>
	  <WritingSystemOptions writingSystemType='vernacular' displayWSAbreviation='false'>
		<Option id='vernacular' isEnabled='true'/>
	  </WritingSystemOptions>
	</ConfigurationItem>
	<ConfigurationItem name='Variant Forms' before='(' between='; ' after=') ' isEnabled='true' field='VariantFormEntryBackRefs'>
	  <ListTypeOptions list='variant'>
		<Option isEnabled='true' id='b0000000-c40e-433e-80b5-31da08771344'/>
		<Option isEnabled='false' id='0c4663b3-4d9a-47af-b9a1-c8565d8112ed'/>
	  </ListTypeOptions>
	</ConfigurationItem>
  </ConfigurationItem>
</DictionaryConfiguration>";

		/// <summary>
		/// Tests the validation code for FW Dict Config files. Data is dated (we do not store a current copy of the schema anywhere in this repo);
		/// tests only that we locate and use a schema definition when FLEx has put it in the S/R repo in a folder named Temp near the config file.
		/// </summary>
		[Test]
		public void ValidatesValidFile()
		{
			File.WriteAllText(_configFile.Path, ValidConfigXml);
			using (TempFolderWithSchema())
				Assert.IsNullOrEmpty(FileHandler.ValidateFile(_configFile.Path, new NullProgress()), "Should validate against the schema");
		}

		[Test]
		public void DoesNotValidateWithoutSchema()
		{
			File.WriteAllText(_configFile.Path, ValidConfigXml);
			Assert.Throws<ArgumentException>(() => FileHandler.ValidateFile(_configFile.Path, new NullProgress()), "Should not validate w/o schema");
		}

		[Test]
		public void DoesNotValidateMalformedXmlFile()
		{
			const string configXml = @"<?xml version='1.0' encoding='utf-8'?>
<DictionaryConfiguration name='Root-based (complex forms as subentries)' allPublications='true' version='1' lastModified='2014-10-07'>
  </ConfigurationItem>
</DictionaryConfiguration>";
			File.WriteAllText(_configFile.Path, configXml);
			using (TempFolderWithSchema())
				Assert.IsNotEmpty(FileHandler.ValidateFile(_configFile.Path, new NullProgress()));
		}

		[Test]
		public void DoesNotValidateInvalidConfigFile()
		{
			const string configXml = @"<?xml version='1.0' encoding='utf-8'?>
<DictionaryConfiguration name='Root-based (complex forms as subentries)' allPublications='true' version='1' badAttribute='prohibited'/>";
			File.WriteAllText(_configFile.Path, configXml);
			using (TempFolderWithSchema())
				Assert.IsNotEmpty(FileHandler.ValidateFile(_configFile.Path, new NullProgress()));
		}

		private TemporaryFolder TempFolderWithSchema()
		{
			var tempFolder = new TemporaryFolder("Temp");
			var xsdPathInProjRepo = Path.Combine(tempFolder.Path, SharedConstants.DictConfigSchemaFilename);
			File.Copy(_xsdSourcePath, xsdPathInProjRepo, true);
			return tempFolder;
		}
	}
}
