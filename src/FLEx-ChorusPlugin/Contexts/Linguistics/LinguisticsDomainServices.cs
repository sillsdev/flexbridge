using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Contexts.Linguistics.Discourse;
using FLEx_ChorusPlugin.Contexts.Linguistics.Lexicon;
using FLEx_ChorusPlugin.Contexts.Linguistics.Reversals;
using FLEx_ChorusPlugin.Contexts.Linguistics.TextCorpus;
using FLEx_ChorusPlugin.Contexts.Linguistics.WordformInventory;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Contexts.Linguistics
{
	/// <summary>
	/// This domain services class interacts with the Linguistics bounded contexts.
	/// </summary>
	internal static class LinguisticsDomainServices
	{
		private const string LinguisticsBaseFolder = "Linguistics";

		internal static void WriteNestedDomainData(XmlReaderSettings readerSettings, string rootDir,
			MetadataCache mdc,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping,
			Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache,
			HashSet<string> skipWriteEmptyClassFiles)
		{
			var linguisticsBaseDir = Path.Combine(rootDir, LinguisticsBaseFolder);
			if (!Directory.Exists(linguisticsBaseDir))
				Directory.CreateDirectory(linguisticsBaseDir);

			ReversalBoundedContextService.NestContext(readerSettings, linguisticsBaseDir, classData, guidToClassMapping, interestingPropertiesCache, skipWriteEmptyClassFiles);

			// TODO: Switch to proper location.
			var multiFileDirRoot = Path.Combine(rootDir, "DataFiles");
			if (!Directory.Exists(multiFileDirRoot))
				Directory.CreateDirectory(multiFileDirRoot);

			TextCorpusBoundedContextService.ExtractBoundedContexts(readerSettings, multiFileDirRoot, mdc, classData, guidToClassMapping, skipWriteEmptyClassFiles);
			DiscourseAnalysisBoundedContextService.ExtractBoundedContexts(readerSettings, multiFileDirRoot, mdc, classData, guidToClassMapping, skipWriteEmptyClassFiles);
			WordformInventoryBoundedContextService.ExtractBoundedContexts(readerSettings, multiFileDirRoot, mdc, classData, guidToClassMapping, skipWriteEmptyClassFiles);
			PunctuationFormBoundedContextService.ExtractBoundedContexts(readerSettings, multiFileDirRoot, mdc, classData, guidToClassMapping, skipWriteEmptyClassFiles);
			LexiconBoundedContextService.ExtractBoundedContexts(readerSettings, multiFileDirRoot, mdc, classData, guidToClassMapping, skipWriteEmptyClassFiles);
			LinguisticsBoundedContextService.ExtractBoundedContexts(readerSettings, multiFileDirRoot, mdc, classData, guidToClassMapping, skipWriteEmptyClassFiles);

			/*
			// Handle the LP TranslationTags prop (OA-CmPossibilityList), if it exists.
			var translationTagsProp = languageProjectElement.Element("TranslationTags");
			if (translationTagsProp != null)
			{
				var translationTagsObjSurElement = translationTagsProp.Element(SharedConstants.Objsur);
				if (translationTagsObjSurElement != null)
				{
					var tranTagListGuid = translationTagsObjSurElement.Attribute(SharedConstants.GuidStr).Value;
					var className = guidToClassMapping[tranTagListGuid];
					var tranTagList = classData[className][tranTagListGuid];

					CmObjectNestingService.NestObject(tranTagList,
						new Dictionary<string, HashSet<string>>(),
						classData,
						interestingPropertiesCache,
						guidToClassMapping);
					// Remove 'ownerguid'.
					tranTagList.Attribute(SharedConstants.OwnerGuid).Remove();
					var listDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
						new XElement("TranslationTags", tranTagList));
					FileWriterService.WriteNestedFile(Path.Combine(scriptureBaseDir, "TranslationTags." + SharedConstants.List), readerSettings, listDoc);
					languageProjectElement.Element("TranslationTags").RemoveNodes();
				}
			}
			*/
		}

		internal static void FlattenDomain(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache,
			string rootDir)
		{
			var linguisticsBaseDir = Path.Combine(rootDir, LinguisticsBaseFolder);
			if (!Directory.Exists(linguisticsBaseDir))
				return;

			ReversalBoundedContextService.FlattenContext(highLevelData, sortedData, interestingPropertiesCache, linguisticsBaseDir);

			/* Currently handled by BaseDomainServices.
			// TODO: Switch to right location.
			var multiFileDirRoot = Path.Combine(rootDir, "DataFiles");
			TextCorpusBoundedContextService.RestoreOriginalFile(writer, readerSettings, multiFileDirRoot);
			DiscourseAnalysisBoundedContextService.RestoreOriginalFile(writer, readerSettings, multiFileDirRoot);
			WordformInventoryBoundedContextService.RestoreOriginalFile(writer, readerSettings, multiFileDirRoot);
			LexiconBoundedContextService.RestoreOriginalFile(writer, readerSettings, multiFileDirRoot);
			PunctuationFormBoundedContextService.RestoreOriginalFile(writer, readerSettings, multiFileDirRoot);
			LinguisticsBoundedContextService.RestoreOriginalFile(writer, readerSettings, multiFileDirRoot);
			*/
		}

		internal static void RemoveBoundedContextData(string pathRoot)
		{
			var linguisticsBaseDir = Path.Combine(pathRoot, LinguisticsBaseFolder);
			if (!Directory.Exists(linguisticsBaseDir))
				return;

			ReversalBoundedContextService.RemoveBoundedContextData(linguisticsBaseDir);
			//TextCorpusBoundedContextService.ExtractBoundedContexts(readerSettings, multiFileDirRoot, mdc, classData, guidToClassMapping, skipwriteEmptyClassFiles);
			//DiscourseAnalysisBoundedContextService.ExtractBoundedContexts(readerSettings, multiFileDirRoot, mdc, classData, guidToClassMapping, skipwriteEmptyClassFiles);
			//WordformInventoryBoundedContextService.ExtractBoundedContexts(readerSettings, multiFileDirRoot, mdc, classData, guidToClassMapping, skipwriteEmptyClassFiles);
			//LexiconBoundedContextService.ExtractBoundedContexts(readerSettings, multiFileDirRoot, mdc, classData, guidToClassMapping, skipwriteEmptyClassFiles);
			//PunctuationFormBoundedContextService.ExtractBoundedContexts(readerSettings, multiFileDirRoot, mdc, classData, guidToClassMapping, skipwriteEmptyClassFiles);
			//LinguisticsBoundedContextService.ExtractBoundedContexts(readerSettings, multiFileDirRoot, mdc, classData, guidToClassMapping, skipwriteEmptyClassFiles);

			FileWriterService.RemoveEmptyFolders(linguisticsBaseDir, true);
		}
	}
}