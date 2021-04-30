// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LibFLExBridgeChorusPlugin.Contexts.Anthropology;
using LibFLExBridgeChorusPlugin.Contexts.General;
using LibFLExBridgeChorusPlugin.Contexts.Linguistics;
using LibFLExBridgeChorusPlugin.Contexts.Scripture;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibFLExBridgeChorusPlugin.DomainServices;
using SIL.IO;
using SIL.Progress;

namespace LibFLExBridgeChorusPlugin.Contexts
{
	internal static class BaseDomainServices
	{
		internal static void PushHumptyOffTheWall(IProgress progress, bool writeVerbose, string pathRoot,
			IDictionary<string, XElement> wellUsedElements,
			Dictionary<string, SortedDictionary<string, byte[]>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			// NB: Don't even think of changing the order these methods are called in.
			LinguisticsDomainServices.WriteNestedDomainData(progress, writeVerbose, pathRoot, wellUsedElements, classData, guidToClassMapping);
			AnthropologyDomainServices.WriteNestedDomainData(progress, writeVerbose, pathRoot, wellUsedElements, classData, guidToClassMapping);
			ScriptureDomainServices.WriteNestedDomainData(progress, writeVerbose, pathRoot, wellUsedElements, classData, guidToClassMapping);
			GeneralDomainServices.WriteNestedDomainData(progress, writeVerbose, pathRoot, wellUsedElements, classData, guidToClassMapping);
			CopySupportingSettingsFilesIntoRepo(progress, writeVerbose, pathRoot);
		}

		internal static SortedDictionary<string, XElement> PutHumptyTogetherAgain(IProgress progress, bool writeVerbose, string pathRoot)
		{
			var retval = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);

			var sortedData = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			var highLevelData = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			// NB: Don't even think of changing the order these methods are called in.
			GeneralDomainServices.FlattenDomain(progress, writeVerbose, highLevelData, sortedData, pathRoot);
			CmObjectFlatteningService.CombineData(retval, sortedData);
			ScriptureDomainServices.FlattenDomain(progress, writeVerbose, highLevelData, sortedData, pathRoot);
			CmObjectFlatteningService.CombineData(retval, sortedData);
			AnthropologyDomainServices.FlattenDomain(progress, writeVerbose, highLevelData, sortedData, pathRoot);
			CmObjectFlatteningService.CombineData(retval, sortedData);
			LinguisticsDomainServices.FlattenDomain(progress, writeVerbose, highLevelData, sortedData, pathRoot);
			CmObjectFlatteningService.CombineData(retval, sortedData);

			foreach (var highLevelElement in highLevelData.Values)
			{
				retval[highLevelElement.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant()] = highLevelElement;
			}
			CopySupportingSettingsFilesIntoProjectFolder(progress, writeVerbose, pathRoot);
			return retval;
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
			CmObjectFlatteningService.FlattenOwnedObject(
				pathname,
				sortedData,
				ownedElement,
				owningElement.Attribute(FlexBridgeConstants.GuidStr).Value, owningElement, owningPropertyName); // Restore 'ownerguid' to ownedElement.
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
			parentProperty.Add(CreateObjSurElement(ownedElement.Attribute(FlexBridgeConstants.GuidStr).Value));
		}

		internal static XElement CreateObjSurElement(string ownedGuid)
		{
			return CreateObjSurElement(ownedGuid, "o");
		}

		internal static XElement CreateObjSurElement(string ownedGuid, string typeValue)
		{
			return new XElement(FlexBridgeConstants.Objsur, CreateAttributes(ownedGuid, typeValue));
		}

		internal static IEnumerable<XAttribute> CreateAttributes(string ownedGuid, string typeValue)
		{
			return new List<XAttribute>
					{
						new XAttribute(FlexBridgeConstants.GuidStr, ownedGuid.ToLowerInvariant()),
						new XAttribute("t", typeValue)
					};
		}

