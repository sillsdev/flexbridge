using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace FLEx_ChorusPlugin.Infrastructure.DomainServices
{
	/// <summary>
	/// Validate a CmObject instance, as represented by an XElement.
	/// </summary>
	internal static class CmObjectValidator
	{
		/// <summary>
		/// Validate a CmObject instance, as represented by an XElement.
		///
		/// A CmObject may, or may not, contain nested CmObjects.
		/// </summary>
		/// <returns>a String with the first problem fouud in the CmObject, or null, if no problems were found.</returns>
		/// <exception cref="ArgumentNullException">Thrown is either <paramref name="mdc"/> of <paramref name="obj"/> are null.</exception>
		internal static string ValidateObject(MetadataCache mdc, XElement obj)
		{
			if (mdc == null)
				throw new ArgumentNullException("mdc");
			if (obj == null)
				throw new ArgumentNullException("obj");

			string result = null;

			var attribute = obj.Attribute(SharedConstants.GuidStr);
			if (attribute == null)
				return "No guid attribute";
			try
			{
				new Guid(attribute.Value); // Will throw if not a guid.

				attribute = obj.Attribute(SharedConstants.OwnerGuid);
				if (attribute != null)
					return "Has 'ownerguid' attribute";

				string className;
				// Check odd cases to make sure they each have the right attributes.
				switch (obj.Name.LocalName)
				{
					default:
						className = obj.Name.LocalName;
						break;
					case SharedConstants.DsChart:
					case SharedConstants.CmAnnotation:
						// Abstract class elements, so get real class from 'class' attribute.
						attribute = obj.Attribute(SharedConstants.Class);
						if (attribute == null)
							return "Has no class attribute.";
						className = attribute.Value;
						break;
					case SharedConstants.curiosity:
						attribute = obj.Attribute(SharedConstants.Class);
						if (attribute == null)
							return "Has no class attribute.";
						className = attribute.Value;
						attribute = obj.Attribute("curiositytype");
						if (attribute == null)
							return "Has no curiositytype attribute.";
						var legalValues = new HashSet<string> {"unowned", "lint"};
						if (!legalValues.Contains(attribute.Value))
							return "Has unrecognized curiositytype attribute value.";
						break;
					case SharedConstants.OwnseqAtomic: // Fall through.
					case SharedConstants.Ownseq:
						attribute = obj.Attribute(SharedConstants.Class);
						if (attribute == null)
							return "Has no class attribute.";
						className = attribute.Value;
						break;
				}
				var classInfo = mdc.GetClassInfo(className);
				if (classInfo == null)
					return "No recognized class";
				if (classInfo.IsAbstract)
					return "Abstract class";

				if (!obj.Elements().Any())
					return null;

				// Check each property
				var allProperties = classInfo.AllProperties.ToList();
				var allPropertyNames = new HashSet<string>(from prop in allProperties select prop.PropertyName);

				foreach (var propertyElement in obj.Elements())
				{
					// Deal with custom properties.
					var isCustomProperty = propertyElement.Name.LocalName == SharedConstants.Custom;
					var propertyName = isCustomProperty ? propertyElement.Attribute(SharedConstants.Name).Value : propertyElement.Name.LocalName;

					if (!allPropertyNames.Contains(propertyName))
						return propertyName + " is not a property element child";

					var currentPropertyinfo = (allProperties.Where(pi => pi.PropertyName == propertyName)).First();

					switch (currentPropertyinfo.DataType)
					{
						case DataType.OwningCollection:
							result = ValidateOwningCollectionProperty(mdc, isCustomProperty, propertyElement);
							if (result != null)
								return result;
							break;
						case DataType.OwningSequence:
							result = ValidateOwningSequenceProperty(mdc, isCustomProperty, propertyElement);
							if (result != null)
								return result;
							break;
						case DataType.OwningAtomic:
							result = ValidateOwningAtomicProperty(mdc, isCustomProperty, propertyElement);
							if (result != null)
								return result;
							break;

						case DataType.ReferenceCollection:
							result = ValidateReferenceCollectionProperty(mdc, isCustomProperty, propertyElement);
							if (result != null)
								return result;
							break;
						case DataType.ReferenceSequence:
							result = ValidateReferenceSequenceProperty(mdc, isCustomProperty, propertyElement);
							if (result != null)
								return result;
							break;
						case DataType.ReferenceAtomic:
							result = ValidateReferenceAtomicProperty(mdc, isCustomProperty, propertyElement);
							if (result != null)
								return result;
							break;

						case DataType.MultiUnicode:
							result = ValidateMultiUnicodeProperty(mdc, isCustomProperty, propertyElement);
							if (result != null)
								return result;
							break;
						case DataType.MultiString:
							result = ValidateMultiStringProperty(mdc, isCustomProperty, propertyElement);
							if (result != null)
								return result;
							break;
						case DataType.Unicode:
							result = ValidateUnicodeProperty(mdc, isCustomProperty, propertyElement);
							if (result != null)
								return result;
							break;
						case DataType.String:
							result = ValidateStringProperty(mdc, isCustomProperty, propertyElement);
							if (result != null)
								return result;
							break;
						case DataType.Binary:
							result = ValidateBinaryProperty(mdc, isCustomProperty, propertyElement);
							if (result != null)
								return result;
							break;
						case DataType.TextPropBinary:
							result = ValidateTextPropBinaryProperty(mdc, isCustomProperty, propertyElement);
							if (result != null)
								return result;
							break;

						case DataType.Integer:
							if (BasicValueTypeAttributeCheck(propertyElement, out result))
							{
								var intAttrVal = Int32.Parse(result);
								result = null;
							}
							break;
						case DataType.Boolean:
							if (BasicValueTypeAttributeCheck(propertyElement, out result))
							{
								var boolAttrVal = bool.Parse(result);
								result = null;
							}
							break;
						case DataType.Time:
							if (BasicValueTypeAttributeCheck(propertyElement, out result))
							{
								var dateTimeAttrVal = DateTime.Parse(result);
								result = null;
							}
							break;
						case DataType.GenDate:
							if (BasicValueTypeAttributeCheck(propertyElement, out result))
							{
								// What is a GenDate?
								//var dateTimeAttrVal = DateTime.Parse(element.Attribute("val").Value);
								// TODO: Check internals of the GenDate.
								result = null;
							}
							break;
						case DataType.Guid:
							if (BasicValueTypeAttributeCheck(propertyElement, out result))
							{
								var guidAttrVal = new Guid(result);
								result = null;
							}
							break;
					}
				}
			}
			catch (Exception err)
			{
				return err.Message;
			}

			return result;
		}

		private static string ValidateTextPropBinaryProperty(MetadataCache mdc, bool isCustomProperty, XElement propertyElement)
		{
			// Has one Prop element
			if (!isCustomProperty && propertyElement.HasAttributes)
				return "Has unrecognized attributes.";
			if (propertyElement.Elements().Count() > 1)
				return "Has too many child elements.";

			// Handle 'Prop' element.
			var propElement = propertyElement.Element(SharedConstants.Prop);
			HashSet<string> attrs;
			var result = CheckTextPropertyAttributes(propElement, out attrs);
			if (result != null)
				return result;
			foreach (var childElement in propertyElement.Elements())
			{
				switch (childElement.Name.LocalName)
				{
					case "BulNumFontInfo":
						if (childElement.Attributes().Any(attr => !attrs.Contains(attr.Name.LocalName)))
						{
							return "Invalid attribute for <BulNumFontInfo> element.";
						}
						break;
					case "WsStyles9999":
						foreach (var grandchildElement in childElement.Elements())
						{
							if (grandchildElement.Name.LocalName != "WsProp")
								return "Invalid nested element in <WsStyle9999> element: " + grandchildElement.Name.LocalName;
							var wsAttr = grandchildElement.Attribute("ws");
							if (wsAttr == null)
								return "WsProp must contain a 'ws' attribute.";
							if (grandchildElement.Attributes().Any(attr => !attrs.Contains(attr.Name.LocalName)))
								return "Invalid attribute for <WsProp> element.";
						}
						break;
					default:
						return "Illegal element in <Prop> element: " + childElement.Name.LocalName;
				}
			}
			return null;
		}

		private static string CheckTextPropertyAttributes(XElement propElement, out HashSet<string> attrs)
		{
			attrs = new HashSet<string>
						{
							"align",
							"backcolor",
							"bold",
							"borderBottom",
							"borderColor",
							"borderLeading",
							"borderTop",
							"borderTrailing",
							"bulNumScheme",
							"bulNumStartAt",
							"bulNumTxtAft",
							"bulNumTxtBef",
							"charStyle",
							"contextString",
							"embedded",
							"externalLink",
							"firstIndent",
							"fontFamily",
							"fontsize",
							"fontsizeUnit",
							"fontVariations",
							"forecolor",
							"italic",
							"keepTogether",
							"keepWithNext",
							"leadingIndent",
							"lineHeight",
							"lineHeightType",
							"lineHeightUnit",
							"link",
							"marginBottom",
							"marginLeading",
							"marginTop",
							"marginTrailing",
							"moveableObj",
							"namedStyle",
							"offset",
							"offsetUnit",
							"ownlink",
							"padBottom",
							"padLeading",
							"padTop",
							"padTrailing",
							"paracolor",
							"paraStyle",
							"rightToLeft",
							"spaceAfter",
							"spaceBefore",
							"spellcheck",
							"superscript",
							"tabDef",
							"tabList",
							"tags",
							"trailingIndent",
							"type",
							"undercolor",
							"underline",
							"widowOrphan",
							"ws",
							"wsBase",
							"wsStyle",
							"space"
						};
			var set = attrs;
			return propElement.Attributes().Any(attr => !set.Contains(attr.Name.LocalName))
				? "Invalid attribute for <Prop> element."
				: null;
		}

		private static string ValidateBinaryProperty(MetadataCache mdc, bool isCustomProperty, XElement propertyElement)
		{
			// contains array of bytes.
			if (!isCustomProperty && propertyElement.HasAttributes)
				return "Has unrecognized attributes.";
			if (propertyElement.Elements().Count() > 1)
				return "Has too many child elements.";
			return null;
		}

		private static string ValidateStringProperty(MetadataCache mdc, bool isCustomProperty, XElement propertyElement)
		{
			// TsString.
			if (!isCustomProperty && propertyElement.HasAttributes)
				return "Has unrecognized attributes.";
			if (propertyElement.Elements().Count() > 1)
				return "Has too many child elements.";
			return ValidateComplexTsString(propertyElement.Element(SharedConstants.Str));
		}

		private static string ValidateUnicodeProperty(MetadataCache mdc, bool isCustomProperty, XElement propertyElement)
		{
			// Ordinary C# string. May, or may not, have content.
			if (!isCustomProperty && propertyElement.HasAttributes)
				return "Has unrecognized attributes.";
			if (propertyElement.Elements().Any(childElement => childElement.NodeType != XmlNodeType.Text))
				return "Has non-text child element";
			return null;
		}

		private static string ValidateMultiStringProperty(MetadataCache mdc, bool isCustomProperty, XElement propertyElement)
		{
			if (!isCustomProperty && propertyElement.HasAttributes)
				return "Has unrecognized attributes.";

			if (propertyElement.Elements().Any(childElement => childElement.Name.LocalName != SharedConstants.AStr))
				return "Has non-AStr child element.";

			var extantAlts = new HashSet<string>();
			foreach (var astrElement in propertyElement.Elements(SharedConstants.AStr))
			{
				var result = ValidateComplexTsString(astrElement);
				if (result != null)
					return result;
				var currentWs = astrElement.Attribute(SharedConstants.Ws).Value;
				if (extantAlts.Contains(currentWs))
					return "Duplicate alternative for ws: " + currentWs;
				extantAlts.Add(currentWs);
			}
			return null;
		}

		private static string ValidateComplexTsString(XContainer complexTsStringElement)
		{
			var runs = complexTsStringElement.Elements("Run").ToList();
			if (!runs.Any())
				return "No <Run> elements.";

			foreach (var runElement in runs)
			{
				HashSet<string> attrs;
				var result = CheckTextPropertyAttributes(runElement, out attrs);
				if (result != null)
					return result;
			}
			return null;
		}

		private static string ValidateMultiUnicodeProperty(MetadataCache mdc, bool isCustomProperty, XElement propertyElement)
		{
			if (!isCustomProperty && propertyElement.HasAttributes)
				return "Has unrecognized attributes.";
			if (propertyElement.Elements().Any(childElement => childElement.Name.LocalName != SharedConstants.AUni))
				return "Has non-AUni child element.";

			var extantAlts = new HashSet<string>();
			foreach (var uniAlt in propertyElement.Elements(SharedConstants.AUni))
			{
				if (uniAlt.Attributes().Count() > 1)
					return "Has too many attributes.";
				var wsAttr = uniAlt.Attribute(SharedConstants.Ws);
				if (wsAttr == null)
					return "Does not have required 'ws' attribute.";
				if (uniAlt.Elements().Any(childElement => childElement.NodeType != XmlNodeType.Text))
					return "Has non-text child element.";
				if (string.IsNullOrEmpty(uniAlt.Value))
					return "Has no text data.";
				var currentWs = wsAttr.Value;
				if (extantAlts.Contains(currentWs))
					return "Duplicate alternative for ws: " + currentWs;
				extantAlts.Add(currentWs);
			}
			return null;
		}

		private static string ValidateReferenceAtomicProperty(MetadataCache mdc, bool isCustomProperty, XElement propertyElement)
		{
			if (!isCustomProperty && propertyElement.HasAttributes)
				return "Has unrecognized attributes.";
			if (propertyElement.Elements().Count() > 1)
				return "Has too many child elements.";
			var objsur = propertyElement.Element(SharedConstants.Objsur);
			if (objsur == null)
				return null;
			if (objsur.Elements().Any())
				return "'objsur' element has child element(s).";
			var objsurAttrs = objsur.Attributes().ToList();
			if (objsurAttrs.Count > 2)
				return "Has too many attributes.";
			string result;
			ReferencePropertyIsInvalid(objsurAttrs, out result);
			return result;
		}

		private static string ValidateReferenceSequenceProperty(MetadataCache mdc, bool isCustomProperty, XElement propertyElement)
		{
			string result = null;
			if (!isCustomProperty && propertyElement.HasAttributes)
				return "Has unrecognized attributes.";
			var otherchildElements = propertyElement.Elements().Where(childElement => childElement.Name.LocalName != SharedConstants.Refseq);
			if (otherchildElements.Any())
				return "Contains child elements that are not 'refseq'.";
			foreach (var refseqElement in propertyElement.Elements(SharedConstants.Refseq).Where(refseqElement => ReferencePropertyIsInvalid(refseqElement.Attributes().ToList(), out result)))
				break;
			return result;
		}

		private static string ValidateReferenceCollectionProperty(MetadataCache mdc, bool isCustomProperty, XElement propertyElement)
		{
			string result = null;
			if (!isCustomProperty && propertyElement.HasAttributes)
				return "Has unrecognized attributes.";
			var otherElements = propertyElement.Elements().Where(childElement => childElement.Name.LocalName != SharedConstants.Refcol);
			if (otherElements.Any())
				return "Contains child elements that are not 'refcol'.";
			foreach (var refcolElement in propertyElement.Elements(SharedConstants.Refcol).Where(refcolElement => ReferencePropertyIsInvalid(refcolElement.Attributes().ToList(), out result)))
				break;
			return result;
		}

		private static string ValidateOwningAtomicProperty(MetadataCache mdc, bool isCustomProperty, XElement propertyElement)
		{
			if (!isCustomProperty && propertyElement.HasAttributes)
				return "Has unrecognized attributes.";
			var children = propertyElement.Elements().ToList();
			return (children.Count > 1)
					? "Has too many child elements."
					: ((children.Count == 0)
						? null
						: ValidateObject(mdc, children[0]));
		}

		private static string ValidateOwningSequenceProperty(MetadataCache mdc, bool isCustomProperty, XElement propertyElement)
		{
			if (!isCustomProperty && propertyElement.HasAttributes)
				return "Has unrecognized attributes.";
			if (!propertyElement.HasElements)
				return null; // No children.
			// ownseq XOR ownseqatomic
			var ownseqChildElements =
				propertyElement.Elements().Where(childElement =>
					childElement.Name.LocalName == SharedConstants.Ownseq ||
					childElement.Name.LocalName == SharedConstants.OwnseqAtomic).ToList();
			if (ownseqChildElements.Count != propertyElement.Elements().Count())
				return "Contains unrecognized child elements.";

			if (ownseqChildElements.Count > 1)
			{
				// Make sure they are all have the same element name.
				var name = ownseqChildElements[0].Name.LocalName;
				var otherName = (name == SharedConstants.Ownseq) ? SharedConstants.OwnseqAtomic : SharedConstants.Ownseq;
				var otherOwnSeqElements = propertyElement.Elements().Where(childElement =>
															 childElement.Name.LocalName == otherName).ToList();
				if (otherOwnSeqElements.Count > 0)
					return "Mixed owning sequence element names.";
			}

			foreach (var ownseqChildElement in ownseqChildElements)
			{
				var result = ValidateObject(mdc, ownseqChildElement);
				if (result != null)
					return result;
			}
			return null;
		}

		private static string ValidateOwningCollectionProperty(MetadataCache mdc, bool isCustomProperty,
															   XElement propertyElement)
		{
			if (!isCustomProperty && propertyElement.HasAttributes)
				return "Has unrecognized attributes.";
			if (!propertyElement.HasElements)
				return null;
			foreach (var ownedElement in propertyElement.Elements())
			{
				var result = ValidateObject(mdc, ownedElement);
				if (result != null)
					return result;
			}
			return null;
		}

		private static bool ReferencePropertyIsInvalid(List<XAttribute> objsurAttrs, out string result)
		{
			new Guid(objsurAttrs.Single(attr => attr.Name.LocalName == SharedConstants.GuidStr).Value);
			var typeAttrValue = objsurAttrs.Single(attr => attr.Name.LocalName == "t").Value;
			if (typeAttrValue != "r")
			{
				result = "Has incorrect attribute value for reference property.";
				return true;
			}
			result = null;
			return false;
		}

		private static bool BasicValueTypeAttributeCheck(XElement element, out string value)
		{
			if (element.Attributes().Count() != 1)
			{
				value = "Wrong number of attributes.";
				return false;
			}
			value = element.Attribute("val").Value;
			return true;
		}
	}
}