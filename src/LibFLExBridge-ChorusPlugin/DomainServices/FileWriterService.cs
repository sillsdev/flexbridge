// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibFLExBridgeChorusPlugin.Properties;
using LibTriboroughBridgeChorusPlugin;
using SIL.Xml;

namespace LibFLExBridgeChorusPlugin.DomainServices
{
	internal static class FileWriterService
	{
		internal static void WriteNestedFile(string newPathname, XmlNode root)
		{
			using (var writer = XmlWriter.Create(newPathname, CanonicalXmlSettings.CreateXmlWriterSettings()))
			{
				XmlUtils.WriteNode(writer, root.OuterXml, new HashSet<string>());
			}
		}

		internal static void WriteNestedFile(string newPathname, XElement root)
		{
			using (var writer = XmlWriter.Create(newPathname, CanonicalXmlSettings.CreateXmlWriterSettings()))
			{
				writer.WriteStartDocument();
				WriteElement(writer, root);
			}
		}

		internal static void WriteElement(XmlWriter writer, XElement element)
		{
			if (WriteWholeNode(element))
			{
				// Write entire element in one gulp, to avoid eating needed spaces in <Run> elements.
				element.WriteTo(writer);
			}
			else
			{
				writer.WriteStartElement(element.Name.LocalName);
				foreach (var attribute in element.Attributes())
				{
					writer.WriteAttributeString(attribute.Name.LocalName, attribute.Value);
				}
				if (element.HasElements)
				{
					foreach (var childNode in element.Elements().ToArray())
					{
						// Recurse on down to the bottom.
						WriteElement(writer, childNode);
						childNode.Remove();
					}
				}
				else
				{
					if (!string.IsNullOrEmpty(element.Value))
						writer.WriteString(element.Value);
				}
				writer.WriteEndElement();
			}
		}

		/// <summary>
		/// This method checks for select elements and the presence of the 'xml:'
		/// prefix (e.g., xml:spaces="preserve") attribute.
		///
		/// When one of these is found, the method returns true, so the caller knows to not drill down to the bottom
		/// in writing out the elements and attributes.
		///
		/// When 'true' is returned, the caller will simply write the whole xml string out (plus pay attention to the indents, etc.)
		/// </summary>
		/// <returns></returns>
		private static bool WriteWholeNode(XElement element)
		{
			var retval = false;
			switch (element.Name.LocalName)
			{
				case FlexBridgeConstants.AStr:
				case FlexBridgeConstants.Str:
				case FlexBridgeConstants.Uni:
				case FlexBridgeConstants.AUni:
				//case SharedConstants.Run: // xml:
					var str = element.ToString();
					if (str.Contains("xml:"))
					//var spaceAttr = element.Attribute("space");
					//if (spaceAttr != null && spaceAttr.Value == "preserve")
						retval = true;
					break;
			}
			return retval;
		}

		internal static void WriteCustomPropertyFile(string newPathname, XElement element)
		{
			if (element == null)
			{
				// Still write out file with just the root element.
				WriteNestedFile(newPathname, new XElement(FlexBridgeConstants.AdditionalFieldsTag));
			}
			else
			{
				using (var writer = XmlWriter.Create(newPathname, CanonicalXmlSettings.CreateXmlWriterSettings()))
					WriteElement(writer, element);
			}
		}

		internal static void WriteVersionNumberFile(string pathRoot, string version)
		{
			File.WriteAllText(Path.Combine(pathRoot, FlexBridgeConstants.ModelVersionFilename),
				Resources.kModelVersion + version + Resources.kCloseCurlyBrace);
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

		internal static void WriteCustomPropertyFile(MetadataCache mdc,
													 string pathRoot,
													 byte[] record)
		{
			WriteCustomPropertyFile(mdc, pathRoot, LibTriboroughBridgeSharedConstants.Utf8.GetString(record));
		}

		internal static void WriteCustomPropertyFile(MetadataCache mdc,
													 string pathRoot,
													 string record)
		{
			// Theory has it that the fwdata file is all sorted.
			var cpElement = DataSortingService.SortCustomPropertiesRecord(record);
			// Not this one, since it leaves out the temporary "key' attr. var cpElement = XElement.Parse(SharedConstants.Utf8.GetString(record));
			// Add custom property info to MDC, since it may need to be sorted in the data files.
			var hasCustomProperties = false;
			foreach (var propElement in cpElement.Elements(FlexBridgeConstants.CustomField))
			{
				hasCustomProperties = true;
				var className = propElement.Attribute(FlexBridgeConstants.Class).Value;
				var propName = propElement.Attribute(FlexBridgeConstants.Name).Value;
				var typeAttr = propElement.Attribute("type");
				var adjustedTypeValue = MetadataCache.AdjustedPropertyType(typeAttr.Value);
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
			WriteCustomPropertyFile(Path.Combine(pathRoot, FlexBridgeConstants.CustomPropertiesFilename), cpElement);
		}

		internal static void CheckPathname(string mainFilePathname)
		{
			if (!String.IsNullOrEmpty(mainFilePathname) // No null or empty string can be valid.
				&& Directory.Exists(Path.GetDirectoryName(mainFilePathname))) // There has to be an actual folder,
				return;

			throw new ApplicationException("Cannot process the given file.");
		}

		internal static void CheckFilename(string mainFilePathname)
		{
			// Just because all of this is true, doesn't mean it is a FW 7.0 related file. :-(
			if (!String.IsNullOrEmpty(mainFilePathname) // No null or empty string can be valid.
				&& File.Exists(mainFilePathname) // There has to be an actual file,
				&& Path.GetExtension(mainFilePathname).ToLowerInvariant() == LibTriboroughBridgeSharedConstants.FwXmlExtension)
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

		internal static void WriteNestedListFileIfItExists(IDictionary<string,
			SortedDictionary<string, byte[]>> classData,
			Dictionary<string, string> guidToClassMapping,
			XElement listOwningElement, string listOwningPropertyName,
			string listPathname)
		{
			var listPropElement = listOwningElement.Element(listOwningPropertyName);
			if (listPropElement == null || !listPropElement.HasElements)
				return;

			var listElement = LibFLExBridgeUtilities.CreateFromBytes(classData[FlexBridgeConstants.CmPossibilityList][listPropElement.Elements().First().Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant()]);
			CmObjectNestingService.NestObject(false,
											  listElement,
											  classData,
											  guidToClassMapping);
			listPropElement.RemoveNodes(); // Remove the single list objsur element.
			WriteNestedFile(listPathname, new XElement(listOwningPropertyName, listElement));
		}

		internal static void FillBuckets(Dictionary<int, SortedDictionary<string, XElement>> buckets, SortedDictionary<string, XElement> data)
		{
			var bucketCount = buckets.Count;
			foreach (var kvp in data)
			{
				var key = kvp.Key;
				buckets[(int)(UInt32.Parse(key.Substring(0, 8), NumberStyles.AllowHexSpecifier) % bucketCount)].Add(key, kvp.Value);
			}
		}

		internal static Dictionary<int, SortedDictionary<string, XElement>> CreateEmptyBuckets(int numberOfBucketsToCreate)
		{
			var emptyBuckets = new Dictionary<int, SortedDictionary<string, XElement>>();

			for (var i = 0; i < numberOfBucketsToCreate; ++i)
			{
				emptyBuckets.Add(i, new SortedDictionary<string, XElement>(StringComparer.InvariantCultureIgnoreCase));
			}

			return emptyBuckets;
		}
	}
}
