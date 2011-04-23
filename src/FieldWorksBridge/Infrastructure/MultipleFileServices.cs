#define USEMULTIPLEFILES
#if USEMULTIPLEFILES
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHanders.FieldWorks;
using Chorus.Utilities;
using Palaso.Xml;

namespace FieldWorksBridge.Infrastructure
{
	/// <summary>
	/// Service that will manage the multiple files and original fwdata file for a full FW data set.
	/// </summary>
	/// <remarks>
	/// The task of the service is twofold:
	/// 1. Break up the main fwdata file into multiple files
	///		A. one for the custom property declarations, and
	///		B. one for each concrete CmObject class instance
	/// 2. Put the multiple files back together into the main fwdata file,
	///		but only if a Send/Receive had new information brought back into the local repo.
	///		NB: The client of the service decides if new information was found, and decides to call the service, or not.
	/// </remarks>
	internal static class MultipleFileServices
	{
		private static readonly Encoding Utf8 = Encoding.UTF8;
		private const string FirstElementTag = "AdditionalFields";
		private const string StartTag = "rt";
		/*
		<languageproject version="7000037">
		</languageproject>
		root\DataFiles\CustomProperties.fwdata
		root\DataFiles\ClassName.fwdata
		*/

		internal static void BreakupMainFile(string mainFilePathname)
		{
			CheckPathname(mainFilePathname);

			var pathRoot = Path.GetDirectoryName(mainFilePathname);
// ReSharper disable AssignNullToNotNullAttribute
			var multiFileDirRoot = Path.Combine(pathRoot, "DataFiles");
// ReSharper restore AssignNullToNotNullAttribute
			if (!Directory.Exists(multiFileDirRoot))
				Directory.CreateDirectory(multiFileDirRoot);

			// Outer Dict has the class name for its key and a sorted (by guid) dictionary as its value.
			// The inner dictionary has a caseless guid as the key and the byte array as the value.
			var classData = new Dictionary<string, SortedDictionary<string, byte[]>>(200, StringComparer.OrdinalIgnoreCase);
			byte[] optionalFirstElement = null;
			using (var fastSplitter = new FastXmlElementSplitter(mainFilePathname))
			{
				bool foundOptionalFirstElement;
				foreach (var record in fastSplitter.GetSecondLevelElementBytes(FirstElementTag, StartTag, out foundOptionalFirstElement))
				{
					if (foundOptionalFirstElement)
					{
						// Cache custom prop file for later write.
						optionalFirstElement = SortCustomPropertiesRecord(record);
						foundOptionalFirstElement = false;
					}
					else
					{
						CacheDataRecord(classData, record);
					}
				}
			}

			// Get the 'version' attr value from main file.
			string version;
			using (var reader = XmlReader.Create(mainFilePathname, new XmlReaderSettings {IgnoreWhitespace = true}))
			{
				reader.MoveToContent();
				reader.MoveToAttribute("version");
				version = reader.Value;
			}

			var readerSettings = new XmlReaderSettings { IgnoreWhitespace = true };
			// Write optional first element.
			if (optionalFirstElement != null)
				WriteSecondaryFile(Path.Combine(multiFileDirRoot, "CustomProperties.fwdata"), readerSettings, version, optionalFirstElement);

			// Write data records in guid sorted order.
			var highVolumeClasses = new HashSet<string> {"Segment", "WfiAnalysis", "WfiMorphBundle"};
			foreach (var kvp in classData)
			{
				var className = kvp.Key;
				if (highVolumeClasses.Contains(className))
				{
					// Get rid of original file.
					if (File.Exists(Path.Combine(multiFileDirRoot, className + ".fwdata")))
						File.Delete(Path.Combine(multiFileDirRoot, className + ".fwdata"));

					// Write 10 files for each high volume class.
					WriteSecondaryFiles(multiFileDirRoot, className, readerSettings, version, kvp.Value);
				}
				else
				{
					// Only write one file.
					WriteSecondaryFile(Path.Combine(multiFileDirRoot, className + ".fwdata"), readerSettings, version, kvp.Value);
				}
			}
		}

