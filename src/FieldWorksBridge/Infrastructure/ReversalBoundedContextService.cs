﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace FieldWorksBridge.Infrastructure
{
	/// <summary>
	/// Read/Write the Reversal bounded context.
	///
	/// The Reversal Index instances, including all they own, need to then be removed from 'classData',
	/// as that stuff will be stored elsewhere.
	///
	/// Each ReversalIndex instance will be in its own file, along with everything it owns (nested ownership as well).
	/// The pattern is:
	/// Linguistics\Reversals\foo.reversal, where foo.reversal is the Reversal Index file and 'foo' is the WritingSystem property of the ReversalIndex.
	/// </summary>
	internal static class ReversalBoundedContextService
	{
		private const string ReversalRootFolder = "Reversals";

		internal static void ExtractBoundedContexts(XmlReaderSettings readerSettings, string baseDirectory,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping,
			Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache,
			HashSet<string> skipWriteEmptyClassFiles)
		{
			SortedDictionary<string, XElement> sortedInstanceData;
			if (!classData.TryGetValue("ReversalIndex", out sortedInstanceData))
				return;

			var reversalDir = Path.Combine(baseDirectory, ReversalRootFolder);
			if (!Directory.Exists(reversalDir))
				Directory.CreateDirectory(reversalDir);

			var srcDataCopy = new SortedDictionary<string, XElement>(sortedInstanceData);
			foreach (var reversalIndexKvp in srcDataCopy)
			{
				var revIndex = reversalIndexKvp.Value;

// ReSharper disable PossibleNullReferenceException
				var ws = revIndex.Element("WritingSystem").Element("Uni").Value;
				var reversalFilename = ws + ".reversal";
// ReSharper restore PossibleNullReferenceException

				CmObjectNestingService.NestObject(revIndex,
					new Dictionary<string, HashSet<string>>(),
					classData,
					interestingPropertiesCache,
					guidToClassMapping);

				FileWriterService.WriteNestedFile(Path.Combine(reversalDir, reversalFilename), readerSettings, revIndex, "Reversal");
			}

			ObjectFinderServices.ProcessLists(classData, skipWriteEmptyClassFiles, new HashSet<string> { "ReversalIndex", "ReversalIndexEntry" });
		}

		internal static void RestoreOriginalFile(XmlWriter writer, XmlReaderSettings readerSettings, string multiFileDirRoot)
		{
			FileWriterService.RestoreFiles(writer, readerSettings, Path.Combine(multiFileDirRoot, Path.Combine(multiFileDirRoot, ReversalRootFolder)));
		}

		public static void RemoveBoundedContextData(string pathRoot)
		{
			var reversalDir = Path.Combine(pathRoot, Path.Combine("Linguistics", ReversalRootFolder));
			foreach (var reversalPathname in Directory.GetFiles(reversalDir, "*.reversal", SearchOption.TopDirectoryOnly))
				File.Delete(reversalPathname);
			FileWriterService.RemoveEmptyFolders(reversalDir, true);
		}

		public static IEnumerable<XElement> FlattenContext(Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache, string linguisticsBaseDir)
		{
			var result = new List<XElement>(50000);
// ReSharper disable PossibleNullReferenceException
			foreach (var reversalDoc in Directory.GetFiles(Path.Combine(linguisticsBaseDir, ReversalRootFolder), "*.reversal", SearchOption.TopDirectoryOnly)
				.Select(reversalPathname => XDocument.Load(reversalPathname)))
			{
				result.AddRange(CmObjectFlatteningService.FlattenObject(interestingPropertiesCache, reversalDoc.Element("Reversal").Element("ReversalIndex"), null));
			}
// ReSharper restore PossibleNullReferenceException
			return result;
		}
	}
}
