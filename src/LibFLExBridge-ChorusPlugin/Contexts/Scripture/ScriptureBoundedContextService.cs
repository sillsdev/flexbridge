// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibFLExBridgeChorusPlugin.DomainServices;

namespace LibFLExBridgeChorusPlugin.Contexts.Scripture
{
	internal static class ScriptureBoundedContextService
	{
		internal static void NestContext(XElement languageProjectElement,
			XElement scriptureElement,
			string baseDirectory,
			IDictionary<string, SortedDictionary<string, byte[]>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			// baseDirectory is root/Scripture and has already been created by caller.
			var scriptureBaseDir = baseDirectory;

			// Split out the optional NoteCategories list.
			FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
										  scriptureElement, FlexBridgeConstants.NoteCategories,
										  Path.Combine(scriptureBaseDir, FlexBridgeConstants.NoteCategoriesListFilename));

			// Extract all 66 required 'ScrBookAnnotations' instances from BookAnnotations property, and write to corresponding subfolder.
			// Leave property, but emptied of <objsur elements>.
			var booksDir = Path.Combine(scriptureBaseDir, FlexBridgeConstants.Books);
			if (!Directory.Exists(booksDir))
				Directory.CreateDirectory(booksDir);
			var allAnnotations = classData[FlexBridgeConstants.ScrBookAnnotations];
			var annotationObjSurElements = scriptureElement.Element(FlexBridgeConstants.BookAnnotations).Elements().ToList();
			scriptureElement.Element(FlexBridgeConstants.BookAnnotations).RemoveNodes();
			for (var canonicalBookNumber = 1; canonicalBookNumber < 67; ++canonicalBookNumber)
			{
				var paddedNumber = ScriptureDomainServices.PaddedCanonicalBookNumer(canonicalBookNumber);
				var currentAnnotationElement = LibFLExBridgeUtilities.CreateFromBytes(allAnnotations[annotationObjSurElements[canonicalBookNumber - 1].Attribute(FlexBridgeConstants.GuidStr).Value]);
				CmObjectNestingService.NestObject(false, currentAnnotationElement,
					classData,
					guidToClassMapping);
				FileWriterService.WriteNestedFile(
					Path.Combine(booksDir, paddedNumber + "." + FlexBridgeConstants.bookannotations),
					new XElement(FlexBridgeConstants.BookAnnotations, currentAnnotationElement));
			}

			// Extract any optional ScrBook instances from 'ScriptureBooks', and write each to corresponding subfolder.
			var allBooks = classData[FlexBridgeConstants.ScrBook];
			var scriptureBooksProperty = scriptureElement.Element(FlexBridgeConstants.ScriptureBooks);
			if (scriptureBooksProperty != null && scriptureBooksProperty.HasElements)
			{
				List<XElement> bookObjSurElements = scriptureElement.Element(FlexBridgeConstants.ScriptureBooks).Elements().ToList();
				scriptureElement.Element(FlexBridgeConstants.ScriptureBooks).RemoveNodes();
				foreach (var objsurEl in bookObjSurElements)
				{
					var currentBookElement = LibFLExBridgeUtilities.CreateFromBytes(allBooks[objsurEl.Attribute(FlexBridgeConstants.GuidStr).Value]);
					var paddedNumber = ScriptureDomainServices.PaddedCanonicalBookNumer(Int32.Parse(currentBookElement.Element("CanonicalNum").Attribute("val").Value));
					CmObjectNestingService.NestObject(false, currentBookElement,
						classData,
						guidToClassMapping);
					FileWriterService.WriteNestedFile(
						Path.Combine(booksDir, paddedNumber + "." + FlexBridgeConstants.book),
						new XElement(FlexBridgeConstants.Book, currentBookElement));
				}
			}

			CmObjectNestingService.NestObject(false, scriptureElement,
				classData,
				guidToClassMapping);

			FileWriterService.WriteNestedFile(
				Path.Combine(scriptureBaseDir, FlexBridgeConstants.ScriptureTransFilename),
				new XElement(FlexBridgeConstants.TranslatedScripture, scriptureElement));

