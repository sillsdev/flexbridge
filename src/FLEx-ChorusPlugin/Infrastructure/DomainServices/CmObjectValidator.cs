using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Palaso.Code;

namespace FLEx_ChorusPlugin.Infrastructure.DomainServices
{
	/// <summary>
	/// Validate a CmObject instance, as represented by an XElement.
	/// </summary>
	internal static class CmObjectValidator
	{
		private static readonly HashSet<DataType> DataTypesForValueTypeData = new HashSet<DataType>
			{
				DataType.Boolean,
				DataType.GenDate,
				DataType.Guid,
				DataType.Integer,
				DataType.Time
			};
		/// <summary>
		/// Validate a CmObject instance, as represented by an XElement.
		///
		/// A CmObject may, or may not, contain nested CmObjects.
		/// </summary>
		/// <returns>a String with the first problem found in the CmObject, or null, if no problems were found.</returns>
		/// <exception cref="ArgumentNullException">Thrown is either <paramref name="mdc"/> of <paramref name="obj"/> are null.</exception>
		internal static string ValidateObject(MetadataCache mdc, XElement obj)
		{
			Guard.AgainstNull(mdc, "mdc");
			Guard.AgainstNull(obj, "obj");

			return ValidateObject(mdc, obj, "");
		}

		/// <summary>
		/// Validate a CmObject instance, as represented by an XElement.
		///
		/// A CmObject may, or may not, contain nested CmObjects.
		/// </summary>
		/// <returns>a String with the first problem found in the CmObject, or null, if no problems were found.</returns>
		/// <exception cref="ArgumentNullException">Thrown is either <paramref name="mdc"/> of <paramref name="obj"/> are null.</exception>
		private static string ValidateObject(MetadataCache mdc, XElement obj, string indentation)
		{
			try
			{
				var attribute = obj.Attribute(SharedConstants.GuidStr);

				string result;
				var className = GetClassName(obj, out result);
				if (attribute == null)
					return GetFormattedResult(indentation, null, null, className ?? "", null, "Reports error: ", "No guid attribute");
				var guid = new Guid(attribute.Value); // Will throw if not a guid.
				if (result != null)
					return GetFormattedResult(indentation, null, null, className ?? "", guid, result);
				var classInfo = mdc.GetClassInfo(className);
				if (classInfo == null)
					return GetFormattedResult(indentation, null, null, className, guid, "No recognized class");
				if (classInfo.IsAbstract)
					return GetFormattedResult(indentation, null, null, className, guid, "Abstract class");

				attribute = obj.Attribute(SharedConstants.OwnerGuid);
				if (attribute != null)
					return GetFormattedResult(indentation, null, null, className, guid, "Has 'ownerguid' attribute");

				// Check each property
				var allProperties = classInfo.AllProperties.ToList();
				var allPropertyNames = new HashSet<string>(from prop in allProperties select prop.PropertyName);

				foreach (var propertyElement in obj.Elements())
				{
					if (propertyElement.NodeType != XmlNodeType.Element)
						return GetFormattedResult(indentation, null, propertyElement.Name.LocalName, className, guid, "Not a property element child");

					// Deal with custom properties.
					var isCustomProperty = propertyElement.Name.LocalName == SharedConstants.Custom;
					var propertyName = isCustomProperty ? propertyElement.Attribute(SharedConstants.Name).Value : propertyElement.Name.LocalName;

					if (!allPropertyNames.Contains(propertyName))
						return GetFormattedResult(indentation, null, propertyName, className, guid, "Not a property element child");

					var currentPropertyinfo = (allProperties.Where(pi => pi.PropertyName == propertyName)).First();
					var nextIndentationLevel = indentation + "\t";
					var nextOwningLevel = Environment.NewLine + indentation;
					var nextPropertyLevel = Environment.NewLine + nextIndentationLevel;
					switch (currentPropertyinfo.DataType)
					{
						case DataType.OwningCollection:
							result = ValidateOwningCollectionProperty(mdc, isCustomProperty, propertyElement, nextIndentationLevel);
							if (result != null)
								return GetFormattedResult(nextOwningLevel, "Collection Owning", propertyName, className, guid, "in:", result);
							break;
						case DataType.OwningSequence:
							result = ValidateOwningSequenceProperty(mdc, isCustomProperty, propertyElement, nextIndentationLevel);
							if (result != null)
								return GetFormattedResult(nextOwningLevel, "Sequence Owning", propertyName, className, guid, "in:", result);
							break;
						case DataType.OwningAtomic:
							result = ValidateOwningAtomicProperty(mdc, isCustomProperty, propertyElement, nextIndentationLevel);
							if (result != null)
								return GetFormattedResult(nextOwningLevel, "Atomic Owning", propertyName, className, guid, "in:", result);
							break;

						case DataType.ReferenceCollection:
							result = ValidateReferenceCollectionProperty(isCustomProperty, propertyElement);
							if (result != null)
								return GetFormattedResult(nextPropertyLevel, "Collection Reference", propertyName, className, guid, result);
							break;
						case DataType.ReferenceSequence:
							result = ValidateReferenceSequenceProperty(isCustomProperty, propertyElement);
							if (result != null)
								return GetFormattedResult(nextPropertyLevel, "Sequence Reference", propertyName, className, guid, result);
							break;
						case DataType.ReferenceAtomic:
							result = ValidateReferenceAtomicProperty(isCustomProperty, propertyElement);
							if (result != null)
								return GetFormattedResult(nextPropertyLevel, "Atomic Reference", propertyName, className, guid, result);
							break;

						case DataType.MultiUnicode:
							result = ValidateMultiUnicodeProperty(isCustomProperty, propertyElement);
							if (result != null)
								return GetFormattedResult(nextPropertyLevel, "MultiUnicode", propertyName, className, guid, result);
							break;
						case DataType.MultiString:
							result = ValidateMultiStringProperty(isCustomProperty, propertyElement);
							if (result != null)
								return GetFormattedResult(nextPropertyLevel, "MultiString", propertyName, className, guid, result);
							break;
						case DataType.Unicode:
							result = ValidateUnicodeProperty(isCustomProperty, propertyElement);
							if (result != null)
								return GetFormattedResult(nextPropertyLevel, "Unicode", propertyName, className, guid, result);
							break;
						case DataType.String:
							result = ValidateStringProperty(isCustomProperty, propertyElement);
							if (result != null)
								return GetFormattedResult(nextPropertyLevel, "String (TsString)", propertyName, className, guid, result);
							break;
						case DataType.Binary:
							result = ValidateBinaryProperty(isCustomProperty, propertyElement);
							if (result != null)
								return GetFormattedResult(nextPropertyLevel, "Binary", propertyName, className, guid, result);
							break;
						case DataType.TextPropBinary:
							result = ValidateTextPropBinaryProperty(isCustomProperty, propertyElement);
							if (result != null)
								return GetFormattedResult(nextPropertyLevel, "TextPropBinary", propertyName, className, guid, result);
							break;

						case DataType.Integer:
							if (BasicValueTypeAttributeCheckIsValid(propertyElement, isCustomProperty, out result))
							{
								Int32.Parse(result);
								result = null;
							}
							else
							{
								return GetFormattedResult(nextPropertyLevel, "Integer", propertyName, className, guid, result);
							}
							break;
						case DataType.Boolean:
							if (BasicValueTypeAttributeCheckIsValid(propertyElement, isCustomProperty, out result))
							{
								bool.Parse(result);
								result = null;
							}
							else
							{
								return GetFormattedResult(nextPropertyLevel, "Boolean", propertyName, className, guid, result);
							}
							break;
						case DataType.Time:
							if (BasicValueTypeAttributeCheckIsValid(propertyElement, isCustomProperty, out result))
							{
								DateTime.Parse(result);
								result = null;
							}
							else
							{
								return GetFormattedResult(nextPropertyLevel, "Time (DateTime)", propertyName, className, guid, result);
							}
							break;
						case DataType.GenDate:
							if (BasicValueTypeAttributeCheckIsValid(propertyElement, isCustomProperty, out result))
							{
								// What is a GenDate?
								//var dateTimeAttrVal = DateTime.Parse(element.Attribute(SharedConstants.Val).Value);
								// TODO: Check internals of the GenDate.
								result = null;
							}
							else
							{
								return GetFormattedResult(nextPropertyLevel, "GenDate", propertyName, className, guid, result);
							}
							break;
						case DataType.Guid:
							if (BasicValueTypeAttributeCheckIsValid(propertyElement, isCustomProperty, out result))
							{
								new Guid(result);
								result = null;
							}
							else
							{
								return GetFormattedResult(nextPropertyLevel, "Guid", propertyName, className, guid, result);
							}
							break;
					}
				}

				// Ensure that all value type data property elements exist.
				if (!EnsureBasicValueTypePropertyElementsExist(mdc, classInfo, obj,
															   new HashSet<string>(from prop in allProperties
																				   where DataTypesForValueTypeData.Contains(prop.DataType)
																				   select prop.PropertyName), out result))
				{
					return GetFormattedResult(indentation, null, null, className, guid, result);
				}
			}
			catch (Exception err)
			{
				return err.Message;
			}

			return null;
		}

