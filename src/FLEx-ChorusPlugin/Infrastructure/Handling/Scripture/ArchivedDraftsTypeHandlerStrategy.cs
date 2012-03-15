using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHanders;
using Chorus.VcsDrivers.Mercurial;
using Chorus.merge;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using Palaso.IO;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.Scripture
{
	internal sealed class ArchivedDraftsTypeHandlerStrategy : IFieldWorksFileHandler
	{
		private const string ArchivedDrafts = "ArchivedDrafts";
		private const string ScrDraft = "ScrDraft";

		#region Implementation of IFieldWorksFileHandler

		public bool CanValidateFile(string pathToFile)
		{
			return FileUtils.CheckValidPathname(pathToFile, SharedConstants.ArchivedDraft);
		}

		public string ValidateFile(string pathToFile)
		{
			try
			{
				var doc = XDocument.Load(pathToFile);
				var root = doc.Root;
				if (root.Name.LocalName != ArchivedDrafts || !root.Elements(ScrDraft).Any())
					return "Not valid archived draft file.";

				foreach (var result in root.Elements(ScrDraft).Select(draft => CmObjectValidator.ValidateObject(MetadataCache.MdCache, root.Element(ScrDraft))).Where(result => result != null))
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
				ScrDraft, SharedConstants.GuidStr);
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			mdc.AddCustomPropInfo(mergeOrder); // NB: Must be done before FieldWorksCommonMergeStrategy is created.

			XmlMergeService.Do3WayMerge(mergeOrder,
				new FieldWorksScrDraftStrategy(),
				null,
				ScrDraft, SharedConstants.GuidStr, WritePreliminaryArchivedDraftInformation);
		}

		public string Extension
		{
			get { return SharedConstants.ArchivedDraft; }
		}

		#endregion

		private static void WritePreliminaryArchivedDraftInformation(XmlReader reader, XmlWriter writer)
		{
			reader.MoveToContent();
			writer.WriteStartElement(ArchivedDrafts);
			reader.Read();
		}
	}
}