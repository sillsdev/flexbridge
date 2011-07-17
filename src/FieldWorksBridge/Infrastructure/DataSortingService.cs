using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FieldWorksBridge.Infrastructure
{
	internal static class DataSortingService
	{
		internal static XElement SortCustomPropertiesRecord(byte[] optionalFirstElement)
		{
			var customPropertiesElement = XElement.Parse(MultipleFileServices.Utf8.GetString(optionalFirstElement));

			// <CustomField name="Certified" class="WfiWordform" type="Boolean" />

			// 1. Sort child elements by using a compound key of 'class'+'name'.
			var sortedProperties = new SortedDictionary<string, XElement>();
			foreach (var customProperty in customPropertiesElement.Elements())
			{
// ReSharper disable PossibleNullReferenceException
				// Needs to add 'key' attr, which is class+name, so fast splitter has one id attr to use in its work.
				customProperty.Add(new XAttribute("key", customProperty.Attribute("class").Value + customProperty.Attribute("name").Value));
				sortedProperties.Add(customProperty.Attribute("key").Value, customProperty);
// ReSharper restore PossibleNullReferenceException
			}
			customPropertiesElement.Elements().Remove();
			foreach (var propertyKvp in sortedProperties)
				customPropertiesElement.Add(propertyKvp.Value);

			// Sort all attributes.
			SortAttributes(customPropertiesElement);

			return customPropertiesElement;
		}

		internal static void SortAttributes(XElement element)
		{
			if (element.HasElements)
			{
				foreach (var childElement in element.Elements())
					SortAttributes(childElement);
			}

			if (!element.HasAttributes || element.Attributes().Count() <= 1)
				return;

			var sortedAttributes = new SortedDictionary<string, XAttribute>();
			foreach (var attr in element.Attributes())
				sortedAttributes.Add(attr.Name.LocalName, attr);

			element.Attributes().Remove();
			foreach (var sortedAttrKvp in sortedAttributes)
				element.Add(sortedAttrKvp.Value);
		}

		internal static void SortMainElement(IDictionary<string, HashSet<string>> collectionPropertiesCache, string className, XElement rtElement)
		{
			// Get collection properties for the class.
			HashSet<string> colPropNames;
			if (!collectionPropertiesCache.TryGetValue(className, out colPropNames))
				colPropNames = new HashSet<string>();

			var sortedPropertyElements = new SortedDictionary<string, XElement>();
			foreach (var propertyElement in rtElement.Elements())
			{
				var propName = propertyElement.Name.LocalName;
				// <Custom name="Certified" val="True" />
// ReSharper disable PossibleNullReferenceException
				if (propName == "Custom")
					propName = propertyElement.Attribute("name").Value; // Sort custom props by their name attrs.
// ReSharper restore PossibleNullReferenceException
				if (colPropNames.Contains(propName))
					SortCollectionProperties(propertyElement);
				sortedPropertyElements.Add(propName, propertyElement);
			}
			rtElement.Elements().Remove();
			foreach (var kvp in sortedPropertyElements)
				rtElement.Add(kvp.Value);

			// 3. Sort attributes at all levels.
			SortAttributes(rtElement);
		}

		private static void SortCollectionProperties(XElement propertyElement)
		{
			// Write collection properties in guid sorted order,
			// since order is not significant in collections,
			// but it will  be easier on Hg.
			var sortCollectionData = new SortedDictionary<string, XElement>();
			foreach (var objsurElement in propertyElement.Elements("objsur"))
			{
// ReSharper disable PossibleNullReferenceException
				sortCollectionData.Add(objsurElement.Attribute("guid").Value, objsurElement);
// ReSharper restore PossibleNullReferenceException
			}
			if (sortCollectionData.Count > 1)
			{
				propertyElement.Elements().Remove();
				foreach (var kvp in sortCollectionData)
					propertyElement.Add(kvp.Value);
			}
		}
	}
}