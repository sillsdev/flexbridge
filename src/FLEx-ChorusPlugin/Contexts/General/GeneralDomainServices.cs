using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Contexts.General.UserDefinedLists;
using FLEx_ChorusPlugin.Infrastructure;
using Palaso.Progress.LogBox;

namespace FLEx_ChorusPlugin.Contexts.General
{
	internal static class GeneralDomainServices
	{
		internal static void WriteNestedDomainData(IProgress progress, string rootDir,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			var generalBaseDir = Path.Combine(rootDir, SharedConstants.General);
			if (!Directory.Exists(generalBaseDir))
				Directory.CreateDirectory(generalBaseDir);

			progress.WriteMessage("Writing user-defined list data....");
			UserDefinedListsBoundedContextService.NestContext(generalBaseDir, classData, guidToClassMapping);
			progress.WriteMessage("Writing language project data....");
			GeneralDomainBoundedContext.NestContext(generalBaseDir, classData, guidToClassMapping);
			progress.WriteMessage("Writing problem data....");
			GeneralDomainOrphansBoundedContext.NestContext(generalBaseDir, classData, guidToClassMapping);
		}

		internal static void FlattenDomain(IProgress progress,
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string rootDir)
		{
			var generalBaseDir = Path.Combine(rootDir, SharedConstants.General);
			if (!Directory.Exists(generalBaseDir))
				return;

			// Do in reverse order from nesting.
			progress.WriteMessage("Collecting the problem data....");
			GeneralDomainOrphansBoundedContext.FlattenContext(highLevelData, sortedData, generalBaseDir);
			progress.WriteMessage("Collecting the language project data....");
			GeneralDomainBoundedContext.FlattenContext(highLevelData, sortedData, generalBaseDir);
			progress.WriteMessage("Collecting the user-defined list data....");
			UserDefinedListsBoundedContextService.FlattenContext(highLevelData, sortedData, generalBaseDir);
		}

		internal static void RemoveBoundedContextData(string pathRoot)
		{
			BaseDomainServices.RemoveBoundedContextDataCore(Path.Combine(pathRoot, SharedConstants.General));
		}
	}
}
