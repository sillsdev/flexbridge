using System.IO;
using System.Linq;
using System.Xml.Linq;
using Chorus.merge.xml.generic;
using Chorus.Properties;
using FLEx_ChorusPlugin.Infrastructure;
using LibChorus.TestUtilities;
using NUnit.Framework;

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
		[Test]
		[Category("UnknownMonoIssue")] // It insists on failing on mono, for some reason.
		public void EnsureRightPersonMadeChanges()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='2d23f428-83a9-44ba-90f1-9e3264b5b982' />
	</header>
	<LexEntry guid='ffdc58c9-5cc3-469f-9118-9f18c0138d02'>
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
		<LexDb guid='2d23f428-83a9-44ba-90f1-9e3264b5b982' />
	</header>
	<LexEntry guid='ffdc58c9-5cc3-469f-9118-9f18c0138d02'>
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
		<LexDb guid='2d23f428-83a9-44ba-90f1-9e3264b5b982' />
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

			const string customPropData =
@"<?xml version='1.0' encoding='utf-8'?>
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

			using (var sueRepo = new RepositoryWithFilesSetup("Sue", SharedConstants.LexiconFilename, commonAncestor))
			{
				var sueProjPath = sueRepo.ProjectFolder.Path;
				// Add model version number file.
				var modelVersionPathname = Path.Combine(sueProjPath, SharedConstants.ModelVersionFilename);
				File.WriteAllText(modelVersionPathname, AnnotationImages.kModelVersion);
				sueRepo.Repository.TestOnlyAddSansCommit(modelVersionPathname);
				// Add custom property data file.
				var customPropsPathname = Path.Combine(sueProjPath, SharedConstants.CustomPropertiesFilename);
				File.WriteAllText(customPropsPathname, customPropData);
				sueRepo.Repository.TestOnlyAddSansCommit(customPropsPathname);
				sueRepo.AddAndCheckIn();

				using (var randyRepo = RepositoryWithFilesSetup.CreateByCloning("Randy", sueRepo))
				{
					// By doing the clone first, we get the common starting state in both repos.
					sueRepo.WriteNewContentsToTestFile(sue);
					sueRepo.AddAndCheckIn();

					var mergeConflictsNotesFile = ChorusNotesMergeEventListener.GetChorusNotesFilePath(randyRepo.UserFile.Path);
					Assert.IsFalse(File.Exists(mergeConflictsNotesFile), "ChorusNotes file should NOT have been in working set.");
					randyRepo.WriteNewContentsToTestFile(randy);
					randyRepo.CheckinAndPullAndMerge(sueRepo);
					Assert.IsTrue(File.Exists(mergeConflictsNotesFile), "ChorusNotes file should have been in working set.");
					var notesContents = File.ReadAllText(mergeConflictsNotesFile);
					Assert.IsNotNullOrEmpty(notesContents);
					Assert.IsTrue(notesContents.Contains("Removed Vs Edited Element Conflict"));
					Assert.IsTrue(notesContents.Contains("Randy deleted this element"));
					Assert.IsTrue(notesContents.Contains("Sue edited it"));
					Assert.IsTrue(notesContents.Contains("The automated merger kept the change made by Sue."));
					Assert.IsTrue(notesContents.Contains("whoWon=\"Sue\""));
					Assert.IsTrue(notesContents.Contains("alphaUserId=\"Randy\""));
					Assert.IsTrue(notesContents.Contains("betaUserId=\"Sue\""));

					// Make sure merged file has both alts.
					var doc = XDocument.Load(randyRepo.UserFile.Path);
					var customParadigmElement = doc.Root.Element("LexEntry").Element("Senses").Element("ownseq").Element("Custom");
					var aStrElements = customParadigmElement.Elements("AStr").ToList();
					Assert.AreEqual(2, aStrElements.Count);
					var aStrZpi = aStrElements.FirstOrDefault(el => el.Attribute("ws").Value == "zpi");
					Assert.IsNotNull(aStrZpi);
					Assert.IsTrue(aStrZpi.Element("Run").Value == "saklo, yzaklo, rzaklo, wzaklo, nzaklo, -");
					var aStrEzpi = aStrElements.FirstOrDefault(el => el.Attribute("ws").Value == "qaa-x-ezpi");
					Assert.IsNotNull(aStrEzpi);
					Assert.IsTrue(aStrEzpi.Element("Run").Value == "saglo, yzaglo, rzaglo, wzaglo, nzaglo, -");
				}
			}
		}
	}
}
