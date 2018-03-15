// --------------------------------------------------------------------------------------------
// Copyright (C) 2015-2017 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Chorus.FileTypeHandlers;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers.Mercurial;
using LibFLExBridgeChorusPlugin.Handling;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibTriboroughBridgeChorusPlugin;
using SIL.IO;

namespace LibFLExBridgeChorusPlugin.Handling.ConfigLayout
{
	[Export(typeof(IFieldWorksFileHandler))]
	internal sealed class DictionaryConfigurationHandlerStrategy : IFieldWorksFileHandler
	{
		private string _xsdPathname;

		private string GetXsdPathname(string configFilePathname)
		{
			if (_xsdPathname != null)
				return _xsdPathname;

			var innerPath = Path.Combine("Temp", FlexBridgeConstants.DictConfigSchemaFilename);
			var parentDir = Path.GetDirectoryName(configFilePathname);
			while (!string.IsNullOrEmpty(parentDir))
			{
				if (File.Exists(_xsdPathname = Path.Combine(parentDir, innerPath)))
					return _xsdPathname;
				parentDir = Path.GetDirectoryName(parentDir);
			}
			return _xsdPathname = string.Empty;
		}

		public bool CanValidateFile(string pathToFile)
		{
			return PathHelper.CheckValidPathname(pathToFile, Extension) && File.Exists(GetXsdPathname(pathToFile));
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
			return Xml2WayDiffService.ReportDifferences(repository, parent, child, null, FlexBridgeConstants.ConfigurationItem, FlexBridgeConstants.GuidStr);
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			var merger = new XmlMerger(mergeOrder.MergeSituation) { EventListener = mergeOrder.EventListener };
			var rootStrategy = ElementStrategy.CreateSingletonElement();
			rootStrategy.IsAtomic = true;
			merger.MergeStrategies.SetStrategy("DictionaryConfiguration", rootStrategy);
			var mergeResults = merger.MergeFiles(mergeOrder.pathToOurs, mergeOrder.pathToTheirs, mergeOrder.pathToCommonAncestor);
			// Write merged data
			File.WriteAllText(mergeOrder.pathToOurs, mergeResults.MergedNode.OuterXml, Encoding.UTF8);
		}

		public string Extension
		{
			get { return FlexBridgeConstants.fwdictconfig; }
		}
	}
}
