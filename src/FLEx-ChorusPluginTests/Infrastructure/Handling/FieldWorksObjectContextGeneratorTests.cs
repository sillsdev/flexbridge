using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.Handling;
using NUnit.Framework;

namespace FLEx_ChorusPluginTests.Infrastructure.Handling
{
	[TestFixture]
	public class FieldWorksObjectContextGeneratorTests
	{
		[Test]
		public void LexEntryPartsFindLexemeForm()
		{
			string source =
				@"<LexEntry
					guid='01efa516-1749-4b60-b43d-00089269e7c5'>
					<HomographNumber
						val='0' />
					<LexemeForm>
						<MoStemAllomorph
							guid='8e982d88-0111-43b9-a25c-420bb5c84cf0'>
							<Form>
								<AUni
									ws='en'>abcdefghijk</AUni>
							</Form>
							<IsAbstract
								val='False' />
							<MorphType>
								<objsur
									guid='d7f713e4-e8cf-11d3-9764-00c04f186933'
									t='r' />
							</MorphType>
						</MoStemAllomorph>
					</LexemeForm>
				</LexEntry>";
			var root = GetNode(source);
			var input = root.ChildNodes[1].ChildNodes[0]; // MoStemAllomorph
			var generator = MakeGenerator();
			var descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Entry abcdefghijk LexemeForm"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label="+ descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "8e982d88-0111-43b9-a25c-420bb5c84cf0"));

			// Try a node that is not part of the LexemeForm.
			input = root.ChildNodes[0];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Entry abcdefghijk HomographNumber"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "01efa516-1749-4b60-b43d-00089269e7c5"));

			// Try a bit deeper
			input = root.ChildNodes[1].ChildNodes[0].ChildNodes[0]; // the <Form>
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Entry abcdefghijk LexemeForm Form"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "8e982d88-0111-43b9-a25c-420bb5c84cf0"));

