﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Contexts.Anthropology;
using FLEx_ChorusPlugin.Contexts.General;
using FLEx_ChorusPlugin.Contexts.Linguistics;
using FLEx_ChorusPlugin.Contexts.Scripture;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using Palaso.Progress.LogBox;

namespace FLEx_ChorusPlugin.Contexts
{
	internal static class BaseDomainServices
	{
		internal static void PushHumptyOffTheWall(IProgress progress, string pathRoot,
			Dictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			// NB: Don't even think of changing the order these methods are called in.
			WriteLinguisticsData(progress, pathRoot, classData, guidToClassMapping);
			WriteAnthropologyData(progress, pathRoot, classData, guidToClassMapping);
			WriteScriptureData(progress, pathRoot, classData, guidToClassMapping);
			WriteGeneralData(progress, pathRoot, classData, guidToClassMapping);
		}

		private static void WriteLinguisticsData(IProgress progress, string pathRoot,
			Dictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			progress.WriteVerbose("Writing the linguistics data....");
			LinguisticsDomainServices.WriteNestedDomainData(progress, pathRoot, classData, guidToClassMapping);
		}

		private static void WriteAnthropologyData(IProgress progress, string pathRoot,
			Dictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			progress.WriteVerbose("Writing the anthropology data....");
			AnthropologyDomainServices.WriteNestedDomainData(pathRoot, classData, guidToClassMapping);
		}

		private static void WriteScriptureData(IProgress progress, string pathRoot,
			Dictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			progress.WriteVerbose("Writing the other data....");
			ScriptureDomainServices.WriteNestedDomainData(pathRoot, classData, guidToClassMapping);
		}

		private static void WriteGeneralData(IProgress progress, string pathRoot,
			Dictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			progress.WriteVerbose("Writing the general data....");
			GeneralDomainServices.WriteNestedDomainData(progress, pathRoot, classData, guidToClassMapping);
		}

		internal static SortedDictionary<string, XElement> PutHumptyTogetherAgain(IProgress progress, string pathRoot)
		{
			var sortedData = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			var highLevelData = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			progress.WriteVerbose("Collecting the general data....");
			GeneralDomainServices.FlattenDomain(progress, highLevelData, sortedData, pathRoot);
			progress.WriteVerbose("Collecting the other data....");
			ScriptureDomainServices.FlattenDomain(highLevelData, sortedData, pathRoot);
			progress.WriteVerbose("Collecting the anthropology data....");
			AnthropologyDomainServices.FlattenDomain(highLevelData, sortedData, pathRoot);
			progress.WriteVerbose("Collecting the linguistics data....");
			LinguisticsDomainServices.FlattenDomain(progress, highLevelData, sortedData, pathRoot);
			return sortedData;
		}

		internal static void RemoveDomainData(string pathRoot)
		{
			LinguisticsDomainServices.RemoveBoundedContextData(pathRoot);
			AnthropologyDomainServices.RemoveBoundedContextData(pathRoot);
			ScriptureDomainServices.RemoveBoundedContextData(pathRoot);
			GeneralDomainServices.RemoveBoundedContextData(pathRoot);
		}

		internal static void RemoveBoundedContextDataCore(string contextBaseDir)
		{
			if (!Directory.Exists(contextBaseDir))
				return;

			foreach (var pathname in Directory.GetFiles(contextBaseDir, "*.*", SearchOption.AllDirectories)
				.Where(pathname => Path.GetExtension(pathname).ToLowerInvariant() != ".chorusnotes"))
			{
				File.Delete(pathname);
			}

			FileWriterService.RemoveEmptyFolders(contextBaseDir, true);
		}

		/************************ Basic operations above here. ****************/

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

		internal static void NestStylesPropertyElement(
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping,
			XElement stylesProperty,
			string outputPathname)
		{
			if (stylesProperty == null)
				return;
			var styleObjSurElements = stylesProperty.Elements().ToList();
			if (!styleObjSurElements.Any())
				return;

			// Use only one file for all of them.
			var root = new XElement(SharedConstants.Styles);
			foreach (var styleObjSurElement in styleObjSurElements)
			{
				var styleGuid = styleObjSurElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
				var className = guidToClassMapping[styleGuid];
				var style = classData[className][styleGuid];
				CmObjectNestingService.NestObject(false, style, classData, guidToClassMapping);
				root.Add(style);
			}

			FileWriterService.WriteNestedFile(outputPathname, root);

			stylesProperty.RemoveNodes();
		}

		internal static void ReplaceElementNameWithAndAddClassAttribute(string replacementElementName, XElement elementToRename)
		{
			var oldElementName = elementToRename.Name.LocalName;
			elementToRename.Name = replacementElementName;
			var sortedAttrs = new SortedDictionary<string, XAttribute>(StringComparer.OrdinalIgnoreCase);
			foreach (var attr in elementToRename.Attributes())
				sortedAttrs.Add(attr.Name.LocalName, attr);
			sortedAttrs.Add(SharedConstants.Class, new XAttribute(SharedConstants.Class, oldElementName));
			elementToRename.Attributes().Remove();
			foreach (var sortedAttr in sortedAttrs.Values)
				elementToRename.Add(sortedAttr);
		}

		internal static List<string> GetGuids(XContainer owningElement, string propertyName)
		{
			var propElement = owningElement.Element(propertyName);

			return (propElement == null) ? new List<string>() : (from osEl in propElement.Elements(SharedConstants.Objsur)
																 select osEl.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant()).ToList();
		}
	}
}
