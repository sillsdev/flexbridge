// --------------------------------------------------------------------------------------------
// Copyright (C) 2016 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.IO;
using System.Reflection;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.Handling.ConfigLayout;
using NUnit.Framework;
using Palaso.IO;
using Palaso.TestUtilities;
using TriboroughBridge_ChorusPlugin;

namespace FLEx_ChorusPluginTests.Infrastructure.Handling.ConfigLayout
{
	[TestFixture]
	public class DictionaryConfigurationHandlerStrategyTests : BaseFieldWorksTypeHandlerTests
	{
		private TempFile _configFile;

		[TestFixtureSetUp]
		public void Setup()
		{
			var baseDir = Path.GetDirectoryName(Utilities.StripFilePrefix(Assembly.GetExecutingAssembly().CodeBase));
			Utilities.FwAppsDir = Path.Combine(baseDir, "TestData"); // allows the Strategy to find the configuration schema
		}

		[SetUp]
		public override void TestSetup()
		{
			base.TestSetup();
			_configFile = TempFile.WithExtension("." + SharedConstants.fwlayout);
		}

		[TearDown]
		public override void TestTearDown()
		{
			base.TestTearDown();
			_configFile.Dispose();
			_configFile = null;
		}
		
		[Test]
		public void CanValidateFiles() // Schema Exists
		{
			Assert.That(new DictionaryConfigurationHandlerStrategy().CanValidateFile("doesntExist.fwdictconfig"));
		}

		[Test]
		public void ValidatesValidFile()
		{
			const string configXml = @"<?xml version='1.0' encoding='utf-8'?>
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
			File.WriteAllText(_configFile.Path, configXml);
			Assert.IsNullOrEmpty(new DictionaryConfigurationHandlerStrategy().ValidateFile(_configFile.Path));
		}

		[Test]
		public void DoesNotValidateMalformedXmlFile()
		{
			const string configXml = @"<?xml version='1.0' encoding='utf-8'?>
<DictionaryConfiguration name='Root-based (complex forms as subentries)' allPublications='true' version='1' lastModified='2014-10-07'>
  </ConfigurationItem>
</DictionaryConfiguration>";
			File.WriteAllText(_configFile.Path, configXml);
			Assert.That(new DictionaryConfigurationHandlerStrategy().ValidateFile(_configFile.Path),
				Is.StringStarting("Exception occurred during validation: "));
		}

		[Test]
		public void DoesNotValidateInvalidConfigFile()
		{
			const string configXml = @"<?xml version='1.0' encoding='utf-8'?>
<DictionaryConfiguration name='Root-based (complex forms as subentries)' allPublications='true' version='1' badAttribute='prohibited'/>";
			File.WriteAllText(_configFile.Path, configXml);
			Assert.That(new DictionaryConfigurationHandlerStrategy().ValidateFile(_configFile.Path),
				Is.StringStarting("Model saved as xml did not validate against schema: "));
		}
	}
}
