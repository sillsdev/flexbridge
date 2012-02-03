using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FLEx_ChorusPlugin.Infrastructure.DomainServices
{
	/// <summary>
	/// This class takes a CmOject (as an XElement) and nests all owned objects,
	/// except any exceptions that are provided.
	/// </summary>
	internal static class CmObjectNestingService
	{
		internal static void NestObject(
			bool isOwningSeqProp,
			XElement obj,
			Dictionary<string, HashSet<string>> exceptions,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			if (obj == null) throw new ArgumentNullException("obj");
			if (exceptions == null) throw new ArgumentNullException("exceptions");
			if (classData == null) throw new ArgumentNullException("classData");
			if (guidToClassMapping == null) throw new ArgumentNullException("guidToClassMapping");

			// 1. Rename element to that of the class, if isOwningSeqProp == false.
			// Otherwise, rename it to "ownseq" and leave class attribute. This allows for a special ElementStrategy for "ownseq" that has isOrderRelevant top be true.
			var className = RenameElement(isOwningSeqProp, obj);
			// Remove 'ownerguid', if present.
			var ownerGuidAttribute = obj.Attribute(SharedConstants.OwnerGuid);
			if (ownerGuidAttribute != null)
				ownerGuidAttribute.Remove();

			// 2. Nest owned objects in 'obj'.
			NestOwnedObjects(exceptions, classData, guidToClassMapping, obj);

			// 3. Reset ref seq prop nodes from "objsur" to "refseq", so they can use a special ElementStrategy for "refseq" that has isOrderRelevant top be true.
			RenameReferenceSequenceObjsurNodes(className, obj);

			// 4. Remove 'obj' from lists.
			var guid = obj.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
			classData[className].Remove(guid);
			guidToClassMapping.Remove(guid);
		}

		private static void RenameReferenceSequenceObjsurNodes(string className, XElement obj)
		{
			var refSeqProps = MetadataCache.MdCache.GetClassInfo(className).AllReferenceSequenceProperties.ToList();
			if (refSeqProps.Count == 0)
				return;
			foreach (var refSeqProp in refSeqProps)
			{
				var propNode = obj.Element(refSeqProp.PropertyName);
				if (propNode == null)
					continue;
				var objsurNodes = propNode.Elements(SharedConstants.Objsur).ToList();
				if (!objsurNodes.Any())
					continue;
				foreach (var objsurNode in objsurNodes)
				{
					objsurNode.Name = SharedConstants.Refseq;
				}
			}
		}

		private static void NestOwnedObjects(
			Dictionary<string, HashSet<string>> exceptions,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping,
			XElement owningObjElement)
		{
			var className = (owningObjElement.Name.LocalName == SharedConstants.Ownseq || owningObjElement.Name.LocalName == SharedConstants.OwnseqAtomic || owningObjElement.Name.LocalName == SharedConstants.Refseq)
								? owningObjElement.Attribute(SharedConstants.Class).Value
								: owningObjElement.Name.LocalName;
			var classInfo = MetadataCache.MdCache.GetClassInfo(className);
			var owningProps = (from owningPropInfo in classInfo.AllOwningProperties select owningPropInfo.PropertyName).ToList();
			foreach (var propertyElement in owningObjElement.Elements())
			{
				var isCustomProperty = propertyElement.Name.LocalName == SharedConstants.Custom;
				var propName = isCustomProperty ? propertyElement.Attribute(SharedConstants.Name).Value : propertyElement.Name.LocalName;
				if (!owningProps.Contains(propName))
					continue;
				if (!propertyElement.HasElements)
					continue;
				// By this point, theory has it that all 'objsur' elements must be owning,
				// but the filter will ensure some unexpected reference data doesn't get treated as owning.
				var owningObjSurElements = propertyElement.Elements(SharedConstants.Objsur).Where(objsurEl => objsurEl.Attribute("t").Value == "o").ToList();
				if (!owningObjSurElements.Any())
					continue;
				// NB: There is no way the user can declare an owning custom property to be an exception, so not to worry about them.
				if (!isCustomProperty)
				{
					// Skip owning properties that are in the 'exceptions' list.
					HashSet<string> exceptionProperties;
					if (exceptions.TryGetValue(owningObjElement.Name.LocalName, out exceptionProperties) && exceptionProperties.Contains(propertyElement.Name.LocalName))
						continue;
				}

				var isOwningSeqProp = classInfo.GetProperty(propName).DataType == DataType.OwningSequence;
				// Replace each objsur node with actual element.
				foreach (var objsurElement in owningObjSurElements.ToArray())
				{
					var guid = objsurElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
					string classOfOwnedObject;
					if (!guidToClassMapping.TryGetValue(guid, out classOfOwnedObject))
						continue;
					guidToClassMapping.Remove(guid);
					var ownedElement = classData[classOfOwnedObject][guid];
					objsurElement.ReplaceWith(ownedElement);
					// Recurse on down to the bottom.
					NestObject(isOwningSeqProp, ownedElement, exceptions, classData, guidToClassMapping);
				}
			}
		}

		private static string RenameElement(bool isOwningSeqProp, XElement obj)
		{
			var classAttr = obj.Attribute(SharedConstants.Class);
			if (isOwningSeqProp)
			{
				var className = obj.Attribute(SharedConstants.Class).Value;
				obj.Name = (className == "StTxtPara" || className == "ScrTxtPara") ? SharedConstants.OwnseqAtomic : SharedConstants.Ownseq;
			}
			else
			{
				obj.Name = classAttr.Value;
				classAttr.Remove();
			}

			return classAttr.Value;
		}
	}
}