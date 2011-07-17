using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHanders.FieldWorks;

namespace FieldWorksBridge.Infrastructure
{
	internal static class AnthropologyBoundedContextService
	{
		private const string AnthropologyRootFolder = "Anthropology";

		public static void ExtractBoundedContexts(XmlReaderSettings readerSettings, string multiFileDirRoot,
												  MetadataCache mdc,
												  IDictionary<string, SortedDictionary<string, byte[]>> classData, Dictionary<string, string> guidToClassMapping,
												  HashSet<string> skipWriteEmptyClassFiles)
		{
			var anthropologyBaseDir = Path.Combine(multiFileDirRoot, AnthropologyRootFolder);
			if (Directory.Exists(anthropologyBaseDir))
				Directory.Delete(anthropologyBaseDir, true);
			// Create it, it even if there is no notebook, since there will be all the lists.
			Directory.CreateDirectory(anthropologyBaseDir);

			SortedDictionary<string, byte[]> sortedInstanceData;
			classData.TryGetValue("RnResearchNbk", out sortedInstanceData);

			var multiClassOutput = new Dictionary<string, SortedDictionary<string, byte[]>>();
			if (sortedInstanceData.Count > 0)
			{
				var guid = sortedInstanceData.Keys.First();
				var dataBytes = sortedInstanceData.Values.First();
				var notebookDir = Path.Combine(anthropologyBaseDir, "Notebook");
				Directory.CreateDirectory(notebookDir);

				// 1. Write out the RnResearchNbk instance in anthropologyBaseDir, but not one of its lists.
				FileWriterService.WriteObject(mdc, classData, guidToClassMapping, notebookDir, readerSettings, multiClassOutput, guid,
					new HashSet<string> { "RecTypes" });
				multiClassOutput.Clear();

				// 2. Write RecTypes list.
				ObjectFinderServices.WritePropertyInFolders(mdc,
					classData, guidToClassMapping, multiClassOutput,
					readerSettings, anthropologyBaseDir,
					XElement.Parse(MultipleFileServices.Utf8.GetString(dataBytes)),
					"RecTypes", "RecordTypes", false);
				multiClassOutput.Clear();
			}

			// Other LangProj props to write here:
			var langProjElement = XElement.Parse(MultipleFileServices.Utf8.GetString(classData["LangProject"].Values.First()));

			//	3. Write AnthroList
			ObjectFinderServices.WritePropertyInFolders(mdc,
				classData, guidToClassMapping, multiClassOutput,
				readerSettings, anthropologyBaseDir,
				langProjElement,
				"AnthroList", "AnthroList", false);
			multiClassOutput.Clear();

			//	4. Write ConfidenceLevels (or lex?)
			ObjectFinderServices.WritePropertyInFolders(mdc,
				classData, guidToClassMapping, multiClassOutput,
				readerSettings, anthropologyBaseDir,
				langProjElement,
				"ConfidenceLevels", "ConfidenceLevels", false);
			multiClassOutput.Clear();

			//	5. Write Restrictions (or lex?)
			ObjectFinderServices.WritePropertyInFolders(mdc,
				classData, guidToClassMapping, multiClassOutput,
				readerSettings, anthropologyBaseDir,
				langProjElement,
				"Restrictions", "Restrictions", false);
			multiClassOutput.Clear();

			//	6. Write Roles
			ObjectFinderServices.WritePropertyInFolders(mdc,
				classData, guidToClassMapping, multiClassOutput,
				readerSettings, anthropologyBaseDir,
				langProjElement,
				"Roles", "Roles", false);
			multiClassOutput.Clear();

			//	7. Write Status
			ObjectFinderServices.WritePropertyInFolders(mdc,
				classData, guidToClassMapping, multiClassOutput,
				readerSettings, anthropologyBaseDir,
				langProjElement,
				"Status", "Status", false);
			multiClassOutput.Clear();

			//	8. Write Locations
			ObjectFinderServices.WritePropertyInFolders(mdc,
				classData, guidToClassMapping, multiClassOutput,
				readerSettings, anthropologyBaseDir,
				langProjElement,
				"Locations", "Locations", false);
			multiClassOutput.Clear();

			//	9. Write People
			ObjectFinderServices.WritePropertyInFolders(mdc,
				classData, guidToClassMapping, multiClassOutput,
				readerSettings, anthropologyBaseDir,
				langProjElement,
				"People", "People", false);
			multiClassOutput.Clear();

			//	10. Write Education
			ObjectFinderServices.WritePropertyInFolders(mdc,
				classData, guidToClassMapping, multiClassOutput,
				readerSettings, anthropologyBaseDir,
				langProjElement,
				"Education", "Education", false);
			multiClassOutput.Clear();

			//	11. Write TimeOfDay
			ObjectFinderServices.WritePropertyInFolders(mdc,
				classData, guidToClassMapping, multiClassOutput,
				readerSettings, anthropologyBaseDir,
				langProjElement,
				"TimeOfDay", "TimeOfDay", false);
			multiClassOutput.Clear();

			//	12. Write Positions
			ObjectFinderServices.WritePropertyInFolders(mdc,
				classData, guidToClassMapping, multiClassOutput,
				readerSettings, anthropologyBaseDir,
				langProjElement,
				"Positions", "Positions", false);
			multiClassOutput.Clear();


			//// No need to process it in the 'soup' now.
			ObjectFinderServices.ProcessLists(classData, skipWriteEmptyClassFiles, new HashSet<string> { "RnResearchNbk", "RnGenericRec", "Reminder", "RnRoledPartic", "CmPerson", "CmAnthroItem", "CmLocation" });
		}

		public static void RestoreOriginalFile(XmlWriter writer, XmlReaderSettings readerSettings, string multiFileDirRoot)
		{
			var anthropologyBaseDir = Path.Combine(multiFileDirRoot, AnthropologyRootFolder);
			if (!Directory.Exists(anthropologyBaseDir))
				return;

			FileWriterService.WriteClassDataToOriginal(writer, anthropologyBaseDir, readerSettings);

			foreach (var directory in Directory.GetDirectories(anthropologyBaseDir))
				FileWriterService.WriteClassDataToOriginal(writer, directory, readerSettings);
		}
	}
}