			// Don't want the AUni level.
			input = input.ChildNodes[0]; // the <AUni>
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Entry abcdefghijk LexemeForm Form"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "8e982d88-0111-43b9-a25c-420bb5c84cf0"));
		}

		private static FieldWorkObjectContextGenerator MakeGenerator()
		{
			var result = new FieldWorkObjectContextGenerator();
			var strategies = new MergeStrategies();
			result.MergeStrategies = strategies;
			strategies.SetStrategy("LexEntry", MakeClassStrategy(new LexEntryContextGenerator(), strategies));
			strategies.SetStrategy("WfiWordform", MakeClassStrategy(new WfiWordformContextGenerator(), strategies));
			strategies.SetStrategy("CmPossibilityList", MakeClassStrategy(new PossibilityListContextGenerator(), strategies));
			strategies.SetStrategy("CmPossibility", MakeClassStrategy(new PossibilityContextGenerator(), strategies));
			strategies.SetStrategy("LexEntryType", MakeClassStrategy(new PossibilityContextGenerator(), strategies));
			strategies.SetStrategy("PhNCSegments", MakeClassStrategy(new NaturalClassesContextGenerator(), strategies));
			return result;
		}

		private static readonly FindByKeyAttribute GuidKey = new FindByKeyAttribute(SharedConstants.GuidStr);

		private static ElementStrategy MakeClassStrategy(FieldWorkObjectContextGenerator descriptor, MergeStrategies strategies)
		{
			var classStrat = new ElementStrategy(false)
			{
				MergePartnerFinder = GuidKey,
				ContextDescriptorGenerator = descriptor,
				IsAtomic = false
			};
			descriptor.MergeStrategies = strategies;
			return classStrat;
		}

		[Test]
		public void WfiWordformPartsFindForm()
		{
			string source =
				@"<WfiWordform
					guid='2a3ccd4f-a2cd-43e5-bd4d-76a84ce00653'>
					<Form>
						<AUni
							ws='jit'>jitWord</AUni>
					</Form>
					<SpellingStatus
						val='0' />
				</WfiWordform>";
			var root = GetNode(source);
			var input = root; // WfiWordform
			var generator = MakeGenerator();
			var descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Wordform jitWord"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "2a3ccd4f-a2cd-43e5-bd4d-76a84ce00653"));

			// Try a child node that isn't a part of the word form
			input = root.ChildNodes[1]; //SpellingStatus
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Wordform jitWord SpellingStatus"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "2a3ccd4f-a2cd-43e5-bd4d-76a84ce00653"));
		}

		[Test]
		public void PossibilityListPartsFindName()
		{
			string source =
				@"	<CmPossibilityList
						guid='1ee09905-63dd-4c7a-a9bd-1d496743ccd6'>
						<Abbreviation>
							<AUni
								ws='en'>Junk</AUni>
						</Abbreviation>
						<Depth
							val='127' />
						<ItemClsid
							val='5118' />
						<Name>
							<AUni
								ws='en'>Complex Form Types</AUni>
						</Name>
						<Possibilities>
							<ownseq
								class='LexEntryType'
								guid='1f6ae209-141a-40db-983c-bee93af0ca3c'>
								<Abbreviation>
									<AUni
										ws='en'>comp. of</AUni>
								</Abbreviation>
								<IsProtected
									val='true' />
								<Name>
									<AUni
										ws='en'>Compound</AUni>
								</Name>
								<ReverseAbbr>
									<AUni
										ws='en'>comp.</AUni>
								</ReverseAbbr>
							</ownseq>
						</Possibilities>
						<PreventDuplicates
							val='true' />
						<WsSelector
							val='-3' />
					</CmPossibilityList>";
			var root = GetNode(source);
			var input = root.ChildNodes[0].ChildNodes[0]; //<Abbreviation><AUni>
			var generator = MakeGenerator();

			// This is the focus of the test:
			var descriptor = generator.GenerateContextDescriptor(input, "myfile"); // myfile is not relevant here.

			Assert.That(descriptor.DataLabel, Is.EqualTo("List 'Complex Form Types' Abbreviation"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "1ee09905-63dd-4c7a-a9bd-1d496743ccd6"));
		}

		[Test]
		public void NaturalClassesPartsFindName()
		{
			string source =
				@"	<ownseq
					class='PhNCSegments'
					guid='085e32ec-eb5b-4eed-9dab-e55854ce88fb'>
					<Abbreviation>
						<AUni
							ws='en'>V+back</AUni>
						<AUni
							ws='pt'>V+back</AUni>
					</Abbreviation>
					<Name>
						<AUni
							ws='en'>back vowels</AUni>
					</Name>
					<Segments>
						<objsur
							guid='14bd1795-d041-4c59-91ae-1d5506c63402'
							t='r' />
						<objsur
							guid='70286ab3-04b7-43ab-afb3-76e978e24142'
							t='r' />
					</Segments>
				</ownseq>";
			var root = GetNode(source);
			var input = root.ChildNodes[0].ChildNodes[0]; //<Abbreviation><AUni>
			var generator = MakeGenerator();

			// This is the focus of the test:
			var descriptor = generator.GenerateContextDescriptor(input, "myfile"); // myfile is not relevant here.

			Assert.That(descriptor.DataLabel, Is.EqualTo("Natural Class 'back vowels' Abbreviation"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "085e32ec-eb5b-4eed-9dab-e55854ce88fb"));

			input = root.ChildNodes[1].ChildNodes[0]; //<Name><AUni>
			descriptor = generator.GenerateContextDescriptor(input, "myfile"); // myfile is not relevant here.
			Assert.That(descriptor.DataLabel, Is.EqualTo("Natural Class 'back vowels' Name"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "085e32ec-eb5b-4eed-9dab-e55854ce88fb"));
		}

		/// <summary>
		/// Given something like a LexEntry containing a LexemeForm containing an MoStemAllomorph containing a Form
		/// which is one or more AUnis, we want to see something like LexemeForm Form.
		/// That is, the MoStemAllomorph level can just be left out.
		/// More generally, all object levels can ust be left out, that is, the ones that have guid attributes.
		/// We don't use the standard names here because we may eventually implement a nicer case for all of LexEntry.
		/// </summary>
		[Test]
		public void UnknownMultiStringPathOmitsObjectLevels()
		{
			string source =
				@"<LexEntry
					guid='01efa516-1749-4b60-b43d-00089269e7c5'>
					<HomographNumber
						val='0' />
					<Outer>
						<Mid
							guid='8e982d88-0111-43b9-a25c-420bb5c84cf0'>
							<Target>
								<AUni ws='en'>abcdefghijk</AUni>
								<AUni ws='fr'>other</AUni>
							</Target>
							<IsAbstract
								val='False' />
							<MorphType>
								<objsur
									guid='d7f713e4-e8cf-11d3-9764-00c04f186933'
									t='r' />
							</MorphType>
						</Mid>
					</Outer>
				</LexEntry>";
			var root = GetNode(source);
			var input = root.ChildNodes[1].ChildNodes[0].ChildNodes[0]; // the Target element.
			var generator = MakeGenerator();
			var descriptor = generator.GenerateContextDescriptor(input, "myfile"); // myfile is not relevant here.
			Assert.That(descriptor.DataLabel, Is.EqualTo("Entry Outer Target"));
		}

		/// <summary>
		/// Given something like a LexEntry containing Senses containing a Gloss
		/// which is one or more AUnis, we want to see something like Sense 1.Gloss en: {text}.
		/// We don't use the standard names here because we may eventually implement a nicer case for all of LexEntry.
		/// </summary>
		[Test]
		public void UnknownMultiStringHandlesOwnSeq()
		{
			string source =
				@"<LexEntry	guid='01efa516-1749-4b60-b43d-00089269e7c5'>
					<HomographNumber
						val='0' />
					<SeqProp>
						<ownseq
							class='SomeClass'
							guid='9ad7591f-e475-43ab-bc21-db082e3a12e5'>
							<Target>
								<AUni ws='en'>first</AUni>
								<AUni ws='fr'>second</AUni>
							</Target>
						</ownseq>
						<ownseq
							class='SomeClass'
							guid='9ad7591f-e475-43ab-bc21-db082e3a12e6'>
							<Target>
								<AUni ws='en'>third</AUni>
								<AUni ws='fr'>fourth</AUni>
							</Target>
						</ownseq>
					</SeqProp>
				</LexEntry>";
			var root = GetNode(source);
			var input = root.ChildNodes[1].ChildNodes[1].ChildNodes[0]; // the Target element (in the second objseq).
			var generator = MakeGenerator();
			var descriptor = generator.GenerateContextDescriptor(input, "myfile"); // myfile is not relevant here.
			Assert.That(descriptor.DataLabel, Is.EqualTo("Entry SeqProp 2 Target"));
		}

		[Test]
		public void TestFirstNonBlankChildsData()
		{
			string source =
				@"	<ArbitraryLabel
						guid='1ee09905-63dd-4c7a-a9bd-1d496743ccd6'>
						<Name>
							<AUni ws='fr'></AUni>
							<AUni ws='fa'>   </AUni>
							<AUni ws='hi'>	</AUni>
							<AUni ws='en'>The Name</AUni>
						</Name>
					</ArbitraryLabel>";
			var root = GetNode(source);
			var input = root.ChildNodes[0];
			var generator = new FieldWorkObjectContextGenerator();

			Assert.That(generator.FirstNonBlankChildsData(input), Is.EqualTo("The Name"));
		}

		[Test]
		public void PossibilityListPartsFindAbbreviationIfNoName()
		{
			string source =
				@"	<CmPossibilityList
						guid='1ee09905-63dd-4c7a-a9bd-1d496743ccd6'>
						<Abbreviation>
							<AUni ws='fr'></AUni>
							<AUni ws='fa'>   </AUni>
							<AUni ws='hi'>	</AUni>
							<AUni ws='en'>Some Random Value</AUni>
						</Abbreviation>
						<Depth
							val='127' />
						<ItemClsid
							val='5118' />
						<Possibilities>
							<ownseq
								class='LexEntryType'
								guid='1f6ae209-141a-40db-983c-bee93af0ca3c'>
								<Abbreviation>
									<AUni
										ws='en'>comp. of</AUni>
								</Abbreviation>
								<IsProtected
									val='true' />
								<Name>
									<AUni
										ws='en'>Compound</AUni>
								</Name>
								<ReverseAbbr>
									<AUni
										ws='en'>comp.</AUni>
								</ReverseAbbr>
							</ownseq>
						</Possibilities>
						<PreventDuplicates
							val='true' />
						<WsSelector
							val='-3' />
					</CmPossibilityList>";
			var root = GetNode(source);
			var input = root.ChildNodes[2]; // ItemClsid
			var generator = MakeGenerator();

			// This is the focus of the test:
			var descriptor = generator.GenerateContextDescriptor(input, "myfile"); // myfile is not relevant here.

			Assert.That(descriptor.DataLabel, Is.EqualTo("List 'Some Random Value' ItemClsid"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "1ee09905-63dd-4c7a-a9bd-1d496743ccd6"));
		}

		[Test]
		public void PossibilityFindName()
		{
			string source =
				@"	<CmPossibilityList
						guid='1ee09905-63dd-4c7a-a9bd-1d496743ccd6'>
						<Abbreviation>
							<AUni
								ws='en'>Junk</AUni>
						</Abbreviation>
						<Depth
							val='127' />
						<ItemClsid
							val='5118' />
						<Name>
							<AUni
								ws='en'>Complex Form Types</AUni>
						</Name>
						<Possibilities>
							<ownseq
								class='LexEntryType'
								guid='1f6ae209-141a-40db-983c-bee93af0ca3c'>
								<Abbreviation>
									<AUni
										ws='en'>comp. of</AUni>
								</Abbreviation>
								<IsProtected
									val='true' />
								<Name>
									<AUni
										ws='en'>Compound</AUni>
								</Name>
								<ReverseAbbr>
									<AUni
										ws='en'>comp.</AUni>
								</ReverseAbbr>
							</ownseq>
						</Possibilities>
						<PreventDuplicates
							val='true' />
						<WsSelector
							val='-3' />
					</CmPossibilityList>";
			var root = GetNode(source);
			var input = root.ChildNodes[4].ChildNodes[0].ChildNodes[3]; // <Possibilities><ownseq><ReverseAbbr>
			var generator = MakeGenerator();

			// This is the focus of the test:
			var descriptor = generator.GenerateContextDescriptor(input, "myfile"); // myfile is not relevant here.

			Assert.That(descriptor.DataLabel, Is.EqualTo("Item 'Compound' from List 'Complex Form Types' ReverseAbbr"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "1f6ae209-141a-40db-983c-bee93af0ca3c"));
		}

		XmlNode GetNode(string input)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(input);
			return doc.DocumentElement;
		}

		/// <summary>
		/// Make sure that GenerateContextDescriptor/PathToUserUnderstandableElement returns the guid of its parent,
		/// even if it is under and Ownseq node.
		/// </summary>
		[Test]
		public void FindGuidOfParentOwnseq()
		{
			string source =
				@"	<CmPossibilityList
						guid='1ee09905-63dd-4c7a-a9bd-1d496743ccd6'>
						<Name>
							<AUni
								ws='en'>Complex Form Types</AUni>
						</Name>
						<Possibilities>
							<ownseq
								class='LexEntryType'
								guid='1f6ae209-141a-40db-983c-bee93af0ca3c'>
								<Name>
									<AUni
										ws='en'>Compound</AUni>
								</Name>
								<ReverseAbbr>
									<AUni
										ws='en'>comp.</AUni>
								</ReverseAbbr>
							</ownseq>
						</Possibilities>
					</CmPossibilityList>";
			var root = GetNode(source);
			var input = root.ChildNodes[1].ChildNodes[0].ChildNodes[1]; // <Possibilities><ownseq><ReverseAbbr>
			var generator = MakeGenerator();

			// This is the focus of the test:
			var descriptor = generator.GenerateContextDescriptor(input, "myfile"); // myfile is not relevant here.

			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "1f6ae209-141a-40db-983c-bee93af0ca3c"));
			Assert.That(descriptor.DataLabel, Is.EqualTo("Item 'Compound' from List 'Complex Form Types' ReverseAbbr"));
		}

		/// <summary>
		/// Make sure that GenerateContextDescriptor/PathToUserUnderstandableElement returns the guid of its parent,
		/// even if it is under and ownseqatomic node.
		/// </summary>
		[Test]
		public void FindGuidOfParentOwnseqatomic()
		{
			const string source =
				 @"<StText
						guid='b314f2f8-ea5e-11de-86b7-0013722f8dec'>
						<Paragraphs>
							<ownseqatomic
								class='StTxtPara'
								guid='b31e7c56-ea5e-11de-85d3-0013722f8dec'>
								<Contents>
									<Str>
										<Run
											ws='en'>Example (English)</Run>
									</Str>
								</Contents>
								<ParseIsCurrent
									val='False' />
							</ownseqatomic>
						</Paragraphs>
					</StText>";
			var root = GetNode(source);
			var input = root.ChildNodes[0].ChildNodes[0].ChildNodes[0]; // <Paragraphs><ownseqatomic><Contents>
			var generator = MakeGenerator();

			// This is the focus of the test:
			var descriptor = generator.GenerateContextDescriptor(input, "myfile"); // myfile is not relevant here.

			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "b31e7c56-ea5e-11de-85d3-0013722f8dec"));
		}

		[Test]
		public void SubPossibilityFindName()
		{
			string source =
				@"<CmPossibilityList
					guid='b0a1eb98-ea5e-11de-888e-0013722f8dec'>
					<Abbreviation>
					  <AUni
							ws='en'>AcDom</AUni>
					</Abbreviation>
					<IsSorted
							val='true' />
					<Name>
					  <AUni
							ws='en'>Academic Domains</AUni>
					</Name>
					<Possibilities>
						<ownseq
							class='CmPossibility'
							guid='b0add746-ea5e-11de-8c5c-0013722f8dec'>
							<Abbreviation>
							  <AUni
									ws='en'>Anat</AUni>
							</Abbreviation>
							<Name>
							  <AUni
									ws='en'>anatomy</AUni>
							</Name>
							<UnderColor
									val='-1073741824' />
						  </ownseq>
						  <ownseq
								class='CmPossibility'
								guid='b0b9c2fe-ea5e-11de-8fed-0013722f8dec'>
							<Abbreviation>
							  <AUni
									ws='en'>Anthro</AUni>
							</Abbreviation>
							<Name>
							  <AUni
									ws='en'>anthropology</AUni>
							</Name>
							<SubPossibilities>
							  <ownseq
									class='CmPossibility'
									guid='b0c5aeac-ea5e-11de-9463-0013722f8dec'>
									<Abbreviation>
									  <AUni
											ws='en'>Cult anthro</AUni>
									</Abbreviation>
									<Name>
									  <AUni
											ws='en'>cultural anthropology</AUni>
									</Name>
								<UnderColor
										val='-1073741824' />
							  </ownseq>
							</SubPossibilities>
							<UnderColor
									val='-1073741824' />
					  </ownseq>
					</Possibilities>
				</CmPossibilityList>";
			var root = GetNode(source);
			var input = root.ChildNodes[3].ChildNodes[1].ChildNodes[2].ChildNodes[0].ChildNodes[2]; // Possibilities><ownseq>[1]<SubPossibilities><ownseq><UnderColor>
			var generator = MakeGenerator();

			// This is the focus of the test:
			var descriptor = generator.GenerateContextDescriptor(input, "myfile"); // myfile is not relevant here.

			Assert.That(descriptor.DataLabel, Is.EqualTo("Item 'cultural anthropology' from List 'Academic Domains' UnderColor"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "b0c5aeac-ea5e-11de-9463-0013722f8dec"));
		}
	}
}
