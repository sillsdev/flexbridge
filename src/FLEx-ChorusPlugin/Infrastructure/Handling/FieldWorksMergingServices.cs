using System;
using System.Linq;
using System.Xml;

namespace FLEx_ChorusPlugin.Infrastructure.Handling
{
	/// <summary>
	/// Service class used by FieldWorksCommonMergeStrategy to make sure "DateModified", "DateResolved", and "RunDate" never conflict.
	///
	/// The latest one will always win, no matter what.
	/// </summary>
	internal static class FieldWorksMergingServices
	{
		internal static void PreMergeTimestamps(MetadataCache mdc, XmlNode ourEntry, XmlNode theirEntry)
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
			var classname = ourEntry.Name == SharedConstants.Ownseq ? ourEntry.Attributes["class"].Value : ourEntry.Name;
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