		private static string GetFormattedResult(string currentIndentation, string propertyType, string propertyName, string className, Guid guid, string error)
		{
			return GetFormattedResult(currentIndentation, propertyType, propertyName, className, guid.ToString(), "Reports error: ", error);
		}

		private static string GetFormattedResult(string currentIndentation, string propertyType, string propertyName, string className, Guid guid, string introduction, string baseText)
		{
			return GetFormattedResult(currentIndentation, propertyType, propertyName, className, guid.ToString(), introduction, baseText);
		}

		private static string GetFormattedResult(string currentIndentation, string propertyType, string propertyName, string className, string guid, string introduction, string baseText)
		{
			return string.Format("{0}{1}{2}{3}{4}{5}{6}",
				currentIndentation, // 0
				(string.IsNullOrEmpty(propertyName) ? "" : string.Format("Invalid {0} '{1}' property of class:", propertyType, propertyName)), // 1
				(className == null ? "" : string.Format(" {0}", className)), // 2
				string.IsNullOrEmpty(guid) ? "" : string.Format("'{0}'", guid), //3
				introduction == null ? " Reports error: " : string.Format(" {0}", introduction), // 4
				baseText, //5
				Environment.NewLine); // 6
		}

		private static string GetClassName(XElement obj, out string result)
		{
			result = null;
			string className;
			XAttribute attribute;
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
					{
						result = "Has no class attribute";
						return null;
					}
					className = attribute.Value;
					break;
				case SharedConstants.Ownseq:
					attribute = obj.Attribute(SharedConstants.Class);
					if (attribute == null)
					{
						result = "Has no class attribute";
						return null;
					}
					className = attribute.Value;
					break;
			}

