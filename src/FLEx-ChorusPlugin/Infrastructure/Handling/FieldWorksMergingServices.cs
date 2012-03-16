using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

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
			PreMergeTimestamps(mdc, ourEntry, theirEntry);
			PreMergeStTxtParaParseIsCurrent(mdc, ourEntry, theirEntry);
			PreMergeBooleans(mdc, ourEntry, theirEntry, commonEntry);
		}

		private static void PreMergeBooleans(MetadataCache mdc, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			if (ourEntry == null || theirEntry == null)
				return;

			var classInfo = mdc.GetClassInfo(GetClassName(ourEntry));
			foreach (var boolPropInfo in classInfo.AllProperties.Where(pi => pi.DataType == DataType.Boolean))
			{
				if (boolPropInfo.PropertyName == "ParseIsCurrent")
					continue; // Handled elsewhere.

				var ourBoolPropElement = ourEntry.SelectSingleNode(boolPropInfo.PropertyName);
				var ourBoolPropAttr = ourBoolPropElement == null ? null : ourBoolPropElement.Attributes["val"];
				var ourBoolPropValue = ourBoolPropAttr != null && bool.Parse(ourBoolPropAttr.Value);
				var theirBoolPropElement = theirEntry.SelectSingleNode(boolPropInfo.PropertyName);
				var theirBoolPropAttr = theirBoolPropElement == null ? null : theirBoolPropElement.Attributes["val"];
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
			if (ourEntry == null || theirEntry == null)
				return;

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
					theirNode.Attributes["val"].Value = ourNode.Attributes["val"].Value;
				else
					ourNode.Attributes["val"].Value = theirNode.Attributes["val"].Value;
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
		private static void PreMergeStTxtParaParseIsCurrent(MetadataCache mdc, XmlNode ourEntry, XmlNode theirEntry)
		{
			// We are only interested in StTxtPara and ScrTxtPara instances, since they have the ParseIsCurrent property.
			// We can quit for a given set on inputs, if we do find the prop, since they do not nest paras inside of paras.
			if (ourEntry == null || theirEntry == null)
				return; // Nothing to do.

			const string xpath = "ParseIsCurrent";
			var ourParseIsCurrentNode = ourEntry.SelectSingleNode(xpath);
			var theirParseIsCurrentNode = theirEntry.SelectSingleNode(xpath);

			if (ourParseIsCurrentNode != null && theirParseIsCurrentNode != null)
			{
				// Only need to pre-merge them if they both exist,
				var ourValue = bool.Parse(ourParseIsCurrentNode.Attributes["val"].Value);
				var theirValue = bool.Parse(theirParseIsCurrentNode.Attributes["val"].Value);
				if (ourValue != theirValue)
				{
					// and, they are different.
					// Set both to False.
					ourParseIsCurrentNode.Attributes["val"].Value = "False";
					theirParseIsCurrentNode.Attributes["val"].Value = "False";
				}
				return;
			}

			// Drill down on other owned stuff.
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
					PreMergeStTxtParaParseIsCurrent(mdc, ourOwnedElement, theirOwnedElement);
				}
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
			return (name == SharedConstants.Ownseq || name == SharedConstants.OwnseqAtomic || name == SharedConstants.curiosity)
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
			return (name == SharedConstants.Ownseq || name == SharedConstants.OwnseqAtomic || name == SharedConstants.curiosity)
				? element.Attribute(SharedConstants.Class).Value
				: name;
		}

		private static XmlNode GetOwningPropertyElement(XmlNode currentEntry, string propName, bool isCustomProperty)
		{
			// May return null, which is fine.
			return isCustomProperty
				? (currentEntry.SelectNodes(SharedConstants.Custom).Cast<XmlNode>().Where(customProp => customProp.Attributes[SharedConstants.Name].Value == propName)).FirstOrDefault()
				: currentEntry.SelectSingleNode(propName);
		}

		private static DateTime GetTimestamp(XmlNode node)
		{
			var timestamp = node.Attributes["val"].Value;
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