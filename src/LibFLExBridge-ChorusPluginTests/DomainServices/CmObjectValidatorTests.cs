// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Xml.Linq;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibFLExBridgeChorusPlugin.DomainServices;
using NUnit.Framework;

namespace LibFLExBridgeChorusPluginTests.DomainServices
{
	[TestFixture]
	public class CmObjectValidatorTests
	{
		private MetadataCache _mdc;

		[SetUp]
		public void TestSetup()
		{
			_mdc = MetadataCache.TestOnlyNewCache;
		}

		[TearDown]
		public void TestTearDown()
		{
			_mdc = null;
		}

		private static void AddBasicPropertyElementsToPossList(XElement element)
		{
			element.Add(new XElement("DateCreated", new XAttribute(FlexBridgeConstants.Val, "2013-1-1 19:39:28.829")));
			element.Add(new XElement("DateModified", new XAttribute(FlexBridgeConstants.Val, "2013-1-1 19:39:28.829")));
			element.Add(new XElement("Depth", new XAttribute(FlexBridgeConstants.Val, 0)));
			element.Add(new XElement("PreventChoiceAboveLevel", new XAttribute(FlexBridgeConstants.Val, 1)));
			element.Add(new XElement("IsSorted", new XAttribute(FlexBridgeConstants.Val, "True")));
			element.Add(new XElement("IsClosed", new XAttribute(FlexBridgeConstants.Val, "True")));
			element.Add(new XElement("PreventDuplicates", new XAttribute(FlexBridgeConstants.Val, "True")));
			element.Add(new XElement("PreventNodeChoices", new XAttribute(FlexBridgeConstants.Val, "True")));
			element.Add(new XElement("UseExtendedFields", new XAttribute(FlexBridgeConstants.Val, "True")));
			element.Add(new XElement("DisplayOption", new XAttribute(FlexBridgeConstants.Val, 2)));
			element.Add(new XElement("ItemClsid", new XAttribute(FlexBridgeConstants.Val, 25)));
			element.Add(new XElement("IsVernacular", new XAttribute(FlexBridgeConstants.Val, "True")));
			element.Add(new XElement("WsSelector", new XAttribute(FlexBridgeConstants.Val, 5)));
			element.Add(new XElement("ListVersion", new XAttribute(FlexBridgeConstants.Val, "c1ee3112-e382-11de-8a39-0800200c9a66")));
		}

		private static void AddBasicPropertyElementsToPoss(XElement element)
		{
			element.Add(new XElement("SortSpec", new XAttribute(FlexBridgeConstants.Val, 1)));
			element.Add(new XElement("DateCreated", new XAttribute(FlexBridgeConstants.Val, "2013-1-1 19:39:28.829")));
			element.Add(new XElement("DateModified", new XAttribute(FlexBridgeConstants.Val, "2013-1-1 19:39:28.829")));
			element.Add(new XElement("ForeColor", new XAttribute(FlexBridgeConstants.Val, 1)));
			element.Add(new XElement("BackColor", new XAttribute(FlexBridgeConstants.Val, 1)));
			element.Add(new XElement("UnderColor", new XAttribute(FlexBridgeConstants.Val, 1)));
			element.Add(new XElement("UnderStyle", new XAttribute(FlexBridgeConstants.Val, 1)));
			element.Add(new XElement("Hidden", new XAttribute(FlexBridgeConstants.Val, "True")));
			element.Add(new XElement("IsProtected", new XAttribute(FlexBridgeConstants.Val, "True")));
		}

		[Test]
		public void EnsureNullMetadataCacheThrows()
		{
			Assert.Throws<ArgumentNullException>(() => CmObjectValidator.ValidateObject(null, new XElement("test")));
		}

		[Test]
		public void EnsureNullObjectThrows()
		{
			Assert.Throws<ArgumentNullException>(() => CmObjectValidator.ValidateObject(_mdc, null));
		}

		[Test]
		public void EnsureValidObjectReturnsNull()
		{
			Assert.IsNull(CmObjectValidator.ValidateObject(_mdc, new XElement("Reminder", new XAttribute(FlexBridgeConstants.GuidStr, "c1ee09ff-e382-11de-8a39-0800200c9a66"),
				new XElement("Date", new XAttribute(FlexBridgeConstants.Val, "SomeGenDataData")))));
		}

