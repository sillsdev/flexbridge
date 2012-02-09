using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FLEx_ChorusPlugin.Infrastructure.DomainServices
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
			var classInfo = mdc.GetClassInfo(ownerElement.Attribute(SharedConstants.Class).Value);
			foreach (var owningProperty in from pi in classInfo.AllProperties
										   where pi.DataType == DataType.OwningAtomic || pi.DataType == DataType.OwningCollection || pi.DataType == DataType.OwningSequence
										   select pi)
			{
				if (excludedProperties.Contains(owningProperty.PropertyName))
				{
					excludedProperties.Remove(owningProperty.PropertyName);
					continue;
				}

				CollectOwnedObjectsForProperty(mdc, classData, guidToClassMapping, multiClassOutput, ownerElement, classInfo.GetProperty(owningProperty.PropertyName).IsCustomProperty, owningProperty.PropertyName);
			}
		}

		private static void CollectOwnedObjectsForProperty(
			MetadataCache mdc,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			IDictionary<string, string> guidToClassMapping,
			IDictionary<string, SortedDictionary<string, XElement>> multiClassOutput,
			XContainer ownerElement,
			bool isCustomProperty,
			string propertyName)
		{
			var propElement = isCustomProperty
				? (from customProp in ownerElement.Elements(SharedConstants.Custom) where customProp.Attribute(SharedConstants.Name).Value == propertyName select customProp).FirstOrDefault()
				: ownerElement.Element(propertyName);
			if (propElement == null)
				return;

			foreach (var guid in propElement.Elements(SharedConstants.Objsur).Select(osElement => osElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant()))
			{
				var currentElement = RegisterDataInBoundedContext(classData, guidToClassMapping, multiClassOutput, guid);
				// Recurse ownership.
				CollectAllOwnedObjects(mdc, classData, guidToClassMapping, multiClassOutput, currentElement, new HashSet<string>());
			}
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

		internal static List<string> GetGuids(XContainer owningElement, string propertyName)
		{
			var propElement = owningElement.Element(propertyName);

			return (propElement == null) ? new List<string>() : (from osEl in propElement.Elements(SharedConstants.Objsur)
																 select osEl.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant()).ToList();
		}
	}
}