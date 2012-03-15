using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHanders;
using Chorus.VcsDrivers.Mercurial;
using Chorus.merge;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using Palaso.IO;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.Linguistics.TextCorpus
{
	internal sealed class TextCorpusFileTypeHandlerStrategy : IFieldWorksFileHandler
	{
		private const string TextInCorpus = "TextInCorpus"; // NB: Not the same as what is in SharedSharedConstants.

		#region Implementation of IFieldWorksFileHandler

		public bool CanValidateFile(string pathToFile)
		{
			return FileUtils.CheckValidPathname(pathToFile, SharedConstants.TextInCorpus);
		}

		public string ValidateFile(string pathToFile)
		{
			try
			{
				var doc = XDocument.Load(pathToFile);
				var root = doc.Root;
				if (root.Name.LocalName != TextInCorpus
					|| root.Element("Text") == null)
				{
					return "Not valid text corpus file";
				}
				return CmObjectValidator.ValidateObject(MetadataCache.MdCache, root.Element("Text"));
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
				"Text", SharedConstants.GuidStr);
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			mdc.AddCustomPropInfo(mergeOrder); // NB: Must be done before FieldWorksReversalMergeStrategy is created.

			XmlMergeService.Do3WayMerge(mergeOrder,
				new FieldWorksCommonMergeStrategy(mergeOrder.MergeSituation, mdc),
				null,
				"Text", SharedConstants.GuidStr, WritePreliminaryTextCorpusInformation);
		}

		public string Extension
		{
			get { return SharedConstants.TextInCorpus; }
		}

		#endregion

		private static void WritePreliminaryTextCorpusInformation(XmlReader reader, XmlWriter writer)
		{
			reader.MoveToContent();
			writer.WriteStartElement(TextInCorpus);
			reader.Read();
		}
	}
}