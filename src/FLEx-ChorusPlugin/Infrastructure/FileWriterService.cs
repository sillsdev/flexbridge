using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Contexts;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using FLEx_ChorusPlugin.Properties;
using Palaso.Xml;

namespace FLEx_ChorusPlugin.Infrastructure
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
				var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement(SharedConstants.OptionalFirstElementTag));
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
													 IDictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache,
													 XmlReaderSettings readerSettings,
													 string pathRoot,
													 string projectName,
													 byte[] record)
		{
			var cpElement = DataSortingService.SortCustomPropertiesRecord(SharedConstants.Utf8.GetString(record));
			// Add custom property info to MDC, since it may need to be sorted in the data files.
			foreach (var propElement in cpElement.Elements("CustomField"))
			{
				var className = propElement.Attribute("class").Value;
				var propName = propElement.Attribute("name").Value;
				var typeAttr = propElement.Attribute("type");
				var adjustedTypeValue = AdjustedPropertyType(interestingPropertiesCache, className, propName, typeAttr.Value);
				if (adjustedTypeValue != typeAttr.Value)
					typeAttr.Value = adjustedTypeValue;
				var customProp = new FdoPropertyInfo(
					propName,
					typeAttr.Value,
					true);
				DataSortingService.CacheProperty(interestingPropertiesCache[className], customProp);
				mdc.AddCustomPropInfo(
					className,
					customProp);
			}
			WriteCustomPropertyFile(Path.Combine(pathRoot, projectName + ".CustomProperties"), readerSettings, SharedConstants.Utf8.GetBytes(cpElement.ToString()));
		}

		private static string AdjustedPropertyType(IDictionary<string, Dictionary<string, HashSet<string>>> sortablePropertiesCache, string className, string propName, string rawType)
		{
			string adjustedType;
			switch (rawType)
			{
				default:
					adjustedType = rawType;
					break;

				case "OC":
					adjustedType = "OwningCollection";
					AddCollectionPropertyToCache(sortablePropertiesCache, className, propName);
					break;
				case "RC":
					adjustedType = "ReferenceCollection";
					AddCollectionPropertyToCache(sortablePropertiesCache, className, propName);
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

		private static void AddCollectionPropertyToCache(IDictionary<string, Dictionary<string, HashSet<string>>> sortablePropertiesCache, string className, string propName)
		{
			Dictionary<string, HashSet<string>> classProps;
			if (!sortablePropertiesCache.TryGetValue(className, out classProps))
			{
				classProps = new Dictionary<string, HashSet<string>>(2)
								{
									{SharedConstants.Collections, new HashSet<string>()},
									{SharedConstants.MultiAlt, new HashSet<string>()}
								};
				sortablePropertiesCache.Add(className, classProps);
			}
			var collProps = classProps[SharedConstants.Collections];
			collProps.Add(propName);
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

		internal static void RestoreMainFile(string mainFilePathname, string projectName)
		{
			CheckPathname(mainFilePathname);

			var pathRoot = Path.GetDirectoryName(mainFilePathname);
			var tempPathname = Path.GetTempFileName();

			try
			{
				// There is no particular reason to ensure the order of objects in 'mainFilePathname' is retained,
				// but the optional custom props element must be first.
				var readerSettings = new XmlReaderSettings { IgnoreWhitespace = true };
				// NB: This should follow current FW write settings practice.
				var fwWriterSettings = new XmlWriterSettings
										{
											OmitXmlDeclaration = false,
											CheckCharacters = true,
											ConformanceLevel = ConformanceLevel.Document,
											Encoding = new UTF8Encoding(false),
											Indent = true,
											IndentChars = (""),
											NewLineOnAttributes = false
										};

				using (var writer = XmlWriter.Create(tempPathname, fwWriterSettings))
				{
					writer.WriteStartElement("languageproject");

					// Write out version number from the ModelVersion file.
					var modelVersionData = File.ReadAllText(Path.Combine(pathRoot, projectName + ".ModelVersion"));
					var splitModelVersionData = modelVersionData.Split(new[] { "{", ":", "}" }, StringSplitOptions.RemoveEmptyEntries);
					var version = splitModelVersionData[1].Trim();
					writer.WriteAttributeString("version", version);

					var mdc = MetadataCache.MdCache; // This may really need to be a reset
					mdc.UpgradeToVersion(Int32.Parse(version));
					var interestingPropertiesCache = DataSortingService.CacheInterestingProperties(mdc);

					// Write out optional custom property data.
					// The foo.CustomProperties file will exist, even if it has nothing in it, but the "AdditionalFields" root element.
					var optionalCustomPropFile = Path.Combine(pathRoot, projectName + ".CustomProperties");
					// Remove 'key' attribute from CustomField elements, before writing to main file.
					var doc = XDocument.Load(optionalCustomPropFile);
					var customFieldElements = doc.Root.Elements("CustomField");
					if (customFieldElements.Count() > 0)
					{
						foreach (var cf in customFieldElements)
						{
							cf.Attribute("key").Remove();
							// Restore type attr for object values.
							var propType = cf.Attribute("type").Value;
							cf.Attribute("type").Value = RestoreAdjustedTypeValue(propType);

							DataSortingService.CacheProperty(interestingPropertiesCache[cf.Attribute("class").Value], new FdoPropertyInfo(cf.Attribute("name").Value, propType, true));
						}
						WriteElement(writer, readerSettings, SharedConstants.Utf8.GetBytes(doc.Root.ToString()));
					}

					BaseDomainServices.RestoreDomainData(writer, readerSettings, interestingPropertiesCache, pathRoot);

					writer.WriteEndElement();
				}

				File.Copy(tempPathname, mainFilePathname, true);
			}
			finally
			{
				if (File.Exists(tempPathname))
					File.Delete(tempPathname);
			}
		}

		private static string RestoreAdjustedTypeValue(string storedType)
		{
			string adjustedType;
			switch (storedType)
			{
				default:
					adjustedType = storedType;
					break;

				case "OwningCollection":
					adjustedType = "OC";
					break;
				case "ReferenceCollection":
					adjustedType = "RC";
					break;

				case "OwningSequence":
					adjustedType = "OS";
					break;
				case "ReferenceSequence":
					adjustedType = "RS";
					break;

				case "OwningAtomic":
					adjustedType = "OA";
					break;
				case "ReferenceAtomic":
					adjustedType = "RA";
					break;
			}
			return adjustedType;
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