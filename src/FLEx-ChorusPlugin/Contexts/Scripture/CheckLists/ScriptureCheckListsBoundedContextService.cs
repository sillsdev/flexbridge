using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Contexts.Scripture.CheckLists
{
	internal static class ScriptureCheckListsBoundedContextService
	{
		private const string CheckLists = "CheckLists";

		internal static void NestContext(XElement langProj,
			XmlReaderSettings readerSettings, string baseDirectory,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping,
			Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache,
			HashSet<string> skipWriteEmptyClassFiles)
		{
			var clDir = Path.Combine(baseDirectory, CheckLists);
			if (!Directory.Exists(clDir))
				Directory.CreateDirectory(clDir);

			var clPropElement = langProj.Element(CheckLists);
			if (clPropElement == null || !clPropElement.HasElements)
				return;

			foreach (var checkListObjSurElement in clPropElement.Elements())
			{
				var checkListGuid = checkListObjSurElement.Attribute(SharedConstants.GuidStr).Value;
				var className = guidToClassMapping[checkListGuid];
				var checkList = classData[className][checkListGuid];

				CmObjectNestingService.NestObject(checkList,
					new Dictionary<string, HashSet<string>>(),
					classData,
					interestingPropertiesCache,
					guidToClassMapping);

				// Remove 'ownerguid'.
				checkList.Attribute(SharedConstants.OwnerGuid).Remove();

				var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
					new XElement("CheckList", checkList));

				FileWriterService.WriteNestedFile(Path.Combine(clDir, checkList.Attribute(SharedConstants.GuidStr).Value + "." + SharedConstants.List), readerSettings, doc);
			}

			clPropElement.RemoveNodes();

			ObjectFinderServices.ProcessLists(classData, skipWriteEmptyClassFiles, new HashSet<string> { "ChkTerm", "ChkRef", "ChkRendering" });
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache,
			string scriptureBaseDir)
		{
			// No subfolders for scriptureBaseDir
			if (!Directory.Exists(scriptureBaseDir))
				return; // Nothing to do.
			var clDir = Path.Combine(scriptureBaseDir, CheckLists);
			if (!Directory.Exists(clDir))
				return;

			var langProjElement = highLevelData["LangProject"];
			var langProjGuid = langProjElement.Attribute(SharedConstants.GuidStr).Value;
			var sortedLists = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
// ReSharper disable ConvertClosureToMethodGroup
			foreach (var listDoc in Directory.GetFiles(clDir, "*.list", SearchOption.TopDirectoryOnly).Select(listPathname => XDocument.Load(listPathname)))
// ReSharper restore ConvertClosureToMethodGroup
			{
				var listElement = listDoc.Element("CheckList").Element("CmPossibilityList");
				CmObjectFlatteningService.FlattenObject(sortedData,
					interestingPropertiesCache,
					listElement,
					langProjGuid); // Restore 'ownerguid' to list.
				var listGuid = listElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
				sortedLists.Add(listGuid, new XElement(SharedConstants.Objsur, new XAttribute(SharedConstants.GuidStr, listGuid), new XAttribute("t", "o")));
			}

			if (sortedLists.Count == 0)
				return;
			// Restore LangProj CheckLists property in sorted order.
			var checkListsProp = langProjElement.Element(CheckLists);
			foreach (var sortedList in sortedLists.Values)
				checkListsProp.Add(sortedList);
		}

		internal static void RemoveBoundedContextData(string scriptureBaseDir)
		{
			if (!Directory.Exists(scriptureBaseDir))
				return;
			var clDir = Path.Combine(scriptureBaseDir, CheckLists);
			if (!Directory.Exists(clDir))
				return;

			foreach (var checkListPathname in Directory.GetFiles(clDir, "*." + SharedConstants.List, SearchOption.TopDirectoryOnly))
				File.Delete(checkListPathname);

			FileWriterService.RemoveEmptyFolders(clDir, true);
		}
	}
}