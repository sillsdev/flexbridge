using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Contexts;
using Palaso.Xml;

namespace FLEx_ChorusPlugin.Infrastructure.DomainServices
{
	/// <summary>
	/// Service that will manage the multiple files and original fwdata file for a full FW data set.
	///
	/// The task of the service is twofold:
	/// 1. Break up the main fwdata file into multiple files
	///		A. One file for the custom property declarations (even if there are no custom properties), and
	///		B. One file for the model version
	///		C. Various files for the CmObject data.
	/// 2. Put the multiple files back together into the main fwdata file,
	///		but only if a Send/Receive had new information brought back into the local repo.
	///		NB: The client of the service decides if new information was found, and decides to call the service, or not.
	/// </summary>
	internal static class MultipleFileServices
	{
		internal static void RestoreMainFile(string mainFilePathname, string projectName)
		{
			FileWriterService.CheckPathname(mainFilePathname);

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
					var customFieldElements = doc.Root.Elements("CustomField").ToList();
					if (customFieldElements.Any())
					{
						foreach (var cf in customFieldElements)
						{
							cf.Attribute("key").Remove();
							// Restore type attr for object values.
							var propType = cf.Attribute("type").Value;
							cf.Attribute("type").Value = RestoreAdjustedTypeValue(propType);

							DataSortingService.CacheProperty(interestingPropertiesCache[cf.Attribute(SharedConstants.Class).Value], new FdoPropertyInfo(cf.Attribute(SharedConstants.Name).Value, propType, true));
						}
						FileWriterService.WriteElement(writer, readerSettings, SharedConstants.Utf8.GetBytes(doc.Root.ToString()));
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

		internal static void BreakupMainFile(string mainFilePathname, string projectName)
		{
			FileWriterService.CheckPathname(mainFilePathname);

			DeleteOldFiles(Path.GetDirectoryName(mainFilePathname), projectName);
			RestoreFiles(mainFilePathname, projectName);

#if DEBUG
			// Enable ONLY for testing a round trip.
			//RestoreMainFile(mainFilePathname, projectName);
#endif
		}

		private static void RestoreFiles(string mainFilePathname, string projectName)
		{
			var mdc = MetadataCache.MdCache; // Upgrade is done shortly.

			var pathRoot = Path.GetDirectoryName(mainFilePathname);
			var readerSettings = new XmlReaderSettings { IgnoreWhitespace = true };
			// 1. Write version number file.
			using (var reader = XmlReader.Create(mainFilePathname, readerSettings))
			{
				reader.MoveToContent();
				reader.MoveToAttribute("version");
				var version = reader.Value;
				FileWriterService.WriteVersionNumberFile(pathRoot, projectName, version);
				mdc.UpgradeToVersion(int.Parse(version));
			}

			var interestingPropertiesCache = DataSortingService.CacheInterestingProperties(mdc);
			// Outer Dict has the class name for its key and a sorted (by guid) dictionary as its value.
			// The inner dictionary has a caseless guid as the key and the byte array as the value.
			var classData = new Dictionary<string, SortedDictionary<string, XElement>>(200, StringComparer.OrdinalIgnoreCase);
			var guidToClassMapping = new Dictionary<string, string>();
			using (var fastSplitter = new FastXmlElementSplitter(mainFilePathname))
			{
				var haveWrittenCustomFile = false;
				bool foundOptionalFirstElement;
				// NB: The main input file *does* have to deal with the optional first element.
				foreach (var record in fastSplitter.GetSecondLevelElementBytes(SharedConstants.AdditionalFieldsTag, SharedConstants.RtTag, out foundOptionalFirstElement))
				{
					if (foundOptionalFirstElement)
					{
						// 2. Write custom properties file.
						FileWriterService.WriteCustomPropertyFile(mdc, interestingPropertiesCache, readerSettings, pathRoot, projectName, record);
						foundOptionalFirstElement = false;
						haveWrittenCustomFile = true;
					}
					else
					{
						CacheDataRecord(interestingPropertiesCache, classData, guidToClassMapping, record);
					}
				}
				if (!haveWrittenCustomFile)
				{
					// Write empty file.
					FileWriterService.WriteCustomPropertyFile(Path.Combine(pathRoot, projectName + ".CustomProperties"), readerSettings, null);
				}
			}

			// 3. Write all data files, here and there. [NB: The CmObject data in the XElements of 'classData' has all been sorted by this point.]
			BaseDomainServices.WriteDomainData(mdc, pathRoot, readerSettings, classData, guidToClassMapping, interestingPropertiesCache);
		}

		private static void DeleteOldFiles(string pathRoot, string projectName)
		{
			// Wipe out custom props file, as it will be re-created, even if it only has the root element in it.
			var customPropPathname = Path.Combine(pathRoot, projectName + ".CustomProperties");
			if (File.Exists(customPropPathname))
				File.Delete(customPropPathname);
			// Delete ModelVersion file, but it gets rewritten soon.
			var modelVersionPathname = Path.Combine(pathRoot, projectName + ".ModelVersion");
			if (File.Exists(modelVersionPathname))
				File.Delete(modelVersionPathname);

			// Deletes stuff in old and new locations. And (for now) makes sure "DataFiles" folder exists.
			// Brutal, but effective. :-) (But, leaves all ChorusNotes files.)
			BaseDomainServices.RemoveDomainData(pathRoot);
		}

		private static void CacheDataRecord(
			IDictionary<string, Dictionary<string, HashSet<string>>> sortablePropertiesCache,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			IDictionary<string, string> guidToClassMapping,
			byte[] record)
		{
			var rtElement = XElement.Parse(SharedConstants.Utf8.GetString(record));
			var className = rtElement.Attribute(SharedConstants.Class).Value;
			var guid = rtElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
			guidToClassMapping.Add(guid, className);

			// 1. Remove 'Checksum' from wordforms.
			if (className == "WfiWordform")
			{
				// Always remove it, and force re-parse.
				// NB: If this is ever removed, then some sort of pre-merge will need to be reinstated for the property.
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