		internal static void NestStylesPropertyElement(
			IDictionary<string, SortedDictionary<string, byte[]>> classData,
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
			var root = new XElement(FlexBridgeConstants.Styles);
			foreach (var styleObjSurElement in styleObjSurElements)
			{
				var styleGuid = styleObjSurElement.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant();
				var className = guidToClassMapping[styleGuid];
				var style = LibFLExBridgeUtilities.CreateFromBytes(classData[className][styleGuid]);
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
			sortedAttrs.Add(FlexBridgeConstants.Class, new XAttribute(FlexBridgeConstants.Class, oldElementName));
			elementToRename.Attributes().Remove();
			foreach (var sortedAttr in sortedAttrs.Values)
				elementToRename.Add(sortedAttr);
		}

		internal static List<string> GetGuids(XContainer owningElement, string propertyName)
		{
			var propElement = owningElement.Element(propertyName);

			return (propElement == null) ? new List<string>() : (from osEl in propElement.Elements(FlexBridgeConstants.Objsur)
																 select osEl.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant()).ToList();
		}

		internal static void CopySupportingSettingsFilesIntoRepo(IProgress progress, bool writeVerbose, string pathRoot)
		{
			progress.WriteMessage("Copying settings files...");
			// copy the dictionary configuration files into the .hg included repo
			var dictionaryConfigFolder = Path.Combine(pathRoot, "ConfigurationSettings");
			CopyFilesIntoCachedFolderWithFileRemoval(progress, writeVerbose, dictionaryConfigFolder,
				Path.Combine(pathRoot, "CachedSettings", "ConfigurationSettings"), "Copying dictionary configuration settings.");
			// copy the lexicon settings files into the.hg included repo
			var sharedSettingsFolder = Path.Combine(pathRoot, "SharedSettings");
			CopyFilesIntoCachedFolderWithFileRemoval(progress, writeVerbose, sharedSettingsFolder,
				Path.Combine(pathRoot, "CachedSettings", "SharedSettings"), "Copying shared lexicon settings.");
			// copy the writing system files into the .hg included repo
			var wsFolder = Path.Combine(pathRoot, "WritingSystemStore");
			CopyFilesIntoCachedFolderWithFileRemoval(progress, writeVerbose, wsFolder,
				Path.Combine(pathRoot, "CachedSettings", "WritingSystemStore"), "Copying writing systems.");
		}

		/// <summary>
		/// Copy the files into the repo location, remove files from the repo that were removed from the local folder.
		/// </summary>
		private static void CopyFilesIntoCachedFolderWithFileRemoval(IProgress progress,
			bool writeVerbose, string sourcePath, string destinationPath, string progressMessage)
		{
			if (Directory.Exists(sourcePath))
			{
				if (writeVerbose)
				{
					progress.WriteMessage(progressMessage);
				}

				if (Directory.Exists(destinationPath))
				{
					RobustIO.DeleteDirectoryAndContents(destinationPath);
				}
				DirectoryHelper.Copy(sourcePath, destinationPath, true);
			}
		}

	  private static void CopySupportingSettingsFilesIntoProjectFolder(IProgress progress, bool writeVerbose, string pathRoot)
		{
			progress.WriteMessage("Copying settings files...");
			var cachedSettingsPath = Path.Combine(pathRoot, "CachedSettings");
			if (!Directory.Exists(cachedSettingsPath))
			{
				// No settings in the repo, nothing to copy
				return;
			}
			// copy the dictionary configuration files out ofthe .hg included repo
			var dictionaryConfigFolder = Path.Combine(cachedSettingsPath, "ConfigurationSettings");
			if (Directory.Exists(dictionaryConfigFolder))
			{
				if (writeVerbose)
				{
					progress.WriteMessage("Copying dictionary configuration settings.");
				}
				DirectoryHelper.Copy(dictionaryConfigFolder, Path.Combine(pathRoot, "ConfigurationSettings"), true);
			}
			// copy the lexicon settings files into the.hg included repo
			var sharedSettingsFolder = Path.Combine(cachedSettingsPath, "SharedSettings");
			if (Directory.Exists(sharedSettingsFolder))
			{
				if (writeVerbose)
				{
					progress.WriteMessage("Copying shared lexicon settings.");
				}
				DirectoryHelper.Copy(sharedSettingsFolder, Path.Combine(pathRoot, "SharedSettings"), true);
			}
			// copy the writing system files into the .hg included repo
			var wsFolder = Path.Combine(cachedSettingsPath, "WritingSystemStore");
			if (Directory.Exists(wsFolder))
			{
				if (writeVerbose)
				{
					progress.WriteMessage("Copying writing systems.");
				}
				DirectoryHelper.Copy(wsFolder, Path.Combine(pathRoot, "WritingSystemStore"), true);
			}
		}
	}
}
