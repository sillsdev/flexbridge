// Copyright (c) 2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Xml;
using LibFLExBridgeChorusPlugin.Handling;
using NUnit.Framework;
using Chorus.merge.xml.generic;
using System;
using SIL.Providers;
using SIL.TestUtilities.Providers;

namespace LibFLExBridgeChorusPluginTests.Handling
{
	[TestFixture]
	public class PreferMostRecentTimePreMergerTests
	{
		private const string CommonAncestor = @"
<root>
	<DateModified val='2010-1-2 3:4:5.678'/>
	<SummaryDefinition>
		<AStr ws='en'><Run ws='en'>English summary</Run></AStr>
	</SummaryDefinition>
</root>";

		private readonly IPremerger _preMerger = new PreferMostRecentTimePreMerger();
		private int _count;

		private XmlNode CreateNode(string xml)
		{
			xml = xml.Replace("2010-1-2 3:4:5.678", string.Format("2010-1-{0} 3:4:5.678", _count++));
			var doc = new XmlDocument();
			doc.LoadXml(xml);
			return doc.SelectSingleNode("/root/DateModified");
		}

		private static string GetDateTimeValue(XmlNode xml)
		{
			return xml.SelectSingleNode("/root/DateModified/@val").Value;
		}

		[SetUp]
		public void SetUp()
		{
			_count = 3;
			BaseFieldWorksTypeHandlerTests._expectedUtcDateTime = DateTime.UtcNow;
			DateTimeProvider.SetProvider(new ReproducibleDateTimeProvider(BaseFieldWorksTypeHandlerTests._expectedUtcDateTime));
		}

		[Test]
		public void PreMerge_NoAncestorAddedSame_UpdatesTimestamp()
		{
			var theirs = CreateNode(CommonAncestor.Replace("</root>", @"
	<HomographNumber val='0' />
</root>"));
			var ours = CreateNode(CommonAncestor.Replace("</root>", @"
	<HomographNumber val='0' />
</root>"));

			_preMerger.Premerge(null, ref ours, theirs, null);
			Assert.That(GetDateTimeValue(ours), Is.EqualTo(BaseFieldWorksTypeHandlerTests.ExpectedUtcDateTimeString));
		}

		[Test]
		public void PreMerge_NoAncestorAddedConflictingChangeOnSameChild_UpdatesTimestamp()
		{
			var theirs = CreateNode(CommonAncestor.Replace("</root>", @"
	<HomographNumber val='0' />
</root>"));
			var ours = CreateNode(CommonAncestor.Replace("</root>", @"
	<HomographNumber val='1' />
</root>"));

			_preMerger.Premerge(null, ref ours, theirs, null);
			Assert.That(GetDateTimeValue(ours), Is.EqualTo(BaseFieldWorksTypeHandlerTests.ExpectedUtcDateTimeString));
		}

		[Test]
		public void PreMerge_NoAncestorAddedDifferentNodes_UpdatesTimestamp()
		{
			var theirs = CreateNode(CommonAncestor.Replace("</root>", @"
	<LiteralMeaning>
		<AStr ws='en'><Run ws='en'>English meaning</Run></AStr>
	</LiteralMeaning>
</root>"));
			var ours = CreateNode(CommonAncestor.Replace("</root>", @"
	<HomographNumber val='1' />
</root>"));

			_preMerger.Premerge(null, ref ours, theirs, null);
			Assert.That(GetDateTimeValue(ours), Is.EqualTo(BaseFieldWorksTypeHandlerTests.ExpectedUtcDateTimeString));
		}

		[Test]
		public void PreMerge_WithAncestorWeAdded_UpdatesTimestamp()
		{
			var ancestor = CreateNode(CommonAncestor);
			var theirs = CreateNode(CommonAncestor);
			var ours = CreateNode(CommonAncestor.Replace("</root>", @"
	<HomographNumber val='0' />
</root>"));

			_preMerger.Premerge(null, ref ours, theirs, ancestor);
			Assert.That(GetDateTimeValue(ours), Is.EqualTo(BaseFieldWorksTypeHandlerTests.ExpectedUtcDateTimeString));
		}

		[Test]
		public void PreMerge_WithAncestorTheyAdded_UpdatesTimestamp()
		{
			var ancestor = CreateNode(CommonAncestor);
			var theirs = CreateNode(CommonAncestor.Replace("</root>", @"
	<HomographNumber val='0' />
</root>"));
			var ours = CreateNode(CommonAncestor);

			_preMerger.Premerge(null, ref ours, theirs, ancestor);
			Assert.That(GetDateTimeValue(ours), Is.EqualTo(BaseFieldWorksTypeHandlerTests.ExpectedUtcDateTimeString));
		}

		[Test]
		public void PreMerge_WithAncestorAddedSame_UpdatesTimestamp()
		{
			var ancestor = CreateNode(CommonAncestor);
			var theirs = CreateNode(CommonAncestor.Replace("</root>", @"
	<HomographNumber val='0' />
</root>"));
			var ours = CreateNode(CommonAncestor.Replace("</root>", @"
	<HomographNumber val='0' />
</root>"));

			_preMerger.Premerge(null, ref ours, theirs, ancestor);
			Assert.That(GetDateTimeValue(ours), Is.EqualTo(BaseFieldWorksTypeHandlerTests.ExpectedUtcDateTimeString));
		}

		[Test]
		public void PreMerge_WithAncestorOnlyTimestampChange_KeepsTimestamp()
		{
			var ancestor = CreateNode(CommonAncestor);
			var theirs = CreateNode(CommonAncestor);
			var ours = CreateNode(CommonAncestor);

			_preMerger.Premerge(null, ref ours, theirs, ancestor);
			Assert.That(GetDateTimeValue(ours), Is.EqualTo("2010-01-03 03:04:05.678"));
		}

				[Test]
		public void PreMerge_WithAncestorAddedConflictingChange_UpdatesTimestamp()
		{
			var ancestor = CreateNode(CommonAncestor);
			var theirs = CreateNode(CommonAncestor.Replace("</root>", @"
	<HomographNumber val='0' />
</root>"));
			var ours = CreateNode(CommonAncestor.Replace("</root>", @"
	<HomographNumber val='1' />
</root>"));

			_preMerger.Premerge(null, ref ours, theirs, ancestor);
			Assert.That(GetDateTimeValue(ours), Is.EqualTo(BaseFieldWorksTypeHandlerTests.ExpectedUtcDateTimeString));
		}
	}
}
