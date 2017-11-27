// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using LibFLExBridgeChorusPlugin.Contexts;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibFLExBridgeChorusPlugin.Properties;

namespace LibFLExBridgeChorusPlugin.DomainServices
{
	/// <summary>
	/// This class takes a CmObject (as an XElement) and flattens out all owned objects.
	/// </summary>
	internal static class CmObjectFlatteningService
	{
		internal static void FlattenOwnerlessObject(
			string pathname,
			SortedDictionary<string, XElement> sortedData,
			XElement element)
		{
			if (element == null) throw new ArgumentNullException("element");
			if (element.Attribute("ownerguid") != null)
				throw new ArgumentException("FlattenOwnerlessObject cannot be safely used to flatten owned objects");
			FlattenObjectCore(pathname, sortedData, element, null);
		}

		/// <summary>
		/// Flatten an owned object and put an appropriate objsur in the list. //AND RETURN THE OBJSUR which should replace it.
		/// </summary>
		internal static void FlattenOwnedObject(string pathname,
			SortedDictionary<string, XElement> sortedData,
			XElement element, string ownerguid, SortedDictionary<string, XElement> sortedSurrogates)
		{
			FlattenObjectCore(pathname, sortedData, element, ownerguid);
			// We MUST create the objsur AFTER flattening the object, which may pathologically change its guid.
			var guid = element.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant();
			sortedSurrogates.Add(guid, BaseDomainServices.CreateObjSurElement(guid));
		}

		/// <summary>
		/// Flatten an owned object and add an appropriate objsur to the specified property of the owning object.
		/// </summary>
		internal static void FlattenOwnedObject(string pathname,
			SortedDictionary<string, XElement> sortedData,
			XElement element, string ownerguid, XContainer owningElement, string propertyName)
		{
			FlattenObjectCore(pathname, sortedData, element, ownerguid);
			// We MUST create the objsur AFTER flattening the object, which may pathologically change its guid.
			var guid = element.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant();
			BaseDomainServices.RestoreObjsurElement(owningElement, propertyName, BaseDomainServices.CreateObjSurElement(guid));
		}

