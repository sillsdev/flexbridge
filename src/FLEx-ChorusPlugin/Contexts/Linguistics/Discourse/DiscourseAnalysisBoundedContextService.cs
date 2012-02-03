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

		internal static void ExtractBoundedContexts(string multiFileDirRoot,
			MetadataCache mdc,
			IDictionary<string, SortedDictionary<string, XElement>> classData, Dictionary<string, string> guidToClassMapping)
		{
			var discourseBaseDir = Path.Combine(multiFileDirRoot, DiscourseRootFolder);
			if (Directory.Exists(discourseBaseDir))
				Directory.Delete(discourseBaseDir, true);

			var sortedInstanceData = classData["DsDiscourseData"];
			if (sortedInstanceData.Count == 0)
				return;

			// ConstChartTempl and ChartMarkers are two lists owned by DsDiscourseData.
			var multiClassOutput = new Dictionary<string, SortedDictionary<string, XElement>>();
			Directory.CreateDirectory(discourseBaseDir);

			var guid = sortedInstanceData.Keys.First();
			var dataEl = sortedInstanceData.Values.First();

			// 1. Write out the DsDiscourseData instance in discourseBaseDir, but not the charts it owns.
			FileWriterService.WriteObject(mdc, classData, guidToClassMapping, discourseBaseDir, multiClassOutput, guid, new HashSet<string> { "Charts" });

			// 2. Each chart it owns needs to be written in its own subfolder of discourseBaseDir, a la texts.
			WritePropertyInFolders(mdc,
				classData, guidToClassMapping, multiClassOutput,
				discourseBaseDir,
				dataEl,
				"Charts", "Chart_", true);
		}

		internal static void RestoreOriginalFile(XmlWriter writer, XmlReaderSettings readerSettings, string multiFileDirRoot)
		{
			OldStyleDomainServices.RestoreFiles(writer, readerSettings, Path.Combine(multiFileDirRoot, Path.Combine(multiFileDirRoot, DiscourseRootFolder)));
		}

		private static void WritePropertyInFolders(MetadataCache mdc, IDictionary<string, SortedDictionary<string, XElement>> classData, IDictionary<string, string> guidToClassMapping, Dictionary<string, SortedDictionary<string, XElement>> multiClassOutput, string baseDir, XElement dataElement, string propertyName, string dirPrefix, bool appendGuid)
		{
			foreach (var guid in ObjectFinderServices.GetGuids(dataElement, propertyName))
			{
				multiClassOutput.Clear();

				var currentElement = ObjectFinderServices.RegisterDataInBoundedContext(classData, guidToClassMapping, multiClassOutput, guid);
				ObjectFinderServices.CollectAllOwnedObjects(mdc,
															classData, guidToClassMapping, multiClassOutput,
															currentElement,
															new HashSet<string>());

				// Write out data in a separate folder.
				var dirPath = Path.Combine(baseDir, dirPrefix);
				if (appendGuid)
					dirPath = Path.Combine(baseDir, dirPrefix + guid);
				if (!Directory.Exists(dirPath))
					Directory.CreateDirectory(dirPath);
				foreach (var kvp in multiClassOutput)
					FileWriterService.WriteSecondaryFile(Path.Combine(dirPath, kvp.Key + ".ClassData"), kvp.Value);
			}
			multiClassOutput.Clear();
		}
	}
}