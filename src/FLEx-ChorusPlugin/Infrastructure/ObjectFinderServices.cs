using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace FLEx_ChorusPlugin.Infrastructure
{
	// Caller should ensure 'ownerElement' is removed from 'classData' and added to 'multiClassOutput', if relevant.
	internal static class ObjectFinderServices
	{
		internal static void CollectAllOwnedObjects(
			MetadataCache mdc,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			IDictionary<string, string> guidToClassMapping,
			IDictionary<string, SortedDictionary<string, XElement>> multiClassOutput,
			XElement ownerElement,
			HashSet<string> excludedProperties)
		{
// ReSharper disable PossibleNullReferenceException
			var classInfo = mdc.GetClassInfo(ownerElement.Attribute("class").Value);
			foreach (var owningProperty in from pi in classInfo.AllProperties
										   where pi.DataType == DataType.OwningAtomic || pi.DataType == DataType.OwningCollection || pi.DataType == DataType.OwningSequence
										   select pi)
			{
				if (excludedProperties.Contains(owningProperty.PropertyName))
				{
					excludedProperties.Remove(owningProperty.PropertyName);
					continue;
				}

				CollectOwnedObjectsForProperty(mdc, classData, guidToClassMapping, multiClassOutput, ownerElement, owningProperty.PropertyName);
			}
// ReSharper restore PossibleNullReferenceException
		}

		private static void CollectOwnedObjectsForProperty(
			MetadataCache mdc,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			IDictionary<string, string> guidToClassMapping,
			IDictionary<string, SortedDictionary<string, XElement>> multiClassOutput,
			XContainer ownerElement,
			string propertyName)
		{
			var propElement = ownerElement.Element(propertyName);
			if (propElement == null)
				return;

// ReSharper disable PossibleNullReferenceException
			foreach (var guid in propElement.Elements("objsur").Select(osElement => osElement.Attribute("guid").Value.ToLowerInvariant()))
			{
				var currentElement = RegisterDataInBoundedContext(classData, guidToClassMapping, multiClassOutput, guid);
				// Recurse ownership.
				CollectAllOwnedObjects(mdc, classData, guidToClassMapping, multiClassOutput, currentElement, new HashSet<string>());
			}
// ReSharper restore PossibleNullReferenceException
		}

		internal static XElement RegisterDataInBoundedContext(IDictionary<string, SortedDictionary<string, XElement>> classData, IDictionary<string, string> guidToClassMapping, IDictionary<string, SortedDictionary<string, XElement>> multiClassOutput, string guid)
		{
			var classname = guidToClassMapping[guid];
			guidToClassMapping.Remove(guid);

			var currentInput = classData[classname];
			var currentBytes = currentInput[guid];
			currentInput.Remove(guid);

			SortedDictionary<string, XElement> output;
			if (!multiClassOutput.TryGetValue(classname, out output))
			{
				output = new SortedDictionary<string, XElement>();
				multiClassOutput.Add(classname, output);
			}
			output.Add(guid, currentBytes);
			return currentBytes;
		}

		internal static List<string> GetGuids(XContainer textElement, string propertyName)
		{
			var propElement = textElement.Element(propertyName);

// ReSharper disable PossibleNullReferenceException
			return (propElement == null) ? new List<string>() : (from osEl in propElement.Elements("objsur")
																 select osEl.Attribute("guid").Value.ToLowerInvariant()).ToList();
// ReSharper restore PossibleNullReferenceException
		}

		internal static void WritePropertyInFolders(MetadataCache mdc, IDictionary<string, SortedDictionary<string, XElement>> classData, IDictionary<string, string> guidToClassMapping, Dictionary<string, SortedDictionary<string, XElement>> multiClassOutput, XmlReaderSettings readerSettings, string baseDir, XElement dataElement, string propertyName, string dirPrefix, bool appendGuid)
		{
			foreach (var guid in GetGuids(dataElement, propertyName))
			{
				multiClassOutput.Clear();

				var currentElement = RegisterDataInBoundedContext(classData, guidToClassMapping, multiClassOutput, guid);
				CollectAllOwnedObjects(mdc,
					classData, guidToClassMapping, multiClassOutput,
					currentElement,
					new HashSet<string>());

				// Write out data in a separate folder.
				var dirPath = Path.Combine(baseDir, dirPrefix);
				if (appendGuid)
					dirPath = Path.Combine(baseDir, dirPrefix + guid);
				if (!Directory.Exists(dirPath))
					Directory.CreateDirectory(dirPath);
				foreach (var kvp in multiClassOutput)
					FileWriterService.WriteSecondaryFile(Path.Combine(dirPath, kvp.Key + ".ClassData"), readerSettings, kvp.Value);
			}
			multiClassOutput.Clear();
		}

		internal static void ProcessLists(IDictionary<string, SortedDictionary<string, XElement>> classData, HashSet<string> skipWriteEmptyClassFiles, HashSet<string> classnames)
		{
			foreach (var classname in classnames)
			{
				skipWriteEmptyClassFiles.Add(classname);
				classData.Remove(classname);
			}
		}
	}
}