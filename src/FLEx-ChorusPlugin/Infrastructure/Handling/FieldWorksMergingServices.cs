using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Chorus.merge.xml.generic;

namespace FLEx_ChorusPlugin.Infrastructure.Handling
{
	/// <summary>
	/// Service class used by FieldWorksCommonMergeStrategy to make sure several properties never conflict.
	///
	/// The latest one will always win, no matter what.
	/// </summary>
	internal static class FieldWorksMergingServices
	{
		internal static void PreMerge(MetadataCache mdc, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			if (ourEntry == null || theirEntry == null)
				return;

			PreMergeTimestamps(mdc, ourEntry, theirEntry);
			PreMergeBooleans(mdc, ourEntry, theirEntry, commonEntry);
		}

		private static void PreMergeBooleans(MetadataCache mdc, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			var classInfo = mdc.GetClassInfo("StTxtPara");
			var stTxtClassHierarchy = new List<FdoClassInfo>(classInfo.AllSubclasses)
				{
					classInfo
				};
			classInfo = mdc.GetClassInfo(GetClassName(ourEntry));
			foreach (var boolPropInfo in classInfo.AllProperties.Where(pi => pi.DataType == DataType.Boolean))
			{
				if (boolPropInfo.PropertyName == "ParseIsCurrent" && stTxtClassHierarchy.Contains(classInfo))
				{
					PreMergeStTxtParaParseIsCurrent(ourEntry, theirEntry, commonEntry);
					continue;
				}

				var query = boolPropInfo.PropertyName;
				if (boolPropInfo.IsCustomProperty)
					query = string.Format("Custom[@name='{0}']", boolPropInfo.PropertyName);

				var ourBoolPropElement = ourEntry.SelectSingleNode(query);
				var ourBoolPropAttr = ourBoolPropElement == null ? null : ourBoolPropElement.Attributes[SharedConstants.Val];
				var ourBoolPropValue = ourBoolPropAttr != null && bool.Parse(ourBoolPropAttr.Value);
				var theirBoolPropElement = theirEntry.SelectSingleNode(query);
				var theirBoolPropAttr = theirBoolPropElement == null ? null : theirBoolPropElement.Attributes[SharedConstants.Val];
				var theirBoolPropValue = theirBoolPropAttr != null && bool.Parse(theirBoolPropAttr.Value);
				if (ourBoolPropValue == theirBoolPropValue)
					continue;

				var commonBoolPropElement = commonEntry == null ? null : commonEntry.SelectSingleNode(boolPropInfo.PropertyName);
				if (commonBoolPropElement != null)
					continue;

				// They are different, and the default value is false, and thus no real change from the null of common.
				// So, make sure they are both true.
				if (!ourBoolPropValue)
					ourBoolPropAttr.Value = "True";
				else
					theirBoolPropAttr.Value = "True";
			}

			// Move on down ownership structure.
			foreach (var owningPropInfo in classInfo.AllOwningProperties)
			{
				var propName = owningPropInfo.PropertyName;
				var isCustomProperty = owningPropInfo.IsCustomProperty;
				var ourOwningPropElement = GetOwningPropertyElement(ourEntry, propName, isCustomProperty);
				if (ourOwningPropElement == null || !ourOwningPropElement.HasChildNodes)
					continue;
				var theirOwningPropElement = GetOwningPropertyElement(theirEntry, propName, isCustomProperty);
				if (theirOwningPropElement == null || !theirOwningPropElement.HasChildNodes)
					continue;
				foreach (XmlNode ourOwnedElement in ourOwningPropElement.ChildNodes)
				{
					var theirOwnedElement = (theirOwningPropElement.ChildNodes.Cast<XmlNode>()
						.Where(theirChildNode => ourOwnedElement.Attributes[SharedConstants.GuidStr].Value.ToLowerInvariant() == theirChildNode.Attributes[SharedConstants.GuidStr].Value.ToLowerInvariant()))
						.FirstOrDefault();
					if (theirOwnedElement == null)
						continue;
					var commonOwningPropElement = commonEntry == null
													? null
													: GetOwningPropertyElement(commonEntry, propName, isCustomProperty);
					var commonOwnedElement = commonOwningPropElement == null ? null : (commonOwningPropElement.ChildNodes.Cast<XmlNode>()
						.Where(commonChildNode => ourOwnedElement.Attributes[SharedConstants.GuidStr].Value.ToLowerInvariant() == commonChildNode.Attributes[SharedConstants.GuidStr].Value.ToLowerInvariant()))
						.FirstOrDefault();
					PreMergeBooleans(mdc, ourOwnedElement, theirOwnedElement, commonOwnedElement);
				}
			}
		}

