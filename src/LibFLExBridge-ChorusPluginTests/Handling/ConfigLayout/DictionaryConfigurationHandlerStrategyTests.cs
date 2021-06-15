// --------------------------------------------------------------------------------------------
// Copyright (C) 2016-2017 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Reflection;
using Chorus.FileTypeHandlers;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibTriboroughBridgeChorusPlugin;
using NUnit.Framework;
using SIL.IO;
using SIL.Progress;
using SIL.TestUtilities;

namespace LibFLExBridgeChorusPluginTests.Handling.ConfigLayout
{
	[TestFixture]
	public class DictionaryConfigurationHandlerStrategyTests
	{
		private IChorusFileTypeHandler _fileHandler;
		private TempFile _configFile;
		private TemporaryFolder _tempFolder;
		private string _xsdSourcePath;

		[OneTimeSetUp]
		public void FixtureSetup()
		{
			var appsDir = Path.GetDirectoryName(PathHelper.StripFilePrefix(Assembly.GetExecutingAssembly().CodeBase));
			_xsdSourcePath = Path.Combine(appsDir, "TestData", "Language Explorer", "Configuration", FlexBridgeConstants.DictConfigSchemaFilename);
			MetadataCache.TestOnlyNewCache.UpgradeToVersion(MetadataCache.MaximumModelVersion);
		}

		[SetUp]
		public void TestSetup()
		{
			// Each starts with a fresh FileHandler, because DictionaryConfigurationHandlerStrategy caches the path to the schema,
			// but some tests require the schema's presence and others require its absence.
			_fileHandler = FieldWorksTestServices.CreateChorusFileHandlers();
			_configFile = TempFile.WithExtension("." + FlexBridgeConstants.fwdictconfig);
		}

		[TearDown]
		public void TestTearDown()
		{
			RemoveOptionalStuff();

			_configFile.Dispose();
			_configFile = null;
			_fileHandler = null;
		}

		private void RemoveOptionalStuff()
		{
			if (_tempFolder != null)
			{
				_tempFolder.Dispose();
				_tempFolder = null;
			}
		}

		private void CopySchema()
		{
			RemoveOptionalStuff();

			// Copy the schema file to where the Strategy looks
			// We are pretending to do what FLEx does (provides the schema).
			_tempFolder = new TemporaryFolder("Temp");
			var xsdPathInProjRepo = Path.Combine(_tempFolder.Path, FlexBridgeConstants.DictConfigSchemaFilename);
			File.Copy(_xsdSourcePath, xsdPathInProjRepo, true);
		}

		[Test]
		public void CanValidateFiles()
		{
			// Config File and Schema both exist
			CopySchema();
			Assert.That(_fileHandler.CanValidateFile(_configFile.Path), "Should be able to validate against the schema");
		}

		[Test]
		public void CantValidateOtherFiles()
		{
			// Config File is of the wrong type
			CopySchema();
			using (var configFile = TempFile.WithExtension(".incorrect"))
			{
				Assert.False(_fileHandler.CanValidateFile(configFile.Path));
			}
		}

		[Test]
		public void CantValidateNonexistentFiles()
		{
			// Config File does not exist
			CopySchema();
			Assert.False(_fileHandler.CanValidateFile(Path.Combine(_tempFolder.Path, "nonexistent_file." + FlexBridgeConstants.fwdictconfig)));
		}

		[Test]
		public void CantValidateWithoutSchema()
		{
			// Schema does not exist
			Assert.False(_fileHandler.CanValidateFile(_configFile.Path), "Should not be able to validate without the schema");
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
		/// Tests the validation code for FW Dict Config files.
		/// Schema and data are dated (we do not store a current copy of the schema anywhere in this repo);
		/// tests only that we locate and use a schema definition when FLEx has put it in the S/R repo in a folder named Temp near the config file.
		/// </summary>
		[Test]
		public void ValidatesValidFile()
		{
			CopySchema();

			File.WriteAllText(_configFile.Path, ValidConfigXml);
			Assert.That(_fileHandler.ValidateFile(_configFile.Path, new NullProgress()), Is.Null.Or.Empty,
				"Should validate against the schema");
		}

		[Test]
		public void ThrowsWithoutSchema()
		{
			File.WriteAllText(_configFile.Path, ValidConfigXml);
			Assert.Throws<ArgumentException>(() => _fileHandler.ValidateFile(_configFile.Path, new NullProgress()),
					"Should not validate w/o schema");
		}

		[Test]
		public void DoesNotValidateMalformedXmlFile()
		{
			CopySchema();

			const string configXml = @"<?xml version='1.0' encoding='utf-8'?>
<DictionaryConfiguration name='Root-based (complex forms as subentries)' allPublications='true' version='1' lastModified='2014-10-07'>
  </ConfigurationItem>
</DictionaryConfiguration>";
			File.WriteAllText(_configFile.Path, configXml);
			Assert.IsNotEmpty(_fileHandler.ValidateFile(_configFile.Path, new NullProgress()));
		}

		[Test]
		public void DoesNotValidateInvalidConfigFile()
		{
			CopySchema();

			const string configXml = @"<?xml version='1.0' encoding='utf-8'?>
<DictionaryConfiguration name='Root-based (complex forms as subentries)' allPublications='true' version='1' badAttribute='prohibited'/>";
			File.WriteAllText(_configFile.Path, configXml);
			Assert.IsNotEmpty(_fileHandler.ValidateFile(_configFile.Path, new NullProgress()));
		}
	}
}
