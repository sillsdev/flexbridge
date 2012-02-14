using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Infrastructure.Handling;
using NUnit.Framework;

namespace FLEx_ChorusPluginTests.Infrastructure.Handling
{
	/// <summary>
	/// Tests the FieldWorksObjectContextGenerator.HtmlContext methods
	/// </summary>
	[TestFixture]
	public class FieldWorksObjectContextGeneratorHtmlTests
	{
		/// <summary>
		/// Given something like a LexEntry containing a LexemeForm containing an MoStemAllomorph containing a Form
		/// which is one or more AUnis, we want to see something like LexemeForm.MoStemAllomorph.Form en: {text}.
		/// We don't use the standard names here because we may eventually implement a nicer case for all of LexEntry.
		/// </summary>
		[Test]
		public void UnknownMultiStringDefault()
		{
			string source =
				@"<RootClass>
					<HomographNumber
						val='0' />
					<Outer>
						<Mid>
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
				</RootClass>";
			var root = GetNode(source);
			var input = root.ChildNodes[1].ChildNodes[0].ChildNodes[0]; // the Target element.
			var generator = new FieldWorkObjectContextGenerator();
			var html = generator.HtmlContext(input);
			Assert.That(html.StartsWith("Outer Mid Target"));
			Assert.That(html, Contains.Substring("<div>en: abcdefghijk</div>"));
			Assert.That(html, Contains.Substring("<div>fr: other</div>"));

			// Get exactly the same starting from one of the AUni children.
			var input2 = input.ChildNodes[1];
			html = generator.HtmlContext(input2);
			Assert.That(html.StartsWith("Outer Mid Target"));
			Assert.That(html, Contains.Substring("<div>en: abcdefghijk</div>"));
			Assert.That(html, Contains.Substring("<div>fr: other</div>"));
		}

		/// <summary>
		/// Given something like a LexEntry containing a LexemeForm containing an MoStemAllomorph containing a Form
		/// which is one or more AUnis, we want to see something like LexemeForm.Form en: {text}.
		/// That is, the MoStemAllomorph level can just be left out.
		/// More generally, all object levels can ust be left out, that is, the ones that have guid attributes.
		/// We don't use the standard names here because we may eventually implement a nicer case for all of LexEntry.
		/// </summary>
		[Test]
		public void UnknownMultiStringPathOmitsObjectLevels()
		{
			string source =
				@"<RootClass
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
				</RootClass>";
			var root = GetNode(source);
			var input = root.ChildNodes[1].ChildNodes[0].ChildNodes[0]; // the Target element.
			var generator = new FieldWorkObjectContextGenerator();
			var html = generator.HtmlContext(input);
			Assert.That(html.StartsWith("Outer Target"));
			Assert.That(html, Contains.Substring("<div>en: abcdefghijk</div>"));
			Assert.That(html, Contains.Substring("<div>fr: other</div>"));
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
				@"<RootClass>
					<HomographNumber
						val='0' />
					<SeqProp>
						<ownseq
							class='SomeClass'>
							<Target>
								<AUni ws='en'>first</AUni>
								<AUni ws='fr'>second</AUni>
							</Target>
						</ownseq>
						<ownseq
							class='SomeClass'>
							<Target>
								<AUni ws='en'>third</AUni>
								<AUni ws='fr'>fourth</AUni>
							</Target>
						</ownseq>					</SeqProp>
				</RootClass>";
			var root = GetNode(source);
			var input = root.ChildNodes[1].ChildNodes[1].ChildNodes[0]; // the Target element (in the second objseq).
			var generator = new FieldWorkObjectContextGenerator();
			var html = generator.HtmlContext(input);
			Assert.That(html.StartsWith("SeqProp 2 Target"));
			Assert.That(html, Contains.Substring("<div>en: third</div>"));
			Assert.That(html, Contains.Substring("<div>fr: fourth</div>"));
		}

		[Test]
		public void LastResortIsHtmlOfXml()
		{
			string source =
			@"<RootClass>
					<Outer>
						<Mid>
							<Target>
								<Rubbish/>
							</Target>
						</Mid>
					</Outer>
				</RootClass>";
			var root = GetNode(source);
			var input = root.ChildNodes[0].ChildNodes[0].ChildNodes[0]; // the Target element.
			var generator = new FieldWorkObjectContextGenerator();
			var html = generator.HtmlContext(input);
			Assert.That(html, Is.EqualTo(XmlUtilities.GetXmlForShowingInHtml(input.OuterXml)));
		}

		XmlNode GetNode(string input)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(input);
			return doc.DocumentElement;
		}
	}
}
