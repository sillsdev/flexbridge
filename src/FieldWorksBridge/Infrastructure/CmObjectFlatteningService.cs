using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FieldWorksBridge.Infrastructure
{
	/// <summary>
	/// This class takes a CmOject (as an XElement) and flattens out all owned objects.
	/// </summary>
	internal static class CmObjectFlatteningService
	{
		internal static IEnumerable<XElement> FlattenObject(Dictionary<string, Dictionary<string, HashSet<string>>> sortableProperties, XElement element, string ownerguid)
		{
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

			var owningPropsForClass = sortableProperties[className][DataSortingService.Owning];
			if (owningPropsForClass.Count == 0)
				return result;

			foreach (var propertyElement in element.Elements())
			{
				if (!owningPropsForClass.Contains(propertyElement.Name.LocalName))
					continue;
				var isCustomProperty = propertyElement.Name.LocalName == "Custom";
				if (isCustomProperty && !owningPropsForClass.Contains(propertyElement.Attribute("name").Value))
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
					result.AddRange(FlattenObject(sortableProperties, ownedElement, element.Attribute("guid").Value));
				}
			}

			return result;
		}
	}
}