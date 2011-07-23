using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHanders.FieldWorks;

namespace FieldWorksBridge.Infrastructure
{
	/// <summary>
	/// Read/Write the Reversal bounded context.
	///
	/// The Reversal Index instances, including all they own, need to then be removed from 'classData',
	/// as that stuff will be stored elsewhere.
	///
	/// Each ReversalIndex instance will be in its own folder, along with everything it owns (nested ownership as well).
	/// The folder pattern is:
	/// DataFiles\Reversals\foo, where foo is the WritingSystem property of a ReversalIndex.
	/// </summary>
	internal static class ReversalBoundedContextService
	{
		private const string ReversalRootFolder = "Reversals";

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
#if USEXELEMENTS
			SortedDictionary<string, XElement> sortedInstanceData;
#else
			SortedDictionary<string, byte[]> sortedInstanceData;
#endif
			if (!classData.TryGetValue("ReversalIndex", out sortedInstanceData))
				return;

			var reversalBaseDir = Path.Combine(multiFileDirRoot, ReversalRootFolder);
			if (!Directory.Exists(reversalBaseDir))
				Directory.CreateDirectory(reversalBaseDir);

#if USEXELEMENTS
			var srcDataCopy = new SortedDictionary<string, XElement>(sortedInstanceData);
#else
			var srcDataCopy = new SortedDictionary<string, byte[]>(sortedInstanceData);
#endif
			foreach (var reversalIndexKvp in srcDataCopy)
			{
#if USEXELEMENTS
				var multiClassOutput = new Dictionary<string, SortedDictionary<string, XElement>>();
				var revIndex = reversalIndexKvp.Value;
#else
				var multiClassOutput = new Dictionary<string, SortedDictionary<string, byte[]>>();
				var revIndex = XElement.Parse(MultipleFileServices.Utf8.GetString(reversalIndexKvp.Value));
#endif

// ReSharper disable PossibleNullReferenceException
				var ws = revIndex.Element("WritingSystem").Element("Uni").Value;
// ReSharper restore PossibleNullReferenceException
				var reversalDir = Path.Combine(reversalBaseDir, ws);
				if (!Directory.Exists(reversalDir))
					Directory.CreateDirectory(reversalDir);

#if USEXELEMENTS
				FileWriterService.WriteObject(mdc, classData, guidToClassMapping, reversalDir, readerSettings, multiClassOutput, reversalIndexKvp.Key, new HashSet<string>());
#else
				FileWriterService.WriteObject(mdc, classData, guidToClassMapping, reversalDir, readerSettings, multiClassOutput, reversalIndexKvp.Key, new HashSet<string>());
#endif
			}

#if USEXELEMENTS
			ObjectFinderServices.ProcessLists(classData, skipWriteEmptyClassFiles, new HashSet<string> { "ReversalIndex", "ReversalIndexEntry" });
#else
			ObjectFinderServices.ProcessLists(classData, skipWriteEmptyClassFiles, new HashSet<string> { "ReversalIndex", "ReversalIndexEntry" });
#endif
		}

		internal static void RestoreOriginalFile(XmlWriter writer, XmlReaderSettings readerSettings, string multiFileDirRoot)
		{
			FileWriterService.RestoreFiles(writer, readerSettings, Path.Combine(multiFileDirRoot, Path.Combine(multiFileDirRoot, ReversalRootFolder)));
		}
	}
}
