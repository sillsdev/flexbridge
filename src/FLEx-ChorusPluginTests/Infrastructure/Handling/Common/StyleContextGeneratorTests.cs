using System.Xml;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Infrastructure.Handling.Common;
using NUnit.Framework;

namespace FLEx_ChorusPluginTests.Infrastructure.Handling.Common
{
	[TestFixture]
	public class StyleContextGeneratorTests
	{
		[Test]
		public void GetStyleLabel()
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
			Assert.That(generator.HtmlContext(input.ChildNodes[0]),
				Is.EqualTo(@"<div class='StStyle'> backcolor (white) fontsize (20000) forecolor (993300) spaceAfter (6000)</div>"));
		}

		[Test]
		public void GetStyleHtml()
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
			Assert.That(generator.HtmlContext(input.ChildNodes[0]),
				Is.EqualTo(@"<div class='StStyle'> backcolor (white) fontsize (14000) forecolor (green) spaceAfter (6000) undercolor (red) underline (double)" +
					@" ws (en [backcolor (red) fontFamily (Verdana) fontsize (12000) fontsizeUnit (mpt) offset (-3000) offsetUnit (mpt) undercolor (yellow) underline (single)])" +
					@" ws (ru [backcolor (white) fontFamily (Vladamir) fontsize (18000) fontsizeUnit (mpt) offsetUnit (mpt) undercolor (black) underline (double)])" +
					@" ws (fr [backcolor (blue) fontFamily (OuiOui) fontsize (24000) fontsizeUnit (mpt) offset (5000) offsetUnit (mpt) underline (tripple)])</div>"));
		}

	}
}
