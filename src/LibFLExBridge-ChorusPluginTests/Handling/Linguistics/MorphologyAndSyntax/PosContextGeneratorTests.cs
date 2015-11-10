// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Xml;
using Chorus.merge.xml.generic;
using LibFLExBridgeChorusPlugin;
using LibFLExBridgeChorusPlugin.Handling.Linguistics.MorphologyAndSyntax;
using NUnit.Framework;

namespace LibFLExBridgeChorusPluginTests.Handling.Linguistics.MorphologyAndSyntax
{
	[TestFixture]
	public class PosContextGeneratorTests
	{
		[Test]
		public void GetMoreCompletePartOfSpeechLabel()
		{
			const string source = @"
			  <LexEntry guid='89942b8e-2b1e-4074-8641-1abca93982f8'>
				<LexemeForm>
				  <MoStemAllomorph guid='4109d3d2-faf4-4f80-b28c-f8e4e0146c11'>
					<Form>
					  <AUni ws='seh'>conflict</AUni>
					</Form>
				  </MoStemAllomorph>
				</LexemeForm>
				<MorphoSyntaxAnalyses>
				  <MoStemMsa guid='54699bf4-7285-4f91-9f65-59b8dab40031'>
					<PartOfSpeech>
					  <objsur guid='8e45de56-5105-48dc-b302-05985432e1e7' t='r' />
					</PartOfSpeech>
				  </MoStemMsa>
				  <MoStemMsa guid='156875ac-9f6a-4bab-9979-e914d8a062fc'>
					<PartOfSpeech>
					  <objsur guid='3ecbfcc8-76d7-43bc-a5ff-3c47fabf355c' t='r' />
					</PartOfSpeech>
				  </MoStemMsa>
				</MorphoSyntaxAnalyses>
				<Senses>
				  <ownseq class='LexSense' guid='4bd15611-5a36-422e-baa6-b6edb943c4da'>
					<MorphoSyntaxAnalysis>
					  <objsur guid='156875ac-9f6a-4bab-9979-e914d8a062fc' t='r' />
					</MorphoSyntaxAnalysis>
				  </ownseq>
				</Senses>
			  </LexEntry>";
			const string posList =
			  @"<?xml version='1.0' encoding='utf-8'?>
			  <PartsOfSpeech>
				<CmPossibilityList guid='d7f7150c-e8cf-11d3-9764-00c04f186933'>
				  <Abbreviation>
					<AUni ws='en'>Pos</AUni>
				  </Abbreviation>
				  <ItemClsid val='5049' />
				  <Name>
					<AUni ws='en'>Parts Of Speech</AUni>
				  </Name>
				  <Possibilities>
					<ownseq class='PartOfSpeech' guid='d7f7150d-e8cf-11d3-9764-00c04f186933'>
					  <Abbreviation>
						<AUni ws='en'>V</AUni>
					  </Abbreviation>
					  <Name>
						<AUni ws='en'></AUni>
					  </Name>
					</ownseq>
					<ownseq class='PartOfSpeech' guid='00a10735-bd2c-4bc5-9555-ef9f784a8c8c'>
					  <Abbreviation>
						<AUni ws='en'>Adv</AUni>
						<AUni ws='es'>Adv</AUni>
						<AUni ws='fr'>Adv</AUni>
					  </Abbreviation>
					  <Name>
						<AUni ws='en'>Adverb</AUni>
						<AUni ws='es'>Adverbo</AUni>
						<AUni ws='fr'>Adverbe</AUni>
					  </Name>
					</ownseq>
					<ownseq class='PartOfSpeech' guid='3ecbfcc8-76d7-43bc-a5ff-3c47fabf355c'>
					  <Abbreviation>
						<AUni ws='en'>N</AUni>
					  </Abbreviation>
					  <Name>
						<AUni ws='en'>Noun</AUni>
					  </Name>
					</ownseq>
				  </Possibilities>
				</CmPossibilityList>
			  </PartsOfSpeech>";
			var root = FieldWorksTestServices.GetNode(source);
			var morphSynData = FieldWorksTestServices.GetNode(posList);
			var input = root.ChildNodes[2].ChildNodes[0].ChildNodes[0]; // MorphoSyntaxAnalysis
			var generator = new MockPosContextGenerator(morphSynData);
			var descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Entry \"conflict\" Noun"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "4bd15611-5a36-422e-baa6-b6edb943c4da"));

