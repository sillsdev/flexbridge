using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Infrastructure.Handling;
using FLEx_ChorusPlugin.Infrastructure.Handling.Linguistics.MorphologyAndSyntax;
using LibChorus.TestUtilities;
using NUnit.Framework;

namespace FLEx_ChorusPluginTests.Infrastructure.Handling.Linguistics.MorphologyAndSyntax
{
	[TestFixture]
	class FeatureSpecsPreMergerTests
	{
		[Test]
		public void BothAddedValueToFeatureSpecsWithSameFeature_GuidsMadeToMatch()
		{
			const string specsPattern =
				@"<FeatureSpecs>
							{0}
							<FsClosedValue
								guid='a9e53847-aa31-4ccc-b355-89fd79dc0e12'>
								<Feature>
									<objsur
										guid='62b8f1bd-b5f2-4939-b38b-5fa9be2739e3'
										t='r' />
								</Feature>
								<RefNumber
									val='0' />
								<Value>
									<objsur
										guid='2278d52c-2b4c-4a79-b737-a5bfbe25a7ec'
										t='r' />
								</Value>
								<ValueState
									val='0' />
							</FsClosedValue>
						</FeatureSpecs>";
			const string fcvPattern = @"<FsClosedValue
								guid='{0}'>
								<Feature>
									<objsur
										guid='0922fb6c-75af-42e9-993a-363e4bf449cd'
										t='r' />
								</Feature>
								<RefNumber
									val='0' />
								<Value>
									<objsur
										guid='62245b04-b269-43ff-9bb8-7e2a784e4755'
										t='r' />
								</Value>
								<ValueState
									val='0' />
							</FsClosedValue>";
			// The ancestor does not have a closed value that references feature 0922fb6c-75af-42e9-993a-363e4bf449cd.
			// We both added one which does, with different guids.
			// We want the merger to modify the new FsClosedValue in 'theirs' to have the same guid as ours.
			// "we win" and the guid difference is ignored.
			var commonAncestor = string.Format(specsPattern, "");
			var ourContent = string.Format(specsPattern, string.Format(fcvPattern, "5a57f521-c4c1-477b-8c08-cdbf92dc9470"));
			var theirContent = string.Format(specsPattern, string.Format(fcvPattern, "99e6edfe-2e74-4119-aab5-a5dbab4186b1"));

			var ancestorNode = XmlUtilities.GetDocumentNodeFromRawXml(commonAncestor, new XmlDocument());
			var ourNode = XmlUtilities.GetDocumentNodeFromRawXml(ourContent, new XmlDocument());
			var theirNode = XmlUtilities.GetDocumentNodeFromRawXml(theirContent, new XmlDocument());
			var premerger = new FeatureSpecsPreMerger();
			premerger.Premerge(new ListenerForUnitTests(), ref ourNode, theirNode, ancestorNode);

			var modNode = theirNode.ChildNodes[1]; // skip some white space
			Assert.That(modNode.Attributes["guid"].Value, Is.EqualTo("5a57f521-c4c1-477b-8c08-cdbf92dc9470"));
		}

