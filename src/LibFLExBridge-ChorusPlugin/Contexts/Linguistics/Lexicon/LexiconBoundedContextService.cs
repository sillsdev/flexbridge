// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibFLExBridgeChorusPlugin.DomainServices;

namespace LibFLExBridgeChorusPlugin.Contexts.Linguistics.Lexicon
{
	internal static class LexiconBoundedContextService
	{
		internal static void NestContext(string linguisticsBaseDir,
			IDictionary<string, XElement> wellUsedElements,
			IDictionary<string, SortedDictionary<string, byte[]>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			var lexiconDir = Path.Combine(linguisticsBaseDir, FlexBridgeConstants.Lexicon);
			if (!Directory.Exists(lexiconDir))
				Directory.CreateDirectory(lexiconDir);

			var lexDb = wellUsedElements[FlexBridgeConstants.LexDb]; // It has had its "ReversalIndexes" property processed already, so it should be an empty element.
			// lexDb is owned by the LP in its LexDb property, so remove its <objsur> node.
			var langProjElement = wellUsedElements[FlexBridgeConstants.LangProject];
			langProjElement.Element(FlexBridgeConstants.LexDb).RemoveNodes();

			// Nest each CmPossibilityList owned by LexDb.
			var lists = classData[FlexBridgeConstants.CmPossibilityList];
			NestLists(classData, guidToClassMapping, lists, lexiconDir, lexDb,
					  new List<string>
						{
							"SenseTypes",
							"UsageTypes",
							"DomainTypes",
							// Moved to Morph & Syn, as per AndyB. "MorphTypes",
							"References",
							"VariantEntryTypes",
							"ComplexEntryTypes",
							"Languages",
							"DialectLabels",
							"ExtendedNoteTypes",
							"PublicationTypes"
						});

			// Nest SemanticDomainList and AffixCategories props of LangProject.
			NestLists(classData, guidToClassMapping, lists, lexiconDir, langProjElement,
					  new List<string>
						{
							"SemanticDomainList",
							"AffixCategories"
						});

			// The LexDb object will go into the <header>, and will still nest these owning props: Appendixes, Introduction, and Resources (plus its basic props).
			// All of the lexical entries will then go in as siblings of, but after, the <header> element.
			// At this point LexDb is ready to go into the <header>.
			CmObjectNestingService.NestObject(false, lexDb,
				classData,
				guidToClassMapping);
			var header = new XElement(FlexBridgeConstants.Header);
			header.Add(lexDb);

			var sortedEntryInstanceData = classData[FlexBridgeConstants.LexEntry];
			var nestedData = new SortedDictionary<string, XElement>();
			if (sortedEntryInstanceData.Count > 0)
			{
				var srcDataCopy = new SortedDictionary<string, byte[]>(sortedEntryInstanceData);
				foreach (var entry in srcDataCopy.Values)
				{
					var entryElement = LibFLExBridgeUtilities.CreateFromBytes(entry);
					CmObjectNestingService.NestObject(false, entryElement,
													  classData,
													  guidToClassMapping);
					nestedData.Add(entryElement.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant(), entryElement);
				}
			}

			var buckets = FileWriterService.CreateEmptyBuckets(10);
			FileWriterService.FillBuckets(buckets, nestedData);

			for (var i = 0; i < buckets.Count; ++i)
			{
				var root = new XElement(FlexBridgeConstants.Lexicon);
				if (i == 0 && header.HasElements)
					root.Add(header);
				var currentBucket = buckets[i];
				foreach (var entry in currentBucket.Values)
					root.Add(entry);
				FileWriterService.WriteNestedFile(PathnameForBucket(lexiconDir, i), root);
			}
		}