		[Test]
		public void NonModelObjectReturnsMessage()
		{
			var obj = new XElement("randomelement");
			var result = CmObjectValidator.ValidateObject(_mdc, obj);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("No guid attribute"));
			obj.Add(new XAttribute(FlexBridgeConstants.GuidStr, "c1ee0a00-e382-11de-8a39-0800200c9a66"));
			result = CmObjectValidator.ValidateObject(_mdc, obj);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("No recognized class"));
		}

		[Test]
		public void NoGuidReturnsMessage()
		{
			Assert.IsNotNull(CmObjectValidator.ValidateObject(_mdc, new XElement("PartOfSpeech")));
		}

		[Test]
		public void NotGuidStringReturnsMessage()
		{
			Assert.IsNotNull(CmObjectValidator.ValidateObject(_mdc, new XElement("PartOfSpeech")));
		}

		[Test]
		public void EnsureObjectWithownerguidAttributeReturnsNotNull()
		{
			Assert.IsNotNull(CmObjectValidator.ValidateObject(_mdc, new XElement("PartOfSpeech", new XAttribute(FlexBridgeConstants.GuidStr, "c1ee0a01-e382-11de-8a39-0800200c9a66"), new XAttribute(FlexBridgeConstants.OwnerGuid, "c1ee0a02-e382-11de-8a39-0800200c9a66"))));
		}

		[Test]
		public void EnsureOwnseqHasClassAttribute()
		{
			var ownSeqElement = new XElement(FlexBridgeConstants.Ownseq, new XAttribute(FlexBridgeConstants.GuidStr, "c1ee0a03-e382-11de-8a39-0800200c9a66"));
			var result = CmObjectValidator.ValidateObject(_mdc, ownSeqElement);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Has no class attribute"));

			ownSeqElement.Add(new XAttribute(FlexBridgeConstants.Class, "PartOfSpeech"));
			AddBasicPropertyElementsToPoss(ownSeqElement);
			result = CmObjectValidator.ValidateObject(_mdc, ownSeqElement);
			Assert.IsNull(result);
		}

		[Test]
		public void EmptyMainObjectElementThatHasNoBasicPropertiesIsValid()
		{
			var element = new XElement("RnGenericRec", new XAttribute(FlexBridgeConstants.GuidStr, "c1ee0a04-e382-11de-8a39-0800200c9a66"));

			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);
		}

		[Test]
		public void AbstractClassIsMostlyNotValid()
		{
			var obj = new XElement("CmObject", new XAttribute(FlexBridgeConstants.GuidStr, "c1ee0a05-e382-11de-8a39-0800200c9a66"));
			var result = CmObjectValidator.ValidateObject(_mdc, obj);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Abstract class"));

			obj.Name = FlexBridgeConstants.DsChart;
			var classAttr = new XAttribute(FlexBridgeConstants.Class, "DsConstChart");
			obj.Add(classAttr);
			obj.Add(new XElement("DateCreated", new XAttribute(FlexBridgeConstants.Val, "2013-1-1 19:39:28.829")));
			obj.Add(new XElement("DateModified", new XAttribute(FlexBridgeConstants.Val, "2013-1-1 19:39:28.829")));
			Assert.IsNull(CmObjectValidator.ValidateObject(_mdc, obj));

			obj.Name = FlexBridgeConstants.CmAnnotation;
			classAttr.Value = "CmBaseAnnotation";
			obj.Add(new XElement("BeginOffset", new XAttribute(FlexBridgeConstants.Val, 1)));
			obj.Add(new XElement("Flid", new XAttribute(FlexBridgeConstants.Val, 1)));
			obj.Add(new XElement("EndOffset", new XAttribute(FlexBridgeConstants.Val, 1)));
			obj.Add(new XElement("WsSelector", new XAttribute(FlexBridgeConstants.Val, 1)));
			obj.Add(new XElement("BeginRef", new XAttribute(FlexBridgeConstants.Val, 1)));
			obj.Add(new XElement("EndRef", new XAttribute(FlexBridgeConstants.Val, 1)));
			Assert.IsNull(CmObjectValidator.ValidateObject(_mdc, obj));
		}

		[Test]
		public void NonPropertyChildHasMessage()
		{
			var element = new XElement("PartOfSpeech", new XAttribute(FlexBridgeConstants.GuidStr, "c1ee0a06-e382-11de-8a39-0800200c9a66"), new XElement("bogusProp"));
			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Not a property element child"));
		}

		[Test]
		public void GuidPropertyHasCorrectResponses()
		{
			_mdc.UpgradeToVersion(MetadataCache.MaximumModelVersion);
			var element = new XElement("CmPossibilityList", new XAttribute(FlexBridgeConstants.GuidStr, "c1ee0a07-e382-11de-8a39-0800200c9a66"));
			AddBasicPropertyElementsToPossList(element);

			var prop = element.Element("ListVersion");
			var attr = prop.Attribute(FlexBridgeConstants.Val);
			attr.Value = "badvalue";
			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);

			attr.Value = "c1ee3110-e382-11de-8a39-0800200c9a66";
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);

			prop.Remove();
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("ListVersion"));
		}

		[Test]
		public void TimePropertyHasCorrectResponses()
		{
			_mdc.UpgradeToVersion(MetadataCache.MaximumModelVersion);
			var element = new XElement("CmPossibilityList", new XAttribute(FlexBridgeConstants.GuidStr, "c1ee0a08-e382-11de-8a39-0800200c9a66"));
			AddBasicPropertyElementsToPossList(element);

			var prop = element.Element("DateCreated"); // new XElement("DateCreated");
			var attr = prop.Attribute(FlexBridgeConstants.Val); // new XAttribute(SharedConstants.Val, "badvalue");
			attr.Value = "badvalue";
			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);

			attr.Value = "2013-1-1 19:39:28.829";
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);

			prop.Remove();
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("DateCreated"));
		}

		[Test]
		public void BooleanPropertyHasCorrectResponses()
		{
			_mdc.UpgradeToVersion(MetadataCache.MaximumModelVersion);
			var element = new XElement("CmPossibility", new XAttribute(FlexBridgeConstants.GuidStr, "c1ee0a09-e382-11de-8a39-0800200c9a66"));
			AddBasicPropertyElementsToPoss(element);

			var prop = element.Element("IsProtected");
			var attr = prop.Attribute(FlexBridgeConstants.Val); // new XAttribute(SharedConstants.Val, "badvalue");
			attr.Value = "badvalue";

			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			attr.Value = "True";
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);

			prop.Remove();
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("IsProtected"));
		}

		[Test]
		public void IntegerPropertyHasCorrectResponses()
		{
			_mdc.UpgradeToVersion(MetadataCache.MaximumModelVersion);
			var element = new XElement("CmPossibility", new XAttribute(FlexBridgeConstants.GuidStr, "c1ee0a0a-e382-11de-8a39-0800200c9a66"));
			AddBasicPropertyElementsToPoss(element);

			var prop = element.Element("ForeColor");
			var attr = prop.Attribute(FlexBridgeConstants.Val);// new XAttribute(SharedConstants.Val, "badvalue");
			attr.Value = "badvalue";
			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);

			attr.Value = "25";
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);

			element.Element("ForeColor").Remove();
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("ForeColor"));
		}

		[Test]
		public void IntegerPropertyForOptionalBasicDataTypesHasCorrectResponses()
		{
			var element = new XElement("CmPossibility", new XAttribute(FlexBridgeConstants.GuidStr, "c1ee0a0b-e382-11de-8a39-0800200c9a66"));
			AddBasicPropertyElementsToPoss(element);

			var prop = element.Element("ForeColor");
			var attr = prop.Attribute(FlexBridgeConstants.Val);// new XAttribute(SharedConstants.Val, "badvalue");
			attr.Value = "badvalue";
			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);

			attr.Value = "25";
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);

			element.Element("ForeColor").Remove();
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);
		}

		[Test]
		public void NullGenDatePropertyHasCorrectResponses()
		{
			_mdc.UpgradeToVersion(MetadataCache.MaximumModelVersion);
			var element = new XElement("Reminder", new XAttribute(FlexBridgeConstants.GuidStr, "c1ee3100-e382-11de-8a39-0800200c9a66"));
			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Date"));
		}

		[Test]
		public void UnicodePropertyHasCorrectResponses()
		{
			var element = new XElement("CmFilter", new XAttribute(FlexBridgeConstants.GuidStr, "c1ee3101-e382-11de-8a39-0800200c9a66"));
			var prop = new XElement("Name");
			var attr = new XAttribute(FlexBridgeConstants.Val, "badvalue");
			prop.Add(attr);
			element.Add(prop);
			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Has unrecognized attribute(s)"));
			attr.Remove();

			var extraElement = new XElement("badchild");
			prop.Add(extraElement);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Unexpected child element"));
			extraElement.Remove();

			var uniElement = new XElement(FlexBridgeConstants.Uni);
			prop.Add(uniElement);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);

			uniElement.Value = "SomeText.";
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);

			uniElement.Add(attr);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Has unrecognized attribute(s)"));
			attr.Remove();

			uniElement.Add(extraElement);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Has non-text child element"));
			extraElement.Remove();

			var extraUniElement = new XElement(FlexBridgeConstants.Uni);
			prop.Add(extraUniElement);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Too many child elements"));
			extraUniElement.Remove();
		}

		[Test]
		public void TsStringHasCorrectResponses()
		{
			const string str = @"<ownseq
						class='StTxtPara'
						guid='cf379f73-9ee5-4e45-b2e2-4b169666d83e'>
						<Contents>
							<Str>
								<Run
									ws='en'>Hi there.</Run>
							</Str>
						</Contents>
						</ownseq>";
			var element = XElement.Parse(str);
			element.Add(new XElement("ParseIsCurrent", new XAttribute(FlexBridgeConstants.Val, "True")));
			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);

			element.Element("Contents").Add(new XAttribute("bogusAttr", "badvalue"));
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Has unrecognized attribute(s)"));
			element.Element("Contents").Attributes().Remove();

			element.Element("Contents").Add(new XElement("extraChild"));
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Has too many child elements"));
		}

		[Test]
		public void MultiStringHasCorrectRepsonses()
		{
			const string str = @"<CmPossibilityList
						guid='cf379f73-9ee5-4e45-b2e2-4b169666d83e'>
		<Description>
			<AStr
				ws='en'>
				<Run
					ws='en'>English multi-string description.</Run>
			</AStr>
			<AStr
				ws='es'>
				<Run
					ws='es'>Spanish multi-string description.</Run>
			</AStr>
		</Description>
						</CmPossibilityList>";
			var element = XElement.Parse(str);
			AddBasicPropertyElementsToPossList(element);
			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);

			var badAttr = new XAttribute("bogusAttr", "badvalue");
			element.Element("Description").Add(badAttr);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Has unrecognized attribute(s)"));
			badAttr.Remove();

			var extraChild = new XElement("extraChild");
			element.Element("Description").Add(extraChild);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Has non-AStr child element"));
			extraChild.Remove();

			// Test the <Run> element.
			var runElement = element.Element("Description").Element("AStr").Element("Run");
			runElement.Add(extraChild);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Has non-text child element"));
			extraChild.Remove();

			runElement.Add(badAttr);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Element <Run> has invalid attribute(s) 'bogusAttr'"));
			badAttr.Remove();
		}

		[Test]
		public void MultiUnicodeHasCorrectRepsonses()
		{
			const string str = @"<CmPossibilityList
						guid='cf379f73-9ee5-4e45-b2e2-4b169666d83e'>
		<Name>
			<AUni
				ws='en'>Genres &amp;</AUni>
			<AUni
				ws='es'>Géneros &amp;</AUni>
		</Name>
						</CmPossibilityList>";
			var element = XElement.Parse(str);
			AddBasicPropertyElementsToPossList(element);
			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);

			element.Element("Name").Add(new XAttribute("bogusAttr", "badvalue"));
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Has unrecognized attribute(s)"));
			element.Element("Name").Attributes().Remove();

			element.Element("Name").Add(new XElement("extraChild"));
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Has non-AUni child element"));
			element.Element("Name").Element("extraChild").Remove();

			element.Element("Name").Element("AUni").Add(new XAttribute("bogusAttr", "badValue"));
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Has too many attributes"));
			var wsAttr = element.Element("Name").Element("AUni").Attribute("ws");
			wsAttr.Remove();
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Does not have required 'ws' attribute"));
			element.Element("Name").Element("AUni").Attribute("bogusAttr").Remove();
			element.Element("Name").Element("AUni").Add(wsAttr);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);

			var extraChild = new XElement("extraChild");
			element.Element("Name").Element("AUni").Add(extraChild);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Has non-text child element"));
			extraChild.Remove();

			// Comment doesn't count, as trouble.
			var comment = new XComment("Some comment.");
			element.Element("Name").Element("AUni").Add(comment);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);
			comment.Remove();
		}

		[Test]
		public void ReferenceAtomicPropertyHasCorrectResponses()
		{
			var element = new XElement("CmPossibility", new XAttribute(FlexBridgeConstants.GuidStr, "c1ee3102-e382-11de-8a39-0800200c9a66"));
			AddBasicPropertyElementsToPoss(element);
			var prop = new XElement("Confidence");
			element.Add(prop);
			var objsurElement = new XElement(FlexBridgeConstants.Objsur);
			prop.Add(objsurElement);
			const string guidValue = "c1ee3113-e382-11de-8a39-0800200c9a66";
			var guidAttr = new XAttribute(FlexBridgeConstants.GuidStr, guidValue);
			var typeAttr = new XAttribute("t", "r");
			objsurElement.Add(guidAttr);
			objsurElement.Add(typeAttr);

			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);

			var extraAttr = new XAttribute("bogus", "badvalue");
			prop.Add(extraAttr);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Has unrecognized attribute(s)"));

			extraAttr.Remove();
			objsurElement.Add(extraAttr);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Has too many attributes"));
			extraAttr.Remove();

			var extraChild = new XElement("BogusChild");
			objsurElement.Add(extraChild);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("'objsur' element has child element(s)"));
			extraChild.Remove();

			prop.Add(extraChild);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Has too many child elements"));
			extraChild.Remove();

			guidAttr.Value = "badValue";
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			guidAttr.Value = guidValue;

			guidAttr.Remove();
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			objsurElement.Add(guidAttr);

			typeAttr.Value = "o";
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			typeAttr.Value = "r";

			typeAttr.Remove();
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
		}

		[Test]
		public void ReferenceSequencePropertyHasCorrectResponses()
		{
			var element = new XElement("Segment", new XAttribute(FlexBridgeConstants.GuidStr, "c1ee3103-e382-11de-8a39-0800200c9a66"), new XElement("BeginOffset", new XAttribute(FlexBridgeConstants.Val, 1)));
			var prop = new XElement("Analyses");
			element.Add(prop);

			var extraAttr = new XAttribute("bogus", "badvalue");
			prop.Add(extraAttr);
			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Has unrecognized attribute(s)"));
			extraAttr.Remove();

			var extraChild = new XElement("BogusChild");
			prop.Add(extraChild);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Contains child elements that are not 'refseq'"));
			extraChild.Remove();

			var refseq1 = new XElement(FlexBridgeConstants.Refseq, new XAttribute(FlexBridgeConstants.GuidStr, "c1ee3104-e382-11de-8a39-0800200c9a66"),
									   new XAttribute("t", "r"));
			var refseq2 = new XElement(FlexBridgeConstants.Refseq, new XAttribute(FlexBridgeConstants.GuidStr, "c1ee3105-e382-11de-8a39-0800200c9a66"),
									   new XAttribute("t", "r"));
			prop.Add(refseq1);
			prop.Add(refseq2);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);
		}

		[Test]
		public void ReferenceCollectionPropertyHasCorrectResponses()
		{
			var element = new XElement("CmPossibility", new XAttribute(FlexBridgeConstants.GuidStr, "c1ee3106-e382-11de-8a39-0800200c9a66"));
			AddBasicPropertyElementsToPoss(element);
			var prop = new XElement("Restrictions");
			element.Add(prop);

			var extraAttr = new XAttribute("bogus", "badvalue");
			prop.Add(extraAttr);
			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Has unrecognized attribute(s)"));
			extraAttr.Remove();

			var extraChild = new XElement("BogusChild");
			prop.Add(extraChild);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Contains child elements that are not 'refcol'"));
			extraChild.Remove();

			var refcol1 = new XElement(FlexBridgeConstants.Refcol, new XAttribute(FlexBridgeConstants.GuidStr, "c1ee3107-e382-11de-8a39-0800200c9a66"),
									   new XAttribute("t", "r"));
			var refcol2 = new XElement(FlexBridgeConstants.Refcol, new XAttribute(FlexBridgeConstants.GuidStr, "c1ee3108-e382-11de-8a39-0800200c9a66"),
									   new XAttribute("t", "r"));
			prop.Add(refcol1);
			prop.Add(refcol2);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);
		}

		[Test]
		public void OwningAtomicPropertyHasCorrectResponses()
		{
			var element = new XElement("CmPossibility", new XAttribute(FlexBridgeConstants.GuidStr, "c1ee3109-e382-11de-8a39-0800200c9a66"));
			AddBasicPropertyElementsToPoss(element);
			var prop = new XElement("Discussion");
			element.Add(prop);

			var extraAttr = new XAttribute("bogus", "badvalue");
			prop.Add(extraAttr);
			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Has unrecognized attribute(s)"));
			extraAttr.Remove();

			var stText1 = new XElement("StText", new XAttribute(FlexBridgeConstants.GuidStr, "c1ee310a-e382-11de-8a39-0800200c9a66"), new XElement("DateModified", new XAttribute(FlexBridgeConstants.Val, "2013-1-1 19:39:28.829")), new XElement("RightToLeft", new XAttribute(FlexBridgeConstants.Val, "True")));
			prop.Add(stText1);
			var stText2 = new XElement("StText", new XAttribute(FlexBridgeConstants.GuidStr, "c1ee310b-e382-11de-8a39-0800200c9a66"), new XElement("DateModified", new XAttribute(FlexBridgeConstants.Val, "2013-1-1 19:39:28.829")), new XElement("RightToLeft", new XAttribute(FlexBridgeConstants.Val, "True")));
			prop.Add(stText2);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Has too many child elements"));
			stText2.Remove();

			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);

			stText1.Attribute(FlexBridgeConstants.GuidStr).Remove();
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("No guid attribute"));
		}

		[Test]
		public void OwningSequencePropertyHasCorrectResponses()
		{
			var element = new XElement("CmPossibility", new XAttribute(FlexBridgeConstants.GuidStr, "c1ee310c-e382-11de-8a39-0800200c9a66"));
			AddBasicPropertyElementsToPoss(element);
			var prop = new XElement("SubPossibilities");
			element.Add(prop);

			var extraAttr = new XAttribute("bogus", "badvalue");
			prop.Add(extraAttr);
			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Has unrecognized attribute(s)"));
			extraAttr.Remove();

			// No children is fine.
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);

			var extraChild = new XElement("BogusChild");
			prop.Add(extraChild);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Contains unrecognized child elements"));
			extraChild.Remove();
		}

		[Test]
		public void OwningCollectionPropertyHasCorrectResponses()
		{
			var element = new XElement("StText", new XAttribute(FlexBridgeConstants.GuidStr, "c1ee310d-e382-11de-8a39-0800200c9a66"), new XElement("DateModified", new XAttribute(FlexBridgeConstants.Val, "2013-1-1 19:39:28.829")), new XElement("RightToLeft", new XAttribute(FlexBridgeConstants.Val, "True")));
			var prop = new XElement("Tags");
			element.Add(prop);

			// Owns col of TextTag

			var extraAttr = new XAttribute("bogus", "badvalue");
			prop.Add(extraAttr);
			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Has unrecognized attribute(s)"));
			extraAttr.Remove();

			// No children is fine.
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);

			var ttElement1 = new XElement("TextTag", new XAttribute(FlexBridgeConstants.GuidStr, "c1ee310e-e382-11de-8a39-0800200c9a66"), new XElement("BeginAnalysisIndex", new XAttribute(FlexBridgeConstants.Val, 1)), new XElement("EndAnalysisIndex", new XAttribute(FlexBridgeConstants.Val, 1)));
			var ttElement2 = new XElement("TextTag", new XAttribute(FlexBridgeConstants.GuidStr, "c1ee310f-e382-11de-8a39-0800200c9a66"), new XElement("BeginAnalysisIndex", new XAttribute(FlexBridgeConstants.Val, 1)), new XElement("EndAnalysisIndex", new XAttribute(FlexBridgeConstants.Val, 1)));
			prop.Add(ttElement1);
			prop.Add(ttElement2);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);
		}
	}
}