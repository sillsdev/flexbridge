// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Chorus.merge.xml.generic;
using Chorus.Properties;
using LibChorus.TestUtilities;
using LibFLExBridgeChorusPlugin.Infrastructure;
using NUnit.Framework;
using SIL.IO;
using SIL.TestUtilities;

namespace FLEx_ChorusPluginTests.Integration
{
	/// <summary>
	/// This class tests a complete series of operations over several units including:
	///		merging and syncing,
	///		Some tests may also include conflicts and the respective ChorusNotes file.
	/// </summary>
	[TestFixture]
	public class MergeIntegrationTests
	{
		private	const string CustomPropData = @"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
	<CustomField
		class='LexEntry'
		destclass='7'
		key='LexEntryTone'
		listRoot='53241fd4-72ae-4082-af55-6b659657083c'
		name='Tone'
		type='ReferenceCollection' />
	<CustomField
		class='LexSense'
		key='LexSenseParadigm'
		name='Paradigm'
		type='MultiString'
		wsSelector='-2' />
	<CustomField
		class='WfiWordform'
		key='WfiWordformCertified'
		name='Certified'
		type='Boolean' />
</AdditionalFields>";

		[Test]
		public void EnsureRightPersonMadeChanges()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='2d23f428-83a9-44ba-90f1-9e3264b5b982' >
			<DateCreated val='2012-12-10 6:29:17.117' />
			<DateModified val='2012-12-10 6:29:17.117' />
			<IsHeadwordCitationForm val='True' />
			<IsBodyInSeparateSubentry val='True' />
		</LexDb>
	</header>
	<LexEntry guid='ffdc58c9-5cc3-469f-9118-9f18c0138d02'>
		<DateCreated val='2012-12-10 6:29:17.117' />
		<DateModified val='2012-12-10 6:29:17.117' />
		<HomographNumber val='1' />
		<DoNotUseForParsing val='True' />
		<ExcludeAsHeadword val='True' />
		<Senses>
			<ownseq class='LexSense' guid='97129e67-e0a5-47c4-a875-05c2b2e1b7df'>
				<Custom
					name='Paradigm'>
					<AStr
						ws='qaa-x-ezpi'>
						<Run
							ws='qaa-x-ezpi'>saklo, yzaklo, rzaklo, wzaklo, nzaklo, -</Run>
					</AStr>
				</Custom>
			</ownseq>
		</Senses>
	</LexEntry>
</Lexicon>";
			const string sue =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='2d23f428-83a9-44ba-90f1-9e3264b5b982' >
			<DateCreated val='2012-12-10 6:29:17.117' />
			<DateModified val='2012-12-10 6:29:17.117' />
			<IsHeadwordCitationForm val='True' />
			<IsBodyInSeparateSubentry val='True' />
		</LexDb>
	</header>
	<LexEntry guid='ffdc58c9-5cc3-469f-9118-9f18c0138d02'>
		<DateCreated val='2012-12-10 6:29:17.117' />
		<DateModified val='2012-12-10 6:29:17.117' />
		<HomographNumber val='1' />
		<DoNotUseForParsing val='True' />
		<ExcludeAsHeadword val='True' />
		<Senses>
			<ownseq class='LexSense' guid='97129e67-e0a5-47c4-a875-05c2b2e1b7df'>
				<Custom
					name='Paradigm'>
					<AStr
						ws='qaa-x-ezpi'>
						<Run
							ws='qaa-x-ezpi'>saglo, yzaglo, rzaglo, wzaglo, nzaglo, -</Run>
					</AStr>
				</Custom>
			</ownseq>
		</Senses>
	</LexEntry>
</Lexicon>";
			const string randy =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='2d23f428-83a9-44ba-90f1-9e3264b5b982' >
		</LexDb>
	</header>
	<LexEntry guid='ffdc58c9-5cc3-469f-9118-9f18c0138d02'>
		<Senses>
			<ownseq class='LexSense' guid='97129e67-e0a5-47c4-a875-05c2b2e1b7df'>
				<Custom
					name='Paradigm'>
					<AStr
						ws='zpi'>
						<Run
							ws='zpi'>saklo, yzaklo, rzaklo, wzaklo, nzaklo, -</Run>
					</AStr>
				</Custom>
			</ownseq>
		</Senses>
	</LexEntry>
</Lexicon>";