		private static void PreMergeTimestamps(MetadataCache mdc, XmlNode ourEntry, XmlNode theirEntry)
		{
			const string xpath = "DateModified | DateResolved | RunDate";
			var ourDateTimeNodes = ourEntry.SelectNodes(xpath);
			var theirDateTimeNodes = theirEntry.SelectNodes(xpath);

			for (var i = 0; i < ourDateTimeNodes.Count; ++i)
			{
				var ourNode = ourDateTimeNodes[i];
				var asUtcOurs = GetTimestamp(ourNode);

				var theirNode = theirDateTimeNodes[i];
				var asUtcTheirs = GetTimestamp(theirNode);

				if (asUtcOurs == asUtcTheirs)
					return;

				if (asUtcOurs > asUtcTheirs)
					theirNode.Attributes[SharedConstants.Val].Value = ourNode.Attributes[SharedConstants.Val].Value;
				else
					ourNode.Attributes[SharedConstants.Val].Value = theirNode.Attributes[SharedConstants.Val].Value;
			}

			// Drill down and do all owned objects
			var classname = GetClassName(ourEntry);
			var classInfo = mdc.GetClassInfo(classname);
			foreach (var owningPropInfo in classInfo.AllOwningProperties)
			{
				var propName = owningPropInfo.PropertyName;
				var isCustomProperty = owningPropInfo.IsCustomProperty;
				var ourOwningPropElement = GetOwningPropertyElement(ourEntry, propName, isCustomProperty);
				if (ourOwningPropElement == null || !ourOwningPropElement.HasChildNodes)
					continue;
				var theirOwningPropElement = GetOwningPropertyElement(theirEntry, propName, isCustomProperty);
				if (theirOwningPropElement == null || !theirOwningPropElement.HasChildNodes)
					continue;
				foreach (XmlNode ourOwnedElement in ourOwningPropElement.ChildNodes)
				{
					var theirOwnedElement = (theirOwningPropElement.ChildNodes.Cast<XmlNode>()
						.Where(theirChildNode => ourOwnedElement.Attributes[SharedConstants.GuidStr].Value.ToLowerInvariant() == theirChildNode.Attributes[SharedConstants.GuidStr].Value.ToLowerInvariant()))
						.FirstOrDefault();
					if (theirOwnedElement == null)
						continue;
					PreMergeTimestamps(mdc, ourOwnedElement, theirOwnedElement);
				}
			}
		}

		/// <summary>
		/// Set any StTxtPara.ParseIsCurrent to false if either party in the merge changed the text.
		/// </summary>
		private static void PreMergeStTxtParaParseIsCurrent(XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			const string xpath = "ParseIsCurrent";
			var ourParseIsCurrentNode = ourEntry.SelectSingleNode(xpath);
			var theirParseIsCurrentNode = theirEntry.SelectSingleNode(xpath);
			var commonParseIsCurrentNode = commonEntry == null ? null : commonEntry.SelectSingleNode(xpath);
			if (commonParseIsCurrentNode != null)
			{
				if (ourParseIsCurrentNode != null)
				{
					var weMadeChange = !XmlUtilities.AreXmlElementsEqual(ourEntry, commonEntry);
					ourParseIsCurrentNode.Attributes[SharedConstants.Val].Value = "False";
					if (theirParseIsCurrentNode != null)
					{
						// All three exist, so check equality of parent nodes.
						if (weMadeChange)
						{
							theirParseIsCurrentNode.Attributes[SharedConstants.Val].Value = "False";
						}
						else
						{
							var theyMadeChange = !XmlUtilities.AreXmlElementsEqual(theirEntry, commonEntry);
							if (theyMadeChange)
							{
								ourParseIsCurrentNode.Attributes[SharedConstants.Val].Value = "False";
								theirParseIsCurrentNode.Attributes[SharedConstants.Val].Value = "False";
							}
						}
					}
					//else
					//{
					//    // commonParseIsCurrentNode != null : ourParseIsCurrentNode != null : theirParseIsCurrentNode == null
					//    // Not to worry.
					//}
				}
				//else
				//{
				//    // commonParseIsCurrentNode != null : ourParseIsCurrentNode == null
				//    // Not to worry about theirParseIsCurrentNode or any changes.
				//}
			}
			else
			{
				// commonParseIsCurrentNode == null
				if (ourParseIsCurrentNode != null && theirParseIsCurrentNode != null)
				{
					// Set both to False.
					ourParseIsCurrentNode.Attributes[SharedConstants.Val].Value = "False";
					theirParseIsCurrentNode.Attributes[SharedConstants.Val].Value = "False";
				}
				//else
				//{
				//    // ours or theirs is false, so the other one added it. Not to worry.
				//}
			}
		}

		/// <summary>
		/// NOTE: Consider moving this to a services class that provides Metadatacache type data for fwdata xml
		/// Q (RBR: Why does it need the MDC?)
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		internal static string GetClassName(XmlNode element)
		{
			// Owning collections do nothing special for the main element name;
			var name = element.Name;
			return (name == SharedConstants.Ownseq || name == SharedConstants.curiosity)
				? element.Attributes[SharedConstants.Class].Value
				: name;
		}

		/// <summary>
		/// NOTE: Consider moving this to a services class that provides Metadatacache type data for fwdata xml
		/// Q (RBR: Why does it need the MDC?)
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		internal static string GetClassName(XElement element)
		{
			// Owning collections do nothing special for the main element name;
			var name = element.Name.LocalName;
			return (name == SharedConstants.Ownseq || name == SharedConstants.curiosity)
				? element.Attribute(SharedConstants.Class).Value
				: name;
		}

		private static XmlNode GetOwningPropertyElement(XmlNode currentEntry, string propName, bool isCustomProperty)
		{
			if (currentEntry == null)
				return null;

			// May return null, which is fine.
			return isCustomProperty
				? (currentEntry.SelectNodes(SharedConstants.Custom).Cast<XmlNode>().Where(customProp => customProp.Attributes[SharedConstants.Name].Value == propName)).FirstOrDefault()
				: currentEntry.SelectSingleNode(propName);
		}

		private static DateTime GetTimestamp(XmlNode node)
		{
			var timestamp = node.Attributes[SharedConstants.Val].Value;
			var dateParts = timestamp.Split(new[] { '-', ' ', ':', '.' });
			return new DateTime(
				Int32.Parse(dateParts[0]),
				Int32.Parse(dateParts[1]),
				Int32.Parse(dateParts[2]),
				Int32.Parse(dateParts[3]),
				Int32.Parse(dateParts[4]),
				Int32.Parse(dateParts[5]),
				Int32.Parse(dateParts[6]));
		}
	}
}