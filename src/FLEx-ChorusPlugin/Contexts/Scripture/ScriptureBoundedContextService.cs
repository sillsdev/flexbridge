using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Contexts.Scripture
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
										  scriptureElement, SharedConstants.NoteCategories,
										  Path.Combine(scriptureBaseDir, SharedConstants.NoteCategoriesListFilename));

			// Extract all 66 required 'ScrBookAnnotations' instances from BookAnnotations property, and write to corresponding subfolder.
			// Leave property, but emptied of <objsur elements>.
			var booksDir = Path.Combine(scriptureBaseDir, SharedConstants.Books);
			if (!Directory.Exists(booksDir))
				Directory.CreateDirectory(booksDir);
			var allAnnotations = classData[SharedConstants.ScrBookAnnotations];
			var annotationObjSurElements = scriptureElement.Element(SharedConstants.BookAnnotations).Elements().ToList();
			scriptureElement.Element(SharedConstants.BookAnnotations).RemoveNodes();
			for (var canonicalBookNumber = 1; canonicalBookNumber < 67; ++canonicalBookNumber)
			{
				var paddedNumber = ScriptureDomainServices.PaddedCanonicalBookNumer(canonicalBookNumber);
				var currentAnnotationElement = XElement.Parse(SharedConstants.Utf8.GetString(allAnnotations[annotationObjSurElements[canonicalBookNumber - 1].Attribute(SharedConstants.GuidStr).Value]));
				CmObjectNestingService.NestObject(false, currentAnnotationElement,
					classData,
					guidToClassMapping);
				FileWriterService.WriteNestedFile(
					Path.Combine(booksDir, paddedNumber + "." + SharedConstants.bookannotations),
					new XElement(SharedConstants.BookAnnotations, currentAnnotationElement));
			}

			// Extract any optional ScrBook instances from 'ScriptureBooks', and write each to corresponding subfolder.
			var allBooks = classData[SharedConstants.ScrBook];
			var scriptureBooksProperty = scriptureElement.Element(SharedConstants.ScriptureBooks);
			if (scriptureBooksProperty != null && scriptureBooksProperty.HasElements)
			{
				List<XElement> bookObjSurElements = scriptureElement.Element(SharedConstants.ScriptureBooks).Elements().ToList();
				scriptureElement.Element(SharedConstants.ScriptureBooks).RemoveNodes();
				foreach (var objsurEl in bookObjSurElements)
				{
					var currentBookElement = XElement.Parse(SharedConstants.Utf8.GetString(allBooks[objsurEl.Attribute(SharedConstants.GuidStr).Value]));
					var paddedNumber = ScriptureDomainServices.PaddedCanonicalBookNumer(Int32.Parse(currentBookElement.Element("CanonicalNum").Attribute("val").Value));
					CmObjectNestingService.NestObject(false, currentBookElement,
						classData,
						guidToClassMapping);
					FileWriterService.WriteNestedFile(
						Path.Combine(booksDir, paddedNumber + "." + SharedConstants.book),
						new XElement(SharedConstants.Book, currentBookElement));
				}
			}

			CmObjectNestingService.NestObject(false, scriptureElement,
				classData,
				guidToClassMapping);

			FileWriterService.WriteNestedFile(
				Path.Combine(scriptureBaseDir, SharedConstants.ScriptureTransFilename),
				new XElement(SharedConstants.TranslatedScripture, scriptureElement));

			languageProjectElement.Element(SharedConstants.TranslatedScripture).RemoveNodes();
			classData[SharedConstants.LangProject][languageProjectElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant()] = SharedConstants.Utf8.GetBytes(languageProjectElement.ToString());
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string scriptureBaseDir)
		{
			if (!Directory.Exists(scriptureBaseDir))
				return;

			// scriptureBaseDir is root/Scripture.
			var pathname = Path.Combine(scriptureBaseDir, SharedConstants.ScriptureTransFilename);
			if (!File.Exists(pathname))
				return; // Nobody home.
			var doc = XDocument.Load(pathname);
			var scrElement = doc.Element(SharedConstants.TranslatedScripture).Elements().First();

			// Put the NoteCategories list back in the right place.
			pathname = Path.Combine(scriptureBaseDir, SharedConstants.NoteCategoriesListFilename);
			if (File.Exists(pathname))
			{
				doc = XDocument.Load(pathname);
				BaseDomainServices.RestoreElement(pathname, sortedData, scrElement, SharedConstants.NoteCategories, doc.Root.Element(SharedConstants.CmPossibilityList));
			}

			// Owned by LangProj in TranslatedScripture prop.
			var langProjElement = highLevelData[SharedConstants.LangProject];

			CmObjectFlatteningService.FlattenOwnedObject(
				pathname,
				sortedData,
				scrElement,
				langProjElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant(),
				langProjElement, SharedConstants.TranslatedScripture); // Restore 'ownerguid' to scrElement.

			// Put the <objsur> elements back into BookAnnotations and ScriptureBooks property elements.
			// There will always be 66 ScrBookAnnotations instances and they need go in canonical book order.
			// There may (or may not) be ScrBook instances, but they all go in canonical book order in ScriptureBooks, if present.
			var booksDir = Path.Combine(scriptureBaseDir, SharedConstants.Books);
			var sortedFiles = new SortedList<int, string>(66);
			foreach (var pathnameForAnn in Directory.GetFiles(booksDir, "*." + SharedConstants.bookannotations))
				sortedFiles.Add(Int32.Parse(Path.GetFileNameWithoutExtension(pathnameForAnn)), pathnameForAnn);
			var scrElementGuid = scrElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
			var scriptureBooksProperty = scrElement.Element(SharedConstants.ScriptureBooks);
			foreach (var sortedPathnameKvp in sortedFiles)
			{
				var sortedDoc = XDocument.Load(sortedPathnameKvp.Value);
				var element = sortedDoc.Root.Element(SharedConstants.ScrBookAnnotations);
				CmObjectFlatteningService.FlattenOwnedObject(
					sortedPathnameKvp.Value,
					sortedData,
					element,
					scrElementGuid, scrElement, SharedConstants.BookAnnotations); // Restore 'ownerguid' to annotation.

				// Deal with optional ScrBook
				var bookPathname = sortedPathnameKvp.Value.Replace(SharedConstants.bookannotations, SharedConstants.book);
				if (!File.Exists(bookPathname))
					continue;

				if (scriptureBooksProperty == null)
				{
					scriptureBooksProperty = new XElement(SharedConstants.ScriptureBooks);
					scrElement.Add(scriptureBooksProperty);
					// Make sure new property is sorted in correct place.
					DataSortingService.SortMainRtElement(scrElement);
				}
				// Add book <objsur> element to scrElement's ScriptureBooks element.
				sortedDoc = XDocument.Load(bookPathname);
				element = sortedDoc.Root.Element(SharedConstants.ScrBook);
				CmObjectFlatteningService.FlattenOwnedObject(
					bookPathname,
					sortedData,
					element,
					scrElementGuid, scrElement, SharedConstants.ScriptureBooks); // Restore 'ownerguid' to book.
			}

			highLevelData.Add(scrElement.Attribute(SharedConstants.Class).Value, scrElement);
		}
	}
}