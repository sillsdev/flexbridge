using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Contexts.Linguistics.Reversals
{
	/// <summary>
	/// Read/Write/Delete the Reversal bounded context.
	///
	/// The Reversal Index instances, including all they own, need to then be removed from 'classData',
	/// as that stuff will be stored elsewhere.
	///
	/// Each ReversalIndex instance will be in its own file, along with everything it owns (nested ownership as well).
	/// The pattern is:
	/// Linguistics\Reversals\foo.reversal, where foo.reversal is the Reversal Index file and 'foo' is the WritingSystem property of the ReversalIndex.
	///
	/// The output file for each will be:
	/// <reversal>
	///		<ReversalIndex>
	/// 1. The "Entries" element's contents will be relocated after the "ReversalIndex" element.
	/// 2. All other owned stuff will be nested here.
	///		</ReversalIndex>
	///		<ReversalInxEntry>Nested for what they own.</ReversalInxEntry>
	///		...
	///		<ReversalInxEntry>Nested for what they own.</ReversalInxEntry>
	/// </reversal>
	/// </summary>
	internal static class ReversalBoundedContextService
	{
		private const string ReversalRootFolder = "Reversals";

		internal static void NestContext(XmlReaderSettings readerSettings, string baseDirectory,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping,
			Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache,
			HashSet<string> skipWriteEmptyClassFiles)
		{
			SortedDictionary<string, XElement> sortedInstanceData;
			if (!classData.TryGetValue("ReversalIndex", out sortedInstanceData))
				return;

			var lexDb = classData["LexDb"].First().Value;
			lexDb.Element("ReversalIndexes").RemoveNodes(); // Restored in FlattenContext method.

			var reversalDir = Path.Combine(baseDirectory, ReversalRootFolder);
			if (!Directory.Exists(reversalDir))
				Directory.CreateDirectory(reversalDir);

			var srcDataCopy = new SortedDictionary<string, XElement>(sortedInstanceData);
			foreach (var reversalIndexKvp in srcDataCopy)
			{
				var revIndex = reversalIndexKvp.Value;

				var ws = revIndex.Element("WritingSystem").Element("Uni").Value;
				var reversalFilename = ws + ".reversal";

				CmObjectNestingService.NestObject(revIndex,
					new Dictionary<string, HashSet<string>>(),
					classData,
					interestingPropertiesCache,
					guidToClassMapping);

				// Remove 'ownerguid'.
				revIndex.Attribute(SharedConstants.OwnerGuid).Remove();

				var entriesElement = revIndex.Element("Entries");
				var root = new XElement("Reversal",
					new XElement(SharedConstants.Header, revIndex));
				root.Add(entriesElement.Elements()); // NB: These were already sorted, why up in MultipleFileServices::CacheDataRecord, since "Entries" is a collection prop.
				entriesElement.RemoveNodes();
				var fullRevObject = new XDocument( new XDeclaration("1.0", "utf-8", "yes"),
					root);

				FileWriterService.WriteNestedFile(Path.Combine(reversalDir, reversalFilename), readerSettings, fullRevObject);
			}

			ObjectFinderServices.ProcessLists(classData, skipWriteEmptyClassFiles, new HashSet<string> { "ReversalIndex", "ReversalIndexEntry" });
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache,
			string linguisticsBaseDir)
		{
			if (!Directory.Exists(linguisticsBaseDir))
				return; // Nothing to do.

			var lexDb = highLevelData["LexDb"];
			var sortedRevs = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			foreach (var reversalDoc in Directory.GetFiles(Path.Combine(linguisticsBaseDir, ReversalRootFolder), "*.reversal", SearchOption.TopDirectoryOnly)
// ReSharper disable ConvertClosureToMethodGroup
				.Select(reversalPathname => XDocument.Load(reversalPathname)))
// ReSharper restore ConvertClosureToMethodGroup
			{
				// Put entries back into index's Entries element.
				var root = reversalDoc.Element("Reversal");
				var header = root.Element(SharedConstants.Header);
				var revIdx = header.Element("ReversalIndex");
				revIdx.Element("Entries").Add(root.Elements("ReversalIndexEntry"));
				CmObjectFlatteningService.FlattenObject(sortedData,
					interestingPropertiesCache,
					revIdx,
					lexDb.Attribute(SharedConstants.GuidStr).Value); // Restore 'ownerguid' to indices.
				var revIdxGuid = revIdx.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
				sortedRevs.Add(revIdxGuid, new XElement(SharedConstants.Objsur, new XAttribute(SharedConstants.GuidStr, revIdxGuid), new XAttribute("t", "o")));
			}

			// Restore lexDb ReversalIndexes property in sorted order.
			if (sortedRevs.Count > 0)
			{
				var reversalsOwningProp = highLevelData["LexDb"].Element("ReversalIndexes");
				foreach (var sortedRev in sortedRevs.Values)
					reversalsOwningProp.Add(sortedRev);
			}
		}

		internal static void RemoveBoundedContextData(string linguisticsBase)
		{
			if (!Directory.Exists(linguisticsBase))
				return;
			var reversalDir = Path.Combine(linguisticsBase, ReversalRootFolder);
			if (!Directory.Exists(reversalDir))
				return;
			foreach (var reversalPathname in Directory.GetFiles(reversalDir, "*.reversal", SearchOption.TopDirectoryOnly))
				File.Delete(reversalPathname);
			FileWriterService.RemoveEmptyFolders(reversalDir, true);
		}
	}
}