			var mdc = MetadataCache.TestOnlyNewCache;
			mdc.AddCustomPropInfo("LexEntry", new FdoPropertyInfo("Tone", DataType.ReferenceCollection, true));
			mdc.AddCustomPropInfo("LexSense", new FdoPropertyInfo("Paradigm", DataType.MultiString, true));
			mdc.AddCustomPropInfo("WfiWordform", new FdoPropertyInfo("Certified", DataType.Boolean, true));
			mdc.ResetCaches();
			using (var sueRepo = new RepositoryWithFilesSetup("Sue", $"{FlexBridgeConstants.Lexicon}_01.{FlexBridgeConstants.Lexdb}", commonAncestor))
			{
				var sueProjPath = sueRepo.ProjectFolder.Path;
				// Add model version number file.
				var modelVersionPathname = Path.Combine(sueProjPath, FlexBridgeConstants.ModelVersionFilename);
				File.WriteAllText(modelVersionPathname, AnnotationImages.kModelVersion);
				sueRepo.Repository.TestOnlyAddSansCommit(modelVersionPathname);
				// Add custom property data file.
				var customPropsPathname = Path.Combine(sueProjPath, FlexBridgeConstants.CustomPropertiesFilename);
				File.WriteAllText(customPropsPathname, CustomPropData);
				sueRepo.Repository.TestOnlyAddSansCommit(customPropsPathname);
				sueRepo.AddAndCheckIn();

				using (var randyRepo = RepositoryWithFilesSetup.CreateByCloning("Randy", sueRepo))
				{
					// By doing the clone first, we get the common starting state in both repos.
					sueRepo.WriteNewContentsToTestFile(sue);
					sueRepo.AddAndCheckIn();

					var mergeConflictsNotesFile = ChorusNotesMergeEventListener.GetChorusNotesFilePath(randyRepo.UserFile.Path);
					Assert.That(File.Exists(mergeConflictsNotesFile), Is.False, "ChorusNotes file should NOT have been in working set.");
					randyRepo.WriteNewContentsToTestFile(randy);
					var result = randyRepo.CheckinAndPullAndMerge(sueRepo);
					Assert.That(result.Succeeded, result.ErrorEncountered?.ToString());
					Assert.That(File.Exists(mergeConflictsNotesFile), Is.True, "ChorusNotes file should have been in working set.");
					var notesContents = File.ReadAllText(mergeConflictsNotesFile);
					Assert.That(notesContents, Is.Not.Null.Or.Empty);
					Assert.That(notesContents, Does.Contain("Removed Vs Edited Element Conflict"));
					Assert.That(notesContents, Does.Contain("Randy deleted this element"));
					Assert.That(notesContents, Does.Contain("Sue edited it"));
					Assert.That(notesContents, Does.Contain("The merger kept the change made by Sue."));
					Assert.That(notesContents, Does.Contain("whoWon=\"Sue\""));
					Assert.That(notesContents, Does.Contain("alphaUserId=\"Randy\""));
					Assert.That(notesContents, Does.Contain("betaUserId=\"Sue\""));

					// Make sure merged file has both alts.
					var doc = XDocument.Load(randyRepo.UserFile.Path);
					var customParadigmElement = doc.Root.Element("LexEntry").Element("Senses").Element("ownseq").Element("Custom");
					var aStrElements = customParadigmElement.Elements("AStr").ToList();
					Assert.AreEqual(2, aStrElements.Count);
					var aStrZpi = aStrElements.FirstOrDefault(el => el.Attribute("ws").Value == "zpi");
					Assert.IsNotNull(aStrZpi);
					Assert.That(aStrZpi.Element("Run").Value, Is.EqualTo("saklo, yzaklo, rzaklo, wzaklo, nzaklo, -"));
					var aStrEzpi = aStrElements.FirstOrDefault(el => el.Attribute("ws").Value == "qaa-x-ezpi");
					Assert.IsNotNull(aStrEzpi);
					Assert.That(aStrEzpi.Element("Run").Value, Is.EqualTo("saglo, yzaglo, rzaglo, wzaglo, nzaglo, -"));
				}
			}
		}