		[Test]
		public void OneReplacedFeatureSpecsWithSameFeatureOtherDidNothing_ChangeModifiedOne()
		{
			const string specsPattern =
				@"<FeatureSpecs>
							{0}
							<FsClosedValue
								guid='a9e53847-aa31-4ccc-b355-89fd79dc0e12'>
								<Feature>
									<objsur
										guid='62b8f1bd-b5f2-4939-b38b-5fa9be2739e3'
										t='r' />
								</Feature>
								<RefNumber
									val='0' />
								<Value>
									<objsur
										guid='2278d52c-2b4c-4a79-b737-a5bfbe25a7ec'
										t='r' />
								</Value>
								<ValueState
									val='0' />
							</FsClosedValue>
						</FeatureSpecs>";
			const string fcvPattern = @"<FsClosedValue
								guid='{0}'>
								<Feature>
									<objsur
										guid='0922fb6c-75af-42e9-993a-363e4bf449cd'
										t='r' />
								</Feature>
								<RefNumber
									val='0' />
								<Value>
									<objsur
										guid='62245b04-b269-43ff-9bb8-7e2a784e4755'
										t='r' />
								</Value>
								<ValueState
									val='0' />
							</FsClosedValue>";
			// The ancestor has a closed value for feature 0922fb6c-75af-42e9-993a-363e4bf449cd.
			// It is unchanged in theirs.
			// Ours also has one, but with a different guid. Our guid should be fixed.
			var commonAncestor = string.Format(specsPattern, string.Format(fcvPattern, "99e6edfe-2e74-4119-aab5-a5dbab4186b1"));
			var ourContent = string.Format(specsPattern, string.Format(fcvPattern, "5a57f521-c4c1-477b-8c08-cdbf92dc9470"));
			var theirContent = string.Format(specsPattern, string.Format(fcvPattern, "99e6edfe-2e74-4119-aab5-a5dbab4186b1"));

			var ancestorNode = XmlUtilities.GetDocumentNodeFromRawXml(commonAncestor, new XmlDocument());
			var ourNode = XmlUtilities.GetDocumentNodeFromRawXml(ourContent, new XmlDocument());
			var theirNode = XmlUtilities.GetDocumentNodeFromRawXml(theirContent, new XmlDocument());
			var premerger = new FeatureSpecsPreMerger();
			premerger.Premerge(new ListenerForUnitTests(), ref ourNode, theirNode, ancestorNode);

			var modNode = ourNode.ChildNodes[1]; // skip some white space
			Assert.That(modNode.Attributes["guid"].Value, Is.EqualTo("99e6edfe-2e74-4119-aab5-a5dbab4186b1"));

			// try the other way around.
			theirNode = XmlUtilities.GetDocumentNodeFromRawXml(ourContent, new XmlDocument());
			ourNode = XmlUtilities.GetDocumentNodeFromRawXml(theirContent, new XmlDocument());
			premerger = new FeatureSpecsPreMerger();
			premerger.Premerge(new ListenerForUnitTests(), ref ourNode, theirNode, ancestorNode);

			modNode = theirNode.ChildNodes[1]; // skip some white space
			Assert.That(modNode.Attributes["guid"].Value, Is.EqualTo("99e6edfe-2e74-4119-aab5-a5dbab4186b1"));
		}

		[Test]
		public void OneReplacedFeatureSpecsWithSameFeatureOtherDeleted_NoChanges()
		{
			const string specsPattern =
				@"<FeatureSpecs>
							{0}
							<FsClosedValue
								guid='a9e53847-aa31-4ccc-b355-89fd79dc0e12'>
								<Feature>
									<objsur
										guid='62b8f1bd-b5f2-4939-b38b-5fa9be2739e3'
										t='r' />
								</Feature>
								<RefNumber
									val='0' />
								<Value>
									<objsur
										guid='2278d52c-2b4c-4a79-b737-a5bfbe25a7ec'
										t='r' />
								</Value>
								<ValueState
									val='0' />
							</FsClosedValue>
						</FeatureSpecs>";
			const string fcvPattern = @"<FsClosedValue
								guid='{0}'>
								<Feature>
									<objsur
										guid='0922fb6c-75af-42e9-993a-363e4bf449cd'
										t='r' />
								</Feature>
								<RefNumber
									val='0' />
								<Value>
									<objsur
										guid='62245b04-b269-43ff-9bb8-7e2a784e4755'
										t='r' />
								</Value>
								<ValueState
									val='0' />
							</FsClosedValue>";
			// The ancestor has a closed value for feature 0922fb6c-75af-42e9-993a-363e4bf449cd.
			// Ours has one for that feature with a different guid
			// Theirs does not have one for that feature at all.
			var commonAncestor = string.Format(specsPattern, string.Format(fcvPattern, "99e6edfe-2e74-4119-aab5-a5dbab4186b1"));
			var ourContent = string.Format(specsPattern, string.Format(fcvPattern, "5a57f521-c4c1-477b-8c08-cdbf92dc9470"));
			var theirContent = string.Format(specsPattern, "");

			var ancestorNode = XmlUtilities.GetDocumentNodeFromRawXml(commonAncestor, new XmlDocument());
			var ourNode = XmlUtilities.GetDocumentNodeFromRawXml(ourContent, new XmlDocument());
			var theirNode = XmlUtilities.GetDocumentNodeFromRawXml(theirContent, new XmlDocument());
			var premerger = new FeatureSpecsPreMerger();
			premerger.Premerge(new ListenerForUnitTests(), ref ourNode, theirNode, ancestorNode);

			var modNode = ourNode.ChildNodes[1]; // skip some white space
			Assert.That(modNode.Attributes["guid"].Value, Is.EqualTo("5a57f521-c4c1-477b-8c08-cdbf92dc9470"));

			// try the other way around.
			premerger = new FeatureSpecsPreMerger();
			premerger.Premerge(new ListenerForUnitTests(), ref theirNode, ourNode, ancestorNode);

			modNode = ourNode.ChildNodes[1]; // skip some white space
			Assert.That(modNode.Attributes["guid"].Value, Is.EqualTo("5a57f521-c4c1-477b-8c08-cdbf92dc9470"));
		}
	}
}
