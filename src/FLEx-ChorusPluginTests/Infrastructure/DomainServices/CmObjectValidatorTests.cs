using System;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using NUnit.Framework;

namespace FLEx_ChorusPluginTests.Infrastructure.DomainServices
{
	[TestFixture]
	public class CmObjectValidatorTests
	{
		private MetadataCache _mdc;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_mdc = MetadataCache.MdCache;
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			_mdc = null;
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
			Assert.IsNull(CmObjectValidator.ValidateObject(_mdc, new XElement("PartOfSpeech", new XAttribute(SharedConstants.GuidStr, Guid.NewGuid()))));
		}

		[Test]
		public void NonModelObjectReturnsMessage()
		{
			var obj = new XElement("randomelement");
			var result = CmObjectValidator.ValidateObject(_mdc, obj);
			Assert.IsNotNull(result);
			Assert.AreEqual("No guid attribute", result);
			obj.Add(new XAttribute(SharedConstants.GuidStr, Guid.NewGuid()));
			result = CmObjectValidator.ValidateObject(_mdc, obj);
			Assert.IsNotNull(result);
			Assert.AreEqual("No recognized class", result);
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
			Assert.IsNotNull(CmObjectValidator.ValidateObject(_mdc, new XElement("PartOfSpeech", new XAttribute(SharedConstants.GuidStr, Guid.NewGuid()), new XAttribute(SharedConstants.OwnerGuid, Guid.NewGuid()))));
		}

		[Test]
		public void EnsureCuriosityHasCorrectAttributes()
		{
			var curiosityElement = new XElement(SharedConstants.curiosity, new XAttribute(SharedConstants.Class, "PartOfSpeech"), new XAttribute(SharedConstants.GuidStr, Guid.NewGuid()));
			var result = CmObjectValidator.ValidateObject(_mdc, curiosityElement);
			Assert.IsNotNull(result);
			Assert.AreEqual("Has no curiositytype attribute.", result);
			var ctAttr = new XAttribute("curiositytype", "notrecognized");
			curiosityElement.Add(ctAttr);
			result = CmObjectValidator.ValidateObject(_mdc, curiosityElement);
			Assert.IsNotNull(result);
			Assert.AreEqual("Has unrecognized curiositytype attribute value.", result);
			ctAttr.Value = "lint";
			result = CmObjectValidator.ValidateObject(_mdc, curiosityElement);
			Assert.IsNull(result);
			ctAttr.Value = "unowned";
			result = CmObjectValidator.ValidateObject(_mdc, curiosityElement);
			Assert.IsNull(result);
		}

		[Test]
		public void EnsureOwnseqHasClassAttribute()
		{
			var ownSeqElement = new XElement(SharedConstants.Ownseq, new XAttribute(SharedConstants.GuidStr, Guid.NewGuid()));
			var result = CmObjectValidator.ValidateObject(_mdc, ownSeqElement);
			Assert.IsNotNull(result);
			Assert.AreEqual("Has no class attribute.", result);
			ownSeqElement.Add(new XAttribute(SharedConstants.Class, "PartOfSpeech"));
			result = CmObjectValidator.ValidateObject(_mdc, ownSeqElement);
			Assert.IsNull(result);
		}

		[Test]
		public void EnsureOwnseqAtomicHasClassAttribute()
		{
			var ownSeqElement = new XElement(SharedConstants.OwnseqAtomic, new XAttribute(SharedConstants.GuidStr, Guid.NewGuid()));
			var result = CmObjectValidator.ValidateObject(_mdc, ownSeqElement);
			Assert.IsNotNull(result);
			Assert.AreEqual("Has no class attribute.", result);
			ownSeqElement.Add(new XAttribute(SharedConstants.Class, "PartOfSpeech"));
			result = CmObjectValidator.ValidateObject(_mdc, ownSeqElement);
			Assert.IsNull(result);
		}

