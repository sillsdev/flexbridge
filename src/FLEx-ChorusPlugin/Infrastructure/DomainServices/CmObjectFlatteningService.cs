using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Properties;

namespace FLEx_ChorusPlugin.Infrastructure.DomainServices
{
	/// <summary>
	/// This class takes a CmObject (as an XElement) and flattens out all owned objects.
	/// </summary>
	internal static class CmObjectFlatteningService
	{
		internal static void FlattenObject(SortedDictionary<string, XElement> sortedData,
			Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache,
			XElement element, string ownerguid)
		{
			if (sortedData == null) throw new ArgumentNullException("sortedData");
			if (interestingPropertiesCache == null) throw new ArgumentNullException("interestingPropertiesCache");
			if (element == null) throw new ArgumentNullException("element");

			// No, since unowned stuff will feed a null.
			//if (string.IsNullOrEmpty(ownerguid)) throw new ArgumentNullException(SharedConstants.OwnerGuid);
			if (ownerguid != null && ownerguid == string.Empty)
				throw new ArgumentException(Resources.kOwnerGuidEmpty, SharedConstants.OwnerGuid);

			var elementGuid = element.Attribute(SharedConstants.GuidStr).Value;
			// TODO: LT-12524 "Handle merge in case of conflicting move object to different destination".
			// This need will manifest itself in the guid already being in 'sortedData' and an exception being thrown.
			// At this point element has not been flattened, so stuff it owns will still be in it.
			// That is good, if we go with JohnT's idea of using a new guid for guids that are already in 'sortedData'.
			// By changing it before flattening, then the owned stuff will get the new one for their ownerguid attrs.
			// The owned stuff will also be dup, so the idea is to also change their guids (NB: HERE).
			// Just be sure to change 'elementGuid' to the new one. :-)
			sortedData.Add(elementGuid, element);

			// The name of 'element' is the class of CmObject.
			var className = element.Name.LocalName;
			element.Name = SharedConstants.RtTag;
			element.Add(new XAttribute(SharedConstants.Class, className));
			//if (element.Attribute(SharedConstants.OwnerGuid) == null)
			if (ownerguid != null) // && element.Attribute(SharedConstants.OwnerGuid) == null)
				element.Add(new XAttribute(SharedConstants.OwnerGuid, ownerguid));

			// Re-sort those attributes.
			var sortedAttrs = new SortedDictionary<string, XAttribute>();
			foreach (var attribute in element.Attributes())
				sortedAttrs.Add(attribute.Name.LocalName, attribute);
			element.Attributes().Remove();
			element.Add(sortedAttrs.Values);

			var owningPropsForClass = interestingPropertiesCache[className][SharedConstants.Owning];
			if (owningPropsForClass.Count == 0)
				return;

			foreach (var propertyElement in element.Elements().ToArray())
			{
				var isCustomProperty = propertyElement.Name.LocalName == "Custom";
				var propName = isCustomProperty ? propertyElement.Attribute(SharedConstants.Name).Value : propertyElement.Name.LocalName;
				if (!owningPropsForClass.Contains(propName))
					continue;
				if (!propertyElement.HasElements)
					continue;
				foreach (var ownedElement in propertyElement.Elements().ToArray())
				{
					if (ownedElement.Name.LocalName == SharedConstants.Objsur)
						break;
					ownedElement.Remove();
					var replacementOjSurElement = new XElement(SharedConstants.Objsur,
															   new XAttribute(SharedConstants.GuidStr, ownedElement.Attribute(SharedConstants.GuidStr).Value),
															   new XAttribute("t", "o"));
					propertyElement.Add(replacementOjSurElement);
					// Move down the nested set of owned objects, and do the same.
					FlattenObject(sortedData, interestingPropertiesCache, ownedElement, elementGuid);
				}
			}
		}

		internal static void RestoreObjsurElement(XContainer owningElement, string owningPropertyName, XElement ownedElement)
		{
			var owningPropElement = owningElement.Element(owningPropertyName);
			owningPropElement.Add(new XElement(SharedConstants.Objsur,
												   new XAttribute(SharedConstants.GuidStr, ownedElement.Attribute(SharedConstants.GuidStr).Value),
												   new XAttribute("t", "o")));
		}
	}
}