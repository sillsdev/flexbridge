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
			XElement element, string ownerguid)
		{
			if (sortedData == null) throw new ArgumentNullException("sortedData");
			if (element == null) throw new ArgumentNullException("element");

			// No, since unowned stuff will feed a null.
			//if (string.IsNullOrEmpty(ownerguid)) throw new ArgumentNullException(SharedConstants.OwnerGuid);
			if (ownerguid != null && ownerguid == string.Empty)
				throw new ArgumentException(Resources.kOwnerGuidEmpty, SharedConstants.OwnerGuid);

			var elementGuid = element.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
			// TODO: LT-12524 "Handle merge in case of conflicting move object to different destination".
			// This need will manifest itself in the guid already being in 'sortedData' and an exception being thrown.
			// At this point element has not been flattened, so stuff it owns will still be in it.
			// That is good, if we go with JohnT's idea of using a new guid for guids that are already in 'sortedData'.
			// By changing it before flattening, then the owned stuff will get the new one for their ownerguid attrs.
			// The owned stuff will also be dup, so the idea is to also change their guids (NB: HERE).
			// Just be sure to change 'elementGuid' to the new one. :-)
			sortedData.Add(elementGuid, element);

			// The name of 'element' is the class of CmObject.
			var isOwnSeqNode = element.Name.LocalName == SharedConstants.Ownseq;
			var className = isOwnSeqNode ? element.Attribute(SharedConstants.Class).Value : element.Name.LocalName;
			element.Name = SharedConstants.RtTag;
			if (!isOwnSeqNode)
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

			var classInfo = MetadataCache.MdCache.GetClassInfo(className);
			// Restore any ref seq props to have 'objsur' elements.
			var refSeqPropNames = (from referenceSequenceProperty in classInfo.AllReferenceSequenceProperties
								  select referenceSequenceProperty.PropertyName).ToList();

			var owningPropsForClass = (from owningPropInfo in classInfo.AllOwningProperties select owningPropInfo.PropertyName).ToList();
			if (owningPropsForClass.Count == 0)
				return;

			foreach (var propertyElement in element.Elements().ToArray())
			{
				var isCustomProperty = propertyElement.Name.LocalName == SharedConstants.Custom;
				var propName = isCustomProperty ? propertyElement.Attribute(SharedConstants.Name).Value : propertyElement.Name.LocalName;
				if (!owningPropsForClass.Contains(propName))
				{
					if (refSeqPropNames.Contains(propName))
					{
						foreach (var refSeqNode in propertyElement.Elements(SharedConstants.Refseq))
						{
							refSeqNode.Name = SharedConstants.Objsur;
						}
					}
					continue;
				}
				if (!propertyElement.HasElements)
					continue;
				foreach (var ownedElement in propertyElement.Elements().ToArray())
				{
					if (ownedElement.Name.LocalName == SharedConstants.Objsur)
						break;
					ownedElement.Remove();
					var replacementOjSurElement = new XElement(SharedConstants.Objsur,
															   new XAttribute(SharedConstants.GuidStr, ownedElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant()),
															   new XAttribute("t", "o"));
					propertyElement.Add(replacementOjSurElement);
					// Move down the nested set of owned objects, and do the same.
					FlattenObject(sortedData, ownedElement, elementGuid);
				}
			}
		}

		internal static void RestoreObjsurElement(XContainer owningElement, string owningPropertyName, XElement ownedElement)
		{
			var owningPropElement = owningElement.Element(owningPropertyName);
			owningPropElement.Add(new XElement(SharedConstants.Objsur,
												   new XAttribute(SharedConstants.GuidStr, ownedElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant()),
												   new XAttribute("t", "o")));
		}
	}
}