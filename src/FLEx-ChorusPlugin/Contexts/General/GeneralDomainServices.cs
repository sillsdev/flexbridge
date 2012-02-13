using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Contexts.General.UserDefinedLists;
using FLEx_ChorusPlugin.Infrastructure;

namespace FLEx_ChorusPlugin.Contexts.General
{
	internal static class GeneralDomainServices
	{
		internal static void WriteNestedDomainData(string rootDir,
			MetadataCache mdc,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			var generalBaseDir = Path.Combine(rootDir, SharedConstants.General);
			if (!Directory.Exists(generalBaseDir))
				Directory.CreateDirectory(generalBaseDir);

			UserDefinedListsBoundedContextService.NestContext(generalBaseDir, classData, guidToClassMapping);
			GeneralDomainBoundedContext.NestContext(generalBaseDir, classData, guidToClassMapping);
			GeneralDomainOrphansBoundedContext.NestContext(generalBaseDir, classData, guidToClassMapping);
		}

		internal static void FlattenDomain(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string rootDir)
		{
			var generalBaseDir = Path.Combine(rootDir, SharedConstants.General);
			if (!Directory.Exists(generalBaseDir))
				return;

			// Do in reverse order from nesting.
			GeneralDomainOrphansBoundedContext.FlattenContext(highLevelData, sortedData, generalBaseDir);
			GeneralDomainBoundedContext.FlattenContext(highLevelData, sortedData, generalBaseDir);
			UserDefinedListsBoundedContextService.FlattenContext(highLevelData, sortedData, generalBaseDir);
		}

		internal static void RemoveBoundedContextData(string pathRoot)
		{
			BaseDomainServices.RemoveBoundedContextDataCore(Path.Combine(pathRoot, SharedConstants.General));
		}
	}
}
