using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Chorus.FileTypeHanders;
using Chorus.VcsDrivers.Mercurial;
using Chorus.merge;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using Palaso.IO;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.Scripture
{
	internal sealed class ImportSettingsTypeHandlerStrategy : IFieldWorksFileHandler
	{
		#region Implementation of IFieldWorksFileHandler

		public bool CanValidateFile(string pathToFile)
		{
			return FileUtils.CheckValidPathname(pathToFile, SharedConstants.ImportSetting) &&
				   Path.GetFileName(pathToFile) == SharedConstants.ImportSettingsFilename;
		}

		public string ValidateFile(string pathToFile)
		{
			try
			{
				var doc = XDocument.Load(pathToFile);
				var root = doc.Root;
				if (root.Name.LocalName != SharedConstants.ImportSettings || !root.Elements(SharedConstants.ScrImportSet).Any())
					return "Not valid Scripture import settings file.";

				foreach (var result in root.Elements(SharedConstants.ScrImportSet).Select(draft => CmObjectValidator.ValidateObject(MetadataCache.MdCache, root.Element(SharedConstants.ScrImportSet))).Where(result => result != null))
				{
					return result;
				}
			}
			catch (Exception e)
			{
				return e.Message;
			}
			return null;
		}

		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			return FieldWorksChangePresenter.GetCommonChangePresenter(report, repository);
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			return Xml2WayDiffService.ReportDifferences(repository, parent, child,
				null,
				SharedConstants.ScrImportSet, SharedConstants.GuidStr);
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			mdc.AddCustomPropInfo(mergeOrder); // NB: Must be done before FieldWorksCommonMergeStrategy is created.

			XmlMergeService.Do3WayMerge(mergeOrder,
				new FieldWorksCommonMergeStrategy(mergeOrder, mdc),
				true,
				null,
				SharedConstants.ScrImportSet, SharedConstants.GuidStr);
		}

		public string Extension
		{
			get { return SharedConstants.ImportSetting; }
		}

		#endregion
	}
}