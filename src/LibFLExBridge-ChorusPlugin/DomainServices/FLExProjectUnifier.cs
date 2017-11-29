// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using LibFLExBridgeChorusPlugin.Contexts;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibTriboroughBridgeChorusPlugin;
using SIL.Code;
using SIL.IO;
using SIL.Progress;

namespace LibFLExBridgeChorusPlugin.DomainServices
{
	/// <summary>
	/// Put the multiple files back together into the main fwdata file,
	/// but only if a Send/Receive had new information brought back into the local repo.
	/// NB: The client of the service decides if new information was found, and decides to call the service, or not.
	/// </summary>
	/// <remarks>
	/// This code only has the most supericial of unit testing.
	/// 
	/// The real tests are done using the FwdataTestApp over my (RandyR) collection of real and sample Flex projects.
	/// Among other things, that app does a round trip (split & join) operation on slected fwdata files,
	/// including removal of all 'ambiguous' elements.
	/// The person running that app is then expected to use a diff program on the proginal file and the rejoined file,
	/// and must be able to account for any differences.
	/// </remarks>
	internal static class FLExProjectUnifier
	{
		internal static void PutHumptyTogetherAgain(IProgress progress, bool writeVerbose, string mainFilePathname)
		{
			Guard.AgainstNull(progress, "progress");
			FileWriterService.CheckPathname(mainFilePathname);

			using (var tempFile = new TempFile())
			{
				using (var writer = XmlWriter.Create(tempFile.Path, new XmlWriterSettings
					// NB: These are the FW bundle of settings, not the canonical settings.
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
					if (writeVerbose)
						progress.WriteVerbose("Processing data model version number....");
					else
						progress.WriteMessage("Processing data model version number....");
					UpgradeToVersion(writer, pathRoot);
					if (writeVerbose)
						progress.WriteVerbose("Processing custom properties....");
					else
						progress.WriteMessage("Processing custom properties....");
					WriteOptionalCustomProperties(writer, pathRoot);

					var sortedData = BaseDomainServices.PutHumptyTogetherAgain(progress, writeVerbose, pathRoot);

					if (writeVerbose)
						progress.WriteVerbose("Writing temporary fwdata file....");
					else
						progress.WriteMessage("Writing temporary fwdata file....");
					foreach (var rtElement in sortedData.Values)
						FileWriterService.WriteElement(writer, rtElement);
					writer.WriteEndElement();
				}
				//Thread.Sleep(2000); In case it blows (access denied) up again on Sue's computer.
				if (writeVerbose)
					progress.WriteVerbose("Copying temporary fwdata file to main file....");
				else
					progress.WriteMessage("Copying temporary fwdata file to main file....");
				File.Copy(tempFile.Path, mainFilePathname, true);
			}

			SplitFileAgainIfNeeded(progress, writeVerbose, mainFilePathname);
		}

		private static void SplitFileAgainIfNeeded(IProgress progress, bool writeVerbose, string mainFilePathname)
		{
			var pathRoot = Path.GetDirectoryName(mainFilePathname);
			// Resplit mainFilePathname, if there are any temp files that mark incompatible moves exists (has 'dupid' extension).
			var baseFoldersThatHaveNestedData = new HashSet<string>
				{
					"Anthropology",
					"General",
					"Linguistics",
					"Other"
				};
			var dupidPathnames = new List<string>();
			foreach (var nestedFolderBase in baseFoldersThatHaveNestedData)
			{
				var nestedFolder = Path.Combine(pathRoot, nestedFolderBase);
				if (Directory.Exists(nestedFolder))
					dupidPathnames.AddRange(Directory.GetFiles(nestedFolder, "*." + LibTriboroughBridgeSharedConstants.dupid, SearchOption.AllDirectories));
			}
			if (dupidPathnames.Count == 0)
				return;

			foreach (var dupidPathname in dupidPathnames)
				File.Delete(dupidPathname);
			var projName = Path.GetFileName(mainFilePathname);
			progress.WriteMessage("Split up project file: {0} (again)", projName);
			FLExProjectSplitter.PushHumptyOffTheWall(progress, writeVerbose, mainFilePathname);
			progress.WriteMessage("Finished splitting up project file: {0} (again)", projName);
		}

		private static void UpgradeToVersion(XmlWriter writer, string pathRoot)
		{
			writer.WriteStartElement("languageproject");

			// Write out version number from the ModelVersion file.
			var version = LibFLExBridgeUtilities.GetFlexModelVersion(pathRoot);
			writer.WriteAttributeString("version", version);

			var mdc = MetadataCache.MdCache; // This may really need to be a reset
			mdc.UpgradeToVersion(Int32.Parse(version));
		}

		private static void WriteOptionalCustomProperties(XmlWriter writer, string pathRoot)
		{
			// Write out optional custom property data to the fwdata file.
			// The foo.CustomProperties file will exist, even if it has nothing in it, but the "AdditionalFields" root element.
			var optionalCustomPropFile = Path.Combine(pathRoot, FlexBridgeConstants.CustomPropertiesFilename);
			var doc = XDocument.Load(optionalCustomPropFile);
			var customFieldElements = doc.Root.Elements(FlexBridgeConstants.CustomField).ToList();
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

				mdc.GetClassInfo(cf.Attribute(FlexBridgeConstants.Class).Value).AddProperty(
					new FdoPropertyInfo(cf.Attribute(FlexBridgeConstants.Name).Value, propType, true));
			}
			mdc.ResetCaches();
			FileWriterService.WriteElement(writer, doc.Root);
		}
	}
}
