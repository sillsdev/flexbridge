using System.Collections.Generic;
using System.IO;
using System.Linq;
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
		internal static void NestContext(string baseDirectory,
			IDictionary<string, SortedDictionary<string, string>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			var sortedInstanceData = classData["ScrRefSystem"];
			if (sortedInstanceData.Count == 0)
				return;
			if (!Directory.Exists(baseDirectory))
				Directory.CreateDirectory(baseDirectory);

			var refSystem = XElement.Parse(sortedInstanceData.First().Value);

			CmObjectNestingService.NestObject(false, refSystem,
				classData,
				guidToClassMapping);

			FileWriterService.WriteNestedFile(Path.Combine(baseDirectory, SharedConstants.ScriptureReferenceSystemFilename), new XElement(SharedConstants.ScriptureReferenceSystem, refSystem));
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string scriptureBaseDir)
		{
			if (!Directory.Exists(scriptureBaseDir))
				return; // Nothing to do.

			var pathname = Path.Combine(scriptureBaseDir, SharedConstants.ScriptureReferenceSystemFilename);
			if (!File.Exists(pathname))
				return; // Nobody home.
			var doc = XDocument.Load(pathname);
			CmObjectFlatteningService.FlattenOwnerlessObject(
				pathname,
				sortedData,
				doc.Element(SharedConstants.ScriptureReferenceSystem).Element("ScrRefSystem"));
		}
	}
}