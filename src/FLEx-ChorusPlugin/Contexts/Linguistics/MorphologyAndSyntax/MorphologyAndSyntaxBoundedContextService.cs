using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Contexts.Linguistics.MorphologyAndSyntax
{
	internal static class MorphologyAndSyntaxBoundedContextService
	{
		internal static void NestContext(string linguisticsBaseDir,
			IDictionary<string, SortedDictionary<string, string>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			var morphAndSynDir = Path.Combine(linguisticsBaseDir, SharedConstants.MorphologyAndSyntax);
			if (!Directory.Exists(morphAndSynDir))
				Directory.CreateDirectory(morphAndSynDir);

			var lexDbAsString = classData["LexDb"].Values.FirstOrDefault();
			var lexDb = string.IsNullOrEmpty(lexDbAsString) ? null : XElement.Parse(lexDbAsString);
			if (lexDb != null)
			{
				// Write out LexDb's "MorphTypes" list, as per AndyB (7 Feb 2012).
				FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
											  lexDb, SharedConstants.MorphTypes,
											  Path.Combine(morphAndSynDir, SharedConstants.MorphTypesListFilename));
				classData["LexDb"][lexDb.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant()] = lexDb.ToString();
			}
			var langProjElement = XElement.Parse(classData[SharedConstants.LangProject].Values.First());
			// 1. Nest: LP's MorphologicalData(MoMorphData OA) (Also does MoMorphData's ProdRestrict(CmPossibilityList)
			//		Remove objsur node from LP.
			var morphologicalDataPropElement = langProjElement.Element("MorphologicalData");
			morphologicalDataPropElement.RemoveNodes();
			var morphDataElement = XElement.Parse(classData["MoMorphData"].Values.First());
			CmObjectNestingService.NestObject(
				false,
				morphDataElement,
				classData,
				guidToClassMapping);
			// Hold off writing it until its list is written.

			// 2. Nest: LP's MsFeatureSystem(FsFeatureSystem OA)
			//		Remove objsur node from LP.
			var morphFeatureSystemPropElement = langProjElement.Element("MsFeatureSystem");
			var morphFeatureSystemElement = XElement.Parse(classData["FsFeatureSystem"][morphFeatureSystemPropElement.Element(SharedConstants.Objsur).Attribute(SharedConstants.GuidStr).Value]);
			morphFeatureSystemPropElement.RemoveNodes();
			CmObjectNestingService.NestObject(
				false,
				morphFeatureSystemElement,
				classData,
				guidToClassMapping);
			FileWriterService.WriteNestedFile(Path.Combine(morphAndSynDir, SharedConstants.MorphAndSynFeaturesFilename), new XElement("FeatureSystem", morphFeatureSystemElement));

			// 3. Nest: LP's PartsOfSpeech(CmPossibilityList OA)
			//		Remove objsur node from LP.
			FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
										  langProjElement, SharedConstants.PartsOfSpeech,
										  Path.Combine(morphAndSynDir, SharedConstants.PartsOfSpeechFilename));

			// 4. Nest: LP's AnalyzingAgents(CmAgent OC) (use some new extension and a fixed name)
			//		Remove objsur node(s) from LP.
			var agents = classData["CmAgent"];
			var rootElement = new XElement(SharedConstants.AnalyzingAgents);
			foreach (var agentGuid in BaseDomainServices.GetGuids(langProjElement, SharedConstants.AnalyzingAgents))
			{
				var agentElement = XElement.Parse(agents[agentGuid]);
				rootElement.Add(agentElement);
				CmObjectNestingService.NestObject(
					false,
					agentElement,
					classData,
					guidToClassMapping);
			}
			FileWriterService.WriteNestedFile(Path.Combine(morphAndSynDir, SharedConstants.AnalyzingAgentsFilename), rootElement);
			langProjElement.Element(SharedConstants.AnalyzingAgents).RemoveNodes();

			// A. Write: MoMorphData's ProdRestrict(CmPossibilityList OA) and write in its own .list file.
			//		Remove ProdRestrict node child in MoMorphData
			var prodRestrictPropElement = morphDataElement.Element("ProdRestrict");
			if (prodRestrictPropElement != null && prodRestrictPropElement.HasElements)
			{
				// NB: Write file, but only if morphDataElement has the list.
				FileWriterService.WriteNestedFile(Path.Combine(morphAndSynDir, "ProdRestrict." + SharedConstants.List), prodRestrictPropElement);
				prodRestrictPropElement.RemoveNodes();
			}

			// B. Write: LP's MorphologicalData(MoMorphData OA) in a new extension (morphdata)
			FileWriterService.WriteNestedFile(Path.Combine(morphAndSynDir, SharedConstants.MorphAndSynDataFilename), new XElement("MorphAndSynData", morphDataElement));

			classData[SharedConstants.LangProject][langProjElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant()] = langProjElement.ToString();
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string linguisticsBaseDir)
		{
			var morphAndSynDir = Path.Combine(linguisticsBaseDir, SharedConstants.MorphologyAndSyntax);
			if (!Directory.Exists(morphAndSynDir))
				return;

			var langProjElement = highLevelData[SharedConstants.LangProject];
			var lexDb = highLevelData["LexDb"];
			var currentPathname = Path.Combine(morphAndSynDir, SharedConstants.MorphTypesListFilename);
			if (lexDb != null && File.Exists(currentPathname))
			{
				// Restore MorphTypes list to LexDb.
				var morphTypesDoc = XDocument.Load(currentPathname);
				BaseDomainServices.RestoreElement(
					currentPathname,
					sortedData,
					lexDb, SharedConstants.MorphTypes,
					morphTypesDoc.Root.Element(SharedConstants.CmPossibilityList)); // Owned elment.
			}
			var langProjGuid = langProjElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();

			currentPathname = Path.Combine(morphAndSynDir, SharedConstants.MorphAndSynFeaturesFilename);
			if (File.Exists(currentPathname))
			{
				var mAndSFeatSysDoc = XDocument.Load(currentPathname);
				BaseDomainServices.RestoreElement(
					currentPathname,
					sortedData,
					langProjElement, "MsFeatureSystem",
					mAndSFeatSysDoc.Root.Element("FsFeatureSystem")); // Owned elment.
			}

			currentPathname = Path.Combine(morphAndSynDir, SharedConstants.PartsOfSpeechFilename);
			if (File.Exists(currentPathname))
			{
				var posDoc = XDocument.Load(currentPathname);
				BaseDomainServices.RestoreElement(
					currentPathname,
					sortedData,
					langProjElement, SharedConstants.PartsOfSpeech,
					posDoc.Root.Element(SharedConstants.CmPossibilityList)); // Owned elment.
			}

			currentPathname = Path.Combine(morphAndSynDir, SharedConstants.AnalyzingAgentsFilename);
			// Put Agents back into LP.
			var sortedAgents = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			var agentDoc = XDocument.Load(currentPathname);
			foreach (var agentElement in agentDoc.Root.Elements())
			{
				CmObjectFlatteningService.FlattenObject(
					currentPathname,
					sortedData,
					agentElement,
					langProjGuid); // Restore 'ownerguid' to agent.
				var agentGuid = agentElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
				sortedAgents.Add(agentGuid, BaseDomainServices.CreateObjSurElement(agentGuid));
			}
			// Restore LP AnalyzingAgents property in sorted order.
			if (sortedAgents.Count > 0)
			{
				var langProjOwningProp = langProjElement.Element(SharedConstants.AnalyzingAgents);
				foreach (var sortedTextObjSurElement in sortedAgents.Values)
					langProjOwningProp.Add(sortedTextObjSurElement);
			}

			currentPathname = Path.Combine(morphAndSynDir, SharedConstants.MorphAndSynDataFilename);
			if (File.Exists(currentPathname))
			{
				var mAndSDataDoc = XDocument.Load(currentPathname);
				var morphDataElement = mAndSDataDoc.Root.Element("MoMorphData");
				BaseDomainServices.RestoreElement(
					currentPathname,
					sortedData,
					langProjElement,
					"MorphologicalData",
					morphDataElement);

				currentPathname = Path.Combine(morphAndSynDir, "ProdRestrict." + SharedConstants.List);
				if (File.Exists(currentPathname))
				{
					var prodRestrictDoc = XDocument.Load(currentPathname);
					var prodRestrictListElement = prodRestrictDoc.Root.Element(SharedConstants.CmPossibilityList);
					BaseDomainServices.RestoreElement(
						currentPathname,
						sortedData,
						morphDataElement, "ProdRestrict", prodRestrictListElement);
				}
			}
		}
	}
}