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
				if (classInfo.IsAbstract && classInfo.ClassName != SharedConstants.DsChart && classInfo.ClassName != SharedConstants.CmAnnotation)
					return "Abstract class";

				// Check each property
				var allProperties = classInfo.AllProperties.ToList();
				var allPropertyNames = new HashSet<string>(from prop in allProperties select prop.PropertyName);
				foreach (var element in obj.Elements())
				{
					if (!allPropertyNames.Contains(element.Name.LocalName))
						return "Not a property element child";

					var currentPropertyinfo = (allProperties.Where(pi => pi.PropertyName == element.Name.LocalName)).First();

					switch (currentPropertyinfo.DataType)
					{
						case DataType.OwningCollection:
							break;
						case DataType.OwningSequence:
							break;
						case DataType.OwningAtomic:
							break;

						case DataType.ReferenceCollection:
							if (element.HasAttributes)
								return "Has unrecognized attributes.";
							var otherElements = element.Elements().Where(childElement => childElement.Name.LocalName != SharedConstants.Refcol);
							if (otherElements.Any())
								return "Contains child elements that are not 'refcol'.";
							if (element.Elements(SharedConstants.Refcol).Any(refcolElement => ReferencePropertyIsInvalid(refcolElement.Attributes().ToList(), out result)))
								return result;
							break;
						case DataType.ReferenceSequence:
							if (element.HasAttributes)
								return "Has unrecognized attributes.";
							var otherchildElements = element.Elements().Where(childElement => childElement.Name.LocalName != SharedConstants.Refseq);
							if (otherchildElements.Any())
								return "Contains child elements that are not 'refseq'.";
							if (element.Elements(SharedConstants.Refseq).Any(refseqElement => ReferencePropertyIsInvalid(refseqElement.Attributes().ToList(), out result)))
								return result;
							break;
						case DataType.ReferenceAtomic:
							if (element.HasAttributes)
								return "Has unrecognized attributes.";
							if (element.Elements().Count() > 1)
								return "Has too many child elements.";
							var objsur = element.Element(SharedConstants.Objsur);
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
							if (element.HasAttributes)
								return "Has unrecognized attributes.";
							if (element.Elements().Any(childElement => childElement.Name.LocalName != SharedConstants.AUni))
								return "Has non-AUni child element.";

							foreach (var uniAlt in element.Elements(SharedConstants.AUni))
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
							if (element.HasAttributes)
								return "Has unrecognized attributes.";
							if (element.Elements().Any(childElement => childElement.Name.LocalName != SharedConstants.AStr))
								return "Has non-AStr child element.";
							// TODO: Deal with AStr content.
							break;
						case DataType.Unicode:
							// Ordinary C# string. May, or may not, have content.
							if (element.HasAttributes)
								return "Has unrecognized attributes.";
							if (element.Elements().Any(childElement => childElement.NodeType != XmlNodeType.Text))
								return "Has non-text child element";
							break;
						case DataType.String:
							// TsString.
							if (element.HasAttributes)
								return "Has unrecognized attributes.";
							if (element.Elements().Count() > 1)
								return "Has too many child elements.";
							var strElement = element.Element(SharedConstants.Str);
							// TODO: Deal with TsStringContents.
							break;
						case DataType.Binary:
							// contains array of bytes.
							if (element.HasAttributes)
								return "Has unrecognized attributes.";
							if (element.Elements().Count() > 1)
								return "Has too many child elements.";
							break;
						case DataType.TextPropBinary:
							// Has one Prop element
							if (element.HasAttributes)
								return "Has unrecognized attributes.";
							if (element.Elements().Count() > 1)
								return "Has too many child elements.";
							var propElement = element.Element(SharedConstants.Prop);
							break;

						case DataType.Integer:
							if (BasicValueTypeAttributeCheck(element, out result))
							{
								var intAttrVal = Int32.Parse(result);
								result = null;
							}
							break;
						case DataType.Boolean:
							if (BasicValueTypeAttributeCheck(element, out result))
							{
								var boolAttrVal = bool.Parse(result);
								result = null;
							}
							break;
						case DataType.Time:
							if (BasicValueTypeAttributeCheck(element, out result))
							{
								var dateTimeAttrVal = DateTime.Parse(result);
								result = null;
							}
							break;
						case DataType.GenDate:
							if (BasicValueTypeAttributeCheck(element, out result))
							{
								// What is a GenDate?
								//var dateTimeAttrVal = DateTime.Parse(element.Attribute("val").Value);
								result = null;
							}
							break;
						case DataType.Guid:
							if (BasicValueTypeAttributeCheck(element, out result))
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