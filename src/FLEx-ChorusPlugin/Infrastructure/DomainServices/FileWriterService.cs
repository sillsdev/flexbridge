using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Properties;
using Palaso.Xml;

namespace FLEx_ChorusPlugin.Infrastructure.DomainServices
{
	internal static class FileWriterService
	{
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
			using (var nodeReader = XmlReader.Create(new MemoryStream(SharedConstants.Utf8.GetBytes(nestedDoc.ToString()), false), readerSettings))
				writer.WriteNode(nodeReader, true);
		}

		internal static void WriteElement(XmlWriter writer, XmlReaderSettings readerSettings, XElement element)
		{
			using (var nodeReader = XmlReader.Create(new MemoryStream(SharedConstants.Utf8.GetBytes(element.ToString()), false), readerSettings))
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
				var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement(SharedConstants.AdditionalFieldsTag));
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

		internal static void WriteCustomPropertyFile(MetadataCache mdc,
													 XmlReaderSettings readerSettings,
													 string pathRoot,
													 string projectName,
													 byte[] record)
		{
			var cpElement = DataSortingService.SortCustomPropertiesRecord(SharedConstants.Utf8.GetString(record));
			// Add custom property info to MDC, since it may need to be sorted in the data files.
			var hasCustomProperties = false;
			foreach (var propElement in cpElement.Elements("CustomField"))
			{
				hasCustomProperties = true;
				var className = propElement.Attribute(SharedConstants.Class).Value;
				var propName = propElement.Attribute(SharedConstants.Name).Value;
				var typeAttr = propElement.Attribute("type");
				var adjustedTypeValue = AdjustedPropertyType(className, propName, typeAttr.Value);
				if (adjustedTypeValue != typeAttr.Value)
					typeAttr.Value = adjustedTypeValue;
				var customProp = new FdoPropertyInfo(
					propName,
					typeAttr.Value,
					true);
				mdc.AddCustomPropInfo(
					className,
					customProp);
			}
			if (hasCustomProperties)
				mdc.ResetCaches();
			WriteCustomPropertyFile(Path.Combine(pathRoot, projectName + ".CustomProperties"), readerSettings, SharedConstants.Utf8.GetBytes(cpElement.ToString()));
		}

		internal static string AdjustedPropertyType(string className, string propName, string rawType)
		{
			string adjustedType;
			switch (rawType)
			{
				default:
					adjustedType = rawType;
					break;

				case "OC":
					adjustedType = "OwningCollection";
					break;
				case "RC":
					adjustedType = "ReferenceCollection";
					break;

				case "OS":
					adjustedType = "OwningSequence";
					break;

				case "RS":
					adjustedType = "ReferenceSequence";
					break;

				case "OA":
					adjustedType = "OwningAtomic";
					break;

				case "RA":
					adjustedType = "ReferenceAtomic";
					break;
			}
			return adjustedType;
		}

		internal static void CheckPathname(string mainFilePathname)
		{
			// Just because all of this is true, doesn't mean it is a FW 7.0 related file. :-(
			if (!String.IsNullOrEmpty(mainFilePathname) // No null or empty string can be valid.
				&& File.Exists(mainFilePathname) // There has to be an actual file,
				&& Path.GetExtension(mainFilePathname).ToLowerInvariant() == ".fwdata")
				return;

			throw new ApplicationException("Cannot process the given file.");
		}

		internal static string GetExtensionFromPathname(string pathname)
		{
			if (String.IsNullOrEmpty(pathname))
				throw new ArgumentNullException("pathname");

			var extension = Path.GetExtension(pathname);
			return String.IsNullOrEmpty(extension)
					? null
					: (extension.Length == 1
						? extension
						: extension.Substring(1));
		}
	}
}