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
using Palaso.IO;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.General
{
	/// <summary>
	/// This class deals with files with extension of "lint".
	/// There is only one of them with a fixed name.
	/// </summary>
	internal sealed class LintFileTypeHandlerStrategy : IFieldWorksFileHandler
	{
		#region Implementation of IFieldWorksFileHandler

		public bool CanValidateFile(string pathToFile)
		{
			if (!FileUtils.CheckValidPathname(pathToFile, SharedConstants.lint))
				return false;
			if (Path.GetFileName(pathToFile) != SharedConstants.LintFilename)
				return false;

			return ValidateFile(pathToFile) == null;
		}

		public string ValidateFile(string pathToFile)
		{
			try
			{
				var doc = XDocument.Load(pathToFile);
				var root = doc.Root;
				if (root.Name.LocalName != "curiosities"
					|| root.Element(SharedConstants.Header) != null
					|| !root.Elements(SharedConstants.curiosity).Any())
				{
					return "Not a valid odd ducks file.";
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
				null,
				SharedConstants.curiosity, SharedConstants.GuidStr);
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			mdc.AddCustomPropInfo(mergeOrder); // NB: Must be done before FieldWorksCommonMergeStrategy is created.

			XmlMergeService.Do3WayMerge(mergeOrder,
				new FieldWorksCommonMergeStrategy(mergeOrder.MergeSituation, mdc),
				null,
				SharedConstants.curiosity, SharedConstants.GuidStr, WritePreliminaryLintInformation);
		}

		public string Extension
		{
			get { return SharedConstants.lint; }
		}

		#endregion

		private static void WritePreliminaryLintInformation(XmlReader reader, XmlWriter writer)
		{
			reader.MoveToContent();
			writer.WriteStartElement("curiosities");
			reader.Read();
		}
	}
}