		[Test]
		public void EnsureDictionaryConfigsUseDictionaryStrategy()
		{
			const string commonAncestor = @"<?xml version='1.0' encoding='utf-8'?>
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

			const string sue = @"<?xml version='1.0' encoding='utf-8'?>
<DictionaryConfiguration name='Root-based (complex forms as subentries)' allPublications='true' version='1' lastModified='2014-10-07'>
  <ConfigurationItem name='Main Entry' style='Dictionary-Normal' isEnabled='true' field='LexEntry' cssClassNameOverride='entry'>
  <ParagraphOptions paragraphStyle='Dictionary-Normal' continuationParagraphStyle='Dictionary-Continuation' />
	<ConfigurationItem name='Headword' between=' ' after='  ' style='Dictionary-Headword' isEnabled='true' field='MLHeadWord' cssClassNameOverride='mainheadword'>
	  <WritingSystemOptions writingSystemType='vernacular' displayWSAbreviation='false'>
		<Option id='vernacular' isEnabled='false'/>
		<Option id='fr' isEnabled='true' />
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

			const string randy = @"<?xml version='1.0' encoding='utf-8'?>
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
		<Option isEnabled='false' id='b0000000-c40e-433e-80b5-31da08771344'/>
		<Option isEnabled='true' id='0c4663b3-4d9a-47af-b9a1-c8565d8112ed'/>
	  </ListTypeOptions>
	</ConfigurationItem>
  </ConfigurationItem>
</DictionaryConfiguration>";

			using( var tempFolder = new TemporaryFolder("Temp"))
			{
				// Copy the Dictionary Configuration Schema to where the Dictionary Configuration Handler Strategy looks
				var appsDir = Path.GetDirectoryName(PathHelper.StripFilePrefix(Assembly.GetExecutingAssembly().CodeBase));
				var xsdPath = Path.Combine(appsDir, "TestData", "Language Explorer", "Configuration", FlexBridgeConstants.DictConfigSchemaFilename);
				var xsdPathInProj = Path.Combine(tempFolder.Path, FlexBridgeConstants.DictConfigSchemaFilename);
				File.Copy(xsdPath, xsdPathInProj, true);

				using (var sueRepo = new RepositoryWithFilesSetup("Sue", $"root.{FlexBridgeConstants.fwdictconfig}", commonAncestor))
				using (var randyRepo = RepositoryWithFilesSetup.CreateByCloning("Randy", sueRepo))
				{
					// By doing the clone before making Sue's changes, we get the common starting state in both repos.
					sueRepo.WriteNewContentsToTestFile(sue);
					sueRepo.AddAndCheckIn();

					var mergeConflictsNotesFile = ChorusNotesMergeEventListener.GetChorusNotesFilePath(randyRepo.UserFile.Path);
					Assert.That(File.Exists(mergeConflictsNotesFile), Is.False, "ChorusNotes file should NOT have been in working set.");
					randyRepo.WriteNewContentsToTestFile(randy);
					var result = randyRepo.CheckinAndPullAndMerge(sueRepo);
					Assert.That(result.Succeeded, result.ErrorEncountered?.ToString());
					Assert.That(File.Exists(mergeConflictsNotesFile), Is.True, "ChorusNotes file should have been in working set.");
					var notesContents = File.ReadAllText(mergeConflictsNotesFile);
					Assert.That(notesContents, Is.Not.Null.Or.Empty);
					Assert.That(notesContents, Does.Contain("Randy and Sue edited the same part of this data."));
					Assert.That(notesContents, Does.Contain("The merger kept the change made by Randy."));
					Assert.That(notesContents, Does.Contain("alphaUserId=\"Randy\""));
					Assert.That(notesContents, Does.Contain("betaUserId=\"Sue\""));

					// Make sure merged file has Randy's changes
					var doc = XDocument.Load(randyRepo.UserFile.Path);
					var options = doc.Root.Element("ConfigurationItem").Elements("ConfigurationItem").Last(/*Variant Forms*/)
						.Element("ListTypeOptions").Elements("Option").ToList();
					Assert.AreEqual(2, options.Count, "There should be two Variant Forms options");
					Assert.AreEqual("b0000000-c40e-433e-80b5-31da08771344", options[0].Attribute("id").Value, "Options are out of order");
					Assert.AreEqual("0c4663b3-4d9a-47af-b9a1-c8565d8112ed", options[1].Attribute("id").Value, "Options are out of order");
					Assert.AreEqual("false", options[0].Attribute("isEnabled").Value, "First option should be disabled");
					Assert.AreEqual("true", options[1].Attribute("isEnabled").Value, "Second option should be enabled");

					// Make sure merged file does *not* have Sue's changes
					options = doc.Root.Element("ConfigurationItem").Element("ConfigurationItem" /*Headword*/)
						.Element("WritingSystemOptions").Elements("Option").ToList();
					Assert.AreEqual(1, options.Count, "There should be only one WS Option");
					Assert.AreEqual("vernacular", options[0].Attribute("id").Value, "should be default vernacular");
					Assert.AreEqual("true", options[0].Attribute("isEnabled").Value, "should be enabled");
				}
			}
		}

