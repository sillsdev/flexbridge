using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
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
			var generator = new FieldWorkObjectContextGenerator();
			var descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Entry abcdefghijk"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label="+ descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "8e982d88-0111-43b9-a25c-420bb5c84cf0"));

			// Try a node that is not part of the LexemeForm.
			input = root.ChildNodes[0];
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Entry abcdefghijk"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "01efa516-1749-4b60-b43d-00089269e7c5"));
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
			var generator = new FieldWorkObjectContextGenerator();
			var descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Wordform jitWord"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "2a3ccd4f-a2cd-43e5-bd4d-76a84ce00653"));

			// Try a child node that isn't a part of the word form
			input = root.ChildNodes[1]; //SpellingStatus
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Wordform jitWord"));
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
			var generator = new FieldWorkObjectContextGenerator();

			// This is the focus of the test:
			var descriptor = generator.GenerateContextDescriptor(input, "myfile"); // myfile is not relevant here.

			Assert.That(descriptor.DataLabel, Is.EqualTo("List 'Complex Form Types'"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "1ee09905-63dd-4c7a-a9bd-1d496743ccd6"));
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
			var generator = new FieldWorkObjectContextGenerator();

			// This is the focus of the test:
			var descriptor = generator.GenerateContextDescriptor(input, "myfile"); // myfile is not relevant here.

			Assert.That(descriptor.DataLabel, Is.EqualTo("List 'Some Random Value'"));
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
			var generator = new FieldWorkObjectContextGenerator();

			// This is the focus of the test:
			var descriptor = generator.GenerateContextDescriptor(input, "myfile"); // myfile is not relevant here.

			Assert.That(descriptor.DataLabel, Is.EqualTo("Item 'Compound' from List 'Complex Form Types'"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "1f6ae209-141a-40db-983c-bee93af0ca3c"));
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
			var generator = new FieldWorkObjectContextGenerator();

			// This is the focus of the test:
			var descriptor = generator.GenerateContextDescriptor(input, "myfile"); // myfile is not relevant here.

			Assert.That(descriptor.DataLabel, Is.EqualTo("Item 'cultural anthropology' from List 'Academic Domains'"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "b0c5aeac-ea5e-11de-9463-0013722f8dec"));
		}

		XmlNode GetNode(string input)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(input);
			return doc.DocumentElement;
		}
	}
}
