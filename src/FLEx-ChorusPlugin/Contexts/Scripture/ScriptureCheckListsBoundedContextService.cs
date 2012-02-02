using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Contexts.Scripture
{
	internal static class ScriptureCheckListsBoundedContextService
	{
		private const string CheckLists = "CheckLists";

		internal static void NestContext(XElement langProj,
			XmlReaderSettings readerSettings, string scriptureBaseDir,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping,
			HashSet<string> skipWriteEmptyClassFiles)
		{
			if (!Directory.Exists(scriptureBaseDir))
				return;

			var clPropElement = langProj.Element(CheckLists);
			if (clPropElement == null || !clPropElement.HasElements)
				return;

			foreach (var checkListObjSurElement in clPropElement.Elements())
			{
				var checkListGuid = checkListObjSurElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
				var className = guidToClassMapping[checkListGuid];
				var checkList = classData[className][checkListGuid];

				CmObjectNestingService.NestObject(false, checkList,
					new Dictionary<string, HashSet<string>>(),
					classData,
					guidToClassMapping);

				// Remove 'ownerguid'.
				checkList.Attribute(SharedConstants.OwnerGuid).Remove();

				var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
					new XElement("CheckList", checkList));

				FileWriterService.WriteNestedFile(Path.Combine(scriptureBaseDir, checkList.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant() + "." + SharedConstants.List), readerSettings, doc);
			}

			clPropElement.RemoveNodes();

			ObjectFinderServices.ProcessLists(classData, skipWriteEmptyClassFiles, new HashSet<string> { "ChkTerm", "ChkRef", "ChkRendering" });
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string scriptureBaseDir)
		{
			if (!Directory.Exists(scriptureBaseDir))
				return; // Nothing to do.

			var langProjElement = highLevelData["LangProject"];
			var langProjGuid = langProjElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
			var sortedLists = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			foreach (var listPathname in Directory.GetFiles(scriptureBaseDir, "*.list", SearchOption.TopDirectoryOnly))
			{
				var listDoc = XDocument.Load(listPathname);
				var listElement = listDoc.Element("CheckList").Element("CmPossibilityList");
				CmObjectFlatteningService.FlattenObject(
					listPathname,
					sortedData,
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

			foreach (var checkListPathname in Directory.GetFiles(scriptureBaseDir, "*." + SharedConstants.List, SearchOption.TopDirectoryOnly))
				File.Delete(checkListPathname);

			// Scripture domain does it all.
			//FileWriterService.RemoveEmptyFolders(clDir, true);
		}
	}
}