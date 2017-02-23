// --------------------------------------------------------------------------------------------
// Copyright (C) 2015-2016 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Chorus.FileTypeHanders;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers.Mercurial;
using Palaso.IO;
using Palaso.Xml;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.ConfigLayout
{
	internal sealed class DictionaryConfigurationHandlerStrategy : IFieldWorksFileHandler
	{
		private string _xsdPathname;

		private string GetXsdPathname(string configFilePathname)
		{
			if (_xsdPathname != null)
				return _xsdPathname;

			var innerPath = Path.Combine("Temp", SharedConstants.DictConfigSchemaFilename);
			var parentDir = Path.GetDirectoryName(configFilePathname);
			while (parentDir != null)
			{
				if (File.Exists(_xsdPathname = Path.Combine(parentDir, innerPath)))
					return _xsdPathname;
				parentDir = Path.GetDirectoryName(parentDir);
			}
			return _xsdPathname = string.Empty;
		}

		public bool CanValidateFile(string pathToFile)
		{
			return FileUtils.CheckValidPathname(pathToFile, Extension) && File.Exists(GetXsdPathname(pathToFile));
		}

		public string ValidateFile(string pathToFile)
		{
			var schemas = new XmlSchemaSet();
			using(var reader = XmlReader.Create(GetXsdPathname(pathToFile)))
			{
				try
				{
					schemas.Add("", reader);
					string result = null;
					XDocument.Load(pathToFile).Validate(schemas, (sender, args) => result = args.Message);
					return FormatMessage(result, schemas, reader);
				}
				catch (XmlException e)
				{
					return FormatMessage(e.Message, schemas, reader);
				}
			}
		}

		private string FormatMessage(string message, XmlSchemaSet schemata, XmlReader reader)
		{
			if (string.IsNullOrEmpty(message))
				return null;
			return string.Format("{1}{0}{4} Contents of {2}:{0}{3}", Environment.NewLine, message, reader.BaseURI, reader.ReadContentAsString(), schemata.Count);
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
			var merger = new XmlMerger(mergeOrder.MergeSituation) { EventListener = mergeOrder.EventListener };
			var rootStrategy = ElementStrategy.CreateSingletonElement();
			rootStrategy.IsAtomic = true;
			merger.MergeStrategies.SetStrategy("DictionaryConfiguration", rootStrategy);
			var mergeResults = merger.MergeFiles(mergeOrder.pathToOurs, mergeOrder.pathToTheirs, mergeOrder.pathToCommonAncestor);
			// Write merged data
			using (var writer = XmlWriter.Create(mergeOrder.pathToOurs, CanonicalXmlSettings.CreateXmlWriterSettings()))
			{
				XmlUtils.WriteNode(writer, mergeResults.MergedNode.OuterXml, new HashSet<string>());
			}
		}

		public string Extension
		{
			get { return SharedConstants.fwdictconfig; }
		}
	}
}
