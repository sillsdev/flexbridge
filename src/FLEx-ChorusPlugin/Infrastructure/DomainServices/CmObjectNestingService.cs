using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TriboroughBridge_ChorusPlugin;

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
			IDictionary<string, SortedDictionary<string, byte[]>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			if (obj == null) throw new ArgumentNullException("obj");
			if (classData == null) throw new ArgumentNullException("classData");
			if (guidToClassMapping == null) throw new ArgumentNullException("guidToClassMapping");

			// 1. Rename element to that of the class, if isOwningSeqProp == false.
			// Otherwise, rename it to "ownseq" and leave class attribute. This allows for a special ElementStrategy for "ownseq" that has isOrderRelevant to be true.
			var className = RenameElement(isOwningSeqProp, obj);
			// Remove 'ownerguid', if present.
			var ownerGuidAttribute = obj.Attribute(SharedConstants.OwnerGuid);
			if (ownerGuidAttribute != null)
				ownerGuidAttribute.Remove();

			var propCacheForClass = MetadataCache.MdCache.PropertyCache[className];
			// 2. Nest owned objects in 'obj', but only if it has any owning props.
			if (propCacheForClass["AllOwning"].Count > 0)
				NestOwnedObjects(classData, guidToClassMapping, obj);

			// 3. Reset ref seq prop nodes from "objsur" to SharedConstants.Refseq,
			// so they can use a special ElementStrategy for SharedConstants.Refseq that has isOrderRelevant to be true.
			if (propCacheForClass["AllReferenceSequence"].Count > 0)
				RenameReferenceSequenceObjsurNodes(className, obj);

			// 4. Rename ref col prop nodes from "objsur" to SharedConstants.Refcol,
			// so they can use a special ElementStrategy for SharedConstants.Refcol that has isOrderRelevant to be false.
			if (propCacheForClass["AllReferenceCollection"].Count > 0)
				RenameReferenceCollectionObjsurNodes(className, obj);

			// 5. Remove 'obj' from lists.
			var guid = obj.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
			classData[className].Remove(guid);
			guidToClassMapping.Remove(guid);
		}

		private static void RenameReferenceCollectionObjsurNodes(string className, XElement obj)
		{
			var refColProps = MetadataCache.MdCache.GetClassInfo(className).AllReferenceCollectionProperties.ToList();
			if (refColProps.Count == 0)
				return;
			foreach (var refColProp in refColProps)
			{
				var propNode = refColProp.IsCustomProperty
					? obj.Elements("Custom").FirstOrDefault(pi => pi.Attribute(SharedConstants.Name).Value == refColProp.PropertyName)
					: obj.Element(refColProp.PropertyName);
				if (propNode == null)
					continue;
				var objsurNodes = propNode.Elements(SharedConstants.Objsur).ToList();
				if (!objsurNodes.Any())
					continue;
				foreach (var objsurNode in objsurNodes)
				{
					objsurNode.Name = SharedConstants.Refcol;
				}
			}
		}

		private static void RenameReferenceSequenceObjsurNodes(string className, XElement obj)
		{
			var refSeqProps = MetadataCache.MdCache.GetClassInfo(className).AllReferenceSequenceProperties.ToList();
			if (refSeqProps.Count == 0)
				return;
			foreach (var refSeqProp in refSeqProps)
			{
				var propNode = refSeqProp.IsCustomProperty
					? obj.Elements("Custom").FirstOrDefault(pi => pi.Attribute(SharedConstants.Name).Value == refSeqProp.PropertyName)
					: obj.Element(refSeqProp.PropertyName);
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
			IDictionary<string, SortedDictionary<string, byte[]>> classData,
			Dictionary<string, string> guidToClassMapping,
			XElement owningObjElement)
		{
			var className = (owningObjElement.Name.LocalName == SharedConstants.Ownseq
				|| owningObjElement.Name.LocalName == SharedConstants.Refseq)
					? owningObjElement.Attribute(SharedConstants.Class).Value
					: owningObjElement.Name.LocalName;
			var classInfo = MetadataCache.MdCache.GetClassInfo(className);
			var owningProps = MetadataCache.MdCache.PropertyCache[className]["AllOwning"];
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

				var isOwningSeqProp = classInfo.GetProperty(propName).DataType == DataType.OwningSequence;
				// Replace each objsur node with actual element.
				foreach (var objsurElement in owningObjSurElements.ToArray())
				{
					var guid = objsurElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
					string classOfOwnedObject;
					if (!guidToClassMapping.TryGetValue(guid, out classOfOwnedObject))
					{
						// Dangling owning ref to non-existant object.
						objsurElement.Remove();
						continue;
					}
					guidToClassMapping.Remove(guid);
					var ownedElement = Utilities.CreateFromBytes(classData[classOfOwnedObject][guid]);
					objsurElement.ReplaceWith(ownedElement);
					// Recurse on down to the bottom.
					NestObject(isOwningSeqProp, ownedElement, classData, guidToClassMapping);
				}
			}
		}

		private static string RenameElement(bool isOwningSeqProp, XElement obj)
		{
			var classAttr = obj.Attribute(SharedConstants.Class);
			var className = classAttr.Value;
			if (isOwningSeqProp)
			{
				obj.Name = SharedConstants.Ownseq;
			}
			else
			{
				obj.Name = className;
				classAttr.Remove();
			}

			return className;
		}
	}
}