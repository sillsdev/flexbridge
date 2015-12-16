// --------------------------------------------------------------------------------------------
// Copyright (C) 2015 SIL International. All rights reserved.
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
				if (_xsdPath == null)
				{
					_xsdPath = Path.Combine(Utilities.FwAppsDir, "Language Explorer", "Configuration", "DictionaryConfiguration.xsd");
					if (!File.Exists(_xsdPath))
						_xsdPath = Path.Combine(Utilities.FwAppsDir, "..", "..", "DistFiles",
							"Language Explorer", "Configuration", "DictionaryConfiguration.xsd");
				}
				return _xsdPath;
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
				schemas.Add("", reader);
				var document = XDocument.Load(pathToFile);
				string result = null;
				document.Validate(schemas, (sender, args) =>
					result = string.Format("Model saved as xml did not validate against schema: {0}", args.Message));
				return result;
			}
		}

		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			throw new System.NotImplementedException();
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			throw new System.NotImplementedException();
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			throw new System.NotImplementedException();
		}

		public string Extension { get { return SharedConstants.fwdictconfig; } }
	}
}
