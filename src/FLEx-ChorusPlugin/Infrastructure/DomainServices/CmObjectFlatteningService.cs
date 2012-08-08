using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Chorus.merge;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Contexts;
using FLEx_ChorusPlugin.Properties;

namespace FLEx_ChorusPlugin.Infrastructure.DomainServices
{
	/// <summary>
	/// This class takes a CmObject (as an XElement) and flattens out all owned objects.
	/// </summary>
	internal static class CmObjectFlatteningService
	{
		internal static void FlattenObject(
			string pathname,
			SortedDictionary<string, XElement> sortedData,
			XElement element, string ownerguid)
		{
			if (string.IsNullOrEmpty(pathname)) throw new ArgumentNullException("pathname");
			if (sortedData == null) throw new ArgumentNullException("sortedData");
			if (element == null) throw new ArgumentNullException("element");

			// No, since unowned stuff will feed a null.
			//if (string.IsNullOrEmpty(ownerguid)) throw new ArgumentNullException(SharedConstants.OwnerGuid);
			if (ownerguid != null && ownerguid == string.Empty)
				throw new ArgumentException(Resources.kOwnerGuidEmpty, SharedConstants.OwnerGuid);

			var mdc = MetadataCache.MdCache;
			FdoClassInfo classInfo;
			string className;
			var isOwnSeqNode = GetClassInfoFromElement(mdc, element, out classInfo, out className);
			var elementGuid = element.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
			if (sortedData.ContainsKey(elementGuid))
			{
				// Does LT-12524 "Handle merge in case of conflicting move object to different destination".
				// This need will manifest itself in the guid already being in 'sortedData' and an exception being thrown.
				// At this point element has not been flattened, so stuff it owns will still be in it.
				// That is good, if we go with JohnT's idea of using a new guid for guids that are already in 'sortedData'.
				// By changing it before flattening, then the owned stuff will get the new one for their ownerguid attrs.
				// The owned stuff will also be dup, so the idea is to also change their guids right now. [ChangeGuids is recursive down the owning tree.]
				// Just be sure to change 'elementGuid' to the new one. :-)
				// The first item added to sortedData has been flattened by this point, but not any following ones.
				elementGuid = ChangeGuids(mdc, classInfo, element);
				using (var listener = new ChorusNotesMergeEventListener(ChorusNotesMergeEventListener.GetChorusNotesFilePath(pathname)))
				{
					// Adding the conflict to the listener, will result in the ChorusNotes file being updated (created if need be.)
					var conflict = new IncompatibleMoveConflict(className, GetXmlNode(element)) {Situation = new NullMergeSituation()};
					listener.RecordContextInConflict(conflict);
					listener.ConflictOccurred(conflict);
				}
			}
			sortedData.Add(elementGuid, element);

			// The name of 'element' is the class of CmObject, or 'ownseq', or....
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

			// Restore any ref seq props to have 'objsur' elements.
			var propCache = mdc.PropertyCache[className];
			var refSeqPropNames = propCache["AllReferenceSequence"];
			// Restore any ref col props to have 'objsur' elements.
			var refColPropNames = propCache["AllReferenceCollection"];
			var owningPropsForClass = propCache["AllOwning"];
			if (owningPropsForClass.Count == 0 && refSeqPropNames.Count == 0 && refColPropNames.Count == 0)
				return; // Nothing special to be done for normal properties.

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
					else if (refColPropNames.Contains(propName))
					{
						foreach (var refColNode in propertyElement.Elements(SharedConstants.Refcol))
						{
							refColNode.Name = SharedConstants.Objsur;
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
					// Do before the removal call, so we know the parent, and thus, the property name.
					if (isCustomProperty)
					{
						BaseDomainServices.RestoreObjsurElement((element.Elements().Where(
							customNode =>
							customNode.Name.LocalName == SharedConstants.Custom
							&& customNode.Attribute(SharedConstants.Name) != null
							&& customNode.Attribute(SharedConstants.Name).Value == propertyElement.Attribute(SharedConstants.Name).Value)).First(), ownedElement);
					}
					else
					{
						BaseDomainServices.RestoreObjsurElement(element, ownedElement.Parent.Name.LocalName, ownedElement);
					}
					ownedElement.Remove();
					// Move down the nested set of owned objects, and do the same.
					FlattenObject(pathname, sortedData, ownedElement, elementGuid);
				}
			}
		}

		private static bool GetClassInfoFromElement(MetadataCache mdc, XElement element, out FdoClassInfo classInfo,
													out string className)
		{
			var isOwnSeqNode = element.Name.LocalName == SharedConstants.Ownseq;
			className = isOwnSeqNode ? element.Attribute(SharedConstants.Class).Value : element.Name.LocalName;
			classInfo = mdc.GetClassInfo(className);
			return isOwnSeqNode;
		}

		internal static string ChangeGuids(MetadataCache mdc, FdoClassInfo classInfo, XElement element)
		{
			var newGuid = Guid.NewGuid().ToString().ToLowerInvariant();

			element.Attribute(SharedConstants.GuidStr).Value = newGuid;

			// TODO: Recurse down through everything that is owned and change those guids.
			foreach (var owningPropInfo in classInfo.AllOwningProperties)
			{
				var isCustomProp = owningPropInfo.IsCustomProperty;
				var owningPropElement = isCustomProp
					? (element.Elements(SharedConstants.Custom).Where(customProp => customProp.Attribute(SharedConstants.Name).Value == owningPropInfo.PropertyName)).FirstOrDefault()
					: element.Element(owningPropInfo.PropertyName);
				if (owningPropElement == null || !owningPropElement.HasElements)
					continue;
				foreach (var ownedElement in owningPropElement.Elements())
				{
					FdoClassInfo ownedClassInfo;
					string className;
					GetClassInfoFromElement(mdc, element, out ownedClassInfo, out className);
					ChangeGuids(mdc, ownedClassInfo, ownedElement);
				}
			}

			return newGuid;
		}

		internal static XmlNode GetXmlNode(XElement element)
		{
			using (XmlReader xmlReader = element.CreateReader())
			{
				var xmlDoc = new XmlDocument();
				xmlDoc.Load(xmlReader);
				return xmlDoc;
			}
		}

		internal static XElement AddNewPropertyElement(XElement parent, string propertyName)
		{
			var sortedRecords = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase)
				{
					{propertyName, new XElement(propertyName)}
				};
			foreach (var propertyNode in parent.Elements())
			{
				var propName = propertyNode.Name.LocalName;
				if (propName == SharedConstants.Custom)
					propName = propertyNode.Attribute(SharedConstants.Name).Value;
				sortedRecords.Add(propName, propertyNode);
			}
			parent.RemoveNodes();
			foreach (var sortedPropElement in sortedRecords.Values)
				parent.Add(sortedPropElement);

			return sortedRecords[propertyName];
		}
	}
}