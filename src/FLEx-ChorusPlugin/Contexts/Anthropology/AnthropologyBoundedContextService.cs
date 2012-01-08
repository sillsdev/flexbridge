using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Contexts.Anthropology
{
	/// <summary>
	/// Read/Write/Delete the Anthropology bounded context.
	///
	/// There is only one file for the anthropology context, which is named:
	/// Root\Anthropology\DataNotebook.ntbk.
	///
	/// File format:
	/// Notebook (root)
	///	singleton - header
	///		High-level RnResearchNbk element, with its Records element remaining, but emptied of content.
	///		Series of lists owned by LangProj, each list wrapped in matching owning prop element name.
	///	series (col prop) of RnGenericRec elements, nested, as needed.
	/// </summary>
	internal static class AnthropologyBoundedContextService
	{
		internal static void NestContext(XmlReaderSettings readerSettings, string anthropologyDir,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping,
			Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache,
			HashSet<string> skipWriteEmptyClassFiles)
		{
			// No subfolders for anthropologyDir
			if (!Directory.Exists(anthropologyDir))
				Directory.CreateDirectory(anthropologyDir);

			SortedDictionary<string, XElement> sortedInstanceData;
			classData.TryGetValue("RnResearchNbk", out sortedInstanceData);
			var langProj = classData["LangProject"].Values.First();

			var headerElement = new XElement(SharedConstants.Header);
			var rootElement = new XElement("Anthropology", headerElement);
			if (sortedInstanceData.Count > 0)
			{
				// 1. Main RnResearchNbk element.
				var notebookElement = sortedInstanceData.Values.First();
				headerElement.Add(notebookElement);

				CmObjectNestingService.NestObject(notebookElement,
					new Dictionary<string, HashSet<string>>(),
					classData,
					interestingPropertiesCache,
					guidToClassMapping);
				var recordsElement = notebookElement.Element("Records");
				if (recordsElement != null && recordsElement.HasElements)
				{
					// Put nested (by this time) records all in as children of root.
					rootElement.Add(recordsElement.Elements()); // NB: These were already sorted, way up in MultipleFileServices::CacheDataRecord, since "Records" is a collection prop.
					recordsElement.RemoveNodes(); // Leaves empty Records element placeholder in RnResearchNbk element.
				}
				else
				{
					// Add one bogus element, so fast splitter need not be changed for optional main sequence.
					// Restore will remove it, if found.
					rootElement.Add(new XElement("RnGenericRec",
												   new XAttribute(SharedConstants.GuidStr, Guid.Empty)));
				}
				// Remove objsur node from owning LangProg
				langProj.Element("ResearchNotebook").RemoveNodes();
			}

			// LangProj props to write. (List props will remain in lang proj, but the list obsur will be removed.)
			NestLists(classData,
				guidToClassMapping,
				interestingPropertiesCache,
				classData["CmPossibilityList"],
				headerElement,
				langProj,
				new List<string>
								{
									"AnthroList",
									"ConfidenceLevels",
									"Education",
									"Locations",
									"People",
									"Positions",
									"Restrictions",
									"Roles",
									"Status",
									"TimeOfDay"
								});

			var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
					rootElement);
			FileWriterService.WriteNestedFile(Path.Combine(anthropologyDir, "DataNotebook.ntbk"), readerSettings, doc);

			//// No need to process these in the 'soup' now.
			ObjectFinderServices.ProcessLists(classData, skipWriteEmptyClassFiles, new HashSet<string> { "RnResearchNbk", "RnGenericRec", "Reminder", "RnRoledPartic", "CmPerson", "CmAnthroItem", "CmLocation" });
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache,
			string anthropologyBaseDir)
		{
			// No subfolders for anthropologyDir
			if (!Directory.Exists(anthropologyBaseDir))
				return; // Nothing to do.

			var langProjElement = highLevelData["LangProject"];
			var doc = XDocument.Load(Path.Combine(anthropologyBaseDir, "DataNotebook.ntbk"));
			var root = doc.Root;
			foreach (var headerChildElement in root.Element(SharedConstants.Header).Elements())
			{
				switch (headerChildElement.Name.LocalName)
				{
					case "RnResearchNbk":
						var owningRnPropElement = langProjElement.Element("ResearchNotebook");
						owningRnPropElement.Add(new XElement(SharedConstants.Objsur,
															   new XAttribute(SharedConstants.GuidStr, headerChildElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant()),
															   new XAttribute("t", "o")));
						// Put all records back in RnResearchNbk, before sort and restore.
						// EXCEPT, if there is only onne of them and it is guid.Empty, then skip it
						var records = root.Elements("RnGenericRec").ToList();
						if (records.Count > 1 || records[0].Attribute(SharedConstants.GuidStr).Value != Guid.Empty.ToString())
							headerChildElement.Element("Records").Add();
						CmObjectFlatteningService.FlattenObject(sortedData, interestingPropertiesCache, headerChildElement, null); // object already has owning guid attr.
						break;
					case "AnthroList": // Fall through
					case "ConfidenceLevels": // Fall through
					case "Education": // Fall through
					case "Locations": // Fall through
					case "People": // Fall through
					case "Positions": // Fall through
					case "Restrictions": // Fall through
					case "Roles": // Fall through
					case "Status": // Fall through
					case "TimeOfDay":
						var listElement = headerChildElement.Element("CmPossibilityList");
						RestoreLangProjListObjsurElement(langProjElement, listElement);
						CmObjectFlatteningService.FlattenObject(sortedData, interestingPropertiesCache, listElement, null); // object already has owning guid attr.
						break;
				}
			}
		}

		internal static void RemoveBoundedContextData(string anthropologyBase)
		{
			var notebookPath = Path.Combine(anthropologyBase, "DataNotebook.ntbk");
			if (File.Exists(notebookPath))
				File.Delete(notebookPath);
			FileWriterService.RemoveEmptyFolders(anthropologyBase, true);
		}

		private static void RestoreLangProjListObjsurElement(XContainer langProjElement, XElement listElement)
		{
			var owningListPropElement = langProjElement.Element(listElement.Parent.Name.LocalName);
			owningListPropElement.Add(new XElement(SharedConstants.Objsur,
												   new XAttribute(SharedConstants.GuidStr, listElement.Attribute(SharedConstants.GuidStr).Value),
												   new XAttribute("t", "o")));
		}

		private static void NestLists(IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping,
			Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache,
			IDictionary<string, XElement> posLists,
			XContainer headerElement,
			XContainer langProjElement,
			IEnumerable<string> propNames)
		{
			var exceptions = new Dictionary<string, HashSet<string>>();
			foreach (var propName in propNames)
			{
				var listPropElement = langProjElement.Element(propName);
				if (listPropElement == null || !listPropElement.HasElements)
					continue;

				var listElement = posLists[listPropElement.Elements().First().Attribute(SharedConstants.GuidStr).Value];
				CmObjectNestingService.NestObject(listElement,
												  exceptions,
												  classData,
												  interestingPropertiesCache,
												  guidToClassMapping);
				listPropElement.RemoveNodes(); // Remove the single list objsur element.
				headerElement.Add(new XElement(propName, listElement));
			}
		}
	}
}