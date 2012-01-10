using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Palaso.Xml;

namespace FLEx_ChorusPlugin.Infrastructure
{
	/// <summary>
	/// Class used to sort all of the data in the XML BEP, so Mercurial doesn't suffer so much.
	/// </summary>
	internal static class DataSortingService
	{
		internal static void SortEntireFile(Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache, XmlWriter writer, string pathname)
		{
			var readerSettings = new XmlReaderSettings { IgnoreWhitespace = true };

			// Step 2: Sort and rewrite file.
			using (var fastSplitter = new FastXmlElementSplitter(pathname))
			{
				var sortedObjects = new SortedDictionary<string, string>();
				bool foundOptionalFirstElement;
				foreach (var record in fastSplitter.GetSecondLevelElementStrings(SharedConstants.OptionalFirstElementTag, SharedConstants.RtTag, out foundOptionalFirstElement))
				{
					if (foundOptionalFirstElement)
					{
						// Step 2A: Write out custom property declaration(s).
						WriteElement(writer, readerSettings, SortCustomPropertiesRecord(record));
						foundOptionalFirstElement = false;
					}
					else
					{
						// Step 2B: Sort main CmObject record.
						var sortedMainObject = SortMainElement(interestingPropertiesCache, record);
						sortedObjects.Add(sortedMainObject.Attribute(SharedConstants.GuidStr).Value, sortedMainObject.ToString());
					}
				}
				foreach (var sortedObjectKvp in sortedObjects)
				{
					WriteElement(writer, readerSettings, sortedObjectKvp.Value);
				}
			}
		}

		internal static XElement SortCustomPropertiesRecord(string optionalFirstElement)
		{
			var customPropertiesElement = XElement.Parse(optionalFirstElement);

			SortCustomPropertiesRecord(customPropertiesElement);

			return customPropertiesElement;
		}

		internal static void SortCustomPropertiesRecord(XElement customPropertiesElement)
		{
			// <CustomField name="Certified" class="WfiWordform" type="Boolean" ... />

			// 1. Sort child elements by using a compound key of 'class'+'name'.
			var sortedCustomProperties = new SortedDictionary<string, XElement>();
			foreach (var customProperty in customPropertiesElement.Elements())
			{
				// Needs to add 'key' attr, which is class+name, so fast splitter has one id attr to use in its work.
				var keyValue = customProperty.Attribute(SharedConstants.Class).Value + customProperty.Attribute(SharedConstants.Name).Value;
				customProperty.Add(new XAttribute("key", keyValue));
				sortedCustomProperties.Add(keyValue, customProperty);
			}
			customPropertiesElement.Elements().Remove();
			foreach (var propertyKvp in sortedCustomProperties)
				customPropertiesElement.Add(propertyKvp.Value);

			// Sort all attributes.
			SortAttributes(customPropertiesElement);
		}

		internal static XElement SortMainElement(Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache, string rootData)
		{
			var sortedResult = XElement.Parse(rootData);

			SortMainElement(interestingPropertiesCache, sortedResult);

			return sortedResult;
		}

