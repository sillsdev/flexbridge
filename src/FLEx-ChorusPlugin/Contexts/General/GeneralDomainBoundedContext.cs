using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using Palaso.Xml;
using TriboroughBridge_ChorusPlugin;

namespace FLEx_ChorusPlugin.Contexts.General
{
	internal class GeneralDomainBoundedContext
	{
		internal static void NestContext(string generalBaseDir,
			IDictionary<string, XElement> wellUsedElements,
			IDictionary<string, SortedDictionary<string, byte[]>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			var langProjElement = wellUsedElements[SharedConstants.LangProject];

			// LP AnnotationDefs (OA-CmPossibilityList). AnnotationDefs.list]
			FileWriterService.WriteNestedListFileIfItExists(classData,
				guidToClassMapping,
				langProjElement, SharedConstants.AnnotationDefs,
				Path.Combine(generalBaseDir, SharedConstants.AnnotationDefsListFilename));

			// LP Styles (OC-StStyle) is used by everyone, but Scripture, so they go here.
			BaseDomainServices.NestStylesPropertyElement(
				classData,
				guidToClassMapping,
				langProjElement.Element(SharedConstants.Styles),
				Path.Combine(generalBaseDir, SharedConstants.FLExStylesFilename));

			// LP Filters (OC) can go into one filters file here. (FLExFilters.filter: new ext)
			var owningPropElement = langProjElement.Element(SharedConstants.Filters);
			if (owningPropElement != null && owningPropElement.HasElements)
			{
				var root = new XElement(SharedConstants.Filters);
				foreach (var filterObjSurElement in owningPropElement.Elements().ToList())
				{
					var filterGuid = filterObjSurElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
					var className = guidToClassMapping[filterGuid];
					var filterElement = Utilities.CreateFromBytes(classData[className][filterGuid]);
					CmObjectNestingService.NestObject(false, filterElement, classData, guidToClassMapping);
					root.Add(filterElement);
				}
				FileWriterService.WriteNestedFile(Path.Combine(generalBaseDir, SharedConstants.FLExFiltersFilename), root);
				owningPropElement.RemoveNodes();
			}

			// LP Annotations (OC). Who still uses them? If all else fails, or they are used by several BCs, then store them in one file here.
			// [FLExAnnotations.annotation: new ext]
			// OJO! Sig is "CmAnnotation", which is abtract class, so handle like in Discourse-land.
			owningPropElement = langProjElement.Element(SharedConstants.Annotations);
			if (owningPropElement != null && owningPropElement.HasElements)
			{
				var root = new XElement(SharedConstants.Annotations);
				foreach (var annotationObjSurElement in owningPropElement.Elements().ToList())
				{
					var annotationGuid = annotationObjSurElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
					var className = guidToClassMapping[annotationGuid];
					var annotationElement = Utilities.CreateFromBytes(classData[className][annotationGuid]);
					CmObjectNestingService.NestObject(false, annotationElement, classData, guidToClassMapping);
					BaseDomainServices.ReplaceElementNameWithAndAddClassAttribute(SharedConstants.CmAnnotation, annotationElement);
					root.Add(annotationElement);
				}
				FileWriterService.WriteNestedFile(Path.Combine(generalBaseDir, SharedConstants.FLExAnnotationsFilename), root);
				owningPropElement.RemoveNodes();
			}

			// Some CmPicture instances may not be owned.
			var rootElement = new XElement(SharedConstants.Pictures);
			var unownedPictures = classData[SharedConstants.CmPicture].Values.Where(listElement => XmlUtils.GetAttributes(listElement, new HashSet<string> { SharedConstants.OwnerGuid })[SharedConstants.OwnerGuid] == null).ToList();
			foreach (var unownedPictureBytes in unownedPictures)
			{
				var element = Utilities.CreateFromBytes(unownedPictureBytes);
				CmObjectNestingService.NestObject(
					false,
					element,
					classData,
					guidToClassMapping);
				rootElement.Add(element);
			}
			FileWriterService.WriteNestedFile(Path.Combine(generalBaseDir, SharedConstants.FLExUnownedPicturesFilename), rootElement);

			// No VirtualOrdering instances are owned.
			if (MetadataCache.MdCache.ModelVersion > MetadataCache.StartingModelVersion)
			{
				rootElement = new XElement(SharedConstants.VirtualOrderings);
				foreach (var element in classData[SharedConstants.VirtualOrdering].Values.ToArray().Select(virtualOrderingBytes => Utilities.CreateFromBytes(virtualOrderingBytes)))
				{
					CmObjectNestingService.NestObject(
						false,
						element,
						classData,
						guidToClassMapping);
					rootElement.Add(element);
				}
				FileWriterService.WriteNestedFile(Path.Combine(generalBaseDir, SharedConstants.FLExVirtualOrderingFilename), rootElement);
			}

			// Yippee!! Write LP here. :-) [LanguageProject.langproj; new ext]
			CmObjectNestingService.NestObject(false,
				langProjElement,
				classData,
				guidToClassMapping);
			FileWriterService.WriteNestedFile(Path.Combine(generalBaseDir, SharedConstants.LanguageProjectFilename), new XElement(SharedConstants.LanguageProject, langProjElement));
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string generalBaseDir)
		{
			var langProjPathname = Path.Combine(generalBaseDir, SharedConstants.LanguageProjectFilename);
			var langProjDoc = XDocument.Load(langProjPathname);
			var langProjElement = langProjDoc.Root.Element(SharedConstants.LangProject);
			// Add LP to highLevelData.
			highLevelData.Add(SharedConstants.LangProject, langProjElement);
			// Flatten it.
			CmObjectFlatteningService.FlattenOwnerlessObject(
				langProjPathname,
				sortedData,
				langProjElement);

			// Add stuff LP owns that is here, then flatten it.
			// LP AnnotationDefs (OA-CmPossibilityList).
			var currentPathname = Path.Combine(generalBaseDir, SharedConstants.AnnotationDefsListFilename);
			if (File.Exists(currentPathname))
			{
				// Flatten it here to get the right pathname into the method.
				BaseDomainServices.RestoreElement(
					currentPathname,
					sortedData,
					langProjElement,
					SharedConstants.AnnotationDefs,
					XDocument.Load(currentPathname).Root.Element(SharedConstants.CmPossibilityList));
			}

			// LP Styles (OC-StStyle)
			var langProjGuid = langProjElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
			var sortedElements = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			currentPathname = Path.Combine(generalBaseDir, SharedConstants.FLExStylesFilename);
			XDocument doc;
			if (File.Exists(currentPathname))
			{
				doc = XDocument.Load(currentPathname);
				foreach (var styleElement in doc.Root.Elements(SharedConstants.StStyle))
				{
					// Put style back into LP's Styles element.
					CmObjectFlatteningService.FlattenOwnedObject(
						currentPathname,
						sortedData,
						styleElement,
						langProjGuid, sortedElements); // Restore 'ownerguid' to style.
				}
				// Restore LP Styles property in sorted order.
				var langProjOwningProp = langProjElement.Element(SharedConstants.Styles);
				foreach (var sortedTextObjSurElement in sortedElements.Values)
					langProjOwningProp.Add(sortedTextObjSurElement);
			}

			// LP Filters (OC-CmFilter)
			currentPathname = Path.Combine(generalBaseDir, SharedConstants.FLExFiltersFilename);
			if (File.Exists(currentPathname))
			{
				sortedElements = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
				doc = XDocument.Load(currentPathname);
				foreach (var filterElement in doc.Root.Elements("CmFilter"))
				{
					// Put CmFilter back into LP's Filters element.
					CmObjectFlatteningService.FlattenOwnedObject(
						currentPathname,
						sortedData,
						filterElement,
						langProjGuid, sortedElements); // Restore 'ownerguid' to style.
				}
				// Restore LP Filters property in sorted order.
				var langProjOwningProp = langProjElement.Element(SharedConstants.Filters);
				foreach (var sortedTextObjSurElement in sortedElements.Values)
					langProjOwningProp.Add(sortedTextObjSurElement);
			}

			// LP Annotations (OC-CmAnnotation). [Odd elements like in Discourse.]
			currentPathname = Path.Combine(generalBaseDir, SharedConstants.FLExAnnotationsFilename);
			if (!File.Exists(currentPathname))
				return;

			sortedElements = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			doc = XDocument.Load(currentPathname);
			foreach (var annotationElement in doc.Root.Elements(SharedConstants.CmAnnotation))
			{
				// Put CmAnnotation back into LP's Annotations element.
				var classAttr = annotationElement.Attribute(SharedConstants.Class);
				annotationElement.Name = classAttr.Value;
				classAttr.Remove();
				CmObjectFlatteningService.FlattenOwnedObject(
					currentPathname,
					sortedData,
					annotationElement,
					langProjGuid, sortedElements); // Restore 'ownerguid' to style.
			}
			// Restore LP Annotations property in sorted order.
			var owningProp = langProjElement.Element(SharedConstants.Annotations);
			foreach (var sortedTextObjSurElement in sortedElements.Values)
				owningProp.Add(sortedTextObjSurElement);

			// No VirtualOrdering instances are owned.
			if (MetadataCache.MdCache.ModelVersion > MetadataCache.StartingModelVersion)
			{
				currentPathname = Path.Combine(generalBaseDir, SharedConstants.FLExVirtualOrderingFilename);
				doc = XDocument.Load(currentPathname);
				foreach (var orderingElement in doc.Root.Elements(SharedConstants.VirtualOrdering))
				{
					CmObjectFlatteningService.FlattenOwnerlessObject(
						currentPathname,
						sortedData,
						orderingElement);
				}
			}

			// Some CmPicture instances may not be owned.
			currentPathname = Path.Combine(generalBaseDir, SharedConstants.FLExUnownedPicturesFilename);
			doc = XDocument.Load(currentPathname);
			foreach (var pictureElement in doc.Root.Elements(SharedConstants.CmPicture))
			{
				CmObjectFlatteningService.FlattenOwnerlessObject(
					currentPathname,
					sortedData,
					pictureElement);
			}
		}
	}
}