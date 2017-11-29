// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibFLExBridgeChorusPlugin.DomainServices;

namespace LibFLExBridgeChorusPlugin.Contexts.Linguistics.MorphologyAndSyntax
{
	internal static class MorphologyAndSyntaxBoundedContextService
	{
		internal static void NestContext(string linguisticsBaseDir,
			IDictionary<string, XElement> wellUsedElements,
			IDictionary<string, SortedDictionary<string, byte[]>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			var morphAndSynDir = Path.Combine(linguisticsBaseDir, FlexBridgeConstants.MorphologyAndSyntax);
			if (!Directory.Exists(morphAndSynDir))
				Directory.CreateDirectory(morphAndSynDir);

			var lexDb = wellUsedElements[FlexBridgeConstants.LexDb];
			if (lexDb != null)
			{
				// Write out LexDb's "MorphTypes" list, as per AndyB (7 Feb 2012).
				FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
											  lexDb, FlexBridgeConstants.MorphTypes,
											  Path.Combine(morphAndSynDir, FlexBridgeConstants.MorphTypesListFilename));
			}
			var langProjElement = wellUsedElements[FlexBridgeConstants.LangProject];
			// 1. Nest: LP's MorphologicalData(MoMorphData OA) (Also does MoMorphData's ProdRestrict(CmPossibilityList)
			//		Remove objsur node from LP.
			var morphologicalDataPropElement = langProjElement.Element("MorphologicalData");
			morphologicalDataPropElement.RemoveNodes();
			var morphDataElement = LibFLExBridgeUtilities.CreateFromBytes(classData["MoMorphData"].Values.First());
			CmObjectNestingService.NestObject(
				false,
				morphDataElement,
				classData,
				guidToClassMapping);
			// Hold off writing it until its list is written.

			// 2. Nest: LP's MsFeatureSystem(FsFeatureSystem OA)
			//		Remove objsur node from LP.
			var morphFeatureSystemPropElement = langProjElement.Element("MsFeatureSystem");
			var morphFeatureSystemElement = LibFLExBridgeUtilities.CreateFromBytes(classData["FsFeatureSystem"][morphFeatureSystemPropElement.Element(FlexBridgeConstants.Objsur).Attribute(FlexBridgeConstants.GuidStr).Value]);
			morphFeatureSystemPropElement.RemoveNodes();
			CmObjectNestingService.NestObject(
				false,
				morphFeatureSystemElement,
				classData,
				guidToClassMapping);
			FileWriterService.WriteNestedFile(Path.Combine(morphAndSynDir, FlexBridgeConstants.MorphAndSynFeaturesFilename), new XElement(FlexBridgeConstants.FeatureSystem, morphFeatureSystemElement));

			// 3. Nest: LP's PartsOfSpeech(CmPossibilityList OA)
			//		Remove objsur node from LP.
			FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
										  langProjElement, FlexBridgeConstants.PartsOfSpeech,
										  Path.Combine(morphAndSynDir, FlexBridgeConstants.PartsOfSpeechFilename));

			// 4. Nest: LP's AnalyzingAgents(CmAgent OC) (use some new extension and a fixed name)
			//		Remove objsur node(s) from LP.
			var agents = classData["CmAgent"];
			var rootElement = new XElement(FlexBridgeConstants.AnalyzingAgents);
			foreach (var agentGuid in BaseDomainServices.GetGuids(langProjElement, FlexBridgeConstants.AnalyzingAgents))
			{
				var agentElement = LibFLExBridgeUtilities.CreateFromBytes(agents[agentGuid]);
				rootElement.Add(agentElement);
				CmObjectNestingService.NestObject(
					false,
					agentElement,
					classData,
					guidToClassMapping);
			}
			FileWriterService.WriteNestedFile(Path.Combine(morphAndSynDir, FlexBridgeConstants.AnalyzingAgentsFilename), rootElement);
			langProjElement.Element(FlexBridgeConstants.AnalyzingAgents).RemoveNodes();

