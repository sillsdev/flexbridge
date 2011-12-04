using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using FieldWorksBridge.Properties;

namespace FieldWorksBridge.Infrastructure
{
	/// <summary>
	/// This class takes a CmOject (as an XElement) and flattens out all owned objects.
	/// </summary>
	internal static class CmObjectFlatteningService
	{
		internal static IEnumerable<XElement> FlattenObject(Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache, XElement element, string ownerguid)
		{
			if (interestingPropertiesCache == null) throw new ArgumentNullException("interestingPropertiesCache");
			if (element == null) throw new ArgumentNullException("element");
			if (ownerguid != null)
			{
				if (ownerguid == string.Empty) throw new ArgumentException(Resources.kOwnerGuidEmpty, "ownerguid");
			}

			var result = new List<XElement>(50000)
							{
								element
							};

			// The name of 'element' is the class of CmObject.
			var className = element.Name.LocalName;
			element.Name = "rt";
			element.Add(new XAttribute("class", className));
			if (ownerguid != null)
				element.Add(new XAttribute("ownerguid", ownerguid));

			var owningPropsForClass = interestingPropertiesCache[className][DataSortingService.Owning];
			if (owningPropsForClass.Count == 0)
				return result;

			foreach (var propertyElement in element.Elements().ToArray())
			{
				var isCustomProperty = propertyElement.Name.LocalName == "Custom";
				var propName = isCustomProperty ? propertyElement.Attribute("name").Value : propertyElement.Name.LocalName;
				if (!owningPropsForClass.Contains(propName))
					continue;
				if (!propertyElement.HasElements)
					continue;
				foreach (var ownedElement in propertyElement.Elements().ToArray())
				{
					ownedElement.Remove();
					var replacementOjSurElement = new XElement("objsur",
															   new XAttribute("guid", ownedElement.Attribute("guid").Value),
															   new XAttribute("t", "o"));
					propertyElement.Add(replacementOjSurElement);
					// Move down the nested set of owned objects, and do the same.
					result.AddRange(FlattenObject(interestingPropertiesCache, ownedElement, element.Attribute("guid").Value));
				}
			}

			return result;
		}
	}
}