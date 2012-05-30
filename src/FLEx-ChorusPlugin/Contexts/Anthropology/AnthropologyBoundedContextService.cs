using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Contexts.Anthropology
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
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			var sortedInstanceData = classData["RnResearchNbk"];
			var langProj = classData["LangProject"].Values.First();

			var headerElement = new XElement(SharedConstants.Header);
			var rootElement = new XElement(SharedConstants.Anthropology, headerElement);
			if (sortedInstanceData.Count > 0)
			{
				// 1. Main RnResearchNbk element.
				var notebookElement = sortedInstanceData.Values.First();
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
						Path.Combine(anthropologyDir, "RecTypes." + SharedConstants.List),
						new XElement("RecTypes", recTypesOwningPropElement.Element(SharedConstants.CmPossibilityList)));
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

			FileWriterService.WriteNestedFile(Path.Combine(anthropologyDir, SharedConstants.DataNotebookFilename), rootElement);

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
										  Path.Combine(anthropologyDir, "AnthroList." + SharedConstants.List));
			FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
										  langProj, "ConfidenceLevels",
										  Path.Combine(anthropologyDir, "ConfidenceLevels." + SharedConstants.List));
			FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
										  langProj, "Education",
										  Path.Combine(anthropologyDir, "Education." + SharedConstants.List));
			FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
										  langProj, "Locations",
										  Path.Combine(anthropologyDir, "Locations." + SharedConstants.List));
			FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
										  langProj, "People",
										  Path.Combine(anthropologyDir, "People." + SharedConstants.List));
			FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
										  langProj, "Positions",
										  Path.Combine(anthropologyDir, "Positions." + SharedConstants.List));
			FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
										  langProj, "Restrictions",
										  Path.Combine(anthropologyDir, "Restrictions." + SharedConstants.List));
			FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
										  langProj, "Roles",
										  Path.Combine(anthropologyDir, "Roles." + SharedConstants.List));
			FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
										  langProj, "Status",
										  Path.Combine(anthropologyDir, "Status." + SharedConstants.List));
			FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
										  langProj, "TimeOfDay",
										  Path.Combine(anthropologyDir, "TimeOfDay." + SharedConstants.List));
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string anthropologyBaseDir)
		{
			var langProjElement = highLevelData["LangProject"];
			var currentPathname = Path.Combine(anthropologyBaseDir, SharedConstants.DataNotebookFilename);
			var doc = XDocument.Load(currentPathname);
			var root = doc.Root;
			var dnMainElement = root.Element(SharedConstants.Header).Element("RnResearchNbk");

			// Add the chart elements into discourseElement.
			var sortedRecords = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			foreach (var recordElement in root.Elements("RnGenericRec").ToList())
			{
				// Add it to Records property of dnMainElement, BUT in sorted order, below, and then flatten dnMainElement.
				sortedRecords.Add(recordElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant(), recordElement);
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
			var recTypesPathname = Path.Combine(anthropologyBaseDir, "RecTypes." + SharedConstants.List);
			if (File.Exists(recTypesPathname))
			{
				var listDoc = XDocument.Load(recTypesPathname);
				BaseDomainServices.RestoreElement(recTypesPathname, sortedData,
					dnMainElement, "RecTypes",
					listDoc.Root.Element(SharedConstants.CmPossibilityList));
			}

			BaseDomainServices.RestoreElement(currentPathname, sortedData,
				langProjElement, "ResearchNotebook",
				dnMainElement);

			// Put the lists back where they belong in LangProj.
			foreach (var listPathname in Directory.GetFiles(anthropologyBaseDir, "*." + SharedConstants.List))
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
					listRoot.Element(SharedConstants.CmPossibilityList));
			}
		}
	}
}