			// A. Write: MoMorphData's ProdRestrict(CmPossibilityList OA) and write in its own .list file.
			//		Remove ProdRestrict node child in MoMorphData
			var prodRestrictPropElement = morphDataElement.Element("ProdRestrict");
			if (prodRestrictPropElement != null && prodRestrictPropElement.HasElements)
			{
				// NB: Write file, but only if morphDataElement has the list.
				FileWriterService.WriteNestedFile(Path.Combine(morphAndSynDir, "ProdRestrict." + FlexBridgeConstants.List), prodRestrictPropElement);
				prodRestrictPropElement.RemoveNodes();
			}

			// B. Write: LP's MorphologicalData(MoMorphData OA) in a new extension (morphdata)
			FileWriterService.WriteNestedFile(Path.Combine(morphAndSynDir, FlexBridgeConstants.MorphAndSynDataFilename), new XElement(FlexBridgeConstants.MorphAndSynData, morphDataElement));
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string linguisticsBaseDir)
		{
			var morphAndSynDir = Path.Combine(linguisticsBaseDir, FlexBridgeConstants.MorphologyAndSyntax);
			if (!Directory.Exists(morphAndSynDir))
				return;

			var langProjElement = highLevelData[FlexBridgeConstants.LangProject];
			var lexDb = highLevelData[FlexBridgeConstants.LexDb];
			var currentPathname = Path.Combine(morphAndSynDir, FlexBridgeConstants.MorphTypesListFilename);
			if (lexDb != null && File.Exists(currentPathname))
			{
				// Restore MorphTypes list to LexDb.
				var morphTypesDoc = XDocument.Load(currentPathname);
				BaseDomainServices.RestoreElement(
					currentPathname,
					sortedData,
					lexDb, FlexBridgeConstants.MorphTypes,
					morphTypesDoc.Root.Element(FlexBridgeConstants.CmPossibilityList)); // Owned elment.
			}
			var langProjGuid = langProjElement.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant();

			currentPathname = Path.Combine(morphAndSynDir, FlexBridgeConstants.MorphAndSynFeaturesFilename);
			if (File.Exists(currentPathname))
			{
				var mAndSFeatSysDoc = XDocument.Load(currentPathname);
				BaseDomainServices.RestoreElement(
					currentPathname,
					sortedData,
					langProjElement, "MsFeatureSystem",
					mAndSFeatSysDoc.Root.Element("FsFeatureSystem")); // Owned elment.
			}

			currentPathname = Path.Combine(morphAndSynDir, FlexBridgeConstants.PartsOfSpeechFilename);
			if (File.Exists(currentPathname))
			{
				var posDoc = XDocument.Load(currentPathname);
				BaseDomainServices.RestoreElement(
					currentPathname,
					sortedData,
					langProjElement, FlexBridgeConstants.PartsOfSpeech,
					posDoc.Root.Element(FlexBridgeConstants.CmPossibilityList)); // Owned elment.
			}

			currentPathname = Path.Combine(morphAndSynDir, FlexBridgeConstants.AnalyzingAgentsFilename);
			// Put Agents back into LP.
			var sortedAgents = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			var agentDoc = XDocument.Load(currentPathname);
			foreach (var agentElement in agentDoc.Root.Elements())
			{
				CmObjectFlatteningService.FlattenOwnedObject(
					currentPathname,
					sortedData,
					agentElement,
					langProjGuid, sortedAgents); // Restore 'ownerguid' to agent.
			}
			// Restore LP AnalyzingAgents property in sorted order.
			if (sortedAgents.Count > 0)
			{
				var langProjOwningProp = langProjElement.Element(FlexBridgeConstants.AnalyzingAgents);
				foreach (var sortedTextObjSurElement in sortedAgents.Values)
					langProjOwningProp.Add(sortedTextObjSurElement);
			}

			currentPathname = Path.Combine(morphAndSynDir, FlexBridgeConstants.MorphAndSynDataFilename);
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

				currentPathname = Path.Combine(morphAndSynDir, "ProdRestrict." + FlexBridgeConstants.List);
				if (File.Exists(currentPathname))
				{
					var prodRestrictDoc = XDocument.Load(currentPathname);
					var prodRestrictListElement = prodRestrictDoc.Root.Element(FlexBridgeConstants.CmPossibilityList);
					BaseDomainServices.RestoreElement(
						currentPathname,
						sortedData,
						morphDataElement, "ProdRestrict", prodRestrictListElement);
				}
			}
		}
	}
}