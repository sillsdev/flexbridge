// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibFLExBridgeChorusPlugin.DomainServices;

namespace LibFLExBridgeChorusPlugin.Contexts.Anthropology
{
	/// <summary>
	/// Read/Write/Delete the Anthropology bounded context.
	///
	/// There is one file for the main anthropology context, which is named:
	/// Root\Anthropology\DataNotebook.ntbk.
	/// There are several "list" files, besides the main data file. These lists
	/// are owned by LangProj, each list is in its own file and the root element is the matching owning prop element name.
	///
	/// File format:
	/// Notebook (root)
	///	singleton - header
	///		Main RnResearchNbk element, with its Records property element remaining, but emptied of content.
	///	series (col prop) of RnGenericRec elements, nested, as needed.
	/// </summary>
	internal static class AnthropologyBoundedContextService
	{
		internal static void NestContext(string anthropologyDir,
			IDictionary<string, XElement> wellUsedElements,
			IDictionary<string, SortedDictionary<string, byte[]>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			var sortedInstanceData = classData["RnResearchNbk"];
			var langProj = wellUsedElements[FlexBridgeConstants.LangProject];
			var headerElement = new XElement(FlexBridgeConstants.Header);
			var rootElement = new XElement(FlexBridgeConstants.Anthropology, headerElement);
			if (sortedInstanceData.Count > 0)
			{
				// 1. Main RnResearchNbk element.
				var notebookElement = LibFLExBridgeUtilities.CreateFromBytes(sortedInstanceData.Values.First());
				headerElement.Add(notebookElement);

				CmObjectNestingService.NestObject(false, notebookElement,
					classData,
					guidToClassMapping);

				// Pull out the RecTypes OA pos list prop from RnResearchNbk and write as its own list file.
				// It is nested by now, if it exists at all.
				var recTypesOwningPropElement = notebookElement.Element("RecTypes");
				if (recTypesOwningPropElement != null && recTypesOwningPropElement.HasElements)
				{
					FileWriterService.WriteNestedFile(
						Path.Combine(anthropologyDir, "RecTypes." + FlexBridgeConstants.List),
						new XElement("RecTypes", recTypesOwningPropElement.Element(FlexBridgeConstants.CmPossibilityList)));
					recTypesOwningPropElement.RemoveNodes();
				}

				var recordsElement = notebookElement.Element("Records");
				if (recordsElement != null && recordsElement.HasElements)
				{
					// Put nested (by this time) records all in as children of root.
					rootElement.Add(recordsElement.Elements()); // NB: These were already sorted, way up in MultipleFileServices::CacheDataRecord, since "Records" is a collection prop.
					recordsElement.RemoveNodes(); // Leaves empty Records element placeholder in RnResearchNbk element.
				}
				// Remove child objsur nodes from owning LangProg
				langProj.Element("ResearchNotebook").RemoveNodes();
			}

			FileWriterService.WriteNestedFile(Path.Combine(anthropologyDir, FlexBridgeConstants.DataNotebookFilename), rootElement);

			// LangProj props to write. (List props will remain in lang proj, but the list obsur will be removed.)
			// Write each of several lists into individual files.
			/* Anthro-related lists owned by LangProj.
					case "AnthroList":
					case "ConfidenceLevels":
					case "Education":
					case "Locations":
					case "People":
					case "Positions":
					case "Restrictions":
					case "Roles":
					case "Status":
					case "TimeOfDay":
			*/
			FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
										  langProj, "AnthroList",
										  Path.Combine(anthropologyDir, "AnthroList." + FlexBridgeConstants.List));
			FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
										  langProj, "ConfidenceLevels",
										  Path.Combine(anthropologyDir, "ConfidenceLevels." + FlexBridgeConstants.List));
			FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
										  langProj, "Education",
										  Path.Combine(anthropologyDir, "Education." + FlexBridgeConstants.List));
			FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
										  langProj, "Locations",
										  Path.Combine(anthropologyDir, "Locations." + FlexBridgeConstants.List));
			FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
										  langProj, "People",
										  Path.Combine(anthropologyDir, "People." + FlexBridgeConstants.List));
			FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
										  langProj, "Positions",
										  Path.Combine(anthropologyDir, "Positions." + FlexBridgeConstants.List));
			FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
										  langProj, "Restrictions",
										  Path.Combine(anthropologyDir, "Restrictions." + FlexBridgeConstants.List));
			FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
										  langProj, "Roles",
										  Path.Combine(anthropologyDir, "Roles." + FlexBridgeConstants.List));
			FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
										  langProj, "Status",
										  Path.Combine(anthropologyDir, "Status." + FlexBridgeConstants.List));
			FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
										  langProj, "TimeOfDay",
										  Path.Combine(anthropologyDir, "TimeOfDay." + FlexBridgeConstants.List));
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string anthropologyBaseDir)
		{
			var langProjElement = highLevelData[FlexBridgeConstants.LangProject];
			var currentPathname = Path.Combine(anthropologyBaseDir, FlexBridgeConstants.DataNotebookFilename);
			var doc = XDocument.Load(currentPathname);
			var root = doc.Root;
			var dnMainElement = root.Element(FlexBridgeConstants.Header).Element("RnResearchNbk");

			// Add the record elements (except the possible dummy one) into dnMainElement.
			var sortedRecords = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			foreach (var recordElement in root.Elements("RnGenericRec")
				.Where(element => element.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant() != FlexBridgeConstants.EmptyGuid))
			{
				// Add it to Records property of dnMainElement, BUT in sorted order, below, and then flatten dnMainElement.
				sortedRecords.Add(recordElement.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant(), recordElement);
			}

			if (sortedRecords.Count > 0)
			{
				var recordsElementOwningProp = dnMainElement.Element("Records")
					?? CmObjectFlatteningService.AddNewPropertyElement(dnMainElement, "Records");
				foreach (var sortedChartElement in sortedRecords.Values)
					recordsElementOwningProp.Add(sortedChartElement);
			}

			// Put the RecTypes list back into place in dnMainElement, before dnMainElement is restored.
			// But only as an <objsur> element.
			var recTypesPathname = Path.Combine(anthropologyBaseDir, "RecTypes." + FlexBridgeConstants.List);
			if (File.Exists(recTypesPathname))
			{
				var listDoc = XDocument.Load(recTypesPathname);
				BaseDomainServices.RestoreElement(recTypesPathname, sortedData,
					dnMainElement, "RecTypes",
					listDoc.Root.Element(FlexBridgeConstants.CmPossibilityList));
			}

			BaseDomainServices.RestoreElement(currentPathname, sortedData,
				langProjElement, "ResearchNotebook",
				dnMainElement);

			// Put the lists back where they belong in LangProj.
			foreach (var listPathname in Directory.GetFiles(anthropologyBaseDir, "*." + FlexBridgeConstants.List))
			{
				var listDoc = XDocument.Load(listPathname);
				var listRoot = listDoc.Root;
				var listRootName = listRoot.Name.LocalName;
				if (listRootName == "RecTypes")
					continue;
				BaseDomainServices.RestoreElement(listPathname,
					sortedData,
					langProjElement,
					listRootName,
					listRoot.Element(FlexBridgeConstants.CmPossibilityList));
			}
		}
	}
}