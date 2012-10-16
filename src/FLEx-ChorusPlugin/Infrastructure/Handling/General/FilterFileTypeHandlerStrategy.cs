using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHanders;
using Chorus.VcsDrivers.Mercurial;
using Chorus.merge;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using Palaso.IO;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.General
{
	/// <summary>
	/// This class deals with files with extension of "filter".
	/// There is only one of them with a fixed name.
	/// </summary>
	internal sealed class FilterFileTypeHandlerStrategy : IFieldWorksFileHandler
	{
		#region Implementation of IFieldWorksFileHandler

		public bool CanValidateFile(string pathToFile)
		{
			return FileUtils.CheckValidPathname(pathToFile, SharedConstants.Filter) &&
				   Path.GetFileName(pathToFile) == SharedConstants.FLExFiltersFilename;
		}

		public string ValidateFile(string pathToFile)
		{
			try
			{
				var doc = XDocument.Load(pathToFile);
				var root = doc.Root;
				if (root.Name.LocalName != SharedConstants.Filters
					|| root.Element(SharedConstants.Header) != null
					|| !root.Elements(SharedConstants.CmFilter).Any())
				{
					return "Not a valid filter file.";
				}

				return root.Elements(SharedConstants.CmFilter)
					.Select(filterElement => CmObjectValidator.ValidateObject(MetadataCache.MdCache, filterElement)).FirstOrDefault(result => result != null);
			}
			catch (Exception e)
			{
				return e.Message;
			}
		}

		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			return FieldWorksChangePresenter.GetCommonChangePresenter(report, repository);
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			return Xml2WayDiffService.ReportDifferences(repository, parent, child,
				null,
				SharedConstants.CmFilter, SharedConstants.GuidStr);
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			mdc.AddCustomPropInfo(mergeOrder); // NB: Must be done before FieldWorksCommonMergeStrategy is created.

			XmlMergeService.Do3WayMerge(mergeOrder,
				new FieldWorksCommonMergeStrategy(mergeOrder, mdc),
				true,
				null,
				SharedConstants.CmFilter, SharedConstants.GuidStr);
		}

		public string Extension
		{
			get { return SharedConstants.Filter; }
		}

		#endregion
	}
}