﻿﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
﻿﻿using Chorus.Utilities;
﻿﻿using FLEx_ChorusPlugin.Contexts;
﻿﻿using Palaso.Code;
using Palaso.Progress;
using Palaso.Xml;

namespace FLEx_ChorusPlugin.Infrastructure.DomainServices
{
	/// <summary>
	/// Encapsulates the splitting of a fieldworks project file as a state machine.
	/// The task is quantized to make use of a palaso progress bar.
	/// Most of the subtasks need to share two dictionaries that are protected
	/// from "public static" access in this class. (ie., main reason to encapsulate)
	/// The subtasks vary greatly in granulatity with most of the time
	/// spent in writing and caching xml "properties" and writing out files.
	/// Two static methods from MultipleFileServices were moved here since
	/// they are only used in this task.
	///
	/// Randy's summary:
	///  1. Break up the main fwdata file into multiple files
	///		A. One file for the custom property declarations (even if there are no custom properties), and
	///		B. One file for the model version
	///		C. Various files for the CmObject data.
	/// </summary>
	internal static class FLExProjectSplitter
	{
		internal static void CheckForUserCancelRequested(IProgress progress)
		{
			if (progress.CancelRequested)
				throw new UserCancelledException(); // the Chorus Synchorinizer class catches this and does the real cancel.
		}

		internal static void PushHumptyOffTheWall(IProgress progress, string mainFilePathname)
		{
			PushHumptyOffTheWall(progress, true, mainFilePathname);
		}

		internal static void PushHumptyOffTheWall(IProgress progress, bool writeVerbose, string mainFilePathname)
		{
			Guard.AgainstNull(progress, "progress");
			FileWriterService.CheckFilename(mainFilePathname);

			var rootDirectoryName = Path.GetDirectoryName(mainFilePathname);
			// NB: This is strictly an ordered list of method calls.
			// Don't even 'think' of changing any of them.
			CheckForUserCancelRequested(progress);
			DeleteOldFiles(rootDirectoryName);
			CheckForUserCancelRequested(progress);
			WriteVersionFile(mainFilePathname);
			// Outer Dict has the class name for its key and a sorted (by guid) dictionary as its value.
			// The inner dictionary has a caseless guid as the key and the byte array as the value.
			// (Only has current concrete classes.)
			var classData = GenerateBasicClassData();
			var guidToClassMapping = WriteOrCacheProperties(mainFilePathname, classData);
			CheckForUserCancelRequested(progress);
			BaseDomainServices.PushHumptyOffTheWall(progress, writeVerbose, rootDirectoryName, classData, guidToClassMapping);

#if DEBUG
			// Enable ONLY for testing a round trip.
			// FLExProjectUnifier.PutHumptyTogetherAgain(progress, writeVerbose, mainFilePathname);
#endif
		}

		private static void DeleteOldFiles(string pathRoot)
		{
			// Wipe out custom props file, as it will be re-created, even if it only has the root element in it.
			var customPropPathname = Path.Combine(pathRoot, SharedConstants.CustomPropertiesFilename);
			if (File.Exists(customPropPathname))
				File.Delete(customPropPathname);
			// Delete ModelVersion file, but it gets rewritten soon.
			var modelVersionPathname = Path.Combine(pathRoot, SharedConstants.ModelVersionFilename);
			if (File.Exists(modelVersionPathname))
				File.Delete(modelVersionPathname);

			// Deletes all files in new locations, except the current ChorusNotes files.
			BaseDomainServices.RemoveDomainData(pathRoot);
		}

		private static void WriteVersionFile(string mainFilePathname)
		{
			var pathRoot = Path.GetDirectoryName(mainFilePathname);
			// 1. Write version number file.
			using (var reader = XmlReader.Create(mainFilePathname, CanonicalXmlSettings.CreateXmlReaderSettings()))
			{
				reader.MoveToContent();
				reader.MoveToAttribute("version");
				var version = reader.Value;
				FileWriterService.WriteVersionNumberFile(pathRoot, version);
				MetadataCache.MdCache.UpgradeToVersion(Int32.Parse(version));
			}
		}

		private static Dictionary<string, string> WriteOrCacheProperties(string mainFilePathname, Dictionary<string, SortedDictionary<string, string>> classData)
		{
			var pathRoot = Path.GetDirectoryName(mainFilePathname);
			var mdc = MetadataCache.MdCache;
			// Key is the guid of the object, and value is the class name.
			var guidToClassMapping = new Dictionary<string, string>();
			using (var fastSplitter = new FastXmlElementSplitter(mainFilePathname))
			{
				var haveWrittenCustomFile = false;
				bool foundOptionalFirstElement;
				// NB: The main input file *does* have to deal with the optional first element.
				foreach (var record in fastSplitter.GetSecondLevelElementStrings(SharedConstants.AdditionalFieldsTag, SharedConstants.RtTag, out foundOptionalFirstElement))
				{
					if (foundOptionalFirstElement)
					{
						// 2. Write custom properties file with custom properties.
						FileWriterService.WriteCustomPropertyFile(mdc, pathRoot, record);
						foundOptionalFirstElement = false;
						haveWrittenCustomFile = true;
					}
					else
					{
						CacheDataRecord(record, classData, guidToClassMapping);
					}
				}
				if (!haveWrittenCustomFile)
				{
					// Write empty custom properties file.
					FileWriterService.WriteCustomPropertyFile(Path.Combine(pathRoot, SharedConstants.CustomPropertiesFilename), null);
				}
			}
			return guidToClassMapping;
		}

		private static Dictionary<string, SortedDictionary<string, string>> GenerateBasicClassData()
		{
			return MetadataCache.MdCache.AllConcreteClasses.ToDictionary(fdoClassInfo => fdoClassInfo.ClassName, fdoClassInfo => new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase));
		}

		private static void CacheDataRecord(string record, Dictionary<string, SortedDictionary<string, string>> classData, Dictionary<string, string> guidToClassMapping)
		{
			var attrValues = XmlUtils.GetAttributes(record, new HashSet<string> {SharedConstants.Class, SharedConstants.GuidStr});
			var className = attrValues[SharedConstants.Class];
			var guid = attrValues[SharedConstants.GuidStr].ToLowerInvariant();
			guidToClassMapping.Add(guid, className);

			// Theory has it the FW data is sorted.
			//// 1. Sort <rt>
			//DataSortingService.SortMainRtElement(rtElement);

			// 2. Cache it.
			classData[className].Add(guid, record);
		}
	}
}