		private static byte[] SortCustomPropertiesRecord(byte[] optionalFirstElement)
		{
			var customPropertiesElement = XElement.Parse(Utf8.GetString(optionalFirstElement));

			// <CustomField name="Certified" class="WfiWordform" type="Boolean" />

			// 1. Sort child elements by using a compound key of 'class'+'name'.
			var sortedProperties = new SortedDictionary<string, XElement>();
			foreach (var customProperty in customPropertiesElement.Elements())
				sortedProperties.Add(customProperty.Attribute("class").Value + customProperty.Attribute("name").Value, customProperty);
			customPropertiesElement.Elements().Remove();
			foreach (var propertyKvp in sortedProperties)
				customPropertiesElement.Add(propertyKvp.Value);

			// Sort all attributes.
			SortAttributes(customPropertiesElement);

			return Utf8.GetBytes(customPropertiesElement.ToString());
		}

		private static void SortAttributes(XElement element)
		{
			if (element.HasElements)
			{
				foreach (var childElement in element.Elements())
					SortAttributes(childElement);
			}

			if (!element.HasAttributes || element.Attributes().Count() <= 1)
				return;

			var sortedAttributes = new SortedDictionary<string, XAttribute>();
			foreach (var attr in element.Attributes())
				sortedAttributes.Add(attr.Name.LocalName, attr);

			element.Attributes().Remove();
			foreach (var sortedAttrKvp in sortedAttributes)
				element.Add(sortedAttrKvp.Value);
		}

		private static void WriteSecondaryFiles(string multiFileDirRoot, string className, XmlReaderSettings readerSettings, string version, SortedDictionary<string, byte[]> data)
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
			WriteSecondaryFile(basePath + "_01.fwdata", readerSettings, version, bucket0); // 1-based files vs 0-based buckets.
			WriteSecondaryFile(basePath + "_02.fwdata", readerSettings, version, bucket1);
			WriteSecondaryFile(basePath + "_03.fwdata", readerSettings, version, bucket2);
			WriteSecondaryFile(basePath + "_04.fwdata", readerSettings, version, bucket3);
			WriteSecondaryFile(basePath + "_05.fwdata", readerSettings, version, bucket4);
			WriteSecondaryFile(basePath + "_06.fwdata", readerSettings, version, bucket5);
			WriteSecondaryFile(basePath + "_07.fwdata", readerSettings, version, bucket6);
			WriteSecondaryFile(basePath + "_08.fwdata", readerSettings, version, bucket7);
			WriteSecondaryFile(basePath + "_09.fwdata", readerSettings, version, bucket8);
			WriteSecondaryFile(basePath + "_10.fwdata", readerSettings, version, bucket9);
		}

		private static void WriteSecondaryFile(string newPathname, XmlReaderSettings readerSettings, string version, SortedDictionary<string, byte[]> data)
		{
			if (File.Exists(newPathname))
				File.Delete(newPathname);
			using (var writer = XmlWriter.Create(newPathname, CanonicalXmlSettings.CreateXmlWriterSettings()))
			{
				writer.WriteStartElement("languageproject");
				writer.WriteAttributeString("version", version);
				foreach (var kvp in data)
					WriteElement(writer, readerSettings, kvp.Value);
				writer.WriteEndElement();
			}
		}

		private static void WriteSecondaryFile(string newPathname, XmlReaderSettings readerSettings, string version, byte[] element)
		{
			if (File.Exists(newPathname))
				File.Delete(newPathname);
			using (var writer = XmlWriter.Create(newPathname, CanonicalXmlSettings.CreateXmlWriterSettings()))
			{
				writer.WriteStartElement("languageproject");
				writer.WriteAttributeString("version", version);
				WriteElement(writer, readerSettings, element);
				writer.WriteEndElement();
			}
		}