			languageProjectElement.Element(FlexBridgeConstants.TranslatedScripture).RemoveNodes();
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string scriptureBaseDir)
		{
			if (!Directory.Exists(scriptureBaseDir))
				return;

			// scriptureBaseDir is root/Scripture.
			var pathname = Path.Combine(scriptureBaseDir, FlexBridgeConstants.ScriptureTransFilename);
			if (!File.Exists(pathname))
				return; // Nobody home.
			var doc = XDocument.Load(pathname);
			var scrElement = doc.Element(FlexBridgeConstants.TranslatedScripture).Elements().First();

			// Put the NoteCategories list back in the right place.
			pathname = Path.Combine(scriptureBaseDir, FlexBridgeConstants.NoteCategoriesListFilename);
			if (File.Exists(pathname))
			{
				doc = XDocument.Load(pathname);
				BaseDomainServices.RestoreElement(pathname, sortedData, scrElement, FlexBridgeConstants.NoteCategories, doc.Root.Element(FlexBridgeConstants.CmPossibilityList));
			}

			// Owned by LangProj in TranslatedScripture prop.
			var langProjElement = highLevelData[FlexBridgeConstants.LangProject];

			CmObjectFlatteningService.FlattenOwnedObject(
				pathname,
				sortedData,
				scrElement,
				langProjElement.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant(),
				langProjElement, FlexBridgeConstants.TranslatedScripture); // Restore 'ownerguid' to scrElement.

			// Put the <objsur> elements back into BookAnnotations and ScriptureBooks property elements.
			// There will always be 66 ScrBookAnnotations instances and they need go in canonical book order.
			// There may (or may not) be ScrBook instances, but they all go in canonical book order in ScriptureBooks, if present.
			var booksDir = Path.Combine(scriptureBaseDir, FlexBridgeConstants.Books);
			var sortedFiles = new SortedList<int, string>(66);
			foreach (var pathnameForAnn in Directory.GetFiles(booksDir, "*." + FlexBridgeConstants.bookannotations))
				sortedFiles.Add(Int32.Parse(Path.GetFileNameWithoutExtension(pathnameForAnn)), pathnameForAnn);
			var scrElementGuid = scrElement.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant();
			var scriptureBooksProperty = scrElement.Element(FlexBridgeConstants.ScriptureBooks);
			foreach (var sortedPathnameKvp in sortedFiles)
			{
				var sortedDoc = XDocument.Load(sortedPathnameKvp.Value);
				var element = sortedDoc.Root.Element(FlexBridgeConstants.ScrBookAnnotations);
				CmObjectFlatteningService.FlattenOwnedObject(
					sortedPathnameKvp.Value,
					sortedData,
					element,
					scrElementGuid, scrElement, FlexBridgeConstants.BookAnnotations); // Restore 'ownerguid' to annotation.

				// Deal with optional ScrBook
				var bookPathname = sortedPathnameKvp.Value.Replace(FlexBridgeConstants.bookannotations, FlexBridgeConstants.book);
				if (!File.Exists(bookPathname))
					continue;

				if (scriptureBooksProperty == null)
				{
					scriptureBooksProperty = new XElement(FlexBridgeConstants.ScriptureBooks);
					scrElement.Add(scriptureBooksProperty);
					// Make sure new property is sorted in correct place.
					DataSortingService.SortMainRtElement(scrElement);
				}
				// Add book <objsur> element to scrElement's ScriptureBooks element.
				sortedDoc = XDocument.Load(bookPathname);
				element = sortedDoc.Root.Element(FlexBridgeConstants.ScrBook);
				CmObjectFlatteningService.FlattenOwnedObject(
					bookPathname,
					sortedData,
					element,
					scrElementGuid, scrElement, FlexBridgeConstants.ScriptureBooks); // Restore 'ownerguid' to book.
			}

			highLevelData.Add(scrElement.Attribute(FlexBridgeConstants.Class).Value, scrElement);
		}
	}
}