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

namespace FLEx_ChorusPlugin.Infrastructure.Handling.Scripture
{
	internal sealed class ScriptureReferenceSystemTypeHandlerStrategy : IFieldWorksFileHandler
	{
		private const string ScrRefSystem = "ScrRefSystem";

		#region Implementation of IFieldWorksFileHandler

		public bool CanValidateFile(string pathToFile)
		{
			return FileUtils.CheckValidPathname(pathToFile, SharedConstants.Srs) &&
				   Path.GetFileName(pathToFile) == SharedConstants.ScriptureReferenceSystemFilename;
		}

		public string ValidateFile(string pathToFile)
		{
			try
			{
				var doc = XDocument.Load(pathToFile);
				var root = doc.Root;
				if (root.Name.LocalName != SharedConstants.ScriptureReferenceSystem || root.Element(ScrRefSystem) == null)
					return "Not valid Scripture reference system file.";

				return CmObjectValidator.ValidateObject(MetadataCache.MdCache, root.Element(ScrRefSystem));
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
				ScrRefSystem, SharedConstants.GuidStr);
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			mdc.AddCustomPropInfo(mergeOrder); // NB: Must be done before FieldWorksCommonMergeStrategy is created.

			XmlMergeService.Do3WayMerge(mergeOrder,
				new FieldWorksCommonMergeStrategy(mergeOrder.MergeSituation, mdc),
				null,
				ScrRefSystem, SharedConstants.GuidStr, WritePreliminaryScriptureReferenceSystemInformation);
		}

		public string Extension
		{
			get { return SharedConstants.Srs; }
		}

		#endregion

		private static void WritePreliminaryScriptureReferenceSystemInformation(XmlReader reader, XmlWriter writer)
		{
			reader.MoveToContent();
			writer.WriteStartElement(SharedConstants.ImportSettings);
			reader.Read();
		}
	}
}