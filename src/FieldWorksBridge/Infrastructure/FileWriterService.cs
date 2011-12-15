using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using Palaso.Xml;

namespace FieldWorksBridge.Infrastructure
{
	internal static class FileWriterService
	{
		internal static void WriteNestedFile(string newPathname,
			XmlReaderSettings readerSettings,
			XElement nestedData,
			string rootElementName)
		{
			using (var writer = XmlWriter.Create(newPathname, CanonicalXmlSettings.CreateXmlWriterSettings()))
			{
				writer.WriteStartElement(rootElementName);
				if (nestedData != null)
					WriteElement(writer, readerSettings, nestedData);
				writer.WriteEndElement();
			}
		}

		internal static void WriteNestedFile(string newPathname,
			XmlReaderSettings readerSettings,
			XDocument nestedDoc)
		{
			using (var writer = XmlWriter.Create(newPathname, CanonicalXmlSettings.CreateXmlWriterSettings()))
			{
					WriteDocument(writer, readerSettings, nestedDoc);
			}
		}

		private static void WriteDocument(XmlWriter writer, XmlReaderSettings readerSettings, XDocument nestedDoc)
		{
			using (var nodeReader = XmlReader.Create(new MemoryStream(MultipleFileServices.Utf8.GetBytes(nestedDoc.ToString()), false), readerSettings))
				writer.WriteNode(nodeReader, true);
		}

		internal static void WriteSecondaryFile(string newPathname, XmlReaderSettings readerSettings, SortedDictionary<string, XElement> data)
		{
			using (var writer = XmlWriter.Create(newPathname, CanonicalXmlSettings.CreateXmlWriterSettings()))
			{
				writer.WriteStartElement("classdata");
				if (data != null)
				{
					foreach (var kvp in data)
						WriteElement(writer, readerSettings, kvp.Value);
				}
				writer.WriteEndElement();
			}
		}

		internal static void WriteSecondaryFiles(string multiFileDirRoot, string className, XmlReaderSettings readerSettings, SortedDictionary<string, XElement> data)
		{
			// Divide 'data' into the 10 zero-based buckets.
			var bucket0 = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			var bucket1 = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			var bucket2 = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			var bucket3 = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			var bucket4 = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			var bucket5 = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			var bucket6 = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			var bucket7 = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			var bucket8 = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			var bucket9 = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);

			foreach (var kvp in data)
			{
				var key = kvp.Key;
				var bucket = (int)((uint)new Guid(key).GetHashCode() % 10);
				SortedDictionary<string, XElement> currentBucket;
				switch (bucket)
				{
					default:
						throw new InvalidOperationException("Bucket not recognized.");
					case 0:
						currentBucket = bucket0;
						break;
					case 1:
						currentBucket = bucket1;
						break;
					case 2:
						currentBucket = bucket2;
						break;
					case 3:
						currentBucket = bucket3;
						break;
					case 4:
						currentBucket = bucket4;
						break;
					case 5:
						currentBucket = bucket5;
						break;
					case 6:
						currentBucket = bucket6;
						break;
					case 7:
						currentBucket = bucket7;
						break;
					case 8:
						currentBucket = bucket8;
						break;
					case 9:
						currentBucket = bucket9;
						break;
				}
				currentBucket.Add(key, kvp.Value);
			}

			// Write out each bucket (another SortedDictionary) using regular WriteSecondaryFile method.
			var basePath = Path.Combine(multiFileDirRoot, className);
			WriteSecondaryFile(basePath + "_01.ClassData", readerSettings, bucket0); // 1-based files vs 0-based buckets.
			WriteSecondaryFile(basePath + "_02.ClassData", readerSettings, bucket1);
			WriteSecondaryFile(basePath + "_03.ClassData", readerSettings, bucket2);
			WriteSecondaryFile(basePath + "_04.ClassData", readerSettings, bucket3);
			WriteSecondaryFile(basePath + "_05.ClassData", readerSettings, bucket4);
			WriteSecondaryFile(basePath + "_06.ClassData", readerSettings, bucket5);
			WriteSecondaryFile(basePath + "_07.ClassData", readerSettings, bucket6);
			WriteSecondaryFile(basePath + "_08.ClassData", readerSettings, bucket7);
			WriteSecondaryFile(basePath + "_09.ClassData", readerSettings, bucket8);
			WriteSecondaryFile(basePath + "_10.ClassData", readerSettings, bucket9);
		}

