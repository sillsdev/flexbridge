using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

/*
BC 1. ScrRefSystem (no owner)
	Books prop owns seq of ScrBookRef (which has all basic props).
	No other props.
	[Put all in one file in a subfolder of Scripture?]
*/
namespace FLEx_ChorusPlugin.Contexts.Scripture
{
	internal static class ScriptureReferenceSystemBoundedContextService
	{
		internal static void NestContext(XmlReaderSettings readerSettings, string baseDirectory,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping,
			HashSet<string> skipWriteEmptyClassFiles)
		{
			SortedDictionary<string, XElement> sortedInstanceData;
			if (!classData.TryGetValue("ScrRefSystem", out sortedInstanceData))
				return;
			if (sortedInstanceData.Count == 0)
				return;

			if (!Directory.Exists(baseDirectory))
				Directory.CreateDirectory(baseDirectory);

			var refSystem = sortedInstanceData.First().Value;

			CmObjectNestingService.NestObject(false, refSystem,
				new Dictionary<string, HashSet<string>>(),
				classData,
				guidToClassMapping);

			var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
				new XElement(SharedConstants.ScriptureReferenceSystem, refSystem));

			FileWriterService.WriteNestedFile(Path.Combine(baseDirectory, SharedConstants.ScriptureReferenceSystemFilename), readerSettings, doc);

			ObjectFinderServices.ProcessLists(classData, skipWriteEmptyClassFiles, new HashSet<string> { "ScrRefSystem", "ScrBookRef" });
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string scriptureBaseDir)
		{
			if (!Directory.Exists(scriptureBaseDir))
				return; // Nothing to do.

			var pathname = Path.Combine(scriptureBaseDir, SharedConstants.ScriptureReferenceSystemFilename);
			var doc = XDocument.Load(pathname);
			CmObjectFlatteningService.FlattenObject(
				pathname,
				sortedData,
				doc.Element(SharedConstants.ScriptureReferenceSystem).Element("ScrRefSystem"),
				null); // Not owned.
		}

		internal static void RemoveBoundedContextData(string scriptureBaseDir)
		{
			if (!Directory.Exists(scriptureBaseDir))
				return;

			var refSysPathname = Path.Combine(scriptureBaseDir, SharedConstants.ScriptureReferenceSystemFilename);
			if (File.Exists(refSysPathname))
				File.Delete(refSysPathname);

			// Scripture domain does it all.
			//FileWriterService.RemoveEmptyFolders(scriptureBaseDir, true);
		}
	}
}