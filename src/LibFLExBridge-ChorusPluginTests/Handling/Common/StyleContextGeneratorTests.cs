// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using LibFLExBridgeChorusPlugin;
using LibFLExBridgeChorusPlugin.Handling.Common;
using NUnit.Framework;

namespace LibFLExBridgeChorusPluginTests.Handling.Common
{
	[TestFixture]
	public class StyleContextGeneratorTests
	{
		[Test]
		public void ContextForSimpleChangeInAStyle()
		{
			const string source = @"
				<StStyle guid='d9aa70f0-ea5e-11de-8efb-0013722f8dec'>
					<Name>
						<Uni>Normal</Uni>
					</Name>
					<Rules>
						<Prop backcolor='white' fontsize='20000' forecolor='993300' spaceAfter='6000' />
					</Rules>
				</StStyle>";
			var root = FieldWorksTestServices.GetNode(source);
			var input = root.ChildNodes[1]; // Rules
			var generator = new StyleContextGenerator();
			var descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Style \"Normal\""));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "d9aa70f0-ea5e-11de-8efb-0013722f8dec"));

			// verify the html context generation
			Assert.That(generator.HtmlContext(input.ChildNodes[0]), // Prop
				Is.EqualTo(@"<div class='StStyle'> backcolor (white) fontsize (20000) forecolor (993300) spaceAfter (6000)</div>"));
		}

		[Test]
		public void ContextForComplexChangeInAStyle()
		{
			const string source = @"
				<StStyle guid='d9aa70f0-ea5e-11de-8efb-0013722f8dec'>
					<Name>
						<Uni>Abnormal</Uni>
					</Name>
					<Rules>
						<Prop backcolor='white' fontsize='14000' forecolor='green' spaceAfter='6000' undercolor='red' underline='double'>
							<WsStyles9999>
								<WsProp backcolor='red' fontFamily='Verdana' fontsize='12000' fontsizeUnit='mpt' offset='-3000' offsetUnit='mpt' undercolor='yellow' underline='single' ws='en' />
								<WsProp backcolor='white' fontFamily='Vladamir' fontsize='18000' fontsizeUnit='mpt' offsetUnit='mpt' undercolor='black' underline='double' ws='ru' />
							</WsStyles9999>
							<WsStyles9999>
								<WsProp backcolor='blue' fontFamily='OuiOui' fontsize='24000' fontsizeUnit='mpt' offset='5000' offsetUnit='mpt' underline='tripple' ws='fr' />
							</WsStyles9999>
						</Prop>
					</Rules>
				</StStyle>";
			var root = FieldWorksTestServices.GetNode(source);
			var input = root.ChildNodes[1]; // Rules
			var generator = new StyleContextGenerator();
			var descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Style \"Abnormal\""));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "d9aa70f0-ea5e-11de-8efb-0013722f8dec"));

			// verify the html context generation
			Assert.That(generator.HtmlContext(input.ChildNodes[0]), // Prop
				Is.EqualTo(@"<div class='StStyle'> backcolor (white) fontsize (14000) forecolor (green) spaceAfter (6000) undercolor (red) underline (double)" +
					@" ws (en [backcolor (red) fontFamily (Verdana) fontsize (12000) fontsizeUnit (mpt) offset (-3000) offsetUnit (mpt) undercolor (yellow) underline (single)])" +
					@" ws (ru [backcolor (white) fontFamily (Vladamir) fontsize (18000) fontsizeUnit (mpt) offsetUnit (mpt) undercolor (black) underline (double)])" +
					@" ws (fr [backcolor (blue) fontFamily (OuiOui) fontsize (24000) fontsizeUnit (mpt) offset (5000) offsetUnit (mpt) underline (tripple)])</div>"));
		}

		[Test]
		public void ContextForDataNotebookSliceStyleChange()
		{
			const string source = @"
				<RnGenericRec guid='29fb1310-385c-46a5-9e0e-d6cdaee7db17'>
					<Description>
						<StText guid='b7c744ed-a59c-4484-aead-f5c3dfd1b604'>
							<Paragraphs>
								<ownseq class='StTxtPara' guid='9febc9a9-20ce-4345-8a82-bf168fd9200d'>
									<StyleRules>
										<Prop namedStyle='Heading 2' />
									</StyleRules>
								</ownseq>
							</Paragraphs>
						</StText>
					</Description>
					<Custom name='Like'>
						<StText guid='f0b19053-f91e-4856-9679-5e801ed66961'>
							<Paragraphs>
								<ownseq class='StTxtPara' guid='4125c1ed-567e-47a3-a6fb-e6186f6f176d'>
									<StyleRules>
										<Prop namedStyle='Block Quote' />
									</StyleRules>
								</ownseq>
							</Paragraphs>
						</StText>
					</Custom>
					<Title>
						<Str>
							<Run ws='en'>Type</Run>
						</Str>
					</Title>
				</RnGenericRec>";

			var root = FieldWorksTestServices.GetNode(source);
			var input = root.ChildNodes[1].ChildNodes[0].ChildNodes[0].ChildNodes[0].ChildNodes[0]; // Custom//StyleRules
			var generator = new StyleContextGenerator();
			var descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Data Notebook Record \"Type\" Custom Field \"Like\""));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("silfw://localhost/link?app=flex&d"));

			// verify the html context generation
			Assert.That(generator.HtmlContext(input.ChildNodes[0]), // Prop
				Is.EqualTo(@"<div class='StStyle'> namedStyle (Block Quote)</div>"));

			input = root.ChildNodes[0].ChildNodes[0].ChildNodes[0].ChildNodes[0].ChildNodes[0]; // Description//StyleRules
			generator = new StyleContextGenerator();
			descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("Data Notebook Record \"Type\" Description"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("silfw://localhost/link?app=flex&d"));

			// verify the html context generation
			Assert.That(generator.HtmlContext(input.ChildNodes[0]), // Prop
				Is.EqualTo(@"<div class='StStyle'> namedStyle (Heading 2)</div>"));
		}

		[Test]
		public void ContextForGeneralStyleChange()
		{
			const string source = @"
				<WfiWordform guid='d9aa70f0-ea5e-11de-8efb-0013722f8dec'> <!-- some element that MdCache.GetClassInfo() knows has a guid -->
					<SomeWsRules>
						<Prop backcolor='green' fontsize='24000' forecolor='yellow' spaceAfter='9000' undercolor='blue' underline='red'>
							<MoreRules>
								<WsProp backcolor='beet' fontFamily='NanoPrint' fontsize='0.00002' fontsizeUnit='mi' offset='-0.003' offsetUnit='m' undercolor='yellow' underline='none' ws='aa' />
							</MoreRules>
							<MoreRules>
								<WsProp backcolor='carrot' fontFamily='ReallyLarge' fontsize='98E-57' fontsizeUnit='au' offset='0.050' offsetUnit='cm' underline='tripple' ws='bb' />
							</MoreRules>
						</Prop>
					</SomeWsRules>
				</WfiWordform>";
			var root = FieldWorksTestServices.GetNode(source);
			var input = root.ChildNodes[1]; // SomeWsRules  - the comment is [0]
			var generator = new StyleContextGenerator();
			var descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("WfiWordform [untitled]"));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "d9aa70f0-ea5e-11de-8efb-0013722f8dec"));

			// verify the html context generation
			Assert.That(generator.HtmlContext(input.ChildNodes[0]), // Prop
				Is.EqualTo(@"<div class='StStyle'> backcolor (green) fontsize (24000) forecolor (yellow) spaceAfter (9000) undercolor (blue) underline (red) "
					+ @"ws (aa [backcolor (beet) fontFamily (NanoPrint) fontsize (0.00002) fontsizeUnit (mi) offset (-0.003) offsetUnit (m) undercolor (yellow) underline (none)]) "
					+ @"ws (bb [backcolor (carrot) fontFamily (ReallyLarge) fontsize (98E-57) fontsizeUnit (au) offset (0.050) offsetUnit (cm) underline (tripple)])</div>"));
		}

		[Test]
		public void ContextForNamedFieldStyleChange()
		{
			const string source = @"
				<WfiWordform guid='d9aa70f0-ea5e-11de-8efb-0013722f8dec'> <!-- some element that MdCache.GetClassInfo() knows has a guid -->
					<Name>
						<Uni>Abnormal</Uni>
					</Name>
					<SomeWsRules>
						<Prop backcolor='green' fontsize='24000' forecolor='yellow' spaceAfter='9000' undercolor='blue' underline='red'>
							<MoreRules>
								<WsProp backcolor='beet' fontFamily='NanoPrint' fontsize='0.00002' fontsizeUnit='mi' offset='-0.003' offsetUnit='m' undercolor='yellow' underline='none' ws='aa' />
							</MoreRules>
							<MoreRules>
								<WsProp backcolor='carrot' fontFamily='ReallyLarge' fontsize='98E-57' fontsizeUnit='au' offset='0.050' offsetUnit='cm' underline='tripple' ws='bb' />
							</MoreRules>
						</Prop>
					</SomeWsRules>
				</WfiWordform>";
			var root = FieldWorksTestServices.GetNode(source);
			var input = root.ChildNodes[2]; // SomeWsRules  - the comment is [0]
			var generator = new StyleContextGenerator();
			var descriptor = generator.GenerateContextDescriptor(input, "myfile");
			Assert.That(descriptor.DataLabel, Is.EqualTo("WfiWordform \"Abnormal\""));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("label=" + descriptor.DataLabel));
			Assert.That(descriptor.PathToUserUnderstandableElement, Contains.Substring("guid=" + "d9aa70f0-ea5e-11de-8efb-0013722f8dec"));

			// verify the html context generation
			Assert.That(generator.HtmlContext(input.ChildNodes[0]), // Prop
				Is.EqualTo(@"<div class='StStyle'> backcolor (green) fontsize (24000) forecolor (yellow) spaceAfter (9000) undercolor (blue) underline (red) "
					+ @"ws (aa [backcolor (beet) fontFamily (NanoPrint) fontsize (0.00002) fontsizeUnit (mi) offset (-0.003) offsetUnit (m) undercolor (yellow) underline (none)]) "
					+ @"ws (bb [backcolor (carrot) fontFamily (ReallyLarge) fontsize (98E-57) fontsizeUnit (au) offset (0.050) offsetUnit (cm) underline (tripple)])</div>"));
		}

	}
}
