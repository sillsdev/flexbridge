using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Contexts;
using FLEx_ChorusPlugin.Contexts.Anthropology;
using FLEx_ChorusPlugin.Contexts.General;
using FLEx_ChorusPlugin.Contexts.Linguistics;
using FLEx_ChorusPlugin.Contexts.Scripture;
using Palaso.Code;
using Palaso.IO;
using Palaso.Progress.LogBox;

namespace FLEx_ChorusPlugin.Infrastructure.DomainServices
{
	/// <summary>
	/// Encapsulates, as a state machine, the joining of split fieldworks project files.
	/// The task is quantized to make use of a palaso progress bar.
	/// Most of the subtasks need to share two dictionaries that are protected
	/// from "public static" access in this class. (ie., main reason to encapsulate)
	/// The subtasks vary greatly in granulatity with most of the time
	/// spent in writing and caching xml "properties" and writing out files.
	/// Two static methods from MultipleFileServices were moved here since
	/// they are only used in this task.
	///
	/// Randy's summary:
	///  2. Put the multiple files back together into the main fwdata file,
	///		but only if a Send/Receive had new information brought back into the local repo.
	///		NB: The client of the service decides if new information was found, and decides to call the service, or not.
	/// </summary>
	internal static class FLExProjectUnifier
	{
		internal static void PutHumptyTogetherAgain(IProgress progress, string mainFilePathname)
		{
			Guard.AgainstNull(progress, "progress");
			FileWriterService.CheckPathname(mainFilePathname);

			using (var tempFile = new TempFile())
			{
				using (var writer = XmlWriter.Create(tempFile.Path, new XmlWriterSettings
																		{
																			OmitXmlDeclaration = false,
																			CheckCharacters = true,
																			ConformanceLevel = ConformanceLevel.Document,
																			Encoding = new UTF8Encoding(false),
																			Indent = true,
																			IndentChars = (""),
																			NewLineOnAttributes = false
																		}))
				{
					var pathRoot = Path.GetDirectoryName(mainFilePathname);
					// NB: The method calls are strictly ordered.
					// Don't even think of changing them.
					progress.WriteMessage("Processing data model version number....");
					UpgradeToVersion(writer, pathRoot);
					progress.WriteMessage("Processing custom properties....");
					WriteOptionalCustomProperties(writer, pathRoot);

					var sortedData = BaseDomainServices.PutHumptyTogetherAgain(progress, pathRoot);

					progress.WriteMessage("Writing temporary fwdata file....");
					foreach (var rtElement in sortedData.Values)
						FileWriterService.WriteElement(writer, rtElement);
					writer.WriteEndElement();
				}
				//Thread.Sleep(2000); In case it blows (access denied) up again on Sue's computer.
				progress.WriteMessage("Copying temporary fwdata file to main file....");
				File.Copy(tempFile.Path, mainFilePathname, true);
			}
		}

		private static void UpgradeToVersion(XmlWriter writer, string pathRoot)
		{
			writer.WriteStartElement("languageproject");

			// Write out version number from the ModelVersion file.
			var modelVersionData = File.ReadAllText(Path.Combine(pathRoot, SharedConstants.ModelVersionFilename));
			var splitModelVersionData = modelVersionData.Split(new[] {"{", ":", "}"}, StringSplitOptions.RemoveEmptyEntries);
			var version = splitModelVersionData[1].Trim();
			writer.WriteAttributeString("version", version);

			var mdc = MetadataCache.MdCache; // This may really need to be a reset
			mdc.UpgradeToVersion(Int32.Parse(version));
		}

		private static void WriteOptionalCustomProperties(XmlWriter writer, string pathRoot)
		{
			// Write out optional custom property data to the fwdata file.
			// The foo.CustomProperties file will exist, even if it has nothing in it, but the "AdditionalFields" root element.
			var optionalCustomPropFile = Path.Combine(pathRoot, SharedConstants.CustomPropertiesFilename);
			var doc = XDocument.Load(optionalCustomPropFile);
			var customFieldElements = doc.Root.Elements("CustomField").ToList();
			if (!customFieldElements.Any())
				return;

			var mdc = MetadataCache.MdCache;
			foreach (var cf in customFieldElements)
			{
				// Remove 'key' attribute from CustomField elements, before writing to main file.
				cf.Attribute("key").Remove();
				// Restore type attr for object values.
				var propType = cf.Attribute("type").Value;
				cf.Attribute("type").Value = MetadataCache.RestoreAdjustedTypeValue(propType);

				mdc.GetClassInfo(cf.Attribute(SharedConstants.Class).Value).AddProperty(
					new FdoPropertyInfo(cf.Attribute(SharedConstants.Name).Value, propType, true));
			}
			mdc.ResetCaches();
			FileWriterService.WriteElement(writer, SharedConstants.Utf8.GetBytes(doc.Root.ToString()));
		}
	}
}
