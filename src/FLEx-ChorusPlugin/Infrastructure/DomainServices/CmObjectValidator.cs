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
						className = obj.Attribute(SharedConstants.Class).Value;
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
							if (!isCustomProperty && propertyElement.HasAttributes)
								return "Has unrecognized attributes.";
							if (!propertyElement.HasElements)
								continue; // No children.
							foreach (var ownedElement in propertyElement.Elements())
							{
								result = ValidateObject(mdc, ownedElement);
								if (result != null)
									return result;
							}
							break;
						case DataType.OwningSequence:
							if (!isCustomProperty && propertyElement.HasAttributes)
								return "Has unrecognized attributes.";
							if (!propertyElement.HasElements)
								continue; // No children.
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
								result = ValidateObject(mdc, ownseqChildElement);
								if (result != null)
									return result;
							}
							break;
						case DataType.OwningAtomic:
							if (!isCustomProperty && propertyElement.HasAttributes)
								return "Has unrecognized attributes.";
							var children = propertyElement.Elements().ToList();
							if (children.Count > 1)
								return "Has too many child elements.";
							if (children.Count == 0)
								continue;
							result = ValidateObject(mdc, children[0]);
							if (result != null)
								return result;
							break;

						case DataType.ReferenceCollection:
							if (!isCustomProperty && propertyElement.HasAttributes)
								return "Has unrecognized attributes.";
							var otherElements = propertyElement.Elements().Where(childElement => childElement.Name.LocalName != SharedConstants.Refcol);
							if (otherElements.Any())
								return "Contains child elements that are not 'refcol'.";
							if (propertyElement.Elements(SharedConstants.Refcol).Any(refcolElement => ReferencePropertyIsInvalid(refcolElement.Attributes().ToList(), out result)))
								return result;
							break;
						case DataType.ReferenceSequence:
							if (!isCustomProperty && propertyElement.HasAttributes)
								return "Has unrecognized attributes.";
							var otherchildElements = propertyElement.Elements().Where(childElement => childElement.Name.LocalName != SharedConstants.Refseq);
							if (otherchildElements.Any())
								return "Contains child elements that are not 'refseq'.";
							if (propertyElement.Elements(SharedConstants.Refseq).Any(refseqElement => ReferencePropertyIsInvalid(refseqElement.Attributes().ToList(), out result)))
								return result;
							break;
						case DataType.ReferenceAtomic:
							if (!isCustomProperty && propertyElement.HasAttributes)
								return "Has unrecognized attributes.";
							if (propertyElement.Elements().Count() > 1)
								return "Has too many child elements.";
							var objsur = propertyElement.Element(SharedConstants.Objsur);
							if (objsur == null)
								continue;
							if (objsur.Elements().Any())
								return "'objsur' element has child element(s).";
							var objsurAttrs = objsur.Attributes().ToList();
							if (objsurAttrs.Count > 2)
								return "Has too many attributes.";
							if (ReferencePropertyIsInvalid(objsurAttrs, out result))
								return result;
							result = null;
							break;

						case DataType.MultiUnicode:
							if (!isCustomProperty && propertyElement.HasAttributes)
								return "Has unrecognized attributes.";
							if (propertyElement.Elements().Any(childElement => childElement.Name.LocalName != SharedConstants.AUni))
								return "Has non-AUni child element.";

							foreach (var uniAlt in propertyElement.Elements(SharedConstants.AUni))
							{
								if (uniAlt.Attributes().Count() > 1)
									return "Has too many attributes.";
								var wsAttr = uniAlt.Attribute(SharedConstants.Ws);
								if (wsAttr == null)
									return "Does not have required 'ws' attribute.";
								if (uniAlt.Elements().Any(childElement => childElement.NodeType != XmlNodeType.Text))
									return "Has non-text child element.";
							}
							break;
						case DataType.MultiString:
							if (!isCustomProperty && propertyElement.HasAttributes)
								return "Has unrecognized attributes.";
							if (propertyElement.Elements().Any(childElement => childElement.Name.LocalName != SharedConstants.AStr))
								return "Has non-AStr child element.";
							// TODO: Deal with AStr content.
							break;
						case DataType.Unicode:
							// Ordinary C# string. May, or may not, have content.
							if (!isCustomProperty && propertyElement.HasAttributes)
								return "Has unrecognized attributes.";
							if (propertyElement.Elements().Any(childElement => childElement.NodeType != XmlNodeType.Text))
								return "Has non-text child element";
							break;
						case DataType.String:
							// TsString.
							if (!isCustomProperty && propertyElement.HasAttributes)
								return "Has unrecognized attributes.";
							if (propertyElement.Elements().Count() > 1)
								return "Has too many child elements.";
							var strElement = propertyElement.Element(SharedConstants.Str);
							// TODO: Deal with TsStringContents.
							break;
						case DataType.Binary:
							// contains array of bytes.
							if (!isCustomProperty && propertyElement.HasAttributes)
								return "Has unrecognized attributes.";
							if (propertyElement.Elements().Count() > 1)
								return "Has too many child elements.";
							break;
						case DataType.TextPropBinary:
							// Has one Prop element
							if (!isCustomProperty && propertyElement.HasAttributes)
								return "Has unrecognized attributes.";
							if (propertyElement.Elements().Count() > 1)
								return "Has too many child elements.";
							var propElement = propertyElement.Element(SharedConstants.Prop);
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