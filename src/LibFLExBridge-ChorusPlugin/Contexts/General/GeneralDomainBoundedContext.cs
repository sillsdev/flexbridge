// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibFLExBridgeChorusPlugin.DomainServices;
using SIL.Xml;

namespace LibFLExBridgeChorusPlugin.Contexts.General
{
	internal sealed class GeneralDomainBoundedContext
	{
		internal static void NestContext(string generalBaseDir,
			IDictionary<string, XElement> wellUsedElements,
			IDictionary<string, SortedDictionary<string, byte[]>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			var langProjElement = wellUsedElements[FlexBridgeConstants.LangProject];

			// LP AnnotationDefs (OA-CmPossibilityList). AnnotationDefs.list]
			FileWriterService.WriteNestedListFileIfItExists(classData,
				guidToClassMapping,
				langProjElement, FlexBridgeConstants.AnnotationDefs,
				Path.Combine(generalBaseDir, FlexBridgeConstants.AnnotationDefsListFilename));

			// LP Styles (OC-StStyle) is used by everyone, but Scripture, so they go here.
			BaseDomainServices.NestStylesPropertyElement(
				classData,
				guidToClassMapping,
				langProjElement.Element(FlexBridgeConstants.Styles),
				Path.Combine(generalBaseDir, FlexBridgeConstants.FLExStylesFilename));

			// LP Filters (OC) can go into one filters file here. (FLExFilters.filter: new ext)
			var owningPropElement = langProjElement.Element(FlexBridgeConstants.Filters);
			if (owningPropElement != null && owningPropElement.HasElements)
			{
				var root = new XElement(FlexBridgeConstants.Filters);
				foreach (var filterObjSurElement in owningPropElement.Elements().ToList())
				{
					var filterGuid = filterObjSurElement.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant();
					var className = guidToClassMapping[filterGuid];
					var filterElement = LibFLExBridgeUtilities.CreateFromBytes(classData[className][filterGuid]);
					CmObjectNestingService.NestObject(false, filterElement, classData, guidToClassMapping);
					root.Add(filterElement);
				}
				FileWriterService.WriteNestedFile(Path.Combine(generalBaseDir, FlexBridgeConstants.FLExFiltersFilename), root);
				owningPropElement.RemoveNodes();
			}

			// LP Annotations (OC). Who still uses them? If all else fails, or they are used by several BCs, then store them in one file here.
			// [FLExAnnotations.annotation: new ext]
			// OJO! Sig is "CmAnnotation", which is abstract class, so handle like in Discourse-land.
			owningPropElement = langProjElement.Element(FlexBridgeConstants.Annotations);
			if (owningPropElement != null && owningPropElement.HasElements)
			{
				var root = new XElement(FlexBridgeConstants.Annotations);
				foreach (var annotationObjSurElement in owningPropElement.Elements().ToList())
				{
					var annotationGuid = annotationObjSurElement.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant();
					var className = guidToClassMapping[annotationGuid];
					var annotationElement = LibFLExBridgeUtilities.CreateFromBytes(classData[className][annotationGuid]);
					CmObjectNestingService.NestObject(false, annotationElement, classData, guidToClassMapping);
					BaseDomainServices.ReplaceElementNameWithAndAddClassAttribute(FlexBridgeConstants.CmAnnotation, annotationElement);
					root.Add(annotationElement);
				}
				FileWriterService.WriteNestedFile(Path.Combine(generalBaseDir, FlexBridgeConstants.FLExAnnotationsFilename), root);
				owningPropElement.RemoveNodes();
			}

			// Some CmPicture instances may not be owned.
			var rootElement = new XElement(FlexBridgeConstants.Pictures);
			var unownedPictures = classData[FlexBridgeConstants.CmPicture].Values.Where(listElement => XmlUtils.GetAttributes(listElement, new HashSet<string> { FlexBridgeConstants.OwnerGuid })[FlexBridgeConstants.OwnerGuid] == null).ToList();
			foreach (var unownedPictureBytes in unownedPictures)
			{
				var element = LibFLExBridgeUtilities.CreateFromBytes(unownedPictureBytes);
				CmObjectNestingService.NestObject(
					false,
					element,
					classData,
					guidToClassMapping);
				rootElement.Add(element);
			}
			FileWriterService.WriteNestedFile(Path.Combine(generalBaseDir, FlexBridgeConstants.FLExUnownedPicturesFilename), rootElement);

			// No VirtualOrdering instances are owned.
			if (MetadataCache.MdCache.ModelVersion > MetadataCache.StartingModelVersion)
			{
				rootElement = new XElement(FlexBridgeConstants.VirtualOrderings);
				foreach (var element in classData[FlexBridgeConstants.VirtualOrdering].Values.ToArray().Select(virtualOrderingBytes => LibFLExBridgeUtilities.CreateFromBytes(virtualOrderingBytes)))
				{
					CmObjectNestingService.NestObject(
						false,
						element,
						classData,
						guidToClassMapping);
					rootElement.Add(element);
				}
				FileWriterService.WriteNestedFile(Path.Combine(generalBaseDir, FlexBridgeConstants.FLExVirtualOrderingFilename), rootElement);
			}

			// Yippee!! Write LP here. :-) [LanguageProject.langproj; new ext]
			CmObjectNestingService.NestObject(false,
				langProjElement,
				classData,
				guidToClassMapping);
			FileWriterService.WriteNestedFile(Path.Combine(generalBaseDir, FlexBridgeConstants.LanguageProjectFilename), new XElement(FlexBridgeConstants.LanguageProject, langProjElement));
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string generalBaseDir)
		{
			var langProjPathname = Path.Combine(generalBaseDir, FlexBridgeConstants.LanguageProjectFilename);
			var langProjDoc = XDocument.Load(langProjPathname);
			var langProjElement = langProjDoc.Root.Element(FlexBridgeConstants.LangProject);
			// Add LP to highLevelData.
			highLevelData.Add(FlexBridgeConstants.LangProject, langProjElement);
			// Flatten it.
			CmObjectFlatteningService.FlattenOwnerlessObject(
				langProjPathname,
				sortedData,
				langProjElement);

			// Add stuff LP owns that is here, then flatten it.
			// LP AnnotationDefs (OA-CmPossibilityList).
			var currentPathname = Path.Combine(generalBaseDir, FlexBridgeConstants.AnnotationDefsListFilename);
			if (File.Exists(currentPathname))
			{
				// Flatten it here to get the right pathname into the method.
				BaseDomainServices.RestoreElement(
					currentPathname,
					sortedData,
					langProjElement,
					FlexBridgeConstants.AnnotationDefs,
					XDocument.Load(currentPathname).Root.Element(FlexBridgeConstants.CmPossibilityList));
			}

			// LP Styles (OC-StStyle)
			var langProjGuid = langProjElement.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant();
			var sortedElements = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			currentPathname = Path.Combine(generalBaseDir, FlexBridgeConstants.FLExStylesFilename);
			XDocument doc;
			if (File.Exists(currentPathname))
			{
				doc = XDocument.Load(currentPathname);
				foreach (var styleElement in doc.Root.Elements(FlexBridgeConstants.StStyle))
				{
					// Put style back into LP's Styles element.
					CmObjectFlatteningService.FlattenOwnedObject(
						currentPathname,
						sortedData,
						styleElement,
						langProjGuid, sortedElements); // Restore 'ownerguid' to style.
				}
				// Restore LP Styles property in sorted order.
				var langProjOwningProp = langProjElement.Element(FlexBridgeConstants.Styles);
				foreach (var sortedTextObjSurElement in sortedElements.Values)
					langProjOwningProp.Add(sortedTextObjSurElement);
			}

			// LP Filters (OC-CmFilter)
			currentPathname = Path.Combine(generalBaseDir, FlexBridgeConstants.FLExFiltersFilename);
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
				var langProjOwningProp = langProjElement.Element(FlexBridgeConstants.Filters);
				foreach (var sortedTextObjSurElement in sortedElements.Values)
					langProjOwningProp.Add(sortedTextObjSurElement);
			}

