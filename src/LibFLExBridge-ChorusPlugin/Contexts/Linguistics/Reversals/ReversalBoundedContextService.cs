// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibFLExBridgeChorusPlugin.DomainServices;

namespace LibFLExBridgeChorusPlugin.Contexts.Linguistics.Reversals
{
	/// <summary>
	/// Read/Write/Delete the Reversal bounded context.
	///
	/// The Reversal Index instances, including all they own, need to then be removed from 'classData',
	/// as that stuff will be stored elsewhere.
	///
	/// Each ReversalIndex instance will be in its own file, along with everything it owns (nested ownership, except the pos list it owns, which goes into its own file).
	/// The pattern is:
	/// Linguistics\Reversals\foo\foo.reversal, where foo.reversal is the Reversal Index file and 'foo' is the WritingSystem property of the ReversalIndex.
	/// Linguistics\Reversals\foo\foo-PartsOfSpeech.list
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

		internal static void NestContext(string linguisticsBaseDir,
			IDictionary<string, XElement> wellUsedElements,
			IDictionary<string, SortedDictionary<string, byte[]>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			var lexDb = wellUsedElements[FlexBridgeConstants.LexDb];
			if (lexDb == null)
				return; // No LexDb, then there can be no reversals.

			SortedDictionary<string, byte[]> sortedInstanceData = classData["ReversalIndex"];
			if (sortedInstanceData.Count == 0)
				return; // no reversals, as in Lela-Teli-3.

			lexDb.Element("ReversalIndexes").RemoveNodes(); // Restored in FlattenContext method.

			var reversalDir = Path.Combine(linguisticsBaseDir, ReversalRootFolder);
			if (!Directory.Exists(reversalDir))
				Directory.CreateDirectory(reversalDir);

			var srcDataCopy = new SortedDictionary<string, byte[]>(sortedInstanceData);
			foreach (var reversalIndexKvp in srcDataCopy)
			{
				var revIndexElement = LibFLExBridgeUtilities.CreateFromBytes(reversalIndexKvp.Value);
				var ws = revIndexElement.Element("WritingSystem").Element("Uni").Value;
				var revIndexDir = Path.Combine(reversalDir, ws);
				if (!Directory.Exists(revIndexDir))
					Directory.CreateDirectory(revIndexDir);

				var reversalFilename = ws + ".reversal";

				// Break out ReversalIndex's PartsOfSpeech(CmPossibilityList OA) and write in its own .list file.
				FileWriterService.WriteNestedListFileIfItExists(
					classData, guidToClassMapping,
					revIndexElement, FlexBridgeConstants.PartsOfSpeech,
					Path.Combine(revIndexDir, ws + "-" + FlexBridgeConstants.PartsOfSpeechFilename));

				CmObjectNestingService.NestObject(false, revIndexElement,
					classData,
					guidToClassMapping);

				var entriesElement = revIndexElement.Element("Entries");
				var root = new XElement("Reversal",
					new XElement(FlexBridgeConstants.Header, revIndexElement));
				if (entriesElement != null && entriesElement.Elements().Any())
				{
					root.Add(entriesElement.Elements());
						// NB: These were already sorted, way up in MultipleFileServices::CacheDataRecord, since "Entries" is a collection prop.
					entriesElement.RemoveNodes();
				}

				FileWriterService.WriteNestedFile(Path.Combine(revIndexDir, reversalFilename), root);
			}
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string linguisticsBaseDir)
		{
			var reversalDir = Path.Combine(linguisticsBaseDir, ReversalRootFolder);
			if (!Directory.Exists(reversalDir))
				return;

			var lexDb = highLevelData[FlexBridgeConstants.LexDb];
			var sortedRevs = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			var unlovedFolders = new HashSet<string>();
			foreach (var revIndexDirectoryName in Directory.GetDirectories(reversalDir))
			{
				var dirInfo = new DirectoryInfo(revIndexDirectoryName);
				var ws = dirInfo.Name;
				var reversalPathname = Path.Combine(revIndexDirectoryName, ws + "." + FlexBridgeConstants.Reversal);
				if (!File.Exists(reversalPathname))
				{
					// If a reversal is deleted but there were ChorusNotes associated with it the directory might be
					// here without any reversal files inside it.
					unlovedFolders.Add(revIndexDirectoryName);
					continue;
				}

				var reversalDoc = XDocument.Load(reversalPathname);

				// Put entries back into index's Entries element.
				var root = reversalDoc.Element("Reversal");
				var header = root.Element(FlexBridgeConstants.Header);
				var revIdxElement = header.Element("ReversalIndex");

				// Restore POS list, if it exists.
				var catPathname = Path.Combine(revIndexDirectoryName, ws + "-" + FlexBridgeConstants.PartsOfSpeechFilename);
				if (File.Exists(catPathname))
				{
					var catListDoc = XDocument.Load(catPathname);
					BaseDomainServices.RestoreElement(
						catPathname,
						sortedData,
						revIdxElement, FlexBridgeConstants.PartsOfSpeech,
						catListDoc.Root.Element(FlexBridgeConstants.CmPossibilityList)); // Owned elment.
				}

				// Put all records back in ReversalIndex, before sort and restore.
				// EXCEPT, if there is only one of them and it is guid.Empty, then skip it
				var sortedRecords = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
				foreach (var recordElement in root.Elements("ReversalIndexEntry")
					.Where(element => element.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant() != FlexBridgeConstants.EmptyGuid))
				{
					// Add it to Records property of revIdxElement, BUT in sorted order, below, and then flatten dnMainElement.
					sortedRecords.Add(recordElement.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant(), recordElement);
				}

				if (sortedRecords.Count > 0)
				{
					var recordsElementOwningProp = revIdxElement.Element("Entries")
						?? CmObjectFlatteningService.AddNewPropertyElement(revIdxElement, "Entries");

					foreach (var sortedChartElement in sortedRecords.Values)
						recordsElementOwningProp.Add(sortedChartElement);
				}
				CmObjectFlatteningService.FlattenOwnedObject(reversalPathname, sortedData, revIdxElement,
					lexDb.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant(), sortedRevs); // Restore 'ownerguid' to indices.
			}

			foreach (var unlovedFolder in unlovedFolders)
				Directory.Delete(unlovedFolder, true);

			// Restore lexDb ReversalIndexes property in sorted order.
			if (sortedRevs.Count == 0)
				return;

			var reversalsOwningProp = lexDb.Element("ReversalIndexes") ?? CmObjectFlatteningService.AddNewPropertyElement(lexDb, "ReversalIndexes");
			foreach (var sortedRev in sortedRevs.Values)
				reversalsOwningProp.Add(sortedRev);
		}
	}
}
