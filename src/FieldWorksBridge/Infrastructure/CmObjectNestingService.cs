using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FieldWorksBridge.Infrastructure
{
	/// <summary>
	/// This class takes a CmOject (as an XElement) and nests all owned objects,
	/// except any exceptions that are provided.
	/// </summary>
	internal static class CmObjectNestingService
	{
		internal static void NestObject(XElement obj,
			Dictionary<string, HashSet<string>> exceptions,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache,
			Dictionary<string, string> guidToClassMapping)
		{
			if (obj == null) throw new ArgumentNullException("obj");
			if (exceptions == null) throw new ArgumentNullException("exceptions");
			if (classData == null) throw new ArgumentNullException("classData");
			if (interestingPropertiesCache == null) throw new ArgumentNullException("interestingPropertiesCache");
			if (guidToClassMapping == null) throw new ArgumentNullException("guidToClassMapping");

			// 1. Rename element to that of the class.
			var className = RenameElement(obj);

			// 2. Nest owned objects in 'obj'.
			NestOwnedObjects(exceptions, classData, interestingPropertiesCache, guidToClassMapping, obj);

			// 3. Remove 'obj' from lists.
			var guid = obj.Attribute("guid").Value.ToLowerInvariant();
			classData[className].Remove(guid);
			guidToClassMapping.Remove(guid);
		}

		private static void NestOwnedObjects(
			Dictionary<string, HashSet<string>> exceptions,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache,
			Dictionary<string, string> guidToClassMapping,
			XElement owningObjElement)
		{
			var owningProps = interestingPropertiesCache[owningObjElement.Name.LocalName][DataSortingService.Owning];
			foreach (var propertyElement in owningObjElement.Elements())
			{
				var isCustomProperty = propertyElement.Name.LocalName == "Custom";
				var propName = isCustomProperty ? propertyElement.Attribute("name").Value : propertyElement.Name.LocalName;
				if (!owningProps.Contains(propName))
					continue;
				if (!propertyElement.HasElements)
					continue;
				// By this point, theory has it that all 'objsur' elemtents must be owning,
				// but the filter will ensure some unexpected reference data doesn't get treated as owning.
				var owningObjSurElements = propertyElement.Elements("objsur").Where(objsurEl => objsurEl.Attribute("t").Value == "o");
				if (owningObjSurElements.Count() == 0)
					continue;
				// NB: There is no way the user can declare an owning custom property to be an exception, so not to worry about them.
				if (!isCustomProperty)
				{
					// Skip owning properties that are in the 'exceptions' list.
					HashSet<string> exceptionProperties;
					if (exceptions.TryGetValue(owningObjElement.Name.LocalName, out exceptionProperties) && exceptionProperties.Contains(propertyElement.Name.LocalName))
						continue;
				}

				// Replace each objsur node with actual element.
				foreach (var objsurElement in owningObjSurElements.ToArray())
				{
					var guid = objsurElement.Attribute("guid").Value;
					var classOfOwnedObject = guidToClassMapping[guid];
					guidToClassMapping.Remove(guid);
					var ownedElement = classData[classOfOwnedObject][guid];
					ownedElement.Attribute("ownerguid").Remove();
					objsurElement.ReplaceWith(ownedElement);
					// Recurse on down to the bottom.
					NestObject(ownedElement, exceptions, classData, interestingPropertiesCache, guidToClassMapping);
				}
			}
		}

		private static string RenameElement(XElement obj)
		{
			var classAttr = obj.Attribute("class");
			obj.Name = classAttr.Value;
			classAttr.Remove();

			return classAttr.Value;
		}
	}
}