		[Test]
		public void DictConfigMerge_DifferentUpgradePathKeepsFileFormat()
		{
			//const string commonAncestor = null;

			const string sue = @"<?xml version='1.0' encoding='utf-8'?>
<DictionaryConfiguration name='Root-based (complex forms as subentries)' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema' allPublications='true' version='14' lastModified='2014-10-07'>
  <ConfigurationItem name='Main Entry' style='Dictionary-Normal' isEnabled='true' field='LexEntry' cssClassNameOverride='entry'>
  <ParagraphOptions paragraphStyle='Dictionary-Normal' continuationParagraphStyle='Dictionary-Continuation' />
	<ConfigurationItem name='Headword' between=' ' after='  ' style='Dictionary-Headword' isEnabled='true' field='MLHeadWord' cssClassNameOverride='mainheadword'>
	  <WritingSystemOptions writingSystemType='vernacular' displayWSAbreviation='false'>
		<Option id='vernacular' isEnabled='false'/>
		<Option id='fr' isEnabled='true' />
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

			const string randy = @"<?xml version='1.0' encoding='utf-8'?>
<DictionaryConfiguration xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema' name='Root-based (complex forms as subentries)' allPublications='true' version='17' lastModified='2014-10-07'>
  <ConfigurationItem name='Main Entry' style='Dictionary-Normal' isEnabled='true' field='LexEntry' cssClassNameOverride='entry'>
  <ParagraphOptions paragraphStyle='Dictionary-Normal' continuationParagraphStyle='Dictionary-Continuation' />
	<ConfigurationItem name='Headword' between=' ' after='  ' style='Dictionary-Headword' isEnabled='true' field='MLHeadWord' cssClassNameOverride='mainheadword'>
	  <WritingSystemOptions writingSystemType='vernacular' displayWSAbreviation='false'>
		<Option id='vernacular' isEnabled='true'/>
	  </WritingSystemOptions>
	</ConfigurationItem>
	<ConfigurationItem name='Variant Forms' before='(' between='; ' after=') ' isEnabled='true' field='VariantFormEntryBackRefs'>
	  <ListTypeOptions list='variant'>
		<Option isEnabled='false' id='b0000000-c40e-433e-80b5-31da08771344'/>
		<Option isEnabled='true' id='0c4663b3-4d9a-47af-b9a1-c8565d8112ed'/>
	  </ListTypeOptions>
	</ConfigurationItem>
  </ConfigurationItem>
</DictionaryConfiguration>";

			using (var tempFolder = new TemporaryFolder("Temp"))
			{
				// Copy the Dictionary Configuration Schema to where the Dictionary Configuration Handler Strategy looks
				var appsDir = Path.GetDirectoryName(PathHelper.StripFilePrefix(Assembly.GetExecutingAssembly().CodeBase));
				var xsdPath = Path.Combine(appsDir, "TestData", "Language Explorer", "Configuration", FlexBridgeConstants.DictConfigSchemaFilename);
				var xsdPathInProj = Path.Combine(tempFolder.Path, FlexBridgeConstants.DictConfigSchemaFilename);
				File.Copy(xsdPath, xsdPathInProj, true);

				using (var sueRepo = new RepositorySetup("Sue", true))
				{
					sueRepo.AddAndCheckinFile("unrelated.txt", "unrelated to SUT, here to get us rev 0 so repos are related");
					using (var randyRepo = new RepositorySetup("Randy", sueRepo))
					{
						// By doing the clone before making Sue's changes, we get the common starting state in both repos.
						sueRepo.AddAndCheckinFile($"root.{FlexBridgeConstants.fwdictconfig}", sue);

						var randyDictConfigInRepoPath = Path.Combine(randyRepo.ProjectFolder.Path, $"root.{FlexBridgeConstants.fwdictconfig}");
						var mergeConflictsNotesFile = ChorusNotesMergeEventListener.GetChorusNotesFilePath(randyDictConfigInRepoPath);
						Assert.That(File.Exists(mergeConflictsNotesFile), Is.False, "ChorusNotes file should NOT have been in working set.");
						randyRepo.AddAndCheckinFile($"root.{FlexBridgeConstants.fwdictconfig}", randy);
						var result = randyRepo.CheckinAndPullAndMerge(sueRepo);
						Assert.That(result.Succeeded, result.ErrorEncountered?.ToString());
						Assert.That(File.Exists(mergeConflictsNotesFile), Is.True, "ChorusNotes file should have been in working set.");
						var notesContents = File.ReadAllText(mergeConflictsNotesFile);
						Assert.That(notesContents, Is.Not.Null.Or.Empty);
						Assert.That(notesContents, Does.Contain("Both added the same element, but with different content"));
						Assert.That(notesContents, Does.Contain("The merger kept the change made by Randy."));
						Assert.That(notesContents, Does.Contain("alphaUserId=\"Randy\""));
						Assert.That(notesContents, Does.Contain("betaUserId=\"Sue\""));

						// Make sure merged file has Randy's changes
						var doc = XDocument.Load(randyDictConfigInRepoPath);
						var options = doc.Root.Element("ConfigurationItem").Elements("ConfigurationItem").Last(/*Variant Forms*/)
							.Element("ListTypeOptions").Elements("Option").ToList();
						Assert.AreEqual(2, options.Count, "There should be two Variant Forms options");
						Assert.AreEqual("b0000000-c40e-433e-80b5-31da08771344", options[0].Attribute("id").Value, "Options are out of order");
						Assert.AreEqual("0c4663b3-4d9a-47af-b9a1-c8565d8112ed", options[1].Attribute("id").Value, "Options are out of order");
						Assert.AreEqual("false", options[0].Attribute("isEnabled").Value, "First option should be disabled");
						Assert.AreEqual("true", options[1].Attribute("isEnabled").Value, "Second option should be enabled");

						// Make sure merged file does *not* have Sue's changes
						options = doc.Root.Element("ConfigurationItem").Element("ConfigurationItem" /*Headword*/)
							.Element("WritingSystemOptions").Elements("Option").ToList();
						Assert.AreEqual(1, options.Count, "There should be only one WS Option");
						Assert.AreEqual("vernacular", options[0].Attribute("id").Value, "should be default vernacular");
						Assert.AreEqual("true", options[0].Attribute("isEnabled").Value, "should be enabled");

						// Make sure the merged file has proper xsd namespace attributes
						var xsiAttr = doc.Root.Attribute("xsi");
						var xsdAttr = doc.Root.Attribute("xsd");
						Assert.Null(xsiAttr, "xsi missing namespace");
						Assert.Null(xsdAttr, "xsd missing namespace");
						//
						XNamespace xsiNs = "http://www.w3.org/2001/XMLSchema-instance";
						XNamespace xsdNs = "http://www.w3.org/2001/XMLSchema";
						xsiAttr = doc.Root.Attribute(xsiNs + "xsi");
						xsdAttr = doc.Root.Attribute(xsdNs + "xsd");
						Assert.Null(xsiAttr, "xsi attribute missing entirely from result");
						Assert.Null(xsdAttr, "xsd attribute missing entirely from result");
					}
				}
			}
		}
	}
}