		internal static void SortMainElement(IDictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache, XElement rootData)
		{
			var className = rootData.Attribute(SharedConstants.Class).Value;

			// Get collection properties for the class.
			Dictionary<string, HashSet<string>> sortablePropertiesForClass;
			if (!interestingPropertiesCache.TryGetValue(className, out sortablePropertiesForClass))
			{
				// Appears to be a newly obsolete instance of 'className'.
				sortablePropertiesForClass = new Dictionary<string, HashSet<string>>(3, StringComparer.OrdinalIgnoreCase)
												{
													{SharedConstants.Collections, new HashSet<string>()},
													{SharedConstants.MultiAlt, new HashSet<string>()}
												};
				interestingPropertiesCache.Add(className, sortablePropertiesForClass);
			}

			var collData = sortablePropertiesForClass[SharedConstants.Collections];
			var multiAltData = sortablePropertiesForClass[SharedConstants.MultiAlt];

			var sortedPropertyElements = new SortedDictionary<string, XElement>();
			foreach (var propertyElement in rootData.Elements())
			{
				var propName = propertyElement.Name.LocalName;
				// <Custom name="Certified" val="True" />
				if (propName == "Custom")
					propName = propertyElement.Attribute(SharedConstants.Name).Value; // Sort custom props by their name attrs.
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
			var sortCollectionData = new SortedDictionary<string, XElement>();
			foreach (var objsurElement in propertyElement.Elements(SharedConstants.Objsur))
			{
				var key = objsurElement.Attribute(SharedConstants.GuidStr).Value;
				if (!sortCollectionData.ContainsKey(key))
					sortCollectionData.Add(key, objsurElement);
			}

			propertyElement.Elements().Remove();
			foreach (var kvp in sortCollectionData)
				propertyElement.Add(kvp.Value);
		}

		internal static void WriteElement(XmlWriter writer, XmlReaderSettings readerSettings, XElement element)
		{
			WriteElement(writer, readerSettings, element.ToString());
		}

		internal static void WriteElement(XmlWriter writer, XmlReaderSettings readerSettings, string element)
		{
			using (var nodeReader = XmlReader.Create(new MemoryStream(SharedConstants.Utf8.GetBytes(element), false), readerSettings))
				writer.WriteNode(nodeReader, true);
		}

		internal static void SortAndStoreElement(IDictionary<string, XElement> sortedData, IDictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache, XElement restorableElement)
		{
			SortMainElement(interestingPropertiesCache, restorableElement);
			sortedData.Add(restorableElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant(), restorableElement);
		}

		internal static void CacheProperty(IDictionary<string, HashSet<string>> interestingPropertiesForClass, FdoPropertyInfo propertyInfo)
		{
			var collData = interestingPropertiesForClass[SharedConstants.Collections];
			var multiAltData = interestingPropertiesForClass[SharedConstants.MultiAlt];
			var owningData = interestingPropertiesForClass[SharedConstants.Owning];
			switch (propertyInfo.DataType)
			{
				case DataType.OwningSequence: // Fall through.
				case DataType.OwningAtomic:
					owningData.Add(propertyInfo.PropertyName);
					break;
				case DataType.OwningCollection:
					owningData.Add(propertyInfo.PropertyName);
					collData.Add(propertyInfo.PropertyName);
					break;
				case DataType.ReferenceCollection:
					collData.Add(propertyInfo.PropertyName);
					break;
				case DataType.MultiUnicode:
				case DataType.MultiString:
					multiAltData.Add(propertyInfo.PropertyName);
					break;
			}
		}

		internal static Dictionary<string, Dictionary<string, HashSet<string>>> CacheInterestingProperties(MetadataCache mdc)
		{
			var concreteClasses = mdc.AllConcreteClasses;
			var results = new Dictionary<string, Dictionary<string, HashSet<string>>>(concreteClasses.Count());

			foreach (var concreteClass in concreteClasses)
			{
				Dictionary<string, HashSet<string>> sortablePropertiesForClass;
				if (!results.TryGetValue(concreteClass.ClassName, out sortablePropertiesForClass))
				{
					// Appears to be a newly obsolete instance of 'className'.
					sortablePropertiesForClass = new Dictionary<string, HashSet<string>>(3, StringComparer.OrdinalIgnoreCase)
													{
														{SharedConstants.Collections, new HashSet<string>()},
														{SharedConstants.MultiAlt, new HashSet<string>()},
														{SharedConstants.Owning, new HashSet<string>()}
													};
					results.Add(concreteClass.ClassName, sortablePropertiesForClass);
				}

				foreach (var propertyInfo in concreteClass.AllProperties)
				{
					CacheProperty(sortablePropertiesForClass, propertyInfo);
				}
			}

			return results;
		}
	}
}