using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Contexts.Anthropology;
using FLEx_ChorusPlugin.Contexts.Linguistics;
using FLEx_ChorusPlugin.Contexts.Scripture;
using FLEx_ChorusPlugin.Properties;
using Palaso.Xml;

namespace FLEx_ChorusPlugin.Infrastructure
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

		internal static void WriteVersionNumberFile(string pathRoot, string projectName, string version)
		{
			File.WriteAllText(Path.Combine(pathRoot, projectName + ".ModelVersion"), Resources.kModelVersion + version + Resources.kCloseCurlyBrace);
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
			if (!Directory.Exists(multiFileDirRoot))
			{
				Directory.CreateDirectory(multiFileDirRoot);
				return;
			}
			RemoveDataFiles(multiFileDirRoot);
			RemoveEmptyFolders(multiFileDirRoot, false); // Leave it, since we want it to be there, after this method is done.
		}

		internal static void WriteDomainData(MetadataCache mdc, string pathRoot,
			XmlReaderSettings readerSettings,
			Dictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping,
			Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache)
		{
			var skipwriteEmptyClassFiles = new HashSet<string>();

			//		LinguisticsDomainServices.WriteNestedDomainData will do old and new for a while yet.
			LinguisticsDomainServices.WriteNestedDomainData(readerSettings, pathRoot, mdc, classData, guidToClassMapping, interestingPropertiesCache, skipwriteEmptyClassFiles);
			//		LinguisticsDomainServices.WriteNestedDomainData does only new.
			AnthropologyDomainServices.WriteNestedDomainData(readerSettings, pathRoot, classData, guidToClassMapping, interestingPropertiesCache, skipwriteEmptyClassFiles);
			//		ScriptureDomainServices.WriteDomainData will do old for a while yet.
			ScriptureDomainServices.WriteDomainData(readerSettings, pathRoot, mdc, classData, guidToClassMapping, interestingPropertiesCache, skipwriteEmptyClassFiles);

			// Remove the data that may be in multiple bounded Contexts.
			// Eventually, there ought not be an need for writing the leftovers in the base folder,
			// but I'm not there yet.
			//ObjectFinderServices.ProcessLists(classData, skipwriteEmptyClassFiles, new HashSet<string> { "N ote" });

			// TODO: Props to not store in nested LangProj:
			// TODO:	These are all for LangProj
			/*
			 * "ResearchNotebook"
			 * "AnthroList",
			 * "ConfidenceLevels",
			 * "Restrictions",
			 * "Roles",
			 * "Status",
			 * "Locations",
			 * "People",
			 * "Education",
			 * "TimeOfDay",
			 * "Positions"
			*/

			// TODO??: Maybe put everything that is left in "classData" in the 'General' context as: 1) series of regular 'rt' elements, or 2) nested objects, with the top elements being the unowned ones.
			// TODO: Once everything is in the BCs, then there should be nothing left in the 'classData' dictionary,
			// TODO: so no class data will be left to write at the 'multiFileDirRoot' level in the following code.
			// Write data records in guid sorted order.
			// Write class file for each concrete class, whether it has data or not.
			var multiFileDirRoot = Path.Combine(pathRoot, "DataFiles");
			foreach (var className in mdc.AllConcreteClasses.Select(concClassInfo => concClassInfo.ClassName))
			{
				var classDataPathname = Path.Combine(multiFileDirRoot, className + ".ClassData");
				SortedDictionary<string, XElement> sortedInstanceData;
				if (classData.TryGetValue(className, out sortedInstanceData))
				{
					// Only write one file, since there are no more high volume instances here.
					WriteSecondaryFile(classDataPathname, readerSettings, sortedInstanceData);
				}
				else
				{
					// Write empty class file, unless it is empty by reason of it being emptied by a Bounded Context.
					if (!skipwriteEmptyClassFiles.Contains(className))
						WriteSecondaryFile(classDataPathname, readerSettings, null);
				}
			}
		}

		internal static void RestoreDomainData(XmlWriter writer, XmlReaderSettings readerSettings, Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache, string pathRoot)
		{
			var sortedData = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			var highLevelData = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);

			// TODO: 'leftover' Domain.
			var multiFileDirRoot = Path.Combine(pathRoot, "DataFiles");
			foreach (var classDataPathname in Directory.GetFiles(multiFileDirRoot, "*.ClassData", SearchOption.AllDirectories))
			{
				var classDataDoc = XDocument.Load(classDataPathname);
				foreach (var rtElement in classDataDoc.Element("classdata").Elements("rt"))
				{
					var className = rtElement.Attribute("class").Value;
					switch (className)
					{
						case "LangProject":
							highLevelData.Add(className, rtElement);
							break;
						case "LexDb":
							highLevelData.Add(className, rtElement);
							break;
					}

					DataSortingService.SortAndStoreElement(sortedData, interestingPropertiesCache, rtElement);
				}
			}

			// NB: These are flattened in reverse order from that of nesting, since I think 'sortedData' will be need for re-establishing some distal properties.
			// TODO: When 'sortedData' is a parm to all Flatten calls, then the loop here can go away.

			// TODO: Add Scripture Domain and 'leftover' Domain.
			//ScriptureBoundedContextService.RestoreOriginalFile(writer, readerSettings, multiFileDirRoot);

			AnthropologyDomainServices.FlattenDomain(highLevelData, sortedData, interestingPropertiesCache, pathRoot);
			LinguisticsDomainServices.FlattenDomain(highLevelData, sortedData, interestingPropertiesCache, pathRoot);

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
			if (!Directory.Exists(baseDataFolder))
				return;

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