using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Chorus.FileTypeHanders.FieldWorks;

namespace FieldWorksBridge.Infrastructure
{
	// Caller should ensure 'ownerElement' is removed from 'classData' and added to 'multiClassOutput', if relevant.
	internal static class ObjectFinderServices
	{
		internal static void CollectAllOwnedObjects(
			MetadataCache mdc,
			IDictionary<string, SortedDictionary<string, byte[]>> classData,
			IDictionary<string, string> guidToClassMapping,
			IDictionary<string, SortedDictionary<string, byte[]>> multiClassOutput,
			XElement ownerElement)
		{
// ReSharper disable PossibleNullReferenceException
			var classInfo = mdc.GetClassInfo(ownerElement.Attribute("class").Value);
			foreach (var owningProperty in from pi in classInfo.AllProperties
										   where pi.DataType == DataType.OwningAtomic || pi.DataType == DataType.OwningCollection || pi.DataType == DataType.OwningSequence
										   select pi)
			{
				CollectOwnedObjectsForProperty(mdc, classData, guidToClassMapping, multiClassOutput, ownerElement, owningProperty.PropertyName);
			}
// ReSharper restore PossibleNullReferenceException
		}

		private static void CollectOwnedObjectsForProperty(
			MetadataCache mdc,
			IDictionary<string, SortedDictionary<string, byte[]>> classData,
			IDictionary<string, string> guidToClassMapping,
			IDictionary<string, SortedDictionary<string, byte[]>> multiClassOutput,
			XContainer ownerElement,
			string propertyName)
		{
			var propElement = ownerElement.Element(propertyName);
			if (propElement == null)
				return;

// ReSharper disable PossibleNullReferenceException
			foreach (var guid in propElement.Elements("objsur").Select(osElement => osElement.Attribute("guid").Value.ToLowerInvariant()))
			{
				var currentBytes = RegisterDataInBoundedContext(classData, guidToClassMapping, multiClassOutput, guid);
				// Recurse ownership.
				CollectAllOwnedObjects(mdc, classData, guidToClassMapping, multiClassOutput, XElement.Parse(MultipleFileServices.Utf8.GetString(currentBytes)));
			}
// ReSharper restore PossibleNullReferenceException
		}

		internal static byte[] RegisterDataInBoundedContext(IDictionary<string, SortedDictionary<string, byte[]>> classData, IDictionary<string, string> guidToClassMapping, IDictionary<string, SortedDictionary<string, byte[]>> multiClassOutput, string guid)
		{
			var classname = guidToClassMapping[guid];
			guidToClassMapping.Remove(guid);

			var currentInput = classData[classname];
			var currentBytes = currentInput[guid];
			currentInput.Remove(guid);

			SortedDictionary<string, byte[]> output;
			if (!multiClassOutput.TryGetValue(classname, out output))
			{
				output = new SortedDictionary<string, byte[]>();
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
	}
}