		private static void WriteElement(XmlWriter writer, XmlReaderSettings readerSettings, byte[] optionalFirstElement)
		{
			using (var nodeReader = XmlReader.Create(new MemoryStream(optionalFirstElement, false), readerSettings))
				writer.WriteNode(nodeReader, true);
		}

		internal static void RestoreMainFile(string mainFilePathname)
		{
			// Q: Where to find the current model version?
			// A: For now, each file is 'fwdata' and thus must have the version number.

			CheckPathname(mainFilePathname);

			var pathRoot = Path.GetDirectoryName(mainFilePathname);
// ReSharper disable AssignNullToNotNullAttribute
			var multiFileDirRoot = Path.Combine(pathRoot, "DataFiles");
// ReSharper restore AssignNullToNotNullAttribute

			var tempPathname = Path.GetTempFileName();

			try
			{
				// There is no particular reason to ensure the order of objects in 'mainFilePathname' is retained,
				// but the custom props element must be first.

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

				var multipleFiles = Directory.GetFiles(multiFileDirRoot, "*.fwdata").ToList();
				using (var writer = XmlWriter.Create(tempPathname, fwWriterSettings))
				{
					writer.WriteStartElement("languageproject");

					// Write out version number from the first handy file.
					// Since the custom property file is optional, it can't really be used.
					using (var reader = XmlReader.Create(multipleFiles[0], readerSettings))
					{
						reader.MoveToContent();
						reader.MoveToAttribute("version");
						writer.WriteAttributeString("version", reader.Value);
					}

					// Write out optional custom property file.
					if (multipleFiles.Contains(Path.Combine(multiFileDirRoot, "CustomProperties.fwdata")))
					{
						using (var reader = XmlReader.Create(Path.Combine(multiFileDirRoot, "CustomProperties.fwdata"), readerSettings))
						{
							reader.MoveToContent();
							reader.Read();
							writer.WriteNode(reader, false);
						}
						multipleFiles.Remove(Path.Combine(multiFileDirRoot, "CustomProperties.fwdata"));
					}

					// Work on all other files, except the custom prop file.
					foreach (var pathname in multipleFiles)
					{
						using (var reader = XmlReader.Create(pathname, readerSettings))
						{
							reader.MoveToContent();
							reader.Read();
							while (reader.IsStartElement())
							{
								writer.WriteNode(reader, false);
							}
						}
					}
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

		private static void CheckPathname(string mainFilePathname)
		{
			var fwFileHandler = new FieldWorksFileHandler();
			if (fwFileHandler.CanValidateFile(mainFilePathname))
				return;

			throw new ApplicationException("Cannot process the given file.");
		}

		private static void CacheDataRecord(IDictionary<string, SortedDictionary<string, byte[]>> classData, byte[] record)
		{
			var rtElement = XElement.Parse(Utf8.GetString(record));
// ReSharper disable PossibleNullReferenceException
			var className = rtElement.Attribute("class").Value;
			var guid = rtElement.Attribute("guid").Value;
// ReSharper restore PossibleNullReferenceException

			// 1. Remove 'Checksum' from wordforms.
			if (className == "WfiWordform")
			{
				var csElement = rtElement.Element("Checksum");
				if (csElement != null)
					csElement.Remove();
			}

			// 2. Sort property elements of <rt>
			var sortedPropertyElements = new SortedDictionary<string, XElement>();
			foreach (var propertyElement in rtElement.Elements())
				sortedPropertyElements.Add(propertyElement.Name.LocalName, propertyElement);
			rtElement.Elements().Remove();
			foreach (var kvp in sortedPropertyElements)
				rtElement.Add(kvp.Value);

			// 3. Sort attributes at all levels.
			SortAttributes(rtElement);

			SortedDictionary<string, byte[]> recordData;
			if (!classData.TryGetValue(className, out recordData))
			{
				recordData = new SortedDictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
				classData.Add(className, recordData);
			}
			recordData.Add(guid, Utf8.GetBytes(rtElement.ToString()));
		}
	}
}
#endif