		internal static string PathnameForBucket(string inventoryDir, int bucket)
		{
			return Path.Combine(inventoryDir, string.Format("{0}_{1}{2}.{3}", FlexBridgeConstants.Lexicon, bucket >= 9 ? "" : "0", bucket + 1, FlexBridgeConstants.Lexdb));
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string linguisticsBaseDir)
		{
			var lexiconDir = Path.Combine(linguisticsBaseDir, FlexBridgeConstants.Lexicon);
			if (!Directory.Exists(lexiconDir))
				return;

			var langProjElement = highLevelData[FlexBridgeConstants.LangProject];
			var langProjGuid = langProjElement.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant();
			var lexDbPathnames = new List<string>(Directory.GetFiles(lexiconDir, string.Format("{0}_??.{1}", FlexBridgeConstants.Lexicon, FlexBridgeConstants.Lexdb), SearchOption.TopDirectoryOnly));
			lexDbPathnames.Sort(StringComparer.InvariantCultureIgnoreCase);
			foreach (var lexDbPathname in lexDbPathnames)
			{
				var lexDbDoc = XDocument.Load(lexDbPathname);
				var rootLexDbDoc = lexDbDoc.Root;
				var headerLexDbDoc = rootLexDbDoc.Element(FlexBridgeConstants.Header);
				if (headerLexDbDoc != null)
				{
					var lexDb = headerLexDbDoc.Element(FlexBridgeConstants.LexDb);
					highLevelData[FlexBridgeConstants.LexDb] = lexDb; // Let MorphAndSyn access it to put "MorphTypes" back into lexDb.
					foreach (var listPathname in Directory.GetFiles(lexiconDir, "*.list", SearchOption.TopDirectoryOnly))
					{
						var listDoc = XDocument.Load(listPathname);
						var listElement = listDoc.Root.Element(FlexBridgeConstants.CmPossibilityList);
						var listFilenameSansExtension = Path.GetFileNameWithoutExtension(listPathname);
						switch (listFilenameSansExtension)
						{
							default:
								// In LexDB. Just add the list to the owning prop, and let it get flattened, normally.
								lexDb.Element(listFilenameSansExtension).Add(listElement);
								break;
							case "SemanticDomainList":
							case "AffixCategories":
								// Flatten the LP list by itself, and add an appropriate surrogate.
								CmObjectFlatteningService.FlattenOwnedObject(
									listPathname,
									sortedData,
									listElement,
									langProjGuid, langProjElement, listFilenameSansExtension); // Restore 'ownerguid' to list.
								break;
						}
					}
					// Flatten lexDb.
					CmObjectFlatteningService.FlattenOwnedObject(
						lexDbPathname,
						sortedData,
						lexDb,
						langProjGuid, langProjElement, FlexBridgeConstants.LexDb); // Restore 'ownerguid' to LexDb.
				}

				// Flatten all entries in root of lexDbDoc. (EXCEPT if it has a guid of Guid.Empty, in which case, just ignore it, and it will go away.)
				foreach (var entryElement in rootLexDbDoc.Elements(FlexBridgeConstants.LexEntry)
					.Where(element => element.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant() != FlexBridgeConstants.EmptyGuid))
				{
					CmObjectFlatteningService.FlattenOwnerlessObject(
						lexDbPathname,
						sortedData,
						entryElement);
				}
			}
		}

		private static void NestLists(IDictionary<string, SortedDictionary<string, byte[]>> classData,
			Dictionary<string, string> guidToClassMapping,
			IDictionary<string, byte[]> posLists,
			string lexiconRootDir,
			XContainer owningElement,
			IEnumerable<string> propNames)
		{
			foreach (var propName in propNames)
			{
				var listPropElement = owningElement.Element(propName);
				if (listPropElement == null || !listPropElement.HasElements)
					continue;

				var root = new XElement(propName);
				var listElement = LibFLExBridgeUtilities.CreateFromBytes(posLists[listPropElement.Elements().First().Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant()]);
				CmObjectNestingService.NestObject(false,
					listElement,
					classData,
					guidToClassMapping);
				listPropElement.RemoveNodes(); // Remove the single list objsur element.
				root.Add(listElement);

				FileWriterService.WriteNestedFile(Path.Combine(lexiconRootDir, propName + "." + FlexBridgeConstants.List), root);
			}
		}
	}
}