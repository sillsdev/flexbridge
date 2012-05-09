using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
				writer.WriteStartElement(root.Name.LocalName);
				foreach (var childNode in root.Elements().ToArray())
				{
					writer.WriteStartElement(childNode.Name.LocalName);
					foreach (var attribute in childNode.Attributes())
					{
						writer.WriteAttributeString(attribute.Name.LocalName, attribute.Value);
					}
					foreach (var innerChildNode in childNode.Elements().ToArray())
					{
						WriteElement(writer, innerChildNode);
						innerChildNode.Remove();
					}
					writer.WriteEndElement();
					childNode.Remove();
				}
				writer.WriteEndElement();
			}
		}

		internal static void WriteElement(XmlWriter writer, XElement element)
		{
			using (var nodeReader = XmlReader.Create(new MemoryStream(SharedConstants.Utf8.GetBytes(element.ToString()), false), CanonicalReaderSettings))
				writer.WriteNode(nodeReader, true);
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
			WriteCustomPropertyFile(Path.Combine(pathRoot, SharedConstants.CustomPropertiesFilename), SharedConstants.Utf8.GetBytes(cpElement.ToString()));
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
	}
}