			return className;
		}

		private static string ValidateTextPropBinaryProperty(bool isCustomProperty, XElement propertyElement)
		{
			if (propertyElement == null)
				return null;
			// Has one Prop element
			if (!isCustomProperty && propertyElement.HasAttributes)
				return "Has unrecognized attribute(s)";
			if (propertyElement.Elements().Count() > 1)
				return "Has too many child elements";

			// Handle 'Prop' element.
			var propElement = propertyElement.Element(SharedConstants.Prop);
			if (propElement == null)
				return null;
			HashSet<string> attrs;
			var result = CheckTextPropertyAttributes(propElement, out attrs);
			if (result != null)
				return result;
			foreach (var childElement in propElement.Elements())
			{
				switch (childElement.Name.LocalName)
				{
					case "BulNumFontInfo":
						if (childElement.Attributes().Any(attr => !attrs.Contains(attr.Name.LocalName)))
						{
							return "Invalid attribute for <BulNumFontInfo> element";
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
								return "Invalid attribute for <WsProp> element";
						}
						break;
					default:
						return "Illegal element in <Prop> element: '" + childElement.Name.LocalName + "'.";
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
			if (propElement == null)
				return null;
			var set = attrs;
			return propElement.Attributes().Any(attr => !set.Contains(attr.Name.LocalName))
				? string.Format("Invalid attribute for <{0}> element", propElement.Name.LocalName)
				: null;
		}

		private static string ValidateBinaryProperty(bool isCustomProperty, XElement propertyElement)
		{
			if (propertyElement == null)
				return null;
			// contains array of bytes.
			if (!isCustomProperty && propertyElement.HasAttributes)
				return "Has unrecognized attribute(s)";
			if (propertyElement.Elements().Count() > 1)
				return "Has too many child elements";
			return null;
		}

		private static string ValidateStringProperty(bool isCustomProperty, XElement propertyElement)
		{
			if (propertyElement == null)
				return null;
			// TsString.
			if (!isCustomProperty && propertyElement.HasAttributes)
				return "Has unrecognized attribute(s)";
			if (propertyElement.Elements().Count() > 1)
				return "Has too many child elements";
			return ValidateComplexTsString(propertyElement.Element(SharedConstants.Str));
		}

		private static string ValidateUnicodeProperty(bool isCustomProperty, XElement propertyElement)
		{
			if (propertyElement == null)
				return null;
			// Ordinary C# string. May, or may not, have content.
			if (!isCustomProperty && propertyElement.HasAttributes)
				return "Has unrecognized attribute(s)";
			if (!propertyElement.HasElements)
				return null; // No <Uni> child.
			if (propertyElement.Elements().Count() > 1)
				return "Too many child elements";
			var uniElement = propertyElement.Element(SharedConstants.Uni);
			if (uniElement == null)
				return "Unexpected child element";
			if (uniElement.HasElements)
				return "Has non-text child element";
			if (uniElement.HasAttributes)
				return "Has unrecognized attribute(s)";
			return null;
		}

		private static string ValidateMultiStringProperty(bool isCustomProperty, XElement propertyElement)
		{
			if (propertyElement == null)
				return null;
			if (!isCustomProperty && propertyElement.HasAttributes)
				return "Has unrecognized attribute(s)";

			if (propertyElement.Elements().Any(childElement => childElement.Name.LocalName != SharedConstants.AStr))
				return "Has non-AStr child element";

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
			if (complexTsStringElement == null)
				return null;
			var runs = complexTsStringElement.Elements("Run").ToList();
			if (!runs.Any())
				return "No <Run> element(s)";

			foreach (var runElement in runs)
			{
				HashSet<string> attrs;
				var result = CheckTextPropertyAttributes(runElement, out attrs);
				if (result != null)
					return result;
				if (runElement.HasElements)
					return "Has non-text child element";
			}
			return null;
		}

		private static string ValidateMultiUnicodeProperty(bool isCustomProperty, XElement propertyElement)
		{
			if (propertyElement == null)
				return null;
			if (!isCustomProperty && propertyElement.HasAttributes)
				return "Has unrecognized attribute(s)";
			if (propertyElement.Elements().Any(childElement => childElement.Name.LocalName != SharedConstants.AUni))
				return "Has non-AUni child element";

			var extantAlts = new HashSet<string>();
			foreach (var uniAlt in propertyElement.Elements(SharedConstants.AUni))
			{
				if (uniAlt.Attributes().Count() > 1)
					return "Has too many attributes";
				var wsAttr = uniAlt.Attribute(SharedConstants.Ws);
				if (wsAttr == null)
					return "Does not have required 'ws' attribute";
				if (uniAlt.HasElements)
					return "Has non-text child element";
				var currentWs = wsAttr.Value;
				if (extantAlts.Contains(currentWs))
					return "Duplicate alternative for ws: " + currentWs;
				extantAlts.Add(currentWs);
			}
			return null;
		}

		private static string ValidateReferenceAtomicProperty(bool isCustomProperty, XElement propertyElement)
		{
			if (propertyElement == null)
				return null;
			if (!isCustomProperty && propertyElement.HasAttributes)
				return "Has unrecognized attribute(s)";
			if (propertyElement.Elements().Count() > 1)
				return "Has too many child elements";
			var objsur = propertyElement.Element(SharedConstants.Objsur);
			if (objsur == null)
				return null;
			if (objsur.Elements().Any())
				return "'objsur' element has child element(s)";
			var objsurAttrs = objsur.Attributes().ToList();
			if (objsurAttrs.Count > 2)
				return "Has too many attributes";
			string result;
			ReferencePropertyIsInvalid(objsurAttrs, out result);
			return result;
		}

		private static string ValidateReferenceSequenceProperty(bool isCustomProperty, XElement propertyElement)
		{
			if (propertyElement == null)
				return null;
			string result = null;
			if (!isCustomProperty && propertyElement.HasAttributes)
				return "Has unrecognized attribute(s)";
			var otherchildElements = propertyElement.Elements().Where(childElement => childElement.Name.LocalName != SharedConstants.Refseq);
			if (otherchildElements.Any())
				return "Contains child elements that are not 'refseq'";
			foreach (var refseqElement in propertyElement.Elements(SharedConstants.Refseq).Where(refseqElement => ReferencePropertyIsInvalid(refseqElement.Attributes().ToList(), out result)))
				break;
			return result;
		}

		private static string ValidateReferenceCollectionProperty(bool isCustomProperty, XElement propertyElement)
		{
			if (propertyElement == null)
				return null;
			string result = null;
			if (!isCustomProperty && propertyElement.HasAttributes)
				return "Has unrecognized attribute(s)";
			var otherElements = propertyElement.Elements().Where(childElement => childElement.Name.LocalName != SharedConstants.Refcol);
			if (otherElements.Any())
				return "Contains child elements that are not 'refcol'";
			foreach (var refcolElement in propertyElement.Elements(SharedConstants.Refcol).Where(refcolElement => ReferencePropertyIsInvalid(refcolElement.Attributes().ToList(), out result)))
				break;
			return result;
		}

		private static string ValidateOwningAtomicProperty(MetadataCache mdc, bool isCustomProperty, XElement propertyElement, string indentation)
		{
			if (propertyElement == null)
				return null;
			if (!isCustomProperty && propertyElement.HasAttributes)
				return "Has unrecognized attribute(s)";
			var children = propertyElement.Elements().ToList();
			return (children.Count > 1)
					? "Has too many child elements"
					: ((children.Count == 0)
						? null
						: ValidateObject(mdc, children[0], indentation));
		}

		private static string ValidateOwningSequenceProperty(MetadataCache mdc, bool isCustomProperty, XElement propertyElement, string indentation)
		{
			if (propertyElement == null)
				return null;
			if (!isCustomProperty && propertyElement.HasAttributes)
				return "Has unrecognized attribute(s)";
			if (!propertyElement.HasElements)
				return null; // No children.
			// ownseq
			var ownseqChildElements = propertyElement.Elements().Where(childElement => childElement.Name.LocalName == SharedConstants.Ownseq).ToList();
			if (ownseqChildElements.Count != propertyElement.Elements().Count())
				return "Contains unrecognized child elements";

			return ownseqChildElements.Select(ownseqChildElement => ValidateObject(mdc, ownseqChildElement, indentation)).FirstOrDefault(result => result != null);
		}

		private static string ValidateOwningCollectionProperty(MetadataCache mdc, bool isCustomProperty, XElement propertyElement, string indentation)
		{
			if (propertyElement == null)
				return null;
			if (!isCustomProperty && propertyElement.HasAttributes)
				return "Has unrecognized attribute(s)";
			if (!propertyElement.HasElements)
				return null;
			return propertyElement.Elements().Select(ownedElement => ValidateObject(mdc, ownedElement, indentation)).FirstOrDefault(result => result != null);
		}

		private static bool ReferencePropertyIsInvalid(List<XAttribute> objsurAttrs, out string result)
		{
			new Guid(objsurAttrs.Single(attr => attr.Name.LocalName == SharedConstants.GuidStr).Value);
			var typeAttrValue = objsurAttrs.Single(attr => attr.Name.LocalName == "t").Value;
			if (typeAttrValue != "r")
			{
				result = "Has incorrect attribute value for reference property";
				return true;
			}
			result = null;
			return false;
		}

		private static bool EnsureBasicValueTypePropertyElementsExist(MetadataCache mdc, FdoClassInfo classInfo, XElement element, IEnumerable<string> basicPropertyNames, out string result)
		{
			result = null;

			if (mdc.ModelVersion < 7000064)
				return true; // The value type data types are only required at DM 7000064, and higher.

			foreach (var basicPropertyName in basicPropertyNames)
			{
				var currentPropName = basicPropertyName;
				var isCustomProperty = classInfo.GetProperty(basicPropertyName).IsCustomProperty;
				var propertyElement = isCustomProperty
					? element.Elements("Custom").FirstOrDefault(propElement => propElement.Attribute(SharedConstants.Name).Value == currentPropName)
					: element.Element(currentPropName);
				if (propertyElement != null)
					continue;

				result = string.Format("Required basic property element '{0}' of class '{1}' is missing.", currentPropName, classInfo.ClassName);
				return false;
			}
			return true;
		}

		private static bool BasicValueTypeAttributeCheckIsValid(XElement element, bool isCustomProperty, out string value)
		{
			Guard.AgainstNull(element, "element");

			if (isCustomProperty)
			{
				if (element.Attributes().Count() != 2)
				{
					value = "Wrong number of attributes";
					return false;
				}
				if (element.Attribute(SharedConstants.Name) == null)
				{
					value = "Custom property has no 'name' attribute";
					return false;
				}
			}
			else
			{
				if (element.Attributes().Count() != 1)
				{
					value = "Wrong number of attributes";
					return false;
				}
			}
			value = element.Attribute(SharedConstants.Val).Value;
			return true;
		}
	}
}