		private static void FlattenObjectCore(
			string pathname,
			SortedDictionary<string, XElement> sortedData,
			XElement element, string ownerguid)
		{
			if (string.IsNullOrEmpty(pathname)) throw new ArgumentNullException("pathname");
			if (sortedData == null) throw new ArgumentNullException("sortedData");
			if (element == null) throw new ArgumentNullException("element");

			// No, since unowned stuff will feed a null.
			//if (string.IsNullOrEmpty(ownerguid)) throw new ArgumentNullException(SharedConstants.OwnerGuid);
			if (ownerguid != null && ownerguid == string.Empty)
				throw new ArgumentException(Resources.kOwnerGuidEmpty, FlexBridgeConstants.OwnerGuid);

			string className;
			bool isOwnSeqNode;
			var elementGuid = CheckForDuplicateElementMethod.CheckForDuplicateGuid(pathname, sortedData, element, out isOwnSeqNode, out className);
			sortedData.Add(elementGuid, element);

			// The name of 'element' is the class of CmObject, or 'ownseq', or....
			element.Name = FlexBridgeConstants.RtTag;
			if (!isOwnSeqNode)
				element.Add(new XAttribute(FlexBridgeConstants.Class, className));
			//if (element.Attribute(SharedConstants.OwnerGuid) == null)
			if (ownerguid != null) // && element.Attribute(SharedConstants.OwnerGuid) == null)
				element.Add(new XAttribute(FlexBridgeConstants.OwnerGuid, ownerguid));

			// Re-sort those attributes.
			var sortedAttrs = new SortedDictionary<string, XAttribute>();
			foreach (var attribute in element.Attributes())
				sortedAttrs.Add(attribute.Name.LocalName, attribute);
			element.Attributes().Remove();
			element.Add(sortedAttrs.Values);

			// Restore any ref seq props to have 'objsur' elements.
			var mdc = MetadataCache.MdCache;
			var propCache = mdc.PropertyCache[className];
			var refSeqPropNames = propCache["AllReferenceSequence"];
			// Restore any ref col props to have 'objsur' elements.
			var refColPropNames = propCache["AllReferenceCollection"];
			var owningPropsForClass = propCache["AllOwning"];
			if (owningPropsForClass.Count == 0 && refSeqPropNames.Count == 0 && refColPropNames.Count == 0)
				return; // Nothing special to be done for normal properties.

			foreach (var propertyElement in element.Elements().ToArray())
			{
				var isCustomProperty = propertyElement.Name.LocalName == FlexBridgeConstants.Custom;
				var propName = isCustomProperty ? propertyElement.Attribute(FlexBridgeConstants.Name).Value : propertyElement.Name.LocalName;
				if (!owningPropsForClass.Contains(propName))
				{
					if (refSeqPropNames.Contains(propName))
					{
						foreach (var refSeqNode in propertyElement.Elements(FlexBridgeConstants.Refseq))
						{
							refSeqNode.Name = FlexBridgeConstants.Objsur;
						}
					}
					else if (refColPropNames.Contains(propName))
					{
						foreach (var refColNode in propertyElement.Elements(FlexBridgeConstants.Refcol))
						{
							refColNode.Name = FlexBridgeConstants.Objsur;
						}
					}
					continue;
				}
				if (!propertyElement.HasElements)
					continue;

				foreach (var ownedElement in propertyElement.Elements().ToArray())
				{
					if (ownedElement.Name.LocalName == FlexBridgeConstants.Objsur)
						break;
					// Do before the removal call, so we know the parent, and thus, the property name.
					if (isCustomProperty)
					{
						var owningPropertyElement = (element.Elements().Where(customNode => customNode.Name.LocalName == FlexBridgeConstants.Custom && customNode.Attribute(FlexBridgeConstants.Name) != null && customNode.Attribute(FlexBridgeConstants.Name).Value == propertyElement.Attribute(FlexBridgeConstants.Name).Value)).First();
						ownedElement.Remove();
						FlattenObjectCore(pathname, sortedData, ownedElement, elementGuid); // BEFORE we make the objsur!
						BaseDomainServices.RestoreObjsurElement(owningPropertyElement, ownedElement);
					}
					else
					{
						var propertyName = ownedElement.Parent.Name.LocalName;
						ownedElement.Remove();
						// Move down the nested set of owned objects, and do the same.
						FlattenOwnedObject(pathname, sortedData, ownedElement, elementGuid, element, propertyName);
					}
				}
			}
		}

		internal static XmlNode GetXmlNode(XElement element)
		{
			using (XmlReader xmlReader = element.CreateReader())
			{
				var xmlDoc = new XmlDocument();
				xmlDoc.Load(xmlReader);
				return xmlDoc;
			}
		}

		internal static XElement AddNewPropertyElement(XElement parent, string propertyName)
		{
			var sortedRecords = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase)
				{
					{propertyName, new XElement(propertyName)}
				};
			foreach (var propertyNode in parent.Elements())
			{
				var propName = propertyNode.Name.LocalName;
				if (propName == FlexBridgeConstants.Custom)
					propName = propertyNode.Attribute(FlexBridgeConstants.Name).Value;
				sortedRecords.Add(propName, propertyNode);
			}
			parent.RemoveNodes();
			foreach (var sortedPropElement in sortedRecords.Values)
				parent.Add(sortedPropElement);

			return sortedRecords[propertyName];
		}

		internal static void CombineData(IDictionary<string, XElement> results, SortedDictionary<string, XElement> sortedData)
		{
			foreach (var kvp in sortedData.Where(kvp => kvp.Value != null))
			{
				results.Add(kvp.Key, kvp.Value);
			}

			foreach (var key in sortedData.Keys.ToArray())
			{
				sortedData[key] = null;
			}
		}
	}
}