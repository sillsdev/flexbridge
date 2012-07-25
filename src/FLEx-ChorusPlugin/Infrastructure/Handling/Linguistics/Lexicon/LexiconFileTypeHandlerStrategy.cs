using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHanders;
using Chorus.VcsDrivers.Mercurial;
using Chorus.merge;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using Palaso.IO;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.Linguistics.Lexicon
{
	internal sealed class LexiconFileTypeHandlerStrategy : IFieldWorksFileHandler
	{
		#region Implementation of IFieldWorksFileHandler

		public bool CanValidateFile(string pathToFile)
		{
			return FileUtils.CheckValidPathname(pathToFile, SharedConstants.Lexdb) &&
				   Path.GetFileName(pathToFile) == SharedConstants.LexiconFilename;
		}

		public string ValidateFile(string pathToFile)
		{
			try
			{
				var doc = XDocument.Load(pathToFile);
				var root = doc.Root;
				if (root.Name.LocalName != SharedConstants.Lexicon
					|| root.Element(SharedConstants.Header) == null
					|| root.Element(SharedConstants.Header).Element("LexDb") == null)
				{
					return "Not valid lexicon file";
				}

				var mdc = MetadataCache.MdCache;
				var result = CmObjectValidator.ValidateObject(mdc, root.Element(SharedConstants.Header).Element("LexDb"));
				if (result != null)
					return result;

				foreach (var entryElement in root.Elements(SharedConstants.LexEntry))
				{
					if (entryElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant() == SharedConstants.EmptyGuid)
						return null;
					result = CmObjectValidator.ValidateObject(mdc, entryElement);
					if (result != null)
						return result;
				}
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
				SharedConstants.Header,
				SharedConstants.LexEntry, SharedConstants.GuidStr);
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			mdc.AddCustomPropInfo(mergeOrder); // NB: Must be done before FieldWorksReversalMergeStrategy is created.

			XmlMergeService.Do3WayMerge(mergeOrder,
				new FieldWorksHeaderedMergeStrategy(mergeOrder.MergeSituation, mdc),
				true,
				SharedConstants.Header,
				SharedConstants.LexEntry, SharedConstants.GuidStr);
		}

		public string Extension
		{
			get { return SharedConstants.Lexdb; }
		}

		#endregion
	}
}