			// verify the html context generation on the objsur element
			Assert.That(generator.HtmlContext(input.ChildNodes[0]),
				Is.EqualTo(@"<div class='PartOfSpeech'>Cat: Noun</div>"));
		}

		[Test]
		public void GetMoreCompletePartOfSpeechLabel_MultilevelPosList()
		{
			const string source = @"
			  <LexEntry guid='89942b8e-2b1e-4074-8641-1abca93982f8'>
				<LexemeForm>
				  <MoStemAllomorph guid='4109d3d2-faf4-4f80-b28c-f8e4e0146c11'>
					<Form>
					  <AUni ws='seh'>conflict</AUni>
					</Form>
				  </MoStemAllomorph>
				</LexemeForm>
				<MorphoSyntaxAnalyses>
				  <MoStemMsa guid='54699bf4-7285-4f91-9f65-59b8dab40031'>
					<PartOfSpeech>
					  <objsur guid='8e45de56-5105-48dc-b302-05985432e1e7' t='r' />
					</PartOfSpeech>
				  </MoStemMsa>
				  <MoStemMsa guid='156875ac-9f6a-4bab-9979-e914d8a062fc'>
					<PartOfSpeech>
					  <objsur guid='00a10735-bd2c-4bc5-9555-ef9f784a8c8c' t='r' />
					</PartOfSpeech>
				  </MoStemMsa>
				</MorphoSyntaxAnalyses>
				<Senses>
				  <ownseq class='LexSense' guid='4bd15611-5a36-422e-baa6-b6edb943c4da'>
					<MorphoSyntaxAnalysis>
					  <objsur guid='156875ac-9f6a-4bab-9979-e914d8a062fc' t='r' />
					</MorphoSyntaxAnalysis>
				  </ownseq>
				</Senses>
			  </LexEntry>";
			const string posList =
			  @"<?xml version='1.0' encoding='utf-8'?>
			  <PartsOfSpeech>
				<CmPossibilityList guid='d7f7150c-e8cf-11d3-9764-00c04f186933'>
				  <Abbreviation>
					<AUni ws='en'>Pos</AUni>
				  </Abbreviation>
				  <ItemClsid val='5049' />
				  <Name>
					<AUni ws='en'>Parts Of Speech</AUni>
				  </Name>
				  <Possibilities>
					<ownseq class='PartOfSpeech' guid='d7f7150d-e8cf-11d3-9764-00c04f186933'>
					  <Abbreviation>
						<AUni ws='en'>V</AUni>
					  </Abbreviation>
					  <Name>
						<AUni ws='en'></AUni>
					  </Name>
					  <SubPossibilities>
						<ownseq class='PartOfSpeech' guid='00a10735-bd2c-4bc5-9555-ef9f784a8c8c'>
						  <Abbreviation>
							<AUni ws='en'>Adv</AUni>
						  </Abbreviation>
						  <Name>
							<AUni ws='en'>Adverb</AUni>
						  </Name>
						</ownseq>
					  </SubPossibilities>
					</ownseq>
					<ownseq class='PartOfSpeech' guid='3ecbfcc8-76d7-43bc-a5ff-3c47fabf355c'>
					  <Abbreviation>
						<AUni ws='en'>N</AUni>
					  </Abbreviation>
					  <Name>
						<AUni ws='en'>Noun</AUni>
					  </Name>
					</ownseq>
				  </Possibilities>
				</CmPossibilityList>
			  </PartsOfSpeech>";
			var root = FieldWorksTestServices.GetNode(source);
			var morphSynData = FieldWorksTestServices.GetNode(posList);
			var input = root.ChildNodes[2].ChildNodes[0].ChildNodes[0]; // MorphoSyntaxAnalysis
			var generator = new MockPosContextGenerator(morphSynData);
			var descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Entry \"conflict\" Adverb"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "4bd15611-5a36-422e-baa6-b6edb943c4da"));

			// verify the html context generation on the objsur element
			Assert.That(generator.HtmlContext(input.ChildNodes[0]),
				Is.EqualTo(@"<div class='PartOfSpeech'>Cat: Adverb</div>"));
		}

		[Test]
		public void GetMoreCompletePartOfSpeechLabel_ToFromCase()
		{
			const string source = @"
			  <LexEntry guid='89942b8e-2b1e-4074-8641-1abca93982f8'>
				<AlternateForms>
				  <objsur guid='bcbc9cd6-ca73-4fb9-a427-12c5a3c7d1ae' t='o' />
				</AlternateForms>
				<LexemeForm>
				  <MoStemAllomorph guid='4109d3d2-faf4-4f80-b28c-f8e4e0146c11'>
					<Form>
					  <AUni ws='seh'>conflict</AUni>
					</Form>
				  </MoStemAllomorph>
				</LexemeForm>
				<MorphoSyntaxAnalyses>
				  <MoStemMsa guid='54699bf4-7285-4f91-9f65-59b8dab40031'>
					<PartOfSpeech>
					  <objsur guid='8e45de56-5105-48dc-b302-05985432e1e7' t='r' />
					</PartOfSpeech>
				  </MoStemMsa>
				  <MoDerivAffMsa guid='156875ac-9f6a-4bab-9979-e914d8a062fc'>
					<FromMsFeatures/>
					<FromPartOfSpeech>
					  <objsur guid='3ecbfcc8-76d7-43bc-a5ff-3c47fabf355c' t='r' />
					</FromPartOfSpeech>
					<ToMsFeatures/>
					<ToPartOfSpeech>
					  <objsur guid='00a10735-bd2c-4bc5-9555-ef9f784a8c8c' t='r' />
					</ToPartOfSpeech>
				  </MoDerivAffMsa>
				</MorphoSyntaxAnalyses>
				<Senses>
				  <ownseq class='LexSense' guid='4bd15611-5a36-422e-baa6-b6edb943c4da'>
					<MorphoSyntaxAnalysis>
					  <objsur guid='156875ac-9f6a-4bab-9979-e914d8a062fc' t='r' />
					</MorphoSyntaxAnalysis>
				  </ownseq>
				</Senses>
			  </LexEntry>";
			const string posList =
			  @"<?xml version='1.0' encoding='utf-8'?>
			  <PartsOfSpeech>
				<CmPossibilityList guid='d7f7150c-e8cf-11d3-9764-00c04f186933'>
				  <Abbreviation>
					<AUni ws='en'>Pos</AUni>
				  </Abbreviation>
				  <ItemClsid val='5049' />
				  <Name>
					<AUni ws='en'>Parts Of Speech</AUni>
				  </Name>
				  <Possibilities>
					<ownseq class='PartOfSpeech' guid='d7f7150d-e8cf-11d3-9764-00c04f186933'>
					  <Abbreviation>
						<AUni ws='en'>V</AUni>
					  </Abbreviation>
					  <Name>
						<AUni ws='en'></AUni>
					  </Name>
					</ownseq>
					<ownseq class='PartOfSpeech' guid='00a10735-bd2c-4bc5-9555-ef9f784a8c8c'>
					  <Abbreviation>
						<AUni ws='en'>Adv</AUni>
						<AUni ws='es'>Adv</AUni>
						<AUni ws='fr'>Adv</AUni>
					  </Abbreviation>
					  <Name>
						<AUni ws='en'>Adverb</AUni>
						<AUni ws='es'>Adverbo</AUni>
						<AUni ws='fr'>Adverbe</AUni>
					  </Name>
					</ownseq>
					<ownseq class='PartOfSpeech' guid='3ecbfcc8-76d7-43bc-a5ff-3c47fabf355c'>
					  <Abbreviation>
						<AUni ws='en'>N</AUni>
					  </Abbreviation>
					  <Name>
						<AUni ws='en'>Noun</AUni>
					  </Name>
					</ownseq>
				  </Possibilities>
				</CmPossibilityList>
			  </PartsOfSpeech>";
			var root = FieldWorksTestServices.GetNode(source);
			var morphSynData = FieldWorksTestServices.GetNode(posList);
			var input = root.ChildNodes[3].ChildNodes[0].ChildNodes[0]; // MorphoSyntaxAnalysis
			var generator = new MockPosContextGenerator(morphSynData);
			var descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Entry \"conflict\" Noun/Adverb:Adverbo:Adverbe"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "4bd15611-5a36-422e-baa6-b6edb943c4da"));

			// verify the html context generation on the objsur element
			Assert.That(generator.HtmlContext(input.ChildNodes[0]),
				Is.EqualTo(@"<div class='PartOfSpeech'>Cat: Noun/Adverb:Adverbo:Adverbe</div>"));
		}
	}

	/// <summary>
	/// Wrapper to allow tests to use expanded PosContextGenerator function.
	/// </summary>
	public class MockPosContextGenerator
	{
		internal PosContextGenerator m_generator;

		public MockPosContextGenerator(XmlNode morphSynData)
		{
			m_generator = new PosContextGenerator();
			LoadPosList(morphSynData);
		}

		internal void LoadPosList(XmlNode data)
		{
			m_generator.LoadPosList(data);
		}

		internal ContextDescriptor GenerateContextDescriptor(XmlNode input, string filepath)
		{
			return m_generator.GenerateContextDescriptor(input, filepath);
		}

		internal string HtmlContext(XmlNode input)
		{
			return m_generator.HtmlContext(input);
		}
	}
}