// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Xml;
using LibFLExBridgeChorusPlugin.Handling;
using LibChorus.TestUtilities;
using NUnit.Framework;

namespace LibFLExBridgeChorusPluginTests.Handling
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
			var resultNode = GetNode(result);
			var checksumFirst = GetPropChecksum(resultNode, "SomeSeq");

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
			var checkSumSecond = GetPropChecksum(resultNode, "SomeSeq");
			Assert.That(checksumFirst, Is.Not.EqualTo(checkSumSecond));
		}

		[Test]
		public void RefColElementsContributeToChecksum()
		{
			string input = @"<Root>
				<Child>SomeText</Child>
				<SomeSeq>
					 <refcol guid='6325799e-8f47-4009-a43c-14b5bc641feb' t='r' />
					 <refcol guid='e8b411be-87a3-4638-ae3e-91a65b378195' t='r' />
					 <refcol guid='c9b63575-11b8-439b-93ac-b2929770f24e' t='r' />
					 <refcol guid='000d8025-63e1-4278-8445-5bb20ab23175' t='r' />
					 <refcol guid='4aea8c74-cd4b-4fd3-9b32-c4a27b527503' t='r' />
				</SomeSeq>
			</Root>";
			var root = GetNode(input);
			string result = "<body>" + new FwGenericHtmlGenerator().MakeHtml(root) + "</body>";
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"body/div[@class='property']/div[@class='property' and text()='Child: SomeText']");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"body/div[text()='Root: ']");
			var resultNode = GetNode(result);
			var checksumFirst = GetPropChecksum(resultNode, "SomeSeq");

			// With a different set of guids we should get a different result.
			input = @"<Root>
				<Child>SomeText</Child>
				<SomeSeq>
					 <refcol guid='6325799e-8f47-4009-a43c-14b5bc641feb' t='r' />
					 <refcol guid='c9b63575-11b8-439b-93ac-b2929770f24e' t='r' />
					 <refcol guid='000d8025-63e1-4278-8445-5bb20ab23175' t='r' />
					 <refcol guid='4aea8c74-cd4b-4fd3-9b32-c4a27b527503' t='r' />
				</SomeSeq>
			</Root>";
			root = GetNode(input);
			result = "<body>" + new FwGenericHtmlGenerator().MakeHtml(root) + "</body>";
			resultNode = GetNode(result);
			var checkSumSecond = GetPropChecksum(resultNode, "SomeSeq");
			Assert.That(checksumFirst, Is.Not.EqualTo(checkSumSecond));
		}

		[Test]
		public void ObjSurElementsContributeToChecksum()
		{
			string input = @"<Root>
				<Child>SomeText</Child>
				<SomeAtomic>
					 <objsur guid='6325799e-8f47-4009-a43c-14b5bc641feb' t='r' />
				</SomeAtomic>
			</Root>";
			var root = GetNode(input);
			string result = "<body>" + new FwGenericHtmlGenerator().MakeHtml(root) + "</body>";
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"body/div[@class='property']/div[@class='property' and text()='Child: SomeText']");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"body/div[text()='Root: ']");
			var resultNode = GetNode(result);
			var checksumFirst = GetPropChecksum(resultNode, "SomeAtomic");

			// With a different set of guids we should get a different result.
			input = @"<Root>
				<Child>SomeText</Child>
				<SomeAtomic>
					 <objsur guid='c9b63575-11b8-439b-93ac-b2929770f24e' t='r' />
				</SomeAtomic>
			</Root>";
			root = GetNode(input);
			result = "<body>" + new FwGenericHtmlGenerator().MakeHtml(root) + "</body>";
			resultNode = GetNode(result);
			var checkSumSecond = GetPropChecksum(resultNode, "SomeAtomic");
			Assert.That(checksumFirst, Is.Not.EqualTo(checkSumSecond));
		}

		[Test]
		public void ChecksumIsMadeForEachRefField()
		{
			string input = @"<Root>
				<Child>SomeText</Child>
				<SomeAtomic>
					 <objsur guid='6325799e-8f47-4009-a43c-14b5bc641feb' t='r' />
				</SomeAtomic>
				<SomeCol>
					 <refcol guid='6325799e-8f47-4009-a43c-14b5bc641feb' t='r' />
					 <refcol guid='c9b63575-11b8-439b-93ac-b2929770f24e' t='r' />
					 <refcol guid='000d8025-63e1-4278-8445-5bb20ab23175' t='r' />
					 <refcol guid='4aea8c74-cd4b-4fd3-9b32-c4a27b527503' t='r' />
				</SomeCol>
				<SomeSeq>
					 <refseq guid='6325799e-8f47-4009-a43c-14b5bc641fec' t='r' />
					 <refseq guid='c9b63575-11b8-439b-93ac-b2929770f24f' t='r' />
					 <refseq guid='000d8025-63e1-4278-8445-5bb20ab23176' t='r' />
					 <refseq guid='4aea8c74-cd4b-4fd3-9b32-c4a27b527504' t='r' />
				</SomeSeq>
				<AnotherAtomic>
					 <objsur guid='6325799e-8f47-4009-a43c-14b5bc641fec' t='r' />
				</AnotherAtomic>
				<AnotherCol>
					 <refcol guid='6325799e-8f47-4009-a43c-14b5bc641fec' t='r' />
					 <refcol guid='c9b63575-11b8-439b-93ac-b2929770f24d' t='r' />
					 <refcol guid='000d8025-63e1-4278-8445-5bb20ab23175' t='r' />
					 <refcol guid='4aea8c74-cd4b-4fd3-9b32-c4a27b527503' t='r' />
				</AnotherCol>
				<AnotherSeq>
					 <refseq guid='6325799e-8f47-4009-a43c-14b5bc641feb' t='r' />
					 <refseq guid='c9b63575-11b8-439b-93ac-b2929770f24e' t='r' />
					 <refseq guid='000d8025-63e1-4278-8445-5bb20ab23175' t='r' />
					 <refseq guid='4aea8c74-cd4b-4fd3-9b32-c4a27b527503' t='r' />
				</AnotherSeq>
				<AnOwningSeq>
					<ChildAtomic>
						 <objsur guid='6325799e-8f47-4009-a43c-14b5bc641fef' t='r' />
					</ChildAtomic>
				</AnOwningSeq>
			</Root>";
			var root = GetNode(input);
			// We want it to produce something like:
			// <div class="property">Root:
			//	<div class="property">Child: SomeText
			//	</div>
			// <div class="checksum">SomeAtomic: [a checksum or the guid]</div>
			// <div class="checksum">SomeCol: [a checksum]</div>
			// etc...
			// </div>
			string result = "<body>" + new FwGenericHtmlGenerator().MakeHtml(root) + "</body>";
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"body/div[@class='property']/div[@class='property' and text()='Child: SomeText']");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"body/div[text()='Root: ']");
			var resultNode = GetNode(result);
			var checksumSomeAtomic = GetPropChecksum(resultNode, "SomeAtomic");
			var checksumAnotherAtomic = GetPropChecksum(resultNode, "AnotherAtomic");
			Assert.AreNotEqual(checksumSomeAtomic, checksumAnotherAtomic);

			var checksumSomeCol = GetPropChecksum(resultNode, "SomeCol");
			var checksumAnotherCol = GetPropChecksum(resultNode, "AnotherCol");
			Assert.AreNotEqual(checksumSomeCol, checksumAnotherCol);

			var checksumSomeSeq = GetPropChecksum(resultNode, "SomeSeq");
			var checksumAnotherSeq = GetPropChecksum(resultNode, "AnotherSeq");
			Assert.AreNotEqual(checksumSomeSeq, checksumAnotherSeq);

			GetPropChecksum(resultNode, "ChildAtomic");
		}

		private static string GetPropChecksum(XmlNode resultNode, string propName)
		{
			var checksum = resultNode.SelectSingleNode("//div[@class='checksum' and text()[contains(., '" + propName + "')]]");
			Assert.That(checksum, Is.Not.Null);
			var checksumText = checksum.InnerText;
			Assert.That(checksumText, Is.StringStarting(propName + ": "));
			var checksumData = checksumText.Substring((propName + ": ").Length);
			return checksumData;
		}

		[Test]
		public void AUniSpecialHandling()
		{
			string input = @"<Root><Child><AUni ws='en'>SomeText</AUni></Child><Child>More text</Child></Root>";
			var root = GetNode(input);
			string result = new FwGenericHtmlGenerator().MakeHtml(root);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"div[@class='property']/div[@class='property' and text()='Child: ']/div[@class='ws']/span[@class='ws' and text()='en']");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"div[@class='property']/div[@class='property' and text()='Child: ']/div[@class='ws' and contains(text(),': SomeText')]");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"div/div[text()='Child: More text']");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"div[text()='Root: ']");
		}

		[Test]
		public void AStrSpecialHandling()
		{
			string input = @"<Root><Child><AStr ws='en'><Run ws='en'>SomeText</Run></AStr></Child><Child>More text</Child></Root>";
			var root = GetNode(input);
			string result = new FwGenericHtmlGenerator().MakeHtml(root);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"div[@class='property']/div[@class='property' and text()='Child: ']/div[@class='ws']/span[@class='ws' and text()='en']");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"div[@class='property']/div[@class='property' and text()='Child: ']/div[@class='ws' and contains(text(),': SomeText')]");
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
