using FLEx_ChorusPlugin.Infrastructure.Handling.Linguistics.MorphologyAndSyntax;
using NUnit.Framework;

namespace FLEx_ChorusPluginTests.Infrastructure.Handling.Linguistics.MorphologyAndSyntax
{
	[TestFixture]
	public class PosContextGeneratorTests
	{
		[Test]
		public void GetPartOfSpeechLabel()
		{
			const string source = @"<LexEntry guid='89942b8e-2b1e-4074-8641-1abca93982f8'>
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
			var root = FieldWorksTestServices.GetNode(source);
			var input = root.ChildNodes[2].ChildNodes[0].ChildNodes[0]; // MorphoSyntaxAnalysis
			var generator = new PosContextGenerator();
			var descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Entry \"conflict\" Grammatical Info."));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "4bd15611-5a36-422e-baa6-b6edb943c4da"));

			// verify the html context generation
			Assert.That(generator.HtmlContext(input),
				Is.EqualTo(@"<div class='guid'>Guid of part of speech: 3ecbfcc8-76d7-43bc-a5ff-3c47fabf355c</div>"));
		}
	}
}