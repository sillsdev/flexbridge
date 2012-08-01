using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Properties;
using Palaso.Xml;

namespace FLEx_ChorusPlugin.Infrastructure.DomainServices
{
	internal static class FileWriterService
	{
		private readonly static XmlReaderSettings ReaderSettings = new XmlReaderSettings { IgnoreWhitespace = true };

		internal static XmlReaderSettings CanonicalReaderSettings
		{
			get { return ReaderSettings; }
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
				WriteElement(writer, Encoding.UTF8.GetBytes(element.ToString()));
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
				case SharedConstants.AStr:
				case SharedConstants.Str:
				case SharedConstants.Uni:
				case SharedConstants.AUni:
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

		internal static void WriteElement(XmlWriter writer, byte[] optionalFirstElement)
		{
			using (var nodeReader = XmlReader.Create(new MemoryStream(optionalFirstElement, false), CanonicalReaderSettings))
				writer.WriteNode(nodeReader, true);
		}

		internal static void WriteCustomPropertyFile(string newPathname, byte[] element)
		{
			if (element == null)
			{
				// Still write out file with just the root element.
				WriteNestedFile(newPathname, new XElement(SharedConstants.AdditionalFieldsTag));
			}
			else
			{
				using (var writer = XmlWriter.Create(newPathname, CanonicalXmlSettings.CreateXmlWriterSettings()))
					WriteElement(writer, element);
			}
		}

		internal static void WriteVersionNumberFile(string pathRoot, string version)
		{
			File.WriteAllText(Path.Combine(pathRoot, SharedConstants.ModelVersionFilename), Resources.kModelVersion + version + Resources.kCloseCurlyBrace);
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
			WriteCustomPropertyFile(mdc, pathRoot, SharedConstants.Utf8.GetString(record));
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
			foreach (var propElement in cpElement.Elements("CustomField"))
			{
				hasCustomProperties = true;
				var className = propElement.Attribute(SharedConstants.Class).Value;
				var propName = propElement.Attribute(SharedConstants.Name).Value;
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
			WriteCustomPropertyFile(Path.Combine(pathRoot, SharedConstants.CustomPropertiesFilename), SharedConstants.Utf8.GetBytes(cpElement.ToString()));
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

		internal static void WriteNestedListFileIfItExists(IDictionary<string,
			SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping,
			XElement listOwningElement, string listOwningPropertyName,
			string listPathname)
		{
			var listPropElement = listOwningElement.Element(listOwningPropertyName);
			if (listPropElement == null || !listPropElement.HasElements)
				return;

			var listElement = classData[SharedConstants.CmPossibilityList][listPropElement.Elements().First().Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant()];
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
				buckets[(int)((uint)new Guid(key).GetHashCode() % bucketCount)].Add(key, kvp.Value);
			}
		}

		internal static Dictionary<int, SortedDictionary<string, XElement>> CreateEmptyBuckets(int numberOfBucketsToCreate)
		{
			var emptyBuckets = new Dictionary<int, SortedDictionary<string, XElement>>(numberOfBucketsToCreate);

			for (var i = 0; i < numberOfBucketsToCreate; ++i)
			{
				emptyBuckets.Add(i, new SortedDictionary<string, XElement>(StringComparer.InvariantCultureIgnoreCase));
			}

			return emptyBuckets;
		}
	}
}