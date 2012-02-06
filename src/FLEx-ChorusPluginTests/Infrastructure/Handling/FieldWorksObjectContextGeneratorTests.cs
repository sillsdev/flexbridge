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
			var input = root.ChildNodes[1].ChildNodes[0];
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

		XmlNode GetNode(string input)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(input);
			return doc.DocumentElement;
		}
	}
}
