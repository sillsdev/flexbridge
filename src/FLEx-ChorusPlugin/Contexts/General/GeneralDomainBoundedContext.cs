using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Contexts.General
{
	internal class GeneralDomainBoundedContext
	{
		internal static void NestContext(string generalBaseDir,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			var langProjElement = classData["LangProject"].Values.First();

			// LP AnnotationDefs (OA-CmPossibilityList). AnnotationDefs.list]
			FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
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
					var filterElement = classData[className][filterGuid];
					CmObjectNestingService.NestObject(false, filterElement, new Dictionary<string, HashSet<string>>(), classData, guidToClassMapping);
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
					var annotationElement = classData[className][annotationGuid];
					CmObjectNestingService.NestObject(false, annotationElement, new Dictionary<string, HashSet<string>>(), classData, guidToClassMapping);
					BaseDomainServices.ReplaceElementNameWithAndAddClassAttribute(SharedConstants.CmAnnotation, annotationElement);
					root.Add(annotationElement);
				}
				FileWriterService.WriteNestedFile(Path.Combine(generalBaseDir, SharedConstants.FLExAnnotationsFilename), root);
				owningPropElement.RemoveNodes();
			}

			// Yippee!! Write LP here. :-) [LanguageProject.langproj; new ext]
			CmObjectNestingService.NestObject(false,
				langProjElement,
				new Dictionary<string, HashSet<string>>(),
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
			var langProjElement = langProjDoc.Root.Element("LangProject");
			// Add LP to highLevelData.
			highLevelData.Add("LangProject", langProjElement);
			// Flatten it.
			CmObjectFlatteningService.FlattenObject(
				langProjPathname,
				sortedData,
				langProjElement,
				null); // No owner.

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
					CmObjectFlatteningService.FlattenObject(
						currentPathname,
						sortedData,
						styleElement,
						langProjGuid); // Restore 'ownerguid' to style.
					var styleGuid = styleElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
					sortedElements.Add(styleGuid, BaseDomainServices.CreateObjSurElement(styleGuid));
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
					CmObjectFlatteningService.FlattenObject(
						currentPathname,
						sortedData,
						filterElement,
						langProjGuid); // Restore 'ownerguid' to style.
					var filterGuid = filterElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
					sortedElements.Add(filterGuid, BaseDomainServices.CreateObjSurElement(filterGuid));
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
				CmObjectFlatteningService.FlattenObject(
					currentPathname,
					sortedData,
					annotationElement,
					langProjGuid); // Restore 'ownerguid' to style.
				// Restore the right main element name from the class attribute.
				var annotationGuid = annotationElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
				sortedElements.Add(annotationGuid, BaseDomainServices.CreateObjSurElement(annotationGuid));
			}
			// Restore LP Annotations property in sorted order.
			var owningProp = langProjElement.Element(SharedConstants.Annotations);
			foreach (var sortedTextObjSurElement in sortedElements.Values)
				owningProp.Add(sortedTextObjSurElement);
		}
	}
}