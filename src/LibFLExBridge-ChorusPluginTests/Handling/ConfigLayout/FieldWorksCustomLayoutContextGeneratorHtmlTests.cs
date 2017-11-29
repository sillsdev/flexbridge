// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Xml;
using Chorus.merge.xml.generic;
using LibFLExBridgeChorusPlugin.Handling.ConfigLayout;
using NUnit.Framework;

namespace LibFLExBridgeChorusPluginTests.Handling.ConfigLayout
{
	/// <summary>
	/// Tests the FieldWorksCustomLayoutContextGenerator.HtmlContext method
	/// </summary>
	[TestFixture]
	public class FieldWorksCustomLayoutContextGeneratorHtmlTests
	{
		private XmlNode GetNode(string input)
		{
			var doc = new XmlDocument();
			doc.LoadXml(input);
			return doc.DocumentElement;
		}

		[Test]
		public void DoVeryLittle()
		{
			const string source =
@"<layout class='foo' type='mytype' name='bar'>
	<part ref='FormPub' label='Lexeme Form' before='' after=' ' visibility='never' ws='vernacular' wsType='vernacular' />
</layout>";
			var root = GetNode(source);
			IGenerateHtmlContext generator = new FieldWorkCustomLayoutContextGenerator();
			var html = generator.HtmlContext(root);
			Assert.That(html, Is.EqualTo("<div>" + XmlUtilities.GetXmlForShowingInHtml(root.OuterXml) + "</div>"));
		}
	}
}