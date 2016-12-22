// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Xml;
using LibFLExBridgeChorusPlugin.Handling;
using NUnit.Framework;

namespace LibFLExBridgeChorusPluginTests.Handling
{
	[TestFixture]
	public class PreferMostRecentTimePreMergerTests
	{
		private const string CommonAncestor = @"<root>
	<DateModified val='2010-1-2 3:4:5.678'/>
	<SummaryDefinition>
		<AStr ws='en'><Run ws='en'>English summary</Run></AStr>
	</SummaryDefinition>
</root>";

		private XmlNode CreateNode(string xml)
		{
			var doc = new XmlDocument();
			doc.LoadXml(xml);
			return doc.SelectSingleNode("/root/DateModified");
		}

		[Test]
		public void BothChangedDifferentChildrenOfParent_NoAncestorAddedSame_ReturnsFalse()
		{
			var theirs = CreateNode(CommonAncestor.Replace("</root>", @"
	<HomographNumber val='0' />
</root>"));
			var ours = CreateNode(CommonAncestor.Replace("</root>", @"
	<HomographNumber val='0' />
</root>"));

			Assert.That(PreferMostRecentTimePreMerger.BothChangedDifferentChildrenOfParent(ours, theirs, null),
				Is.False);
		}

		[Test]
		public void BothChangedDifferentChildrenOfParent_NoAncestorAddedConflictingChangeOnSameChild_ReturnsFalse()
		{
			var theirs = CreateNode(CommonAncestor.Replace("</root>", @"
	<HomographNumber val='0' />
</root>"));
			var ours = CreateNode(CommonAncestor.Replace("</root>", @"
	<HomographNumber val='1' />
</root>"));

			Assert.That(PreferMostRecentTimePreMerger.BothChangedDifferentChildrenOfParent(ours, theirs, null),
				Is.False);
		}

		[Test]
		public void BothChangedDifferentChildrenOfParent_NoAncestorAddedDifferentNodes_ReturnsTrue()
		{
			var theirs = CreateNode(CommonAncestor.Replace("</root>", @"
	<LiteralMeaning>
		<AStr ws='en'><Run ws='en'>English meaning</Run></AStr>
	</LiteralMeaning>
</root>"));
			var ours = CreateNode(CommonAncestor.Replace("</root>", @"
	<HomographNumber val='1' />
</root>"));

			Assert.That(PreferMostRecentTimePreMerger.BothChangedDifferentChildrenOfParent(ours, theirs, null),
				Is.True);
		}

		[Test]
		public void BothChangedDifferentChildrenOfParent_WithAncestorNoTheir_ReturnsFalse()
		{
			var ancestor = CreateNode(CommonAncestor);
			var ours = CreateNode(CommonAncestor.Replace("</root>", @"
	<HomographNumber val='0' />
</root>"));

			Assert.That(PreferMostRecentTimePreMerger.BothChangedDifferentChildrenOfParent(ours, null, ancestor),
				Is.False);
		}

		[Test]
		public void BothChangedDifferentChildrenOfParent_WithAncestorNoOur_ReturnsFalse()
		{
			var ancestor = CreateNode(CommonAncestor);
			var theirs = CreateNode(CommonAncestor.Replace("</root>", @"
	<HomographNumber val='0' />
</root>"));

			Assert.That(PreferMostRecentTimePreMerger.BothChangedDifferentChildrenOfParent(null, theirs, ancestor),
				Is.False);
		}

		[Test]
		public void BothChangedDifferentChildrenOfParent_WithAncestorWeAdded_ReturnsFalse()
		{
			var ancestor = CreateNode(CommonAncestor);
			var theirs = CreateNode(CommonAncestor);
			var ours = CreateNode(CommonAncestor.Replace("</root>", @"
	<HomographNumber val='0' />
</root>"));

			Assert.That(PreferMostRecentTimePreMerger.BothChangedDifferentChildrenOfParent(ours, theirs, ancestor),
				Is.False);
		}

		[Test]
		public void BothChangedDifferentChildrenOfParent_WithAncestorTheyAdded_ReturnsFalse()
		{
			var ancestor = CreateNode(CommonAncestor);
			var theirs = CreateNode(CommonAncestor.Replace("</root>", @"
	<HomographNumber val='0' />
</root>"));
			var ours = CreateNode(CommonAncestor);

			Assert.That(PreferMostRecentTimePreMerger.BothChangedDifferentChildrenOfParent(ours, theirs, ancestor),
				Is.False);
		}

		[Test]
		public void BothChangedDifferentChildrenOfParent_WithAncestorWeDeleted_ReturnsFalse()
		{
			var ancestor = CreateNode(CommonAncestor);
			var theirs = CreateNode(CommonAncestor);
			var ours = CreateNode(CommonAncestor.Replace(@"
	<SummaryDefinition>
		<AStr ws='en'><Run ws='en'>English summary</Run></AStr>
	</SummaryDefinition>
</root>", @"</root>"));

			Assert.That(PreferMostRecentTimePreMerger.BothChangedDifferentChildrenOfParent(ours, theirs, ancestor),
				Is.False);
		}

		[Test]
		public void BothChangedDifferentChildrenOfParent_WithAncestorTheyDeleted_ReturnsFalse()
		{
			var ancestor = CreateNode(CommonAncestor);
			var theirs = CreateNode(CommonAncestor.Replace(@"
	<SummaryDefinition>
		<AStr ws='en'><Run ws='en'>English summary</Run></AStr>
	</SummaryDefinition>
</root>", @"</root>"));
			var ours = CreateNode(CommonAncestor);

			Assert.That(PreferMostRecentTimePreMerger.BothChangedDifferentChildrenOfParent(ours, theirs, ancestor),
				Is.False);
		}

		[Test]
		public void BothChangedDifferentChildrenOfParent_WithAncestorAddedSame_ReturnsFalse()
		{
			var ancestor = CreateNode(CommonAncestor);
			var theirs = CreateNode(CommonAncestor.Replace("</root>", @"
	<HomographNumber val='0' />
</root>"));
			var ours = CreateNode(CommonAncestor.Replace("</root>", @"
	<HomographNumber val='0' />
</root>"));

			Assert.That(PreferMostRecentTimePreMerger.BothChangedDifferentChildrenOfParent(ours, theirs, ancestor),
				Is.False);
		}

		[Test]
		public void BothChangedDifferentChildrenOfParent_WithAncestorAddedConflictingChangeOnSameChild_ReturnsFalse()
		{
			var ancestor = CreateNode(CommonAncestor);
			var theirs = CreateNode(CommonAncestor.Replace("</root>", @"
	<HomographNumber val='0' />
</root>"));
			var ours = CreateNode(CommonAncestor.Replace("</root>", @"
	<HomographNumber val='1' />
</root>"));

			Assert.That(PreferMostRecentTimePreMerger.BothChangedDifferentChildrenOfParent(ours, theirs, ancestor),
				Is.False);
		}

		[Test]
		public void BothChangedDifferentChildrenOfParent_WithAncestorDifferentChildrenAdded_ReturnsTrue()
		{
			var ancestor = CreateNode(CommonAncestor);
			var theirs = CreateNode(CommonAncestor.Replace("</root>", @"
	<LiteralMeaning>
		<AStr ws='en'><Run ws='en'>English meaning</Run></AStr>
	</LiteralMeaning>
</root>"));
			var ours = CreateNode(CommonAncestor.Replace("</root>", @"
	<HomographNumber val='0' />
</root>"));

			Assert.That(PreferMostRecentTimePreMerger.BothChangedDifferentChildrenOfParent(ours, theirs, ancestor),
				Is.True);
		}

		[Test]
		public void BothChangedDifferentChildrenOfParent_WithAncestorDifferentChildrenModified_ReturnsTrue()
		{
			var ancestor = CreateNode(CommonAncestor.Replace("</root>", @"
	<Custom name='first custom'/>
	<Custom name='second custom'/>
	<LiteralMeaning>
		<AStr ws='en'><Run ws='en'>English meaning</Run></AStr>
	</LiteralMeaning>
</root>"));
			var theirs = CreateNode(ancestor.ParentNode.OuterXml.Replace("</LiteralMeaning>", @"
	<AStr ws='fr'><Run ws='fr'>French meaning</Run></AStr>
</LiteralMeaning>"));
			var ours = CreateNode(ancestor.ParentNode.OuterXml.Replace("</SummaryDefinition>", @"
	<AStr ws='fr'><Run ws='fr'>French summary</Run></AStr>
</SummaryDefinition>"));

			Assert.That(PreferMostRecentTimePreMerger.BothChangedDifferentChildrenOfParent(ours, theirs, ancestor),
				Is.True);
		}

		[Test]
		public void BothChangedDifferentChildrenOfParent_WithAncestorSameChildModified_ReturnsFalse()
		{
			var ancestor = CreateNode(CommonAncestor);
			var theirs = CreateNode(CommonAncestor.Replace("</SummaryDefinition>", @"
	<AStr ws='de'><Run ws='de'>German summary</Run></AStr>
</SummaryDefinition>"));
			var ours = CreateNode(CommonAncestor.Replace("</SummaryDefinition>", @"
	<AStr ws='fr'><Run ws='fr'>French summary</Run></AStr>
</SummaryDefinition>"));

			Assert.That(PreferMostRecentTimePreMerger.BothChangedDifferentChildrenOfParent(ours, theirs, ancestor),
				Is.False);
		}

		[Test]
		public void BothChangedDifferentChildrenOfParent_WithAncestorWeModifiedChild_ReturnsFalse()
		{
			var ancestor = CreateNode(CommonAncestor);
			var theirs = CreateNode(CommonAncestor);
			var ours = CreateNode(CommonAncestor.Replace("</SummaryDefinition>", @"
	<AStr ws='fr'><Run ws='fr'>French summary</Run></AStr>
</SummaryDefinition>"));

			Assert.That(PreferMostRecentTimePreMerger.BothChangedDifferentChildrenOfParent(ours, theirs, ancestor),
				Is.False);
		}

		[Test]
		public void BothChangedDifferentChildrenOfParent_WithAncestorTheyModifiedChild_ReturnsFalse()
		{
			var ancestor = CreateNode(CommonAncestor);
			var theirs = CreateNode(CommonAncestor.Replace("</SummaryDefinition>", @"
	<AStr ws='fr'><Run ws='fr'>French summary</Run></AStr>
</SummaryDefinition>"));
			var ours = CreateNode(CommonAncestor);

			Assert.That(PreferMostRecentTimePreMerger.BothChangedDifferentChildrenOfParent(ours, theirs, ancestor),
				Is.False);
		}
	}
}
