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

		internal static void NestContext(string linguisticsBaseDir, IDictionary<string, SortedDictionary<string, XElement>> classData, Dictionary<string, string> guidToClassMapping)
		{
			var lexiconDir = Path.Combine(linguisticsBaseDir, SharedConstants.Lexicon);
			if (!Directory.Exists(lexiconDir))
				Directory.CreateDirectory(lexiconDir);

			var lexDb = classData[LexDb].First().Value; // It has had its "ReversalIndexes" property processed already, so it should be an empty element.
			// lexDb is owned by the LP in its LexDb property, so remove its <objsur> node.
			var langProjElement = classData["LangProject"].Values.First();
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
			var root = new XElement(SharedConstants.Lexicon);
			var header = new XElement(SharedConstants.Header);
			root.Add(header);
			header.Add(lexDb);
			CmObjectNestingService.NestObject(false, lexDb,
				new Dictionary<string, HashSet<string>>(),
				classData,
				guidToClassMapping);

			var sortedInstanceData = classData[SharedConstants.LexEntry];
			if (sortedInstanceData.Count == 0)
			{
				// Add a dummy LexEntry, so fast xml splitter won't choke.
				// Restore will remove it, if found.
				root.Add(new XElement(SharedConstants.LexEntry, new XAttribute(SharedConstants.GuidStr, Guid.Empty.ToString().ToLowerInvariant())));
			}
			else
			{
				var srcDataCopy = new SortedDictionary<string, XElement>(sortedInstanceData);
				foreach (var entry in srcDataCopy.Values)
				{
					CmObjectNestingService.NestObject(false, entry,
						new Dictionary<string, HashSet<string>>(),
						classData,
						guidToClassMapping);
					root.Add(entry);
				}
			}

			FileWriterService.WriteNestedFile(Path.Combine(lexiconDir, SharedConstants.LexiconFilename), root);
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string linguisticsBaseDir)
		{
			var lexiconDir = Path.Combine(linguisticsBaseDir, SharedConstants.Lexicon);
			if (!Directory.Exists(lexiconDir))
				return;

			var langProjElement = highLevelData["LangProject"];
			var langProjGuid = langProjElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
			var lexDbPathname = Path.Combine(lexiconDir, SharedConstants.LexiconFilename);
			var lexDbDoc = XDocument.Load(lexDbPathname);
			var rootLexDbDoc = lexDbDoc.Root;
			var headerLexDbDoc = rootLexDbDoc.Element(SharedConstants.Header);
			var lexDb = headerLexDbDoc.Element(LexDb);
			highLevelData[LexDb] = lexDb; // Let MorphAndSyn access it to put "MorphTypes" back into lexDb.
			// Add LexDb <objsur> element to LP.
			BaseDomainServices.RestoreObjsurElement(langProjElement, LexDb, lexDb);
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
						// In LP. Restore the <objsur> element in LP, and then flatten the list by itself.
						BaseDomainServices.RestoreObjsurElement(langProjElement, listFilenameSansExtension, listElement);
						// Flatten the LP list by itself.
						CmObjectFlatteningService.FlattenObject(
							listPathname,
							sortedData,
							listElement,
							langProjGuid); // Restore 'ownerguid' to list.
						break;
				}
			}
			// Flatten lexDb.
			CmObjectFlatteningService.FlattenObject(
				lexDbPathname,
				sortedData,
				lexDb,
				langProjGuid); // Restore 'ownerguid' to LexDb.

			// Flatten all entries in root of lexDbDoc. (EXCEPT if it has a guid of Guid.Empty, in which case, just ignore it, and it will go away.)
			foreach (var entryElement in rootLexDbDoc.Elements(SharedConstants.LexEntry).TakeWhile(entryElement => entryElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant() != Guid.Empty.ToString().ToLowerInvariant()))
			{
				CmObjectFlatteningService.FlattenObject(
					lexDbPathname,
					sortedData,
					entryElement,
					null); // Entries are not owned.
			}
		}

		private static void NestLists(IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping,
			IDictionary<string, XElement> posLists,
			string lexiconRootDir,
			XContainer owningElement,
			IEnumerable<string> propNames)
		{
			var exceptions = new Dictionary<string, HashSet<string>>();
			foreach (var propName in propNames)
			{
				var listPropElement = owningElement.Element(propName);
				if (listPropElement == null || !listPropElement.HasElements)
					continue;

				var root = new XElement(propName);
				var listElement = posLists[listPropElement.Elements().First().Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant()];
				CmObjectNestingService.NestObject(false,
					listElement,
					exceptions,
					classData,
					guidToClassMapping);
				listPropElement.RemoveNodes(); // Remove the single list objsur element.
				root.Add(listElement);

				FileWriterService.WriteNestedFile(Path.Combine(lexiconRootDir, propName + "." + SharedConstants.List), root);
			}
		}
	}
}