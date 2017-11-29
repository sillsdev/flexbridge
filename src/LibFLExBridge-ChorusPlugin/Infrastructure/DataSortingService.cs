// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LibFLExBridgeChorusPlugin.Infrastructure
{
	/// <summary>
	/// Class used to sort all of the data in the XML BEP, so Mercurial doesn't suffer so much.
	/// </summary>
	internal static class DataSortingService
	{
		internal static XElement SortCustomPropertiesRecord(string optionalFirstElement)
		{
			var customPropertiesElement = XElement.Parse(optionalFirstElement);

			//SortCustomPropertiesRecord(customPropertiesElement);
			// <CustomField name="Certified" class="WfiWordform" type="Boolean" ... />

			// 1. Sort child elements by using a compound key of 'class'+'name'.
			var sortedCustomProperties = new SortedDictionary<string, XElement>();
			foreach (var customProperty in customPropertiesElement.Elements())
			{
				// Needs to add 'key' attr, which is class+name, so fast splitter has one id attr to use in its work.
				var keyValue = customProperty.Attribute(FlexBridgeConstants.Class).Value + customProperty.Attribute(FlexBridgeConstants.Name).Value;
				customProperty.Add(new XAttribute("key", keyValue));
				sortedCustomProperties.Add(keyValue, customProperty);
			}
			customPropertiesElement.Elements().Remove();
			foreach (var propertyKvp in sortedCustomProperties)
				customPropertiesElement.Add(propertyKvp.Value);

			// Sort all attributes.
			SortAttributes(customPropertiesElement);

			return customPropertiesElement;
		}

		internal static XElement TestingSortMainElement(string rootData)
		{
			var sortedResult = XElement.Parse(rootData);

			SortMainRtElement(sortedResult);

			return sortedResult;
		}

		internal static void SortMainRtElement(XElement rootData)
		{
			var className = rootData.Attribute(FlexBridgeConstants.Class).Value;
			var classInfo = MetadataCache.MdCache.GetClassInfo(className);

			// Get collection properties for the class.
			var collData = (from collProp in classInfo.AllCollectionProperties select collProp.PropertyName).ToList();
			var multiAltData = (from multiAltProp in classInfo.AllMultiAltProperties select multiAltProp.PropertyName).ToList();

			var sortedPropertyElements = new SortedDictionary<string, XElement>();
			foreach (var propertyElement in rootData.Elements())
			{
				var propName = propertyElement.Name.LocalName;
				// <Custom name="Certified" val="True" />
				if (propName == FlexBridgeConstants.Custom)
					propName = propertyElement.Attribute(FlexBridgeConstants.Name).Value; // Sort custom props by their name attrs.
				if (collData.Contains(propName))
					SortCollectionProperties(propertyElement);
				if (multiAltData.Contains(propName))
					SortMultiSomethingProperty(propertyElement);
				sortedPropertyElements.Add(propName, propertyElement);
			}
			rootData.Elements().Remove();
			foreach (var kvp in sortedPropertyElements)
				rootData.Add(kvp.Value);

			// 3. Sort attributes at all levels.
			SortAttributes(rootData);
		}

		internal static void SortAttributes(XElement element)
		{
			if (element.HasElements)
			{
				foreach (var childElement in element.Elements())
					SortAttributes(childElement);
			}

			if (element.Attributes().Count() < 2)
				return;

			var sortedAttributes = new SortedDictionary<string, XAttribute>();
			foreach (var attr in element.Attributes())
				sortedAttributes.Add(attr.Name.LocalName, attr);

			element.Attributes().Remove();
			foreach (var sortedAttrKvp in sortedAttributes)
				element.Add(sortedAttrKvp.Value);
		}

		internal static void SortMultiSomethingProperty(XContainer multiSomethingProperty)
		{
			if (multiSomethingProperty.Elements().Count() < 2)
				return;

			var sortedAlternativeElements = new SortedDictionary<string, XElement>();
			foreach (var alternativeElement in multiSomethingProperty.Elements())
			{
				var ws = alternativeElement.Attribute("ws").Value;
				sortedAlternativeElements.Add(ws, alternativeElement);
			}

			multiSomethingProperty.Elements().Remove();
			foreach (var kvp in sortedAlternativeElements)
				multiSomethingProperty.Add(kvp.Value);
		}

		internal static void SortCollectionProperties(XContainer propertyElement)
		{
			if (propertyElement.Elements().Count() < 2)
				return;

			// Write collection properties in guid sorted order,
			// since order is not significant in collections.
			var sortCollectionData = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			foreach (var objsurElement in propertyElement.Elements(FlexBridgeConstants.Objsur))
			{
				var key = objsurElement.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant();
				if (!sortCollectionData.ContainsKey(key))
					sortCollectionData.Add(key, objsurElement);
			}

			propertyElement.Elements().Remove();
			foreach (var kvp in sortCollectionData)
				propertyElement.Add(kvp.Value);
		}
	}
}