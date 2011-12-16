using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;

namespace FLEx_ChorusPlugin.Contexts.Anthropology
{
	internal static class AnthropologyBoundedContextService
	{
		private const string AnthropologyRootFolder = "Anthropology";

		internal static void ExtractBoundedContexts(XmlReaderSettings readerSettings, string multiFileDirRoot,
												  MetadataCache mdc,
												  IDictionary<string, SortedDictionary<string, XElement>> classData, Dictionary<string, string> guidToClassMapping,
												  HashSet<string> skipWriteEmptyClassFiles)
		{
			var anthropologyBaseDir = Path.Combine(multiFileDirRoot, AnthropologyRootFolder);
			if (!Directory.Exists(anthropologyBaseDir))
				Directory.CreateDirectory(anthropologyBaseDir);

			var multiClassOutput = new Dictionary<string, SortedDictionary<string, XElement>>();
			SortedDictionary<string, XElement> sortedInstanceData;
			classData.TryGetValue("RnResearchNbk", out sortedInstanceData);

			if (sortedInstanceData.Count > 0)
			{
				var guid = sortedInstanceData.Keys.First();
				var dataBytes = sortedInstanceData.Values.First();
				var notebookDir = Path.Combine(anthropologyBaseDir, "Notebook");
				if (!Directory.Exists(notebookDir))
					Directory.CreateDirectory(notebookDir);

				// 1. Write out the RnResearchNbk instance in anthropologyBaseDir, but not one of its lists.
				FileWriterService.WriteObject(mdc, classData, guidToClassMapping, notebookDir, readerSettings, multiClassOutput, guid,
					new HashSet<string> { "RecTypes" });

				// 2. Write RecTypes list.
				ObjectFinderServices.WritePropertyInFolders(mdc,
					classData, guidToClassMapping, multiClassOutput,
					readerSettings, anthropologyBaseDir,
					dataBytes,
					"RecTypes", "RecordTypes", false);
			}

			// Other LangProj props to write here:
			var langProjElement = classData["LangProject"].Values.First();

			//	3. Write AnthroList
			ObjectFinderServices.WritePropertyInFolders(mdc,
				classData, guidToClassMapping, multiClassOutput,
				readerSettings, anthropologyBaseDir,
				langProjElement,
				"AnthroList", "AnthroList", false);

			//	4. Write ConfidenceLevels (or lex?)
			ObjectFinderServices.WritePropertyInFolders(mdc,
				classData, guidToClassMapping, multiClassOutput,
				readerSettings, anthropologyBaseDir,
				langProjElement,
				"ConfidenceLevels", "ConfidenceLevels", false);

			//	5. Write Restrictions (or lex?)
			ObjectFinderServices.WritePropertyInFolders(mdc,
				classData, guidToClassMapping, multiClassOutput,
				readerSettings, anthropologyBaseDir,
				langProjElement,
				"Restrictions", "Restrictions", false);

			//	6. Write Roles
			ObjectFinderServices.WritePropertyInFolders(mdc,
				classData, guidToClassMapping, multiClassOutput,
				readerSettings, anthropologyBaseDir,
				langProjElement,
				"Roles", "Roles", false);

			//	7. Write Status
			ObjectFinderServices.WritePropertyInFolders(mdc,
				classData, guidToClassMapping, multiClassOutput,
				readerSettings, anthropologyBaseDir,
				langProjElement,
				"Status", "Status", false);

			//	8. Write Locations
			ObjectFinderServices.WritePropertyInFolders(mdc,
				classData, guidToClassMapping, multiClassOutput,
				readerSettings, anthropologyBaseDir,
				langProjElement,
				"Locations", "Locations", false);

			//	9. Write People
			ObjectFinderServices.WritePropertyInFolders(mdc,
				classData, guidToClassMapping, multiClassOutput,
				readerSettings, anthropologyBaseDir,
				langProjElement,
				"People", "People", false);

			//	10. Write Education
			ObjectFinderServices.WritePropertyInFolders(mdc,
				classData, guidToClassMapping, multiClassOutput,
				readerSettings, anthropologyBaseDir,
				langProjElement,
				"Education", "Education", false);

			//	11. Write TimeOfDay
			ObjectFinderServices.WritePropertyInFolders(mdc,
				classData, guidToClassMapping, multiClassOutput,
				readerSettings, anthropologyBaseDir,
				langProjElement,
				"TimeOfDay", "TimeOfDay", false);

			//	12. Write Positions
			ObjectFinderServices.WritePropertyInFolders(mdc,
				classData, guidToClassMapping, multiClassOutput,
				readerSettings, anthropologyBaseDir,
				langProjElement,
				"Positions", "Positions", false);

			//// No need to process it in the 'soup' now.
			ObjectFinderServices.ProcessLists(classData, skipWriteEmptyClassFiles, new HashSet<string> { "RnResearchNbk", "RnGenericRec", "Reminder", "RnRoledPartic", "CmPerson", "CmAnthroItem", "CmLocation" });
		}

		internal static void RestoreOriginalFile(XmlWriter writer, XmlReaderSettings readerSettings, string multiFileDirRoot)
		{
			FileWriterService.RestoreFiles(writer, readerSettings, Path.Combine(multiFileDirRoot, Path.Combine(multiFileDirRoot, AnthropologyRootFolder)));
		}
	}
}