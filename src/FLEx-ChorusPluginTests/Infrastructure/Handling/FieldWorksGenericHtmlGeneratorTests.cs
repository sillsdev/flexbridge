using System.Xml;
using FLEx_ChorusPlugin.Infrastructure.Handling;
using FLEx_ChorusPluginTests.BorrowedCode;
using NUnit.Framework;

namespace FLEx_ChorusPluginTests.Infrastructure.Handling
{
	/// <summary>
	/// This class tests the way FieldWorks generates default HTML for arbitrary XML elements.
	/// </summary>
	[TestFixture]
	public class FieldWorksGenericHtmlGeneratorTests
	{
		[Test]
		public void DefaultShowsHierarchicalContent()
		{
			string input = @"<Root><Child>SomeText</Child><Child>More text</Child></Root>";
			var root = GetNode(input);
			string result = new FwGenericHtmlGenerator().MakeHtml(root);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"div[@class='property']/div[@class='property' and text()='Child: SomeText']");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"div/div[text()='Child: More text']");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"div[text()='Root: ']");
		}

		[Test]
		public void GuidElementsAreOmittedFromHierarchy()
		{
			string input = @"<Root guid='abcdef1234'><Child>SomeText</Child><Child>More text</Child></Root>";
			var root = GetNode(input);
			string result = "<body>" + new FwGenericHtmlGenerator().MakeHtml(root) + "</body>";
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"body/div[text()='Child: SomeText']");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"body/div[text()='Child: More text']");
		}

		[Test]
		public void RefSeqElementsContributeToChecksum()
		{
			string input = @"<Root>
				<Child>SomeText</Child>
				<SomeSeq>
					 <refseq guid='6325799e-8f47-4009-a43c-14b5bc641feb' t='r' />
					 <refseq guid='e8b411be-87a3-4638-ae3e-91a65b378195' t='r' />
					 <refseq guid='c9b63575-11b8-439b-93ac-b2929770f24e' t='r' />
					 <refseq guid='000d8025-63e1-4278-8445-5bb20ab23175' t='r' />
					 <refseq guid='4aea8c74-cd4b-4fd3-9b32-c4a27b527503' t='r' />
				</SomeSeq>
			</Root>";
			var root = GetNode(input);
			string result = "<body>" + new FwGenericHtmlGenerator().MakeHtml(root) + "</body>";
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"body/div[@class='property']/div[@class='property' and text()='Child: SomeText']");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"body/div[text()='Root: ']");
			XmlTestHelper.AssertXPathIsNull(result, @"//div[text()[contains(., 'SomeSeq')]]");
			var resultNode = GetNode(result);
			var checksum = resultNode.SelectSingleNode("div[@class='checksum']");
			Assert.That(checksum, Is.Not.Null);
			var checksumText = checksum.InnerText;
			Assert.That(checksumText, Is.StringContaining("Checksum"));

			// With a different set of guids we should get a different result.
			input = @"<Root>
				<Child>SomeText</Child>
				<SomeSeq>
					 <refseq guid='6325799e-8f47-4009-a43c-14b5bc641feb' t='r' />
					 <refseq guid='c9b63575-11b8-439b-93ac-b2929770f24e' t='r' />
					 <refseq guid='000d8025-63e1-4278-8445-5bb20ab23175' t='r' />
					 <refseq guid='4aea8c74-cd4b-4fd3-9b32-c4a27b527503' t='r' />
				</SomeSeq>
			</Root>";
			root = GetNode(input);
			result = "<body>" + new FwGenericHtmlGenerator().MakeHtml(root) + "</body>";
			resultNode = GetNode(result);
			checksum = resultNode.SelectSingleNode("div[@class='checksum']");
			Assert.That(checksum, Is.Not.Null);
			var checksumText2 = checksum.InnerText;
			Assert.That(checksumText2, Is.Not.EqualTo(checksumText));
		}

		[Test]
		public void AUniSpecialHandling()
		{
			string input = @"<Root><Child><AUni ws='en'>SomeText</AUni></Child><Child>More text</Child></Root>";
			var root = GetNode(input);
			string result = new FwGenericHtmlGenerator().MakeHtml(root);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"div[@class='property']/div[@class='property' and text()='Child: ']/div[@class='ws' and text()='en: SomeText']");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"div/div[text()='Child: More text']");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"div[text()='Root: ']");
		}

		[Test]
		public void AStrSpecialHandling()
		{
			string input = @"<Root><Child><AStr ws='en'><Run ws='en'>SomeText</Run></AStr></Child><Child>More text</Child></Root>";
			var root = GetNode(input);
			string result = new FwGenericHtmlGenerator().MakeHtml(root);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"div[@class='property']/div[@class='property' and text()='Child: ']/div[@class='ws' and text()='en: SomeText']");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"div/div[text()='Child: More text']");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"div[text()='Root: ']");
		}

		[Test]
		public void StrSpecialHandling()
		{
			string input = @"<Root><Child><Str><Run ws='en'>SomeText</Run></Str></Child><Child>More text</Child></Root>";
			var root = GetNode(input);
			string result = new FwGenericHtmlGenerator().MakeHtml(root);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"div[@class='property']/div[@class='property' and text()='Child: SomeText']");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"div[text()='Root: ']");
		}

		[Test]
		public void ValAttrSpecialHandling()
		{
			string input = @"<Root><HomographNumber val='3'/><Child>More text</Child></Root>";
			var root = GetNode(input);
			string result = new FwGenericHtmlGenerator().MakeHtml(root);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"div[@class='property']/div[@class='property' and text()='HomographNumber: 3']");
		}

		[Test]
		public void BeginOffsetSpecialHandling()
		{
			string input = @"<Root><BeginOffset val='3'/><Child>More text</Child></Root>";
			var root = GetNode(input);
			string result = new FwGenericHtmlGenerator().MakeHtml(root);
			XmlTestHelper.AssertXPathIsNull(result, @"//div[text()[contains(., 'BeginOffset')]]");
		}

		XmlNode GetNode(string input)
		{
			var doc = new XmlDocument();
			doc.LoadXml(input);
			return doc.DocumentElement;
		}
	}
}
