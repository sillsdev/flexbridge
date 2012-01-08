using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Contexts.General;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Contexts.Linguistics.Discourse
{
	/// <summary>
	/// Read/Write the Discourse Analysis Bounded Context.
	///
	/// This will be the DsDiscourseData instance and all it owns.
	/// </summary>
	internal static class DiscourseAnalysisBoundedContextService
	{
		private const string DiscourseRootFolder = "Discourse";

		internal static void ExtractBoundedContexts(XmlReaderSettings readerSettings, string multiFileDirRoot,
			MetadataCache mdc,
			IDictionary<string, SortedDictionary<string, XElement>> classData, Dictionary<string, string> guidToClassMapping,
			HashSet<string> skipWriteEmptyClassFiles)
		{
			var discourseBaseDir = Path.Combine(multiFileDirRoot, DiscourseRootFolder);
			if (Directory.Exists(discourseBaseDir))
				Directory.Delete(discourseBaseDir, true);

			SortedDictionary<string, XElement> sortedInstanceData;
			if (!classData.TryGetValue("DsDiscourseData", out sortedInstanceData))
				return;

			// TODO: Are there any other lists, other than thse two?
			// ConstChartTempl and ChartMarkers are two lists owned by DsDiscourseData.
			// How about lang proj's:  <owning num="55" id="TextMarkupTags" card="atomic" sig="CmPossibilityList">
			var multiClassOutput = new Dictionary<string, SortedDictionary<string, XElement>>();
			if (sortedInstanceData.Count > 0)
			{
				Directory.CreateDirectory(discourseBaseDir);

				var guid = sortedInstanceData.Keys.First();
				var dataEl = sortedInstanceData.Values.First();

				// 1. Write out the DsDiscourseData instance in discourseBaseDir, but not the charts it owns.
				FileWriterService.WriteObject(mdc, classData, guidToClassMapping, discourseBaseDir, readerSettings, multiClassOutput, guid, new HashSet<string> { "Charts" });

				// 2. Each chart it owns needs to be written in its own subfolder of discourseBaseDir, a la texts.
				ObjectFinderServices.WritePropertyInFolders(mdc,
					classData, guidToClassMapping, multiClassOutput,
					readerSettings, discourseBaseDir,
					dataEl,
					"Charts", "Chart_", true);
			}

			// No need to process these in the 'soup' now.
			ObjectFinderServices.ProcessLists(classData, skipWriteEmptyClassFiles, new HashSet<string> { "DsDiscourseData", "DsConstChart", "ConstChartRow", "ConstChartWordGroup", "ConstChartMovedTextMarker", "ConstChartClauseMarker", "ConstChartTag" });
		}

		internal static void RestoreOriginalFile(XmlWriter writer, XmlReaderSettings readerSettings, string multiFileDirRoot)
		{
			OldStyleDomainServices.RestoreFiles(writer, readerSettings, Path.Combine(multiFileDirRoot, Path.Combine(multiFileDirRoot, DiscourseRootFolder)));
		}
	}
}