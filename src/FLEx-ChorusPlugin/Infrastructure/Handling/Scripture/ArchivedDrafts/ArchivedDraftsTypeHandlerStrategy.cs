using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHanders;
using Chorus.VcsDrivers.Mercurial;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Palaso.IO;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.Scripture.ArchivedDrafts
{
	internal sealed class ArchivedDraftsTypeHandlerStrategy : IFieldWorksFileHandler
	{
		private const string Draft = "Draft";
		private const string ScrDraft = "ScrDraft";

		#region Implementation of IFieldWorksFileHandler

		public bool CanValidateFile(string pathToFile)
		{
			if (!FileUtils.CheckValidPathname(pathToFile, SharedConstants.ArchivedDraftExt))
				return false;

			return ValidateFile(pathToFile) == null;
		}

		public string ValidateFile(string pathToFile)
		{
			try
			{
				var doc = XDocument.Load(pathToFile);
				var root = doc.Root;
				if (root.Name.LocalName != Draft || !root.Elements(ScrDraft).Any())
					return "Not valid archived draft file.";

				return null;
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
				ScrDraft, SharedConstants.GuidStr);
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			FieldWorksMergeStrategyServices.AddCustomPropInfo(mdc, mergeOrder, SharedConstants.ArchivedDrafts, 2); // NB: Must be done before FieldWorksCommonMergeStrategy is created.

			XmlMergeService.Do3WayMerge(mergeOrder,
				new FieldWorksCommonMergeStrategy(mergeOrder.MergeSituation, mdc),
				null,
				ScrDraft, SharedConstants.GuidStr, WritePreliminaryArchivedDraftInformation);
		}

		public string Extension
		{
			get { return SharedConstants.ArchivedDraftExt; }
		}

		#endregion

		private static void WritePreliminaryArchivedDraftInformation(XmlReader reader, XmlWriter writer)
		{
			reader.MoveToContent();
			writer.WriteStartElement(Draft);
			reader.Read();
		}
	}
}