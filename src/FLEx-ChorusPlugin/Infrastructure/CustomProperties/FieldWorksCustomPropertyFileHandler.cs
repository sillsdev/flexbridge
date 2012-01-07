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
using Palaso.IO;
using Palaso.Progress.LogBox;
using FieldWorksChangePresenter = FLEx_ChorusPlugin.Contexts.General.FieldWorksChangePresenter;

namespace FLEx_ChorusPlugin.Infrastructure.CustomProperties
{
	/// <summary>
	/// Handle the FieldWorks custom properties file
	/// </summary>
	internal sealed class FieldWorksCustomPropertyFileHandler : FieldWorksInternalFieldWorksFileHandlerBase
	{
		private const string Extension = "CustomProperties";

		internal override bool CanValidateFile(string pathToFile)
		{
			if (!FileUtils.CheckValidPathname(pathToFile, Extension))
				return false;

			return DoValidation(pathToFile) == null;
		}

		/// <summary>
		/// Do a 3-file merge, placing the result over the "ours" file and returning an error status
		/// </summary>
		/// <remarks>Implementations can exit with an exception, which the caller will catch and deal with.
		/// The must not have any UI, no interaction with the user.</remarks>
		internal override void Do3WayMerge(MergeOrder mergeOrder)
		{
			XmlMergeService.Do3WayMerge(mergeOrder,
				new FieldWorksCustomPropertyMergingStrategy(mergeOrder.MergeSituation),
				null,
				"CustomField", "key", WritePreliminaryInformation);
		}

		internal override IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			return Xml2WayDiffService.ReportDifferences(repository, parent, child,
				null,
				"CustomField", "key");
		}

		internal override IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			if (report is IXmlChangeReport)
				return new FieldWorksChangePresenter((IXmlChangeReport)report);

			if (report is ErrorDeterminingChangeReport)
				return (IChangePresenter)report;

			return new DefaultChangePresenter(report, repository);
		}

		/// <summary>
		/// return null if valid, otherwise nice verbose description of what went wrong
		/// </summary>
		internal override string ValidateFile(string pathToFile, IProgress progress)
		{
			return DoValidation(pathToFile);
		}

		private static string DoValidation(string pathToFile)
		{
			try
			{
				var doc = XDocument.Load(pathToFile);
				var root = doc.Root;
				if (root.Name.LocalName != "AdditionalFields" || root.Elements("CustomField").Count() == 0)
					return "Not valid custom properties file";

				return null;
			}
			catch (Exception e)
			{
				return e.Message;
			}
		}

		private static void WritePreliminaryInformation(XmlReader reader, XmlWriter writer)
		{
			reader.MoveToContent();
			writer.WriteStartElement("AdditionalFields");
			reader.Read();
		}
	}
}
