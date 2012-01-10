using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHanders;
using Chorus.FileTypeHanders.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers.Mercurial;
using FLEx_ChorusPlugin.Infrastructure.Handling.CustomProperties;
using Palaso.IO;

namespace FLEx_ChorusPlugin.Infrastructure.Handling
{
	internal sealed class CustomPropertiesTypeHandlerStrategy : IFieldWorksFileHandler
	{
		#region Implementation of IFieldWorksFileHandler

		public bool CanValidateFile(string pathToFile)
		{
			if (!FileUtils.CheckValidPathname(pathToFile, SharedConstants.CustomProperties))
				return false;

			return ValidateFile(pathToFile) == null;
		}

		public string ValidateFile(string pathToFile)
		{
			try
			{
				var doc = XDocument.Load(pathToFile);
				var root = doc.Root;
				if (root.Name.LocalName != SharedConstants.OptionalFirstElementTag || root.Elements("CustomField").Count() == 0)
					return "Not valid custom properties file";

				return null;
			}
			catch (Exception e)
			{
				return e.Message;
			}
		}

		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			if (report is IXmlChangeReport)
				return new FieldWorksChangePresenter((IXmlChangeReport)report);

			if (report is ErrorDeterminingChangeReport)
				return (IChangePresenter)report;

			return new DefaultChangePresenter(report, repository);
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			return Xml2WayDiffService.ReportDifferences(repository, parent, child,
				null,
				"CustomField", "key");
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			// NB: Doesn't need the mdc updated with custom props.
			XmlMergeService.Do3WayMerge(mergeOrder,
				new FieldWorksCustomPropertyMergingStrategy(mergeOrder.MergeSituation),
				null,
				"CustomField", "key", WritePreliminaryCustomPropertyInformation);
		}

		public string Extension
		{
			get { return SharedConstants.CustomProperties; }
		}

		#endregion

		private static void WritePreliminaryCustomPropertyInformation(XmlReader reader, XmlWriter writer)
		{
			reader.MoveToContent();
			writer.WriteStartElement(SharedConstants.OptionalFirstElementTag);
			reader.Read();
		}
	}
}