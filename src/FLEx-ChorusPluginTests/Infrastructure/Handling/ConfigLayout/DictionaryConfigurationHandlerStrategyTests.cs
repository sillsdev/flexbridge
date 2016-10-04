// --------------------------------------------------------------------------------------------
// Copyright (C) 2016 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Reflection;
using System.Threading;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.Handling;
using FLEx_ChorusPlugin.Infrastructure.Handling.ConfigLayout;
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
		private TemporaryFolder _tempFolder;
		private string _xsdPathInProjRepo;

		[TestFixtureSetUp]
		public override void FixtureSetup()
		{
			base.FixtureSetup();
			// Copy the schema file to where the Strategy looks
			var appsDir = Path.GetDirectoryName(Utilities.StripFilePrefix(Assembly.GetExecutingAssembly().CodeBase));
			var xsdPath = Path.Combine(appsDir, "TestData", "Language Explorer", "Configuration", SharedConstants.DictConfigSchemaFilename);
			_tempFolder = new TemporaryFolder("Temp");
			_xsdPathInProjRepo = Path.Combine(_tempFolder.Path, SharedConstants.DictConfigSchemaFilename);
			File.Copy(xsdPath, _xsdPathInProjRepo, true);
		}

		[TestFixtureTearDown]
		public override void FixtureTearDown()
		{
			ClearCachedSchemaPathFromHandler();
			_tempFolder.Dispose();
			_tempFolder = null;
			base.FixtureTearDown();
		}

		[SetUp]
		public override void TestSetup()
		{
			base.TestSetup();
			_configFile = TempFile.WithExtension("." + SharedConstants.fwdictconfig);
		}

		[TearDown]
		public override void TestTearDown()
		{
			_configFile.Dispose();
			_configFile = null;
			base.TestTearDown();
		}
		
		[Test]
		public void CanValidateFiles() // Config File and Schema both exist
		{
			RunOneAtATime(() =>Assert.That(FileHandler.CanValidateFile(_configFile.Path), "Should be able to validate against the schema"));
		}

		[Test]
		public void CantValidateOtherFiles() // Config File is of the wrong type
		{
			RunOneAtATime(() =>
			{
				using (var configFile = TempFile.WithExtension(".incorrect"))
					Assert.False(FileHandler.CanValidateFile(configFile.Path));
			});
		}

		[Test]
		public void CantValidateNonexistentFiles() // Config File does not exist
		{
			RunOneAtATime(() =>
			{
				string path;
				using (var configFile = TempFile.WithExtension(SharedConstants.fwdictconfig))
					path = configFile.Path;
				Assert.False(FileHandler.CanValidateFile(path));
			});
		}

		[Test]
		public void CantValidateWithoutSchema() // Schema does not exist
		{
			RunOneAtATime(() =>
			{
				using (new SchemaSuppressor(_xsdPathInProjRepo))
					Assert.False(FileHandler.CanValidateFile(_configFile.Path), "Should not be able to validate without the schema");
			});
		}

			const string ValidConfigXml = @"<?xml version='1.0' encoding='utf-8'?>
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
			RunOneAtATime(() =>
			{
				File.WriteAllText(_configFile.Path, ValidConfigXml);
				Assert.IsNullOrEmpty(FileHandler.ValidateFile(_configFile.Path, new NullProgress()), "Should validate against the schema");
			});
		}

		[Test]
		public void DoesNotValidateWithoutSchema()
		{
			RunOneAtATime(() =>
			{
				File.WriteAllText(_configFile.Path, ValidConfigXml);
				using (new SchemaSuppressor(_xsdPathInProjRepo))
					Assert.Throws<ArgumentException>(() => FileHandler.ValidateFile(_configFile.Path, new NullProgress()),
						"Should not validate w/o schema");
			});
		}

		[Test]
		public void DoesNotValidateMalformedXmlFile()
		{
			RunOneAtATime(() =>
			{
				const string configXml = @"<?xml version='1.0' encoding='utf-8'?>
<DictionaryConfiguration name='Root-based (complex forms as subentries)' allPublications='true' version='1' lastModified='2014-10-07'>
  </ConfigurationItem>
</DictionaryConfiguration>";
				File.WriteAllText(_configFile.Path, configXml);
				Assert.IsNotEmpty(FileHandler.ValidateFile(_configFile.Path, new NullProgress()));
			});
		}

		[Test]
		public void DoesNotValidateInvalidConfigFile()
		{
			RunOneAtATime(() =>
			{
				const string configXml = @"<?xml version='1.0' encoding='utf-8'?>
<DictionaryConfiguration name='Root-based (complex forms as subentries)' allPublications='true' version='1' badAttribute='prohibited'/>";
				File.WriteAllText(_configFile.Path, configXml);
				Assert.IsNotEmpty(FileHandler.ValidateFile(_configFile.Path, new NullProgress()));
			});
		}

		private void RunOneAtATime(Action test)
		{
			lock (_tempFolder)
			{
				ClearCachedSchemaPathFromHandler();
				test();
				while (!File.Exists(_xsdPathInProjRepo))
					Thread.Sleep(10);
			}
		}

		private void ClearCachedSchemaPathFromHandler()
		{
			var handler = ((FieldWorksCommonFileHandler)FileHandler).GetHandlerfromExtension(SharedConstants.fwdictconfig);
			((DictionaryConfigurationHandlerStrategy)handler)._xsdPathname = null;
		}

		private sealed class SchemaSuppressor : IDisposable
		{
			private readonly string _suppressedSchemaPath;

			public SchemaSuppressor(string schemaPath)
			{
				_suppressedSchemaPath = Path.ChangeExtension(schemaPath, "hidden");
				File.Move(schemaPath, _suppressedSchemaPath);
			}

			public void Dispose()
			{
				File.Move(_suppressedSchemaPath, Path.ChangeExtension(_suppressedSchemaPath, "xsd"));
			}
		}
	}
}
