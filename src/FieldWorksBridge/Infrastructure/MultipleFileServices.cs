using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHanders.FieldWorks;
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
		internal static readonly Encoding Utf8 = Encoding.UTF8;
		private const string OptionalFirstElementTag = "AdditionalFields";
		private const string StartTag = "rt";

		internal static void BreakupMainFile(string mainFilePathname, string projectName)
		{
			CheckPathname(mainFilePathname);

			var pathRoot = Path.GetDirectoryName(mainFilePathname);
// ReSharper disable AssignNullToNotNullAttribute
			var multiFileDirRoot = Path.Combine(pathRoot, "DataFiles");
// ReSharper restore AssignNullToNotNullAttribute
			var customPropPathname = Path.Combine(pathRoot, projectName + ".CustomProperties");
			if (File.Exists(customPropPathname))
				File.Delete(customPropPathname);
			// Leave ModelVersion file.

			if (Directory.Exists(multiFileDirRoot))
			{
				// Brutal, but effective. :-)
				FileWriterService.RemoveDomainData(pathRoot); // Deletes stuff in old and new locations.
				// Leave all ChorusNotes files.
			}
			else
			{
				Directory.CreateDirectory(multiFileDirRoot);
			}

			var mdc = new MetadataCache();
			var interestingPropertiesCache = DataSortingService.CacheInterestingProperties(mdc);

			// Outer Dict has the class name for its key and a sorted (by guid) dictionary as its value.
			// The inner dictionary has a caseless guid as the key and the byte array as the value.
			var classData = new Dictionary<string, SortedDictionary<string, XElement>>(200, StringComparer.OrdinalIgnoreCase);
			var guidToClassMapping = new Dictionary<string, string>();
			byte[] optionalFirstElement = null;
			using (var fastSplitter = new FastXmlElementSplitter(mainFilePathname))
			{
				bool foundOptionalFirstElement;
				// NB: The main input file *does* have to deal with the optional first element.
				foreach (var record in fastSplitter.GetSecondLevelElementBytes(OptionalFirstElementTag, StartTag, out foundOptionalFirstElement))
				{
					if (foundOptionalFirstElement)
					{
						// Cache custom prop file for later write.
						var cpElement = DataSortingService.SortCustomPropertiesRecord(Utf8.GetString(record));
						// Add custom property info to MDC, since it may need to be sorted in the data files.
						foreach (var propElement in cpElement.Elements("CustomField"))
						{
// ReSharper disable PossibleNullReferenceException
							var className = propElement.Attribute("class").Value;
							var propName = propElement.Attribute("name").Value;
							var typeAttr = propElement.Attribute("type");
							var adjustedTypeValue = AdjustedPropertyType(interestingPropertiesCache, className, propName, typeAttr.Value);
// ReSharper disable RedundantCheckBeforeAssignment
							if (adjustedTypeValue != typeAttr.Value)
								typeAttr.Value = adjustedTypeValue;
// ReSharper restore RedundantCheckBeforeAssignment
							var customProp = new FdoPropertyInfo(
								propName,
								typeAttr.Value,
								true);
							DataSortingService.CacheProperty(interestingPropertiesCache[className], customProp);
							mdc.AddCustomPropInfo(
								className,
								customProp);
// ReSharper restore PossibleNullReferenceException
						}
						optionalFirstElement = Utf8.GetBytes(cpElement.ToString());
						foundOptionalFirstElement = false;
					}
					else
					{
						CacheDataRecord(interestingPropertiesCache, classData, guidToClassMapping, record);
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

			// Write version number file.
			FileWriterService.WriteVersionNumberFile(pathRoot, projectName, version);
			// Write custom properties file, even if has no custom innards.
			var readerSettings = new XmlReaderSettings { IgnoreWhitespace = true };
			FileWriterService.WriteCustomPropertyFile(Path.Combine(pathRoot, projectName + ".CustomProperties"), readerSettings, optionalFirstElement);

			// NB: The CmObject data in the byte arrays of 'classData' has all been sorted by this point.
			var skipwriteEmptyClassFiles = new HashSet<string>();
			FileWriterService.WriteDomainData(mdc, pathRoot, readerSettings, classData, guidToClassMapping, interestingPropertiesCache, skipwriteEmptyClassFiles);

			// TODO: Once everything is in the BCs, then there should be nothing left in the 'classData' dictionary,
			// TODO: so no class data will be left to write at the 'multiFileDirRoot' level in the following code.
			// Write data records in guid sorted order.
			// Write class file for each concrete class, whether it has data or not.
			foreach (var className in mdc.AllConcreteClasses.Select(concClassInfo => concClassInfo.ClassName))
			{
				SortedDictionary<string, XElement> sortedInstanceData;
				if (classData.TryGetValue(className, out sortedInstanceData))
				{
					// Only write one file, since there are no more high volume instacnes here.
					FileWriterService.WriteSecondaryFile(Path.Combine(multiFileDirRoot, className + ".ClassData"), readerSettings, sortedInstanceData);
				}
				else
				{
					// Write empty class file, unless it is empty by reason of it being emptied by a Bounded Context.
					if (!skipwriteEmptyClassFiles.Contains(className))
						FileWriterService.WriteSecondaryFile(Path.Combine(multiFileDirRoot, className + ".ClassData"), readerSettings, null);
				}
			}
			//RestoreMainFile(mainFilePathname, projectName);
		}

		internal static void RestoreMainFile(string mainFilePathname, string projectName)
		{
			CheckPathname(mainFilePathname);

			var pathRoot = Path.GetDirectoryName(mainFilePathname);
			var tempPathname = Path.GetTempFileName();
			var mdc = new MetadataCache();
			var interestingPropertiesCache = DataSortingService.CacheInterestingProperties(mdc);

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
					writer.WriteAttributeString("version", splitModelVersionData[1].Trim());

					// Write out optional custom property file.
					// Actually, the file will exist, even if it has nothing in it, but the "AdditionalFields" root element.
					var optionalCustomPropFile = Path.Combine(pathRoot, projectName + ".CustomProperties");
					// Remove 'key' attribute from CustomField elements, before writing to main file.
					var doc = XDocument.Load(optionalCustomPropFile);
// ReSharper disable PossibleNullReferenceException
					var customFieldElements = doc.Root.Elements("CustomField");
// ReSharper restore PossibleNullReferenceException
					if (customFieldElements.Count() > 0)
					{
// ReSharper disable PossibleNullReferenceException
						foreach (var cf in customFieldElements)
						{
							cf.Attribute("key").Remove();
							// Restore type attr for object values.
							var propType = cf.Attribute("type").Value;
							cf.Attribute("type").Value = RestoreAdjustedTypeValue(propType);

							DataSortingService.CacheProperty(interestingPropertiesCache[cf.Attribute("class").Value], new FdoPropertyInfo(cf.Attribute("name").Value, propType, true));
						}
						FileWriterService.WriteElement(writer, readerSettings, Utf8.GetBytes(doc.Root.ToString()));
// ReSharper restore PossibleNullReferenceException
					}
					FileWriterService.RestoreDomainData(writer, readerSettings, interestingPropertiesCache, pathRoot);
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
			// Just because all of this is true, doesn't mean it is a FW 7.0 related file. :-(
			if (!string.IsNullOrEmpty(mainFilePathname) // No null or empty string can be valid.
				&& File.Exists(mainFilePathname) // There has to be an actual file,
// ReSharper disable PossibleNullReferenceException
				&& Path.GetExtension(mainFilePathname).ToLowerInvariant() == ".fwdata")
// ReSharper restore PossibleNullReferenceException
				return;

			throw new ApplicationException("Cannot process the given file.");
		}

		private static void CacheDataRecord(Dictionary<string, Dictionary<string, HashSet<string>>> sortablePropertiesCache, IDictionary<string, SortedDictionary<string, XElement>> classData, IDictionary<string, string> guidToClassMapping, byte[] record)
		{
			var rtElement = XElement.Parse(Utf8.GetString(record));
// ReSharper disable PossibleNullReferenceException
			var className = rtElement.Attribute("class").Value;
			var guid = rtElement.Attribute("guid").Value;
			guidToClassMapping.Add(guid.ToLowerInvariant(), className);
// ReSharper restore PossibleNullReferenceException

			// 1. Remove 'Checksum' from wordforms.
			if (className == "WfiWordform")
			{
				var csElement = rtElement.Element("Checksum");
				if (csElement != null)
					csElement.Remove();
			}

			// 2. Sort <rt>
			DataSortingService.SortMainElement(sortablePropertiesCache, rtElement);

			// 3. Cache it.
			SortedDictionary<string, XElement> recordData;
			if (!classData.TryGetValue(className, out recordData))
			{
				recordData = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
				classData.Add(className, recordData);
			}
			recordData.Add(guid, rtElement);
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
									{DataSortingService.Collections, new HashSet<string>()},
									{DataSortingService.MultiAlt, new HashSet<string>()}
								};
				sortablePropertiesCache.Add(className, classProps);
			}
			var collProps = classProps[DataSortingService.Collections];
			collProps.Add(propName);
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
	}
}
