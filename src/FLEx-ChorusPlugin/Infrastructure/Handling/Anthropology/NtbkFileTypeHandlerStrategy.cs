using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Chorus.FileTypeHanders;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers.Mercurial;
using Palaso.IO;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.Anthropology
{
	internal sealed class NtbkFileTypeHandlerStrategy : IFieldWorksFileHandler
	{
		#region Implementation of IFieldWorksFileHandler

		public bool CanValidateFile(string pathToFile)
		{
			if (!FileUtils.CheckValidPathname(pathToFile, SharedConstants.Ntbk))
				return false;

			if (Path.GetFileName(pathToFile) != "DataNotebook.ntbk")
				return false;

			return ValidateFile(pathToFile) == null;
		}

		public string ValidateFile(string pathToFile)
		{
			try
			{
				var settings = new XmlReaderSettings { ValidationType = ValidationType.None };
				using (var reader = XmlReader.Create(pathToFile, settings))
				{
					reader.MoveToContent();
					if (reader.LocalName == "Anthropology")
					{
						// It would be nice, if it could really validate it.
						while (reader.Read())
						{
						}
					}
					else
					{
						throw new InvalidOperationException("Not a FieldWorks data notebook file.");
					}
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
				"RnGenericRec", SharedConstants.GuidStr);
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			FieldWorksMergeStrategyServices.AddCustomPropInfo(mdc, mergeOrder, "Anthropology", 1); // NB: Must be done before FieldWorksAnthropologyMergeStrategy is created.

			XmlMergeService.Do3WayMerge(mergeOrder,
				new FieldWorksCommonMergeStrategy(mergeOrder.MergeSituation, mdc),
				SharedConstants.Header,
				"RnGenericRec", SharedConstants.GuidStr, WritePreliminaryAnthropologyInformation);
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