using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Contexts.Linguistics.Lexicon
{
	internal static class LexiconBoundedContextService
	{
		private const string LexDb = "LexDb";

		internal static void NestContext(string linguisticsBaseDir,
			IDictionary<string, SortedDictionary<string, byte[]>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			var lexiconDir = Path.Combine(linguisticsBaseDir, SharedConstants.Lexicon);
			if (!Directory.Exists(lexiconDir))
				Directory.CreateDirectory(lexiconDir);

			var lexDb = XElement.Parse(SharedConstants.Utf8.GetString(classData[LexDb].First().Value)); // It has had its "ReversalIndexes" property processed already, so it should be an empty element.
			// lexDb is owned by the LP in its LexDb property, so remove its <objsur> node.
			var langProjElement = XElement.Parse(SharedConstants.Utf8.GetString(classData[SharedConstants.LangProject].Values.First()));
			langProjElement.Element(LexDb).RemoveNodes();

			// Nest each CmPossibilityList owned by LexDb.
			var lists = classData[SharedConstants.CmPossibilityList];
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
			var header = new XElement(SharedConstants.Header);
			header.Add(lexDb);

			var sortedEntryInstanceData = classData[SharedConstants.LexEntry];
			var nestedData = new SortedDictionary<string, string>();
			if (sortedEntryInstanceData.Count > 0)
			{
				var srcDataCopy = new SortedDictionary<string, byte[]>(sortedEntryInstanceData);
				foreach (var entry in srcDataCopy.Values)
				{
					var entryElement = XElement.Parse(SharedConstants.Utf8.GetString(entry));
					CmObjectNestingService.NestObject(false, entryElement,
													  classData,
													  guidToClassMapping);
					nestedData.Add(entryElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant(), entryElement.ToString());
				}
			}

			var buckets = FileWriterService.CreateEmptyBuckets(10);
			FileWriterService.FillBuckets(buckets, nestedData);

			for (var i = 0; i < buckets.Count; ++i)
			{
				var root = new XElement(SharedConstants.Lexicon);
				if (i == 0 && header.HasElements)
					root.Add(header);
				var currentBucket = buckets[i];
				foreach (var entryString in currentBucket.Values)
					root.Add(XElement.Parse(entryString));
				FileWriterService.WriteNestedFile(PathnameForBucket(lexiconDir, i), root);
			}

			classData[SharedConstants.LangProject][langProjElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant()] = SharedConstants.Utf8.GetBytes(langProjElement.ToString());
		}

		internal static string PathnameForBucket(string inventoryDir, int bucket)
		{
			return Path.Combine(inventoryDir, string.Format("{0}_{1}{2}.{3}", SharedConstants.Lexicon, bucket >= 9 ? "" : "0", bucket + 1, SharedConstants.Lexdb));
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string linguisticsBaseDir)
		{
			var lexiconDir = Path.Combine(linguisticsBaseDir, SharedConstants.Lexicon);
			if (!Directory.Exists(lexiconDir))
				return;

			var langProjElement = highLevelData[SharedConstants.LangProject];
			var langProjGuid = langProjElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
			var lexDbPathnames = new List<string>(Directory.GetFiles(lexiconDir, string.Format("{0}_??.{1}", SharedConstants.Lexicon, SharedConstants.Lexdb), SearchOption.TopDirectoryOnly));
			lexDbPathnames.Sort(StringComparer.InvariantCultureIgnoreCase);
			foreach (var lexDbPathname in lexDbPathnames)
			{
				var lexDbDoc = XDocument.Load(lexDbPathname);
				var rootLexDbDoc = lexDbDoc.Root;
				var headerLexDbDoc = rootLexDbDoc.Element(SharedConstants.Header);
				if (headerLexDbDoc != null)
				{
					var lexDb = headerLexDbDoc.Element(LexDb);
					highLevelData[LexDb] = lexDb; // Let MorphAndSyn access it to put "MorphTypes" back into lexDb.
					foreach (var listPathname in Directory.GetFiles(lexiconDir, "*.list", SearchOption.TopDirectoryOnly))
					{
						var listDoc = XDocument.Load(listPathname);
						var listElement = listDoc.Root.Element(SharedConstants.CmPossibilityList);
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
						langProjGuid, langProjElement, LexDb); // Restore 'ownerguid' to LexDb.
				}

				// Flatten all entries in root of lexDbDoc. (EXCEPT if it has a guid of Guid.Empty, in which case, just ignore it, and it will go away.)
				foreach (var entryElement in rootLexDbDoc.Elements(SharedConstants.LexEntry)
					.Where(element => element.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant() != SharedConstants.EmptyGuid))
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
				var listElement = XElement.Parse(SharedConstants.Utf8.GetString(posLists[listPropElement.Elements().First().Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant()]));
				CmObjectNestingService.NestObject(false,
					listElement,
					classData,
					guidToClassMapping);
				listPropElement.RemoveNodes(); // Remove the single list objsur element.
				root.Add(listElement);

				FileWriterService.WriteNestedFile(Path.Combine(lexiconRootDir, propName + "." + SharedConstants.List), root);
			}
		}
	}
}