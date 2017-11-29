// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Xml;
using Chorus.merge.xml.generic;
using LibFLExBridgeChorusPlugin.Handling;
using NUnit.Framework;

namespace LibFLExBridgeChorusPluginTests.Handling
{
	/// <summary>
	/// Tests the FieldWorksObjectContextGenerator.HtmlContext methods
	/// </summary>
	[TestFixture]
	public class FieldWorksObjectContextGeneratorHtmlTests
	{
		/// <summary>
		/// Given something like a LexEntry containing a LexemeForm containing an MoStemAllomorph containing a Form
		/// which is one or more AUnis, we want to see something like <span class="ws">en</span>: {text} on a line for each alternative.
		/// We don't use the standard names here because we may eventually implement a nicer case for all of LexEntry.
		/// </summary>
		[Test]
		public void UnknownMultiStringDefault()
		{
			const string source = @"<RootClass>
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
			IGenerateHtmlContext generator = new FieldWorkObjectContextGenerator();
			var html = generator.HtmlContext(input);
			Assert.That(html.StartsWith("<div>"));
			Assert.That(html, Contains.Substring("<div><span class=\"ws\">en</span>: abcdefghijk</div>"));
			Assert.That(html, Contains.Substring("<div><span class=\"ws\">fr</span>: other</div>"));

			// Get exactly the same starting from one of the AUni children.
			var input2 = input.ChildNodes[1];
			html = generator.HtmlContext(input2);
			Assert.That(html.StartsWith("<div>"));
			Assert.That(html, Contains.Substring("<div><span class=\"ws\">en</span>: abcdefghijk</div>"));
			Assert.That(html, Contains.Substring("<div><span class=\"ws\">fr</span>: other</div>"));
		}

		/// <summary>
		/// With a multistring that has only one alternative, we don't need to see the label.
		/// </summary>
		[Test]
		public void UnknownMultiStringDefaultSingleAlternative()
		{
			const string source = @"<RootClass>
					<HomographNumber
						val='0' />
					<Outer>
						<Mid>
							<Target>
								<AUni ws='en'>abcdefghijk</AUni>
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
			IGenerateHtmlContext generator = new FieldWorkObjectContextGenerator();
			var html = generator.HtmlContext(input);
			Assert.That(html, Is.EqualTo("<span class=\"ws\">en</span>: abcdefghijk"));

			// Get exactly the same starting from one of the AUni children.
			var input2 = input.ChildNodes[0];
			html = generator.HtmlContext(input2);
			Assert.That(html, Is.EqualTo("<span class=\"ws\">en</span>: abcdefghijk"));
		}

		/// <summary>
		/// With a simple unicode string, we just want the content
		/// </summary>
		[Test]
		public void UnicodeStringJustShowsText()
		{
			const string source = @"<RootClass>
					<HomographNumber
						val='0' />
					<Outer>
						<Mid>
							<Target>
								<Uni>abcdefghijk</Uni>
							</Target>

						</Mid>
					</Outer>
				</RootClass>";
			var root = GetNode(source);
			var input = root.ChildNodes[1].ChildNodes[0].ChildNodes[0]; // the Target element.
			IGenerateHtmlContext generator = new FieldWorkObjectContextGenerator();
			var html = generator.HtmlContext(input);
			Assert.That(html, Is.EqualTo("abcdefghijk"));

			// Get exactly the same starting from  the Uni child.
			var input2 = input.ChildNodes[0];
			html = generator.HtmlContext(input2);
			Assert.That(html, Is.EqualTo("abcdefghijk"));
		}

		[Test]
		public void LastResortIsGenericHtmlGenerator()
		{
			const string source = @"<RootClass>
					<Outer>
						<Mid>
							<Target>
								some rubbish
							</Target>
						</Mid>
					</Outer>
				</RootClass>";
			var root = GetNode(source);
			var input = root.ChildNodes[0].ChildNodes[0].ChildNodes[0]; // the Target element.
			IGenerateHtmlContext generator = new FieldWorkObjectContextGenerator();
			var html = generator.HtmlContext(input);
			Assert.That(html, Is.EqualTo(new FwGenericHtmlGenerator().MakeHtml(input)));
		}

		static XmlNode GetNode(string input)
		{
			var doc = new XmlDocument();
			doc.LoadXml(input);
			return doc.DocumentElement;
		}
	}
}
