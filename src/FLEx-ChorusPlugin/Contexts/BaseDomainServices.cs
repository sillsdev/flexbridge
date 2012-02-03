using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Contexts.Anthropology;
using FLEx_ChorusPlugin.Contexts.General;
using FLEx_ChorusPlugin.Contexts.Linguistics;
using FLEx_ChorusPlugin.Contexts.Scripture;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Contexts
{
	internal static class BaseDomainServices
	{
		internal static void WriteDomainData(MetadataCache mdc, string pathRoot,
											 Dictionary<string, SortedDictionary<string, XElement>> classData,
											 Dictionary<string, string> guidToClassMapping)
		{
			// TODO: There will be some 'leftover' domain that holds stuff like Lang Proj and any other 'clutter', and it needs to be added in this method somewhere.
			var skipwriteEmptyClassFiles = new HashSet<string>();

			// Does both old and new for a while yet.
			LinguisticsDomainServices.WriteNestedDomainData(pathRoot, mdc, classData, guidToClassMapping, skipwriteEmptyClassFiles);
			// Does only new.
			AnthropologyDomainServices.WriteNestedDomainData(pathRoot, classData, guidToClassMapping, skipwriteEmptyClassFiles);
			// Does only new.
			ScriptureDomainServices.WriteNestedDomainData(pathRoot, mdc, classData, guidToClassMapping, skipwriteEmptyClassFiles);

			// Remove the data that may be in multiple bounded Contexts.
			// Eventually, there ought not be an need for writing the leftovers in the base folder,
			// but I'm not there yet.
			//ObjectFinderServices.ProcessLists(classData, skipwriteEmptyClassFiles, new HashSet<string> { "N ote" });

			// TODO: Props to not store in nested LangProj:
			// TODO:	These are all for LangProj
			/*
			 * "ResearchNotebook"
			 * "AnthroList",
			 * "ConfidenceLevels",
			 * "Restrictions",
			 * "Roles",
			 * "Status",
			 * "Locations",
			 * "People",
			 * "Education",
			 * "TimeOfDay",
			 * "Positions"
			*/
			// Does 'leftover' stuff in old style.
			OldStyleDomainServices.WriteData(pathRoot, mdc, classData, skipwriteEmptyClassFiles);
		}

		internal static void RestoreDomainData(XmlWriter writer, string pathRoot)
		{
			var sortedData = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			var highLevelData = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);

			// TODO: There will be some 'leftover' domain that holds stuff like Lang Proj and any other 'clutter', and it needs to be added in this method somewhere.
			OldStyleDomainServices.RestoreOldStyleData(sortedData, highLevelData, pathRoot);

			ScriptureDomainServices.FlattenDomain(highLevelData, sortedData, pathRoot);
			AnthropologyDomainServices.FlattenDomain(highLevelData, sortedData, pathRoot);
			LinguisticsDomainServices.FlattenDomain(highLevelData, sortedData, pathRoot);

			foreach (var rtElement in sortedData.Values)
				FileWriterService.WriteElement(writer, rtElement);
		}

		internal static void RemoveDomainData(string pathRoot)
		{
			LinguisticsDomainServices.RemoveBoundedContextData(pathRoot); // TODO: Does all new, but no old.
			AnthropologyDomainServices.RemoveBoundedContextData(pathRoot); // Does all.
			ScriptureDomainServices.RemoveBoundedContextData(pathRoot); // Does all.

			// TODO: Leave OldStyleDomainServices.RemoveDataFiles in until Linguistics does it all.
			// TODO: Even then, there will be some 'leftover' domain that holds stuff like Lang Proj and any other 'clutter', and it needs to be added in this method somewhere.
			OldStyleDomainServices.RemoveDataFiles(pathRoot);
		}

		internal static void RestoreElement(
			string pathname,
			SortedDictionary<string, XElement> sortedData,
			XElement owningElement, string owningPropertyName,
			XElement ownedElement)
		{
			RestoreObjsurElement(owningElement, owningPropertyName, ownedElement);
			CmObjectFlatteningService.FlattenObject(
				pathname,
				sortedData,
				ownedElement,
				owningElement.Attribute(SharedConstants.GuidStr).Value); // Restore 'ownerguid' to ownedElement.
		}

		internal static void NestLists(IDictionary<string, SortedDictionary<string, XElement>> classData,
									   Dictionary<string, string> guidToClassMapping,
									   IDictionary<string, XElement> posLists,
									   XContainer nestedListParentElement,
									   XContainer listOwningElement,
									   IEnumerable<string> propNames)
		{
			foreach (var propName in propNames)
				NestList(classData, guidToClassMapping, posLists, nestedListParentElement, listOwningElement, propName);
		}

		internal static void NestList(IDictionary<string, SortedDictionary<string, XElement>> classData, Dictionary<string, string> guidToClassMapping, IDictionary<string, XElement> posLists,
									  XContainer nestedListParentElement, XContainer listOwningElement, string propName)
		{
			var listPropElement = listOwningElement.Element(propName);
			if (listPropElement == null || !listPropElement.HasElements)
				return;

			var listElement = posLists[listPropElement.Elements().First().Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant()];
			CmObjectNestingService.NestObject(false,
											  listElement,
											  new Dictionary<string, HashSet<string>>(),
											  classData,
											  guidToClassMapping);
			listPropElement.RemoveNodes(); // Remove the single list objsur element.
			nestedListParentElement.Add(new XElement(propName, listElement));
		}

		internal static void RestoreObjsurElement(XElement owningPropertyElement, XElement ownedElement)
		{
			AddObjSurElement(owningPropertyElement, ownedElement);
		}

		internal static void RestoreObjsurElement(XContainer owningElement, string propertyName, XElement ownedElement)
		{
			AddObjSurElement(owningElement.Element(propertyName), ownedElement);
		}

		internal static void AddObjSurElement(XElement parentProperty, XElement ownedElement)
		{
			parentProperty.Add(CreateObjSurElement(ownedElement.Attribute(SharedConstants.GuidStr).Value));
		}

		internal static XElement CreateObjSurElement(string ownedGuid)
		{
			return CreateObjSurElement(ownedGuid, "o");
		}

		internal static XElement CreateObjSurElement(string ownedGuid, string typeValue)
		{
			return new XElement(SharedConstants.Objsur, CreateAttributes(ownedGuid, typeValue));
		}

		internal static IEnumerable<XAttribute> CreateAttributes(string ownedGuid, string typeValue)
		{
			return new List<XAttribute>
					{
						new XAttribute(SharedConstants.GuidStr, ownedGuid.ToLowerInvariant()),
						new XAttribute("t", typeValue)
					};
		}
	}
}
