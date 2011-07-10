using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHanders.FieldWorks;
using Palaso.Xml;

namespace FieldWorksBridge.Infrastructure
{
	internal static class FileWriterService
	{
		internal static void WriteSecondaryFile(string newPathname, XmlReaderSettings readerSettings, SortedDictionary<string, byte[]> data)
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

		internal static void WriteSecondaryFiles(string multiFileDirRoot, string className, XmlReaderSettings readerSettings, SortedDictionary<string, byte[]> data)
		{
			// Divide 'data' into the 10 zero-based buckets.
			var bucket0 = new SortedDictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
			var bucket1 = new SortedDictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
			var bucket2 = new SortedDictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
			var bucket3 = new SortedDictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
			var bucket4 = new SortedDictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
			var bucket5 = new SortedDictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
			var bucket6 = new SortedDictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
			var bucket7 = new SortedDictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
			var bucket8 = new SortedDictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
			var bucket9 = new SortedDictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

			foreach (var kvp in data)
			{
				var key = kvp.Key;
				var bucket = (int)((uint)new Guid(key).GetHashCode() % 10);
				SortedDictionary<string, byte[]> currentBucket;
				switch(bucket)
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

		internal static void WriteBoundedContexts(MetadataCache mdc, string multiFileDirRoot, XmlReaderSettings readerSettings, Dictionary<string, SortedDictionary<string, byte[]>> classData, Dictionary<string, string> guidToClassMapping, HashSet<string> skipwriteEmptyClassFiles)
		{
			ReversalBoundedContextService.ExtractBoundedContexts(readerSettings, multiFileDirRoot, mdc, classData, guidToClassMapping, skipwriteEmptyClassFiles);
			TextCorpusBoundedContextService.ExtractBoundedContexts(readerSettings, multiFileDirRoot, mdc, classData, guidToClassMapping, skipwriteEmptyClassFiles);
			DiscourseAnalysisBoundedContextService.ExtractBoundedContexts(readerSettings, multiFileDirRoot, mdc, classData, guidToClassMapping, skipwriteEmptyClassFiles);
			WordformInventoryBoundedContextService.ExtractBoundedContexts(readerSettings, multiFileDirRoot, mdc, classData, guidToClassMapping, skipwriteEmptyClassFiles);

			// Remove the data that may be in multiple bounded Contexts.
			// Eventually, there ought not be an need for writing the leftovers in the base folder,
			// but I'm not there yet.

			//skipwriteEmptyClassFiles.Add("Note");

			//classData.Remove("Note");
		}

		internal static void RestoreBoundedContexts(XmlWriter writer, XmlReaderSettings readerSettings, string multiFileDirRoot)
		{
			ReversalBoundedContextService.RestoreOriginalFile(writer, readerSettings, multiFileDirRoot);
			TextCorpusBoundedContextService.RestoreOriginalFile(writer, readerSettings, multiFileDirRoot);
			DiscourseAnalysisBoundedContextService.RestoreOriginalFile(writer, readerSettings, multiFileDirRoot);
			WordformInventoryBoundedContextService.RestoreOriginalFile(writer, readerSettings, multiFileDirRoot);
		}

		internal static void WriteObject(MetadataCache mdc,
			IDictionary<string, SortedDictionary<string, byte[]>> classData, IDictionary<string, string> guidToClassMapping,
			string baseDir,
			XmlReaderSettings readerSettings, Dictionary<string, SortedDictionary<string, byte[]>> multiClassOutput, string guid,
			HashSet<string> omitProperties)
		{
			var dataBytes = ObjectFinderServices.RegisterDataInBoundedContext(classData, guidToClassMapping, multiClassOutput, guid);
			ObjectFinderServices.CollectAllOwnedObjects(mdc,
														classData, guidToClassMapping, multiClassOutput,
														XElement.Parse(MultipleFileServices.Utf8.GetString(dataBytes)),
														omitProperties);
			foreach (var kvp in multiClassOutput)
				WriteSecondaryFile(Path.Combine(baseDir, kvp.Key + ".ClassData"), readerSettings, kvp.Value);
		}
	}
}