			// LP Annotations (OC-CmAnnotation). [Odd elements like in Discourse.]
			currentPathname = Path.Combine(generalBaseDir, FlexBridgeConstants.FLExAnnotationsFilename);
			if (File.Exists(currentPathname))
			{
				sortedElements = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
				doc = XDocument.Load(currentPathname);
				foreach (var annotationElement in doc.Root.Elements(FlexBridgeConstants.CmAnnotation))
				{
					// Put CmAnnotation back into LP's Annotations element.
					var classAttr = annotationElement.Attribute(FlexBridgeConstants.Class);
					annotationElement.Name = classAttr.Value;
					classAttr.Remove();
					CmObjectFlatteningService.FlattenOwnedObject(
						currentPathname,
						sortedData,
						annotationElement,
						langProjGuid, sortedElements); // Restore 'ownerguid' to style.
				}
				// Restore LP Annotations property in sorted order.
				var owningProp = langProjElement.Element(FlexBridgeConstants.Annotations);
				foreach (var sortedTextObjSurElement in sortedElements.Values)
					owningProp.Add(sortedTextObjSurElement);
			}

			// No VirtualOrdering instances are owned.
			if (MetadataCache.MdCache.ModelVersion > MetadataCache.StartingModelVersion)
			{
				currentPathname = Path.Combine(generalBaseDir, FlexBridgeConstants.FLExVirtualOrderingFilename);
				doc = XDocument.Load(currentPathname);
				foreach (var orderingElement in doc.Root.Elements(FlexBridgeConstants.VirtualOrdering))
				{
					CmObjectFlatteningService.FlattenOwnerlessObject(
						currentPathname,
						sortedData,
						orderingElement);
				}
			}

			// Some CmPicture instances may not be owned.
			currentPathname = Path.Combine(generalBaseDir, FlexBridgeConstants.FLExUnownedPicturesFilename);
			doc = XDocument.Load(currentPathname);
			foreach (var pictureElement in doc.Root.Elements(FlexBridgeConstants.CmPicture))
			{
				CmObjectFlatteningService.FlattenOwnerlessObject(
					currentPathname,
					sortedData,
					pictureElement);
			}
		}
	}
}