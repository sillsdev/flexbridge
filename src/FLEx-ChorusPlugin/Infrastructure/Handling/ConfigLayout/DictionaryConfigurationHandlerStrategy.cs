// --------------------------------------------------------------------------------------------
// Copyright (C) 2015-2016 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Chorus.FileTypeHanders;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers.Mercurial;
using Palaso.IO;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.ConfigLayout
{
	internal sealed class DictionaryConfigurationHandlerStrategy : IFieldWorksFileHandler
	{
		private string _xsdPath;

		private string GetXsdPath(string configFilePath)
		{
			if (_xsdPath != null)
				return _xsdPath;

			var innerPath = Path.Combine("Temp", "DictionaryConfiguration.xsd");
			var parentDir = Path.GetDirectoryName(configFilePath);
			while (parentDir != null)
			{
				if (File.Exists(_xsdPath = Path.Combine(parentDir, innerPath)))
					return _xsdPath;
				parentDir = Path.GetDirectoryName(parentDir);
			}
			return _xsdPath = null;
		}

		public bool CanValidateFile(string pathToFile)
		{
			return FileUtils.CheckValidPathname(pathToFile, Extension) && File.Exists(GetXsdPath(pathToFile));
		}

		public string ValidateFile(string pathToFile)
		{
			var schemas = new XmlSchemaSet();
			using(var reader = XmlReader.Create(GetXsdPath(pathToFile)))
			{
				try
				{
					schemas.Add("", reader);
					string result = null;
					XDocument.Load(pathToFile).Validate(schemas, (sender, args) => result = args.Message);
					return result;
				}
				catch (XmlException e)
				{
					return e.Message;
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
