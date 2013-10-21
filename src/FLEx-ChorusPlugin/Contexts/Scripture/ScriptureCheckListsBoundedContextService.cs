// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using TriboroughBridge_ChorusPlugin;

namespace FLEx_ChorusPlugin.Contexts.Scripture
{
	internal static class ScriptureCheckListsBoundedContextService
	{
		private const string CheckLists = "CheckLists";

		internal static void NestContext(XElement langProj,
			string scriptureBaseDir,
			IDictionary<string, SortedDictionary<string, byte[]>> classData,
			Dictionary<string, string> guidToClassMapping)
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
				var checkList = Utilities.CreateFromBytes(classData[className][checkListGuid]);

				CmObjectNestingService.NestObject(false, checkList,
					classData,
					guidToClassMapping);

				FileWriterService.WriteNestedFile(Path.Combine(scriptureBaseDir, checkList.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant() + "." + SharedConstants.List), new XElement("CheckList", checkList));
			}

			clPropElement.RemoveNodes();
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string scriptureBaseDir)
		{
			if (!Directory.Exists(scriptureBaseDir))
				return; // Nothing to do.

			var langProjElement = highLevelData[SharedConstants.LangProject];
			var langProjGuid = langProjElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
			var sortedLists = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			foreach (var listPathname in Directory.GetFiles(scriptureBaseDir, "*.list", SearchOption.TopDirectoryOnly))
			{
				if (listPathname.EndsWith(SharedConstants.NoteCategoriesListFilename))
					continue; // Wrong list.

				var listDoc = XDocument.Load(listPathname);
				var listElement = listDoc.Element("CheckList").Element(SharedConstants.CmPossibilityList);
				CmObjectFlatteningService.FlattenOwnedObject(
					listPathname,
					sortedData,
					listElement,
					langProjGuid, sortedLists); // Restore 'ownerguid' to list.
			}

			if (sortedLists.Count == 0)
				return;
			// Restore LangProj CheckLists property in sorted order.
			var checkListsProp = langProjElement.Element(CheckLists)
								 ?? CmObjectFlatteningService.AddNewPropertyElement(langProjElement, CheckLists);
			foreach (var sortedList in sortedLists.Values)
				checkListsProp.Add(sortedList);
		}
	}
}