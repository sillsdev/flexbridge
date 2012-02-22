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
			strategies.SetStrategy("PhEnvironment", MakeClassStrategy(new EnvironmentContextGenerator(), strategies));
			strategies.SetStrategy("DsChart", MakeClassStrategy(new DiscourseChartContextGenerator(), strategies));
			strategies.SetStrategy("DsConstChart", MakeClassStrategy(new DiscourseChartContextGenerator(), strategies));
			strategies.SetStrategy("ConstChartRow", MakeClassStrategy(new DiscourseChartContextGenerator(), strategies));
			strategies.SetStrategy("ConstChartWordGroup", MakeClassStrategy(new DiscourseChartContextGenerator(), strategies));
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
		/// More generally, all object levels can just be left out, that is, the ones that have guid attributes.
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

		[Test]
		public void EnvironmentsPartsFindEnvironment()
		{
			string source =
@"<PhonologicalData>
	<PhPhonData
		guid='86980496-9fe8-428d-bd53-fa2e623fe1c0'>
		<Environments>
			<ownseq
				class='PhEnvironment'
				guid='02132942-0ff7-45e8-8f09-f4918535a31e'>
				<Description>
					<AStr
						ws='en'>
						<Run
							ws='en'>used for e&gt;i change on vext</Run>
					</AStr>
				</Description>
				<Name>
					<AUni
						ws='en'>mid vowel in previous syllable</AUni>
				</Name>
				<StringRepresentation>
					<Str>
						<Run
							ws='en'>/[</Run>
						<Run
							ws='en'>V+mid] ([preNas]) [C-nas] ([Mod]) _</Run>
					</Str>
				</StringRepresentation>
			</ownseq>
			<ownseq
				class='PhEnvironment'
				guid='1ae6eb4a-84a0-4134-b684-5b446ad83708'>
				<StringRepresentation>
					<Str>
						<Run
							ws='en'>/ [C] _</Run>
					</Str>
				</StringRepresentation>
			</ownseq>
			<ownseq
				class='PhEnvironment'
				guid='e5e81505-6f2b-42df-88dc-a76c2dfcad87'>
				<StringRepresentation>
					<Str>
						<Run
							ws='en'>/ _ [-Lab-Lat-Nas</Run>
						<Run
							ws='en'>]</Run>
					</Str>
				</StringRepresentation>
			</ownseq>
		</Environments>
	</PhPhonData>
</PhonologicalData>";
			var root = GetNode(source); // PhonologicalData
			var input = root.ChildNodes[0].ChildNodes[0].ChildNodes[0]; // 1st ownseq
			var generator = MakeGenerator();
			var descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Environment mid vowel in previous syllable"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "02132942-0ff7-45e8-8f09-f4918535a31e"));

			// Try a node that has no name, only a representation.
			input = root.ChildNodes[0].ChildNodes[0].ChildNodes[1]; // 2nd ownseq
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Environment / [C] _"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "1ae6eb4a-84a0-4134-b684-5b446ad83708"));

			// Try the Description to see we still get the name
			input = root.ChildNodes[0].ChildNodes[0].ChildNodes[0].ChildNodes[0]; // the <Description>
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Environment mid vowel in previous syllable Description"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "02132942-0ff7-45e8-8f09-f4918535a31e"));

			// See that the runs are merged
			input = root.ChildNodes[0].ChildNodes[0].ChildNodes[2]; // 3rd ownseq
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Environment / _ [-Lab-Lat-Nas]"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "e5e81505-6f2b-42df-88dc-a76c2dfcad87"));

			// See that the first run is prepended to the last one
			input = root.ChildNodes[0].ChildNodes[0].ChildNodes[2].ChildNodes[0].ChildNodes[0].ChildNodes[1]; // 3rd ownseq, 2nd Run
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Environment / _ [-Lab-Lat-Nas] StringRepresentation Str Run"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "e5e81505-6f2b-42df-88dc-a76c2dfcad87"));
		}
		[Test]
		public void DiscoursePartsFindChart()
		{
			string source =
@"<Discourse>
	<header>
		<DsDiscourseData
			guid='aae5a99c-2333-4d6f-934e-d2c059de249a'>
			<ChartMarkers />
			<Charts />
			<ConstChartTempl />
		</DsDiscourseData>
	</header>
	<DsChart
		class='DsConstChart'
		guid='78a7df6d-4e58-47ac-8714-52aeeca8b66c'>
		<BasedOn>
			<objsur
				guid='95663db0-64d9-41f8-ab11-e388ce54e3f2'
				t='r' />
		</BasedOn>
		<DateCreated
			val='2009-4-9 16:43:0.290' />
		<DateModified
			val='2009-4-23 20:21:42.447' />
		<Rows>
			<ownseq
				class='ConstChartRow'
				guid='367829ee-3b53-493a-a057-1eccde2c45e4'>
				<Cells>
					<ownseq
						class='ConstChartWordGroup'
						guid='991d2812-1b2c-46ac-bb74-12819e45ce9a'>
						<BeginAnalysisIndex
							val='0' />
						<BeginSegment>
							<objsur
								guid='efa04f83-2d6d-4dd7-90df-d057b4aefe05'
								t='r' />
						</BeginSegment>
						<Column>
							<objsur
								guid='f6468942-5e24-4dcb-9d54-c8a634b07a5a'
								t='r' />
						</Column>
						<EndAnalysisIndex
							val='1' />
						<EndSegment>
							<objsur
								guid='efa04f83-2d6d-4dd7-90df-d057b4aefe05'
								t='r' />
						</EndSegment>
						<MergesAfter
							val='False' />
						<MergesBefore
							val='False' />
					</ownseq>
					<ownseq
						class='ConstChartWordGroup'
						guid='870c0faf-1eb7-4a52-9172-9bb9d338017c'>
						<BeginAnalysisIndex
							val='3' />
						<BeginSegment>
							<objsur
								guid='efa04f83-2d6d-4dd7-90df-d057b4aefe05'
								t='r' />
						</BeginSegment>
						<Column>
							<objsur
								guid='664b434a-e7b1-4c02-8970-d89c09d43c54'
								t='r' />
						</Column>
						<EndAnalysisIndex
							val='4' />
						<EndSegment>
							<objsur
								guid='efa04f83-2d6d-4dd7-90df-d057b4aefe05'
								t='r' />
						</EndSegment>
						<MergesAfter
							val='False' />
						<MergesBefore
							val='False' />
					</ownseq>
				</Cells>
				<ClauseType
					val='0' />
				<EndDependentClauseGroup
					val='False' />
				<EndParagraph
					val='False' />
				<EndSentence
					val='True' />
				<Label>
					<Str>
						<Run
							ws='en'>1</Run>
					</Str>
				</Label>
				<StartDependentClauseGroup
					val='False' />
			</ownseq>
			<ownseq
				class='ConstChartRow'
				guid='449ab63e-33b1-43e8-a7bb-b1fe517b0e7e'>
				<Cells>
					<ownseq
						class='ConstChartWordGroup'
						guid='1d3102c9-7f9d-499f-a85c-22d3c3b1af04'>
						<BeginAnalysisIndex
							val='0' />
						<BeginSegment>
							<objsur
								guid='83360f37-ff92-4910-8ceb-360d51c342c2'
								t='r' />
						</BeginSegment>
						<Column>
							<objsur
								guid='39775c44-e64a-4fb1-b4ca-d5da3d7dbb24'
								t='r' />
						</Column>
						<EndAnalysisIndex
							val='0' />
						<EndSegment>
							<objsur
								guid='83360f37-ff92-4910-8ceb-360d51c342c2'
								t='r' />
						</EndSegment>
						<MergesAfter
							val='False' />
						<MergesBefore
							val='False' />
					</ownseq>
					<ownseq
						class='ConstChartWordGroup'
						guid='3f371913-8108-4e06-8d7d-a5f0fe69c413'>
						<BeginAnalysisIndex
							val='1' />
						<BeginSegment>
							<objsur
								guid='83360f37-ff92-4910-8ceb-360d51c342c2'
								t='r' />
						</BeginSegment>
						<Column>
							<objsur
								guid='7e958a20-1512-4af9-863a-d92f7175a68c'
								t='r' />
						</Column>
						<EndAnalysisIndex
							val='1' />
						<EndSegment>
							<objsur
								guid='83360f37-ff92-4910-8ceb-360d51c342c2'
								t='r' />
						</EndSegment>
						<MergesAfter
							val='False' />
						<MergesBefore
							val='False' />
					</ownseq>
				</Cells>
				<ClauseType
					val='0' />
				<EndDependentClauseGroup
					val='False' />
				<EndParagraph
					val='False' />
				<EndSentence
					val='False' />
				<Label>
					<Str>
						<Run
							ws='en'>2a</Run>
					</Str>
				</Label>
				<StartDependentClauseGroup
					val='False' />
			</ownseq>
		</Rows>
		<Template>
			<objsur
				guid='cb178bdb-97b8-49e2-9436-2f188a2b8589'
				t='r' />
		</Template>
	</DsChart>
	<DsChart
		class='DsConstChart'
		guid='88482f27-8a9f-4d25-b375-2c8720aa2b62'>
		<BasedOn>
			<objsur
				guid='40b3e88f-ae59-48d0-a2ba-e498a27cc9b3'
				t='r' />
		</BasedOn>
		<DateCreated
			val='2008-4-8 21:34:30.653' />
		<DateModified
			val='2008-4-8 21:34:30.653' />
		<Template>
			<objsur
				guid='cb178bdb-97b8-49e2-9436-2f188a2b8589'
				t='r' />
		</Template>
	</DsChart>
</Discourse>";
			var root = GetNode(source); // Discourse
			var generator = MakeGenerator();

			// 2nd DsChart
			var input = root.ChildNodes[2];
			var descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 2"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "88482f27-8a9f-4d25-b375-2c8720aa2b62"));

			// 1st DsChart BasedOn
			input = root.ChildNodes[1].ChildNodes[0];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 BasedOn"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "78a7df6d-4e58-47ac-8714-52aeeca8b66c"));

			// 2nd DsChart DateCreated
			input = root.ChildNodes[2].ChildNodes[1];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 2 DateCreated"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "88482f27-8a9f-4d25-b375-2c8720aa2b62"));

			// 2nd DsChart DateModified
			input = root.ChildNodes[2].ChildNodes[2];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 2 DateModified"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "88482f27-8a9f-4d25-b375-2c8720aa2b62"));

			// 1st DsChart Template
			input = root.ChildNodes[1].ChildNodes[4];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 Template"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "78a7df6d-4e58-47ac-8714-52aeeca8b66c"));

			// 1st DsChart Rows
			input = root.ChildNodes[1].ChildNodes[3];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 Rows"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "78a7df6d-4e58-47ac-8714-52aeeca8b66c"));

			// 1st DsChart Row 2 Cells
			input = root.ChildNodes[1].ChildNodes[3].ChildNodes[1].ChildNodes[0];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 Row 2 Cells"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "449ab63e-33b1-43e8-a7bb-b1fe517b0e7e"));

			// 1st DsChart Row 1 Column 2
			input = root.ChildNodes[1].ChildNodes[3].ChildNodes[0].ChildNodes[0].ChildNodes[1].ChildNodes[2];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			// Discourse Chart 1 Row 1 Column 2 Column <== should be a way to suppreee FieldWorkObjectContextGenerator.GetLabel()/GetPathAppend()
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 Row 1 Column 2 Column"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "870c0faf-1eb7-4a52-9172-9bb9d338017c"));

			// 1st DsChart Row 1 Column 2 BeginSegment
			input = root.ChildNodes[1].ChildNodes[3].ChildNodes[0].ChildNodes[0].ChildNodes[1].ChildNodes[1];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 Row 1 Column 2 BeginSegment"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "870c0faf-1eb7-4a52-9172-9bb9d338017c"));

			// 1st DsChart Row 2 Column 2 EndSegment
			input = root.ChildNodes[1].ChildNodes[3].ChildNodes[1].ChildNodes[0].ChildNodes[1].ChildNodes[4];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 Row 2 Column 2 EndSegment"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "3f371913-8108-4e06-8d7d-a5f0fe69c413"));

			// 1st DsChart Row 2 Column 1 BeginAnalysisIndex
			input = root.ChildNodes[1].ChildNodes[3].ChildNodes[1].ChildNodes[0].ChildNodes[0].ChildNodes[0];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 Row 2 Column 1 BeginAnalysisIndex"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "1d3102c9-7f9d-499f-a85c-22d3c3b1af04"));

			// 1st DsChart Row 1 Column 1 EndAnalysisIndex
			input = root.ChildNodes[1].ChildNodes[3].ChildNodes[0].ChildNodes[0].ChildNodes[0].ChildNodes[3];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 Row 1 Column 1 EndAnalysisIndex"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "991d2812-1b2c-46ac-bb74-12819e45ce9a"));

			// 1st DsChart Row 2 Column 2 MergesBefore
			input = root.ChildNodes[1].ChildNodes[3].ChildNodes[1].ChildNodes[0].ChildNodes[1].ChildNodes[6];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 Row 2 Column 2 MergesBefore"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "3f371913-8108-4e06-8d7d-a5f0fe69c413"));

			// 1st DsChart Row 1 Column 2 MergesAfter
			input = root.ChildNodes[1].ChildNodes[3].ChildNodes[0].ChildNodes[0].ChildNodes[1].ChildNodes[5];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 Row 1 Column 2 MergesAfter"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "870c0faf-1eb7-4a52-9172-9bb9d338017c"));

			// 1st DsChart Row 1 ClauseType
			input = root.ChildNodes[1].ChildNodes[3].ChildNodes[0].ChildNodes[1];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 Row 1 ClauseType"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "367829ee-3b53-493a-a057-1eccde2c45e4"));

			// 1st DsChart Row 2 EndDependentClauseGroup
			input = root.ChildNodes[1].ChildNodes[3].ChildNodes[1].ChildNodes[2];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 Row 2 EndDependentClauseGroup"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "449ab63e-33b1-43e8-a7bb-b1fe517b0e7e"));

			// 1st DsChart Row 2 EndParagraph
			input = root.ChildNodes[1].ChildNodes[3].ChildNodes[1].ChildNodes[3];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 Row 2 EndParagraph"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "449ab63e-33b1-43e8-a7bb-b1fe517b0e7e"));

			// 1st DsChart Row 1 EndSentence
			input = root.ChildNodes[1].ChildNodes[3].ChildNodes[0].ChildNodes[4];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 Row 1 EndSentence"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "367829ee-3b53-493a-a057-1eccde2c45e4"));

			// 1st DsChart Row 1 Label
			input = root.ChildNodes[1].ChildNodes[3].ChildNodes[0].ChildNodes[5];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 Row 1 Label"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "367829ee-3b53-493a-a057-1eccde2c45e4"));

			// 1st DsChart Row 2 StartDependentClauseGroup
			input = root.ChildNodes[1].ChildNodes[3].ChildNodes[1].ChildNodes[6];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Discourse Chart 1 Row 2 StartDependentClauseGroup"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "449ab63e-33b1-43e8-a7bb-b1fe517b0e7e"));
		}

	}
}
