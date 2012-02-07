using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Contexts.General
{
	internal static class OldStyleDomainServices
	{
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

		internal static void WriteData(string pathRoot, MetadataCache mdc, Dictionary<string, SortedDictionary<string, XElement>> classData)
		{
			// TODO??: Maybe put everything that is left in "classData" in the 'General' context as: 1) series of regular 'rt' elements, or 2) nested objects, with the top elements being the unowned ones.
			// TODO: Once everything is in the BCs, then there should be nothing left in the 'classData' dictionary,
			// TODO: so no class data will be left to write at the 'multiFileDirRoot' level in the following code.
			// Write data records in guid sorted order.
			// Write class file for each concrete class, whether it has data or not.
			var multiFileDirRoot = Path.Combine(pathRoot, "DataFiles");
			foreach (var className in mdc.AllConcreteClasses.Select(concClassInfo => concClassInfo.ClassName))
			{
				var classDataPathname = Path.Combine(multiFileDirRoot, className + ".ClassData");
				SortedDictionary<string, XElement> sortedInstanceData = classData[className];
				if (sortedInstanceData.Count == 0)
					continue; // Skip all empties.
				// Only write one file, since there are no more high volume instances here.
				FileWriterService.WriteSecondaryFile(classDataPathname, sortedInstanceData);
			}
		}

		internal static void RemoveDataFiles(string pathRoot)
		{
			var multiFileDirRoot = Path.Combine(pathRoot, "DataFiles");
			if (!Directory.Exists(multiFileDirRoot))
				return;

			// Delete all data files at any folder depth.
			foreach (var dataFilePathname in Directory.GetFiles(multiFileDirRoot, "*.ClassData", SearchOption.AllDirectories))
				File.Delete(dataFilePathname);

			FileWriterService.RemoveEmptyFolders(multiFileDirRoot, true);
		}

		internal static void RestoreFiles(XmlWriter writer, XmlReaderSettings readerSettings, string baseDir)
		{
			if (!Directory.Exists(baseDir))
				return;

			WriteClassDataToOriginal(writer, baseDir, readerSettings);

			foreach (var directory in Directory.GetDirectories(baseDir))
				RestoreFiles(writer, readerSettings, directory);
		}

		internal static void RestoreOldStyleData(SortedDictionary<string, XElement> sortedData, SortedDictionary<string, XElement> highLevelData, string pathRoot)
		{
			// NB: These are flattened in reverse order from that of nesting, since I think 'sortedData' will be need for re-establishing some distal properties.
			// TODO: When 'sortedData' is a parm to all Flatten calls, then the loop here can go away.
			// TODO: 'leftover' Domain.
			var multiFileDirRoot = Path.Combine(pathRoot, "DataFiles");
			foreach (var classDataPathname in Directory.GetFiles(multiFileDirRoot, "*.ClassData", SearchOption.AllDirectories))
			{
				var classDataDoc = XDocument.Load(classDataPathname);
				foreach (var rtElement in classDataDoc.Element("classdata").Elements(SharedConstants.RtTag))
				{
					var className = rtElement.Attribute(SharedConstants.Class).Value;
					switch (className)
					{
						case "LangProject":
							highLevelData.Add(className, rtElement);
							break;
					}

					DataSortingService.SortAndStoreElement(sortedData, rtElement);
				}
			}
		}
	}
}