		[Test]
		public void AbstractClassIsMostlyNotValid()
		{
			var obj = new XElement("CmObject", new XAttribute(SharedConstants.GuidStr, Guid.NewGuid()));
			var result = CmObjectValidator.ValidateObject(_mdc, obj);
			Assert.IsNotNull(result);
			Assert.AreEqual("Abstract class", result);
			obj.Name = SharedConstants.DsChart;
			var classAttr = new XAttribute(SharedConstants.Class, "DsConstChart");
			obj.Add(classAttr);
			Assert.IsNull(CmObjectValidator.ValidateObject(_mdc, obj));
			obj.Name = SharedConstants.CmAnnotation;
			classAttr.Value = "CmBaseAnnotation";
			Assert.IsNull(CmObjectValidator.ValidateObject(_mdc, obj));
		}

		[Test]
		public void NonPropertyChildHasMessage()
		{
			var element = new XElement("PartOfSpeech", new XAttribute(SharedConstants.GuidStr, Guid.NewGuid()), new XElement("bogusProp"));
			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.IsTrue(result.EndsWith(" is not a property element child"));
		}

		[Test]
		public void GuidPropertyHasCorrectResponses()
		{
			var element = new XElement("CmPossibilityList", new XAttribute(SharedConstants.GuidStr, Guid.NewGuid()));
			var prop = new XElement("ListVersion");
			var attr = new XAttribute("val", "badvalue");
			prop.Add(attr);
			element.Add(prop);
			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			attr.Value = Guid.NewGuid().ToString();
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);
		}

		[Test]
		public void TimePropertyHasCorrectResponses()
		{
			var element = new XElement("CmPossibilityList", new XAttribute(SharedConstants.GuidStr, Guid.NewGuid()));
			var prop = new XElement("DateCreated");
			var attr = new XAttribute("val", "badvalue");
			prop.Add(attr);
			element.Add(prop);
			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			attr.Value = DateTime.Now.ToString();
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);
		}

		[Test]
		public void BooleanPropertyHasCorrectResponses()
		{
			var element = new XElement("CmPossibility", new XAttribute(SharedConstants.GuidStr, Guid.NewGuid()));
			var prop = new XElement("IsProtected");
			var attr = new XAttribute("val", "badvalue");
			prop.Add(attr);
			element.Add(prop);
			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			attr.Value = "True";
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);
		}

		[Test]
		public void IntegerPropertyHasCorrectResponses()
		{
			var element = new XElement("CmPossibility", new XAttribute(SharedConstants.GuidStr, Guid.NewGuid()));
			var prop = new XElement("ForeColor");
			var attr = new XAttribute("val", "badvalue");
			prop.Add(attr);
			element.Add(prop);
			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			attr.Value = "25";
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);
		}

		[Test]
		public void UnicodePropertyHasCorrectResponses()
		{
			var element = new XElement("CmFilter", new XAttribute(SharedConstants.GuidStr, Guid.NewGuid()));
			var prop = new XElement("Name");
			var attr = new XAttribute("val", "badvalue");
			prop.Add(attr);
			element.Add(prop);
			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.AreEqual("Has unrecognized attributes.", result);
			prop.Attribute("val").Remove();

			prop.Add(new XElement("badchild"));
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.AreEqual("Has non-text child element.", result);
		}

		[Test]
		public void TsStringHasCorrectRepsonses()
		{
			const string str = @"<ownseqatomic
						class='StTxtPara'
						guid='cf379f73-9ee5-4e45-b2e2-4b169666d83e'>
						<Contents>
							<Str>
								<Run
									ws='en'>Hi there.</Run>
							</Str>
						</Contents>
						</ownseqatomic>";
			var element = XElement.Parse(str);
			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);

			element.Element("Contents").Add(new XAttribute("bogusAttr", "badvalue"));
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.AreEqual("Has unrecognized attributes.", result);
			element.Element("Contents").Attributes().Remove();

			element.Element("Contents").Add(new XElement("extraChild"));
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.AreEqual("Has too many child elements.", result);
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
			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);

			var badAttr = new XAttribute("bogusAttr", "badvalue");
			element.Element("Description").Add(badAttr);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.AreEqual("Has unrecognized attributes.", result);
			badAttr.Remove();

			var extraChild = new XElement("extraChild");
			element.Element("Description").Add(extraChild);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.AreEqual("Has non-AStr child element.", result);
			extraChild.Remove();

			// Test the <Run> element.
			var runElement = element.Element("Description").Element("AStr").Element("Run");
			runElement.Add(extraChild);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.AreEqual("Has non-text child element.", result);
			extraChild.Remove();

			runElement.Add(badAttr);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.AreEqual("Invalid attribute for <Run> element.", result);
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
			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);

			element.Element("Name").Add(new XAttribute("bogusAttr", "badvalue"));
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.AreEqual("Has unrecognized attributes.", result);
			element.Element("Name").Attributes().Remove();

			element.Element("Name").Add(new XElement("extraChild"));
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.AreEqual("Has non-AUni child element.", result);
			element.Element("Name").Element("extraChild").Remove();

			element.Element("Name").Element("AUni").Add(new XAttribute("bogusAttr", "badValue"));
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.AreEqual("Has too many attributes.", result);
			var wsAttr = element.Element("Name").Element("AUni").Attribute("ws");
			wsAttr.Remove();
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.AreEqual("Does not have required 'ws' attribute.", result);
			element.Element("Name").Element("AUni").Attribute("bogusAttr").Remove();
			element.Element("Name").Element("AUni").Add(wsAttr);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);

			var extraChild = new XElement("extraChild");
			element.Element("Name").Element("AUni").Add(extraChild);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.AreEqual("Has non-text child element.", result);
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
			var element = new XElement("CmPossibility", new XAttribute(SharedConstants.GuidStr, Guid.NewGuid()));
			var prop = new XElement("Confidence");
			element.Add(prop);
			var objsurElement = new XElement(SharedConstants.Objsur);
			prop.Add(objsurElement);
			var guidValue = Guid.NewGuid().ToString();
			var guidAttr = new XAttribute(SharedConstants.GuidStr, guidValue);
			var typeAttr = new XAttribute("t", "r");
			objsurElement.Add(guidAttr);
			objsurElement.Add(typeAttr);

			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);

			var extraAttr = new XAttribute("bogus", "badvalue");
			prop.Add(extraAttr);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.AreEqual("Has unrecognized attributes.", result);

			extraAttr.Remove();
			objsurElement.Add(extraAttr);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.AreEqual("Has too many attributes.", result);
			extraAttr.Remove();

			var extraChild = new XElement("BogusChild");
			objsurElement.Add(extraChild);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.AreEqual("'objsur' element has child element(s).", result);
			extraChild.Remove();

			prop.Add(extraChild);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.AreEqual("Has too many child elements.", result);
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
			var element = new XElement("Segment", new XAttribute(SharedConstants.GuidStr, Guid.NewGuid()));
			var prop = new XElement("Analyses");
			element.Add(prop);

			var extraAttr = new XAttribute("bogus", "badvalue");
			prop.Add(extraAttr);
			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.AreEqual("Has unrecognized attributes.", result);
			extraAttr.Remove();

			var extraChild = new XElement("BogusChild");
			prop.Add(extraChild);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.AreEqual("Contains child elements that are not 'refseq'.", result);
			extraChild.Remove();

			var refseq1 = new XElement(SharedConstants.Refseq, new XAttribute(SharedConstants.GuidStr, Guid.NewGuid().ToString()),
									   new XAttribute("t", "r"));
			var refseq2 = new XElement(SharedConstants.Refseq, new XAttribute(SharedConstants.GuidStr, Guid.NewGuid().ToString()),
									   new XAttribute("t", "r"));
			prop.Add(refseq1);
			prop.Add(refseq2);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);
		}

		[Test]
		public void ReferenceCollectionPropertyHasCorrectResponses()
		{
			var element = new XElement("CmPossibility", new XAttribute(SharedConstants.GuidStr, Guid.NewGuid()));
			var prop = new XElement("Restrictions");
			element.Add(prop);

			var extraAttr = new XAttribute("bogus", "badvalue");
			prop.Add(extraAttr);
			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.AreEqual("Has unrecognized attributes.", result);
			extraAttr.Remove();

			var extraChild = new XElement("BogusChild");
			prop.Add(extraChild);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.AreEqual("Contains child elements that are not 'refcol'.", result);
			extraChild.Remove();

			var refcol1 = new XElement(SharedConstants.Refcol, new XAttribute(SharedConstants.GuidStr, Guid.NewGuid().ToString()),
									   new XAttribute("t", "r"));
			var refcol2 = new XElement(SharedConstants.Refcol, new XAttribute(SharedConstants.GuidStr, Guid.NewGuid().ToString()),
									   new XAttribute("t", "r"));
			prop.Add(refcol1);
			prop.Add(refcol2);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);
		}

		[Test]
		public void OwningAtomicPropertyHasCorrectResponses()
		{
			var element = new XElement("CmPossibility", new XAttribute(SharedConstants.GuidStr, Guid.NewGuid()));
			var prop = new XElement("Discussion");
			element.Add(prop);

			var extraAttr = new XAttribute("bogus", "badvalue");
			prop.Add(extraAttr);
			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.AreEqual("Has unrecognized attributes.", result);
			extraAttr.Remove();

			var stText1 = new XElement("StText", new XAttribute(SharedConstants.GuidStr, Guid.NewGuid()));
			prop.Add(stText1);
			var stText2 = new XElement("StText", new XAttribute(SharedConstants.GuidStr, Guid.NewGuid()));
			prop.Add(stText2);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.AreEqual("Has too many child elements.", result);
			stText2.Remove();

			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);

			stText1.Attribute(SharedConstants.GuidStr).Remove();
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
		}

		[Test]
		public void OwningSequencePropertyHasCorrectResponses()
		{
			var element = new XElement("CmPossibility", new XAttribute(SharedConstants.GuidStr, Guid.NewGuid()));
			var prop = new XElement("SubPossibilities");
			element.Add(prop);

			var extraAttr = new XAttribute("bogus", "badvalue");
			prop.Add(extraAttr);
			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.AreEqual("Has unrecognized attributes.", result);
			extraAttr.Remove();

			// No children is fine.
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);

			var extraChild = new XElement("BogusChild");
			prop.Add(extraChild);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.AreEqual("Contains unrecognized child elements.", result);
			extraChild.Remove();

			var osElement = new XElement(SharedConstants.Ownseq);
			var osaElement = new XElement(SharedConstants.OwnseqAtomic);
			prop.Add(osElement);
			prop.Add(osaElement);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.AreEqual("Mixed owning sequence element names.", result);
		}

		[Test]
		public void OwningCollectionPropertyHasCorrectResponses()
		{
			var element = new XElement("StText", new XAttribute(SharedConstants.GuidStr, Guid.NewGuid()));
			var prop = new XElement("Tags");
			element.Add(prop);

			// Owns col of TextTag

			var extraAttr = new XAttribute("bogus", "badvalue");
			prop.Add(extraAttr);
			var result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNotNull(result);
			Assert.AreEqual("Has unrecognized attributes.", result);
			extraAttr.Remove();

			// No children is fine.
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);

			var ttElement1 = new XElement("TextTag", new XAttribute(SharedConstants.GuidStr, Guid.NewGuid()));
			var ttElement2 = new XElement("TextTag", new XAttribute(SharedConstants.GuidStr, Guid.NewGuid()));
			prop.Add(ttElement1);
			prop.Add(ttElement2);
			result = CmObjectValidator.ValidateObject(_mdc, element);
			Assert.IsNull(result);
		}
	}
}