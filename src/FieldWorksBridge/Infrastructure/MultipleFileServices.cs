#define USEMULTIPLEFILES
#if USEMULTIPLEFILES
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
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

		private const string IdentfierAttribute = "guid";
		private static readonly byte[] IdentifierAttributeWithDoubleOpen = Utf8.GetBytes(IdentfierAttribute + "=\"");
		private static readonly byte[] IdentifierAttributeWithSingleOpen = Utf8.GetBytes(IdentfierAttribute + "='");

		private const string ClassAttribute = "class";
		private static readonly byte[] ClassAttributeWithDoubleOpen = Utf8.GetBytes(ClassAttribute + "=\"");
		private static readonly byte[] ClassAttributeWithSingleOpen = Utf8.GetBytes(ClassAttribute + "='");

		private static readonly byte DoubleQuote = Utf8.GetBytes("\"")[0];
		private static readonly byte SingleQuote = Utf8.GetBytes("'")[0];
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
						optionalFirstElement = record;
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
			// TODO: Deal with all other sortings at some point.
			// Write optional first element.
			if (optionalFirstElement != null)
				WriteSecondaryFile(Path.Combine(multiFileDirRoot, "CustomProperties.fwdata"), readerSettings, version, optionalFirstElement);

			// Write data records in guid sorted order.
			foreach (var kvp in classData)
				WriteSecondaryFile(Path.Combine(multiFileDirRoot, kvp.Key + ".fwdata"), readerSettings, version, kvp.Value);
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
			var className = GetAttribute(false, ClassAttributeWithDoubleOpen, DoubleQuote, record)
					   ?? GetAttribute(false, ClassAttributeWithSingleOpen, SingleQuote, record);
			SortedDictionary<string, byte[]> recordData;
			if (!classData.TryGetValue(className, out recordData))
			{
				recordData = new SortedDictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
				classData.Add(className, recordData);
			}
			var guid = GetAttribute(true, IdentifierAttributeWithDoubleOpen, DoubleQuote, record)
				   ?? GetAttribute(true, IdentifierAttributeWithSingleOpen, SingleQuote, record);
			recordData.Add(guid, record);
		}

		private static string GetAttribute(bool shiftCase, byte[] name, byte closeQuote, byte[] input)
		{
			var start = input.IndexOfSubArray(name);
			if (start == -1)
				return null;

			var isWhiteSpace = IsWhitespace(input[start - 1]);
			start += name.Length;
			var end = Array.IndexOf(input, closeQuote, start);

			// Check to make sure start -1 is not another letter in a similarly named attr (e.g., id vs guid).
			if (isWhiteSpace)
			{
				return (end == -1)
						? null
						: ReplaceBasicSetOfEntitites(shiftCase ? Utf8.GetString(input.SubArray(start, end - start)).ToLowerInvariant() : Utf8.GetString(input.SubArray(start, end - start)));
			}

			return GetAttribute(shiftCase, name, closeQuote, input.SubArray(end + 1, input.Length - end));
		}

		private static string ReplaceBasicSetOfEntitites(string input)
		{
			return input
				.Replace("&amp;", "&")
				.Replace("&lt;", "<")
				.Replace("&gt;", ">")
				.Replace("&quot;", "\"")
				.Replace("&apos;", "'");
		}

		private static bool IsWhitespace(byte input)
		{
			return (input == ' ' || input == '\t' || input == '\r' || input == '\n');
		}
	}
}
#endif
