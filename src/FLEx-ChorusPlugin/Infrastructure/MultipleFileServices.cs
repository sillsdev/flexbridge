using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Contexts;
using Palaso.Xml;

namespace FLEx_ChorusPlugin.Infrastructure
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
		internal static void RestoreMainFile(string mainFilePathname, string projectName)
		{
			FileWriterService.RestoreMainFile(mainFilePathname, projectName);
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
				bool foundOptionalFirstElement;
				// NB: The main input file *does* have to deal with the optional first element.
				foreach (var record in fastSplitter.GetSecondLevelElementBytes(SharedConstants.OptionalFirstElementTag, SharedConstants.RtTag, out foundOptionalFirstElement))
				{
					if (foundOptionalFirstElement)
					{
						// 2. Write custom properties file, even if has no custom innards.
						FileWriterService.WriteCustomPropertyFile(mdc, interestingPropertiesCache, readerSettings, pathRoot, projectName, record);
						foundOptionalFirstElement = false;
					}
					else
					{
						CacheDataRecord(interestingPropertiesCache, classData, guidToClassMapping, record);
					}
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
			var className = rtElement.Attribute("class").Value;
			var guid = rtElement.Attribute(SharedConstants.GuidStr).Value;
			guidToClassMapping.Add(guid.ToLowerInvariant(), className);

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
	}
}
