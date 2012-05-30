using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHanders;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers.Mercurial;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using Palaso.IO;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.Anthropology
{
	internal sealed class NtbkFileTypeHandlerStrategy : IFieldWorksFileHandler
	{
		#region Implementation of IFieldWorksFileHandler

		private const string RnGenericRec = "RnGenericRec";

		public bool CanValidateFile(string pathToFile)
		{
			return FileUtils.CheckValidPathname(pathToFile, SharedConstants.Ntbk) &&
				   Path.GetFileName(pathToFile) == SharedConstants.DataNotebookFilename;
		}

		public string ValidateFile(string pathToFile)
		{
			try
			{
				var doc = XDocument.Load(pathToFile);
				var root = doc.Root;
				if (root.Name.LocalName != SharedConstants.Anthropology)
					return "Not a FieldWorks data notebook file.";
				var header = root.Element(SharedConstants.Header);
				var result = CmObjectValidator.ValidateObject(MetadataCache.MdCache, header.Element("RnResearchNbk"));
				if (result != null)
					return result;
				foreach (var record in root.Elements(RnGenericRec))
				{
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
				RnGenericRec, SharedConstants.GuidStr);
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			mdc.AddCustomPropInfo(mergeOrder); // NB: Must be done before FieldWorksAnthropologyMergeStrategy is created.

			XmlMergeService.Do3WayMerge(mergeOrder,
				new FieldWorksHeaderedMergeStrategy(mergeOrder.MergeSituation, mdc),
				SharedConstants.Header,
				RnGenericRec, SharedConstants.GuidStr, WritePreliminaryAnthropologyInformation);
		}

		public string Extension
		{
			get { return SharedConstants.Ntbk; }
		}

		#endregion

		private static void WritePreliminaryAnthropologyInformation(XmlReader reader, XmlWriter writer)
		{
			reader.MoveToContent();
			writer.WriteStartElement("Anthropology");
			reader.Read();
		}
	}
}