using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHanders;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers.Mercurial;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using Palaso.IO;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.Linguistics.Reversal
{
	internal sealed class ReversalFileTypeHandlerStrategy : IFieldWorksFileHandler
	{
		#region Implementation of IFieldWorksFileHandler

		public bool CanValidateFile(string pathToFile)
		{
			return FileUtils.CheckValidPathname(pathToFile, SharedConstants.Reversal);
		}

		public string ValidateFile(string pathToFile)
		{
			try
			{
				var doc = XDocument.Load(pathToFile);
				var root = doc.Root;
				if (root.Name.LocalName != "Reversal")
					return "Not a FieldWorks reversal file.";
				var header = root.Element(SharedConstants.Header);
				var result = CmObjectValidator.ValidateObject(MetadataCache.MdCache, header.Element("ReversalIndex"));
				if (result != null)
					return result;
				foreach (var record in root.Elements(SharedConstants.ReversalIndexEntry))
				{
					if (record.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant() == SharedConstants.EmptyGuid)
						return null;
					result = CmObjectValidator.ValidateObject(MetadataCache.MdCache, record);
					if (result != null)
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
				SharedConstants.Header,
				SharedConstants.ReversalIndexEntry, SharedConstants.GuidStr);
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			mdc.AddCustomPropInfo(mergeOrder); // NB: Must be done before FieldWorksReversalMergeStrategy is created.

			XmlMergeService.Do3WayMerge(mergeOrder,
				new FieldWorksHeaderedMergeStrategy(mergeOrder, mdc),
				true,
				SharedConstants.Header,
				SharedConstants.ReversalIndexEntry, SharedConstants.GuidStr);
		}

		public string Extension
		{
			get { return SharedConstants.Reversal; }
		}

		#endregion
	}
}