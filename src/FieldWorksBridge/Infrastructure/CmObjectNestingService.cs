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
			Dictionary<string, string> guidToClassMapping)
		{
			if (obj == null) throw new ArgumentNullException("obj");
			if (exceptions == null) throw new ArgumentNullException("exceptions");
			if (classData == null) throw new ArgumentNullException("classData");
			if (guidToClassMapping == null) throw new ArgumentNullException("guidToClassMapping");

			// 1. Rename element to that of the class.
			var className = RenameElement(obj);

			// 2. Nest owned objects in 'obj'.
			NestOwnedObjects(obj, exceptions, classData, guidToClassMapping);

			// 3. Remove 'obj' from 'classData'.
			classData[className].Remove(obj.Attribute("guid").Value);
		}

		private static void NestOwnedObjects(XElement obj, Dictionary<string, HashSet<string>> exceptions, IDictionary<string, SortedDictionary<string, XElement>> classData, Dictionary<string, string> guidToClassMapping)
		{
			foreach (var owningPropertyElement in from propertyElement in obj.Descendants()
												  from ownedItemElement in
													(from objsurEl in propertyElement.Elements("objsur")
													 where objsurEl.Attribute("t").Value == "o"
													 select objsurEl).ToArray()
												  select propertyElement)
			{
				// Skip owning properties that are in the 'exceptions' list.
				HashSet<string> exceptionProperties;
				if (exceptions.TryGetValue(obj.Name.LocalName, out exceptionProperties) && exceptionProperties.Contains(owningPropertyElement.Name.LocalName))
					continue;

				// Replace each objsur node with actual element.
				foreach (var objsurElement in owningPropertyElement.Elements("objsur").ToArray())
				{
					var guid = objsurElement.Attribute("guid").Value;
					var classOfOwnedObject = guidToClassMapping[guid];
					guidToClassMapping.Remove(guid);
					var ownedElement = classData[classOfOwnedObject][guid];
					ownedElement.Attribute("ownerguid").Remove();
					objsurElement.ReplaceWith(ownedElement);
					// Recurse on down to the bottom.
					NestObject(ownedElement, exceptions, classData, guidToClassMapping);
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