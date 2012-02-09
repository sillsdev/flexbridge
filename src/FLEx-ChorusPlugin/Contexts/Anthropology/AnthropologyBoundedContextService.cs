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
					new Dictionary<string, HashSet<string>>(),
					classData,
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
												   new XAttribute(SharedConstants.GuidStr, Guid.Empty.ToString().ToLowerInvariant())));
				}
				// Remove child objsur nodes from owning LangProg
				langProj.Element("ResearchNotebook").RemoveNodes();
			}

			// LangProj props to write. (List props will remain in lang proj, but the list obsur will be removed.)
			BaseDomainServices.NestLists(classData,
				guidToClassMapping,
				classData[SharedConstants.CmPossibilityList],
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

			FileWriterService.WriteNestedFile(Path.Combine(anthropologyDir, SharedConstants.DataNotebookFilename), rootElement);
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string anthropologyBaseDir)
		{
			var langProjElement = highLevelData["LangProject"];
			var dnPathname = Path.Combine(anthropologyBaseDir, SharedConstants.DataNotebookFilename);
			var doc = XDocument.Load(dnPathname);
			var root = doc.Root;
			foreach (var headerChildElement in root.Element(SharedConstants.Header).Elements())
			{
				switch (headerChildElement.Name.LocalName)
				{
					case "RnResearchNbk":
						// Put all records back in RnResearchNbk, before sort and restore.
						// EXCEPT, if there is only one of them and it is guid.Empty, then skip it
						var records = root.Elements("RnGenericRec").ToList();
						if (records.Count > 1 || records[0].Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant() != Guid.Empty.ToString().ToLowerInvariant())
							headerChildElement.Element("Records").Add(records);
						BaseDomainServices.RestoreElement(dnPathname, sortedData,
							langProjElement, "ResearchNotebook",
							headerChildElement);
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
						BaseDomainServices.RestoreElement(dnPathname, sortedData,
							langProjElement, headerChildElement.Name.LocalName,
							headerChildElement.Element(SharedConstants.CmPossibilityList));
						break;
				}
			}
		}
	}
}