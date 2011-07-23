using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHanders.FieldWorks;

namespace FieldWorksBridge.Infrastructure
{
	/// <summary>
	/// Read/Write the Discourse Analysis Bounded Context.
	///
	/// This will be the DsDiscourseData instance and all it owns.
	/// </summary>
	internal static class DiscourseAnalysisBoundedContextService
	{
		private const string DiscourseRootFolder = "Discourse";

#if USEXELEMENTS
		internal static void ExtractBoundedContexts(XmlReaderSettings readerSettings, string multiFileDirRoot,
			MetadataCache mdc,
			IDictionary<string, SortedDictionary<string, XElement>> classData, Dictionary<string, string> guidToClassMapping,
			HashSet<string> skipWriteEmptyClassFiles)
#else
		internal static void ExtractBoundedContexts(XmlReaderSettings readerSettings, string multiFileDirRoot,
			MetadataCache mdc,
			IDictionary<string, SortedDictionary<string, byte[]>> classData, Dictionary<string, string> guidToClassMapping,
			HashSet<string> skipWriteEmptyClassFiles)
#endif
		{
			var discourseBaseDir = Path.Combine(multiFileDirRoot, DiscourseRootFolder);
			if (Directory.Exists(discourseBaseDir))
				Directory.Delete(discourseBaseDir, true);

#if USEXELEMENTS
			SortedDictionary<string, XElement> sortedInstanceData;
#else
			SortedDictionary<string, byte[]> sortedInstanceData;
#endif
			if (!classData.TryGetValue("DsDiscourseData", out sortedInstanceData))
				return;

			// TODO: Are there any other lists, other than thse two?
			// ConstChartTempl and ChartMarkers are two lists owned by DsDiscourseData.
			// How about lang proj's:  <owning num="55" id="TextMarkupTags" card="atomic" sig="CmPossibilityList">

#if USEXELEMENTS
			var multiClassOutput = new Dictionary<string, SortedDictionary<string, XElement>>();
#else
			var multiClassOutput = new Dictionary<string, SortedDictionary<string, byte[]>>();
#endif
			if (sortedInstanceData.Count > 0)
			{
				Directory.CreateDirectory(discourseBaseDir);

				var guid = sortedInstanceData.Keys.First();
#if USEXELEMENTS
				var dataEl = sortedInstanceData.Values.First();
#else
				var dataBytes = sortedInstanceData.Values.First();
#endif

				// 1. Write out the DsDiscourseData instance in discourseBaseDir, but not the charts it owns.
				FileWriterService.WriteObject(mdc, classData, guidToClassMapping, discourseBaseDir, readerSettings, multiClassOutput, guid, new HashSet<string> { "Charts" });

				// 2. Each chart it owns needs to be written in its own subfolder of discourseBaseDir, a la texts.
#if USEXELEMENTS
				ObjectFinderServices.WritePropertyInFolders(mdc,
					classData, guidToClassMapping, multiClassOutput,
					readerSettings, discourseBaseDir,
					dataEl,
					"Charts", "Chart_", true);
#else
				ObjectFinderServices.WritePropertyInFolders(mdc,
					classData, guidToClassMapping, multiClassOutput,
					readerSettings, discourseBaseDir,
					XElement.Parse(MultipleFileServices.Utf8.GetString(dataBytes)),
					"Charts", "Chart_", true);
#endif
			}

			// No need to process these in the 'soup' now.
			ObjectFinderServices.ProcessLists(classData, skipWriteEmptyClassFiles, new HashSet<string> { "DsDiscourseData", "DsConstChart", "ConstChartRow", "ConstChartWordGroup", "ConstChartMovedTextMarker", "ConstChartClauseMarker", "ConstChartTag" });
		}

		internal static void RestoreOriginalFile(XmlWriter writer, XmlReaderSettings readerSettings, string multiFileDirRoot)
		{
			FileWriterService.RestoreFiles(writer, readerSettings, Path.Combine(multiFileDirRoot, Path.Combine(multiFileDirRoot, DiscourseRootFolder)));
		}
	}
}