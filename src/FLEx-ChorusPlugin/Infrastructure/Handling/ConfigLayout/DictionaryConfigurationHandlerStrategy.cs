// --------------------------------------------------------------------------------------------
// Copyright (C) 2015-2016 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Chorus.FileTypeHanders;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers.Mercurial;
using TriboroughBridge_ChorusPlugin;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.ConfigLayout
{
	internal sealed class DictionaryConfigurationHandlerStrategy : IFieldWorksFileHandler
	{
		private string _xsdPath;
		private string DictionaryConfigXsdPath
		{
			get
			{
				if (_xsdPath != null)
					return _xsdPath;

				var innerPath = Path.Combine("Language Explorer", "Configuration", "DictionaryConfiguration.xsd");
				// When called from FLEx (via FLExBridge), we have the FLEx directory (received as a command-line argument).
				// When called from Mercurial (via ChorusMerge), we do not. In this case, we must look all over the place,
				// first in the relative directory we would expect on a user machine,
				// then in the relative directory we would expect on a developer machine.
				if (string.IsNullOrEmpty(Utilities.FwAppsDir))
				{
					//MessageBox.Show("Executing from code at " + Assembly.GetExecutingAssembly().CodeBase, "Debug FLExBridge Merge Strategy");
					var flexBridgeParentDir =
						Path.GetDirectoryName(Path.GetDirectoryName(Utilities.StripFilePrefix(Assembly.GetExecutingAssembly().CodeBase)));
					// Try to find FieldWorks installed in a directory neighbouring FLExbridge
					var possibleFlexDirs = Directory.GetDirectories(flexBridgeParentDir, "fieldworks*");
					for (var i = possibleFlexDirs.Length - 1; i >= 0; --i) // loop backwards to check the latest installed FLEx version first
					{
						_xsdPath = Path.Combine(possibleFlexDirs[i], innerPath);
						if (File.Exists(_xsdPath))
							return _xsdPath;
					}

					// This is probably a developer machine; try looking in a few common places
					var devInnerPath = Path.Combine("fw", "DistFiles", innerPath);
					flexBridgeParentDir = Path.GetDirectoryName(Path.GetDirectoryName(flexBridgeParentDir));
					_xsdPath = Path.Combine(flexBridgeParentDir, devInnerPath);
					if (File.Exists(_xsdPath))
						return _xsdPath;
					devInnerPath = Path.Combine("fwrepo", devInnerPath);
					_xsdPath = Path.Combine(flexBridgeParentDir, devInnerPath);
					if (File.Exists(_xsdPath))
						return _xsdPath;
					if (Utilities.IsUnix)
					{
						// ~/fwrepo/fw/DistFiles/Language\ Explorer/Configuration/DictionaryConfiguration.xsd
						_xsdPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), devInnerPath);
					}
					else
					{
						// "C:\fwrepo\fw\DistFiles\Language Explorer\Configuration\DictionaryConfiguration.xsd"
						_xsdPath = Path.Combine(Path.GetPathRoot(flexBridgeParentDir), devInnerPath);
					}
					// Review (Hasso) 2015.12: We can easily get here, since devs use different repo structures. Should we check the registry?
				}
				else
				{
					_xsdPath = Path.Combine(Utilities.FwAppsDir, innerPath);
					if (!File.Exists(_xsdPath))
						_xsdPath = Path.Combine(Utilities.FwAppsDir, "..", "..", "DistFiles", innerPath);
				}
				return _xsdPath; // Either we have found it by now or we never will.
			}
		}

		public bool CanValidateFile(string pathToFile)
		{
			return File.Exists(DictionaryConfigXsdPath);
		}

		public string ValidateFile(string pathToFile)
		{
			var schemas = new XmlSchemaSet();
			using(var reader = XmlReader.Create(DictionaryConfigXsdPath))
			{
				try
				{
					schemas.Add("", reader);
					var document = XDocument.Load(pathToFile);
					string result = null;
					document.Validate(schemas, (sender, args) =>
						result = string.Format("Model saved as xml did not validate against schema: {0}", args.Message));
					return result;
				}
				catch (XmlException e)
				{
					return string.Format("Exception occurred during validation: {0}", e.Message);
				}
			}
		}

		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			return FieldWorksChangePresenter.GetCommonChangePresenter(report, repository);
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			return Xml2WayDiffService.ReportDifferences(repository, parent, child, null, SharedConstants.ConfigurationItem, SharedConstants.GuidStr);
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			XmlMergeService.AddConflictToListener(mergeOrder.EventListener, new UnmergableFileTypeConflict(mergeOrder.MergeSituation));
			if (mergeOrder.MergeSituation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.TheyWin)
			{
				File.Copy(mergeOrder.pathToTheirs, mergeOrder.pathToOurs, true);
			}
			// else (WeWin et al.), simply leave our file in place
		}

		public string Extension
		{
			get { return SharedConstants.fwdictconfig; }
		}
	}
}