		internal static void WriteElement(XmlWriter writer, XmlReaderSettings readerSettings, XElement element)
		{
			using (var nodeReader = XmlReader.Create(new MemoryStream(MultipleFileServices.Utf8.GetBytes(element.ToString()), false), readerSettings))
				writer.WriteNode(nodeReader, true);
		}

		internal static void WriteElement(XmlWriter writer, XmlReaderSettings readerSettings, byte[] optionalFirstElement)
		{
			using (var nodeReader = XmlReader.Create(new MemoryStream(optionalFirstElement, false), readerSettings))
				writer.WriteNode(nodeReader, true);
		}

		internal static void WriteCustomPropertyFile(string newPathname, XmlReaderSettings readerSettings, byte[] element)
		{
			if (element == null)
			{
				// Still write out file with just the root element.
				var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("AdditionalFields"));
				doc.Save(newPathname);
			}
			else
			{
				using (var writer = XmlWriter.Create(newPathname, CanonicalXmlSettings.CreateXmlWriterSettings()))
					WriteElement(writer, readerSettings, element);
			}
		}

		internal static void WriteVersionNumberFile(string multiFileDirRoot, string projectName, string version)
		{
			File.WriteAllText(Path.Combine(multiFileDirRoot, projectName + ".ModelVersion"), "{\"modelversion\": " + version + "}");
		}

		internal static void WriteClassDataToOriginal(XmlWriter writer, string rootFolder, XmlReaderSettings readerSettings)
		{
			foreach (var pathname in Directory.GetFiles(rootFolder, "*.ClassData"))
			{
				using (var reader = XmlReader.Create(pathname, readerSettings))
				{
					reader.MoveToContent();
					if (reader.IsEmptyElement)
						continue; // No <rt> child elements.
					reader.Read();
					while (reader.IsStartElement())
						writer.WriteNode(reader, false);
				}
			}
		}

		internal static void RemoveDomainData(string pathRoot)
		{
			LinguisticsDomainServices.RemoveBoundedContextData(pathRoot);
			AnthropologyDomainServices.RemoveBoundedContextData(pathRoot);
			ScriptureDomainServices.RemoveBoundedContextData(pathRoot);

			// TODO: Remove below stuff, after everything is shifted to domains.
			var multiFileDirRoot = Path.Combine(pathRoot, "DataFiles");
			RemoveDataFiles(multiFileDirRoot);
			RemoveEmptyFolders(multiFileDirRoot, false);
		}

		internal static void WriteDomainData(MetadataCache mdc, string pathRoot,
			XmlReaderSettings readerSettings,
			Dictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping,
			Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache,
			HashSet<string> skipwriteEmptyClassFiles)
		{
			LinguisticsDomainServices.WriteDomainData(readerSettings, pathRoot, mdc, classData, guidToClassMapping, interestingPropertiesCache, skipwriteEmptyClassFiles);
			AnthropologyDomainServices.WriteDomainData(readerSettings, pathRoot, mdc, classData, guidToClassMapping, interestingPropertiesCache, skipwriteEmptyClassFiles);
			ScriptureDomainServices.WriteDomainData(readerSettings, pathRoot, mdc, classData, guidToClassMapping, interestingPropertiesCache, skipwriteEmptyClassFiles);

			// Remove the data that may be in multiple bounded Contexts.
			// Eventually, there ought not be an need for writing the leftovers in the base folder,
			// but I'm not there yet.
			//ObjectFinderServices.ProcessLists(classData, skipwriteEmptyClassFiles, new HashSet<string> { "N ote" });
		}

		internal static void RestoreDomainData(XmlWriter writer, XmlReaderSettings readerSettings, Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache, string pathRoot)
		{
			var sortedData = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			foreach (var restoredElement in LinguisticsDomainServices.FlattenDomain(interestingPropertiesCache, pathRoot))
			{
				DataSortingService.SortAndStoreElement(sortedData, interestingPropertiesCache, restoredElement);
			}

			// TODO: Add other two Domains and move remaining ling stuff into Ling domain.
			//ReversalBoundedContextService.RestoreOriginalFile(writer, readerSettings, multiFileDirRoot.Replace("DataFiles", "Linguistics"));
			//TextCorpusBoundedContextService.RestoreOriginalFile(writer, readerSettings, multiFileDirRoot);
			//DiscourseAnalysisBoundedContextService.RestoreOriginalFile(writer, readerSettings, multiFileDirRoot);
			//WordformInventoryBoundedContextService.RestoreOriginalFile(writer, readerSettings, multiFileDirRoot);
			//LexiconBoundedContextService.RestoreOriginalFile(writer, readerSettings, multiFileDirRoot);
			//PunctuationFormBoundedContextService.RestoreOriginalFile(writer, readerSettings, multiFileDirRoot);
			//LinguisticsBoundedContextService.RestoreOriginalFile(writer, readerSettings, multiFileDirRoot);

			//AnthropologyBoundedContextService.RestoreOriginalFile(writer, readerSettings, multiFileDirRoot);

			//ScriptureBoundedContextService.RestoreOriginalFile(writer, readerSettings, multiFileDirRoot);
			var multiFileDirRoot = Path.Combine(pathRoot, "DataFiles");
			foreach (var classDataPathname in Directory.GetFiles(multiFileDirRoot, "*.ClassData", SearchOption.AllDirectories))
			{
				var classDataDoc = XDocument.Load(classDataPathname);
				foreach (var rtElement in classDataDoc.Element("classdata").Elements("rt"))
				{
					DataSortingService.SortAndStoreElement(sortedData, interestingPropertiesCache, rtElement);
				}
			}

			foreach (var rtElement in sortedData.Values)
				WriteElement(writer, readerSettings, rtElement);
		}

		internal static void WriteObject(MetadataCache mdc,
			IDictionary<string, SortedDictionary<string, XElement>> classData, IDictionary<string, string> guidToClassMapping,
			string baseDir,
			XmlReaderSettings readerSettings, Dictionary<string, SortedDictionary<string, XElement>> multiClassOutput, string guid,
			HashSet<string> omitProperties)
		{
			multiClassOutput.Clear();
			var dataEl = ObjectFinderServices.RegisterDataInBoundedContext(classData, guidToClassMapping, multiClassOutput, guid);
			ObjectFinderServices.CollectAllOwnedObjects(mdc,
														classData, guidToClassMapping, multiClassOutput,
														dataEl,
														omitProperties);
			foreach (var kvp in multiClassOutput)
				WriteSecondaryFile(Path.Combine(baseDir, kvp.Key + ".ClassData"), readerSettings, kvp.Value);
			multiClassOutput.Clear();
		}

		internal static void RemoveDataFiles(string baseDataFolder)
		{
			// Delete all data files at any folder depth.
			foreach (var dataFilePathname in Directory.GetFiles(baseDataFolder, "*.ClassData", SearchOption.AllDirectories))
				File.Delete(dataFilePathname);
		}

		internal static void RemoveEmptyFolders(string baseDataFolder, bool removeTopLevelFolder)
		{
			foreach (var folder in Directory.GetDirectories(baseDataFolder))
			{
				if (Directory.GetFileSystemEntries(folder).Length > 0)
					RemoveEmptyFolders(folder, false); // Work down to leaf folders first.

				if (Directory.GetFileSystemEntries(folder).Length == 0)
					Directory.Delete(folder); // Empty now, so zap it.
			}
			if (removeTopLevelFolder && Directory.GetFileSystemEntries(baseDataFolder).Length == 0)
				Directory.Delete(baseDataFolder); // Empty now, so zap it.
		}

		internal static void RestoreFiles(XmlWriter writer, XmlReaderSettings readerSettings, string baseDir)
		{
			if (!Directory.Exists(baseDir))
				return;

			WriteClassDataToOriginal(writer, baseDir, readerSettings);

			foreach (var directory in Directory.GetDirectories(baseDir))
				RestoreFiles(writer, readerSettings, directory);
		}
	}
}