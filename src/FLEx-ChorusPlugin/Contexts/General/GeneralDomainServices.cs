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
		internal static void WriteNestedDomainData(IProgress progress, bool writeVerbose, string rootDir,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			var generalBaseDir = Path.Combine(rootDir, SharedConstants.General);
			if (!Directory.Exists(generalBaseDir))
				Directory.CreateDirectory(generalBaseDir);

			if (writeVerbose)
			{
				progress.WriteVerbose("Writing the general data....");
				progress.WriteVerbose("Writing user-defined list data....");
			}
			else
			{
				progress.WriteMessage("Writing the general data....");
				progress.WriteMessage("Writing user-defined list data....");
			}
			UserDefinedListsBoundedContextService.NestContext(generalBaseDir, classData, guidToClassMapping);

			if (writeVerbose)
				progress.WriteVerbose("Writing language project data....");
			else
				progress.WriteMessage("Writing language project data....");
			GeneralDomainBoundedContext.NestContext(generalBaseDir, classData, guidToClassMapping);

			if (writeVerbose)
				progress.WriteVerbose("Writing problem data....");
			else
				progress.WriteMessage("Writing problem data....");
			GeneralDomainOrphansBoundedContext.NestContext(generalBaseDir, classData, guidToClassMapping);
		}

		internal static void FlattenDomain(IProgress progress, bool writeVerbose,
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string rootDir)
		{
			var generalBaseDir = Path.Combine(rootDir, SharedConstants.General);
			if (!Directory.Exists(generalBaseDir))
				return;

			// Do in reverse order from nesting.
			if (writeVerbose)
			{
				progress.WriteVerbose("Collecting the general data....");
				progress.WriteVerbose("Collecting the problem data....");
			}
			else
			{
				progress.WriteMessage("Collecting the general data....");
				progress.WriteMessage("Collecting the problem data....");
			}
			GeneralDomainOrphansBoundedContext.FlattenContext(highLevelData, sortedData, generalBaseDir);

			if (writeVerbose)
				progress.WriteVerbose("Collecting the language project data....");
			else
				progress.WriteMessage("Collecting the language project data....");
			GeneralDomainBoundedContext.FlattenContext(highLevelData, sortedData, generalBaseDir);

			if (writeVerbose)
				progress.WriteVerbose("Collecting the user-defined list data....");
			else
				progress.WriteMessage("Collecting the user-defined list data....");
			UserDefinedListsBoundedContextService.FlattenContext(highLevelData, sortedData, generalBaseDir);
		}

		internal static void RemoveBoundedContextData(string pathRoot)
		{
			BaseDomainServices.RemoveBoundedContextDataCore(Path.Combine(pathRoot, SharedConstants.General));
		}
	}
}
