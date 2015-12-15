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

namespace FLEx_ChorusPlugin.Infrastructure.Handling.ConfigLayout
{
	internal sealed class DictionaryConfigurationHandlerStrategy : IFieldWorksFileHandler
	{
		private static string dictionaryConfigXsdPath;
		public static string FlexFolder
		{
			set
			{
				dictionaryConfigXsdPath = Path.Combine(Path.Combine(Path.Combine(value,
#if DEBUG
					"../../DistFiles",
#endif
					"Language Explorer"), "Configuration"), "DictionaryConfiguration.xsd");
		}

		public bool CanValidateFile(string pathToFile)
		{
			return !string.IsNullOrEmpty(dictionaryConfigXsdPath) && File.Exists(dictionaryConfigXsdPath);
		}

		public string ValidateFile(string pathToFile)
		{
			var schemas = new XmlSchemaSet();
			using(var reader = XmlReader.Create(dictionaryConfigXsdPath))
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
