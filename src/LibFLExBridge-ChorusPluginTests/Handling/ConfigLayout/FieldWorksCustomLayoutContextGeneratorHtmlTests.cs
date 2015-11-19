// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

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
			var generator = new FieldWorkCustomLayoutContextGenerator();
			var html = generator.HtmlContext(root);
			Assert.That(html, Is.EqualTo("<div>" + XmlUtilities.GetXmlForShowingInHtml(root.OuterXml) + "</div>"));
		}
	}
}