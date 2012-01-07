using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Chorus.FileTypeHanders;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers.Mercurial;
using FLEx_ChorusPlugin.Infrastructure;
using Palaso.IO;
using Palaso.Progress.LogBox;

namespace FLEx_ChorusPlugin.Contexts.Anthropology
{
	internal sealed class FieldWorksAnthropologyTypeHandler : FieldWorksInternalFieldWorksFileHandlerBase
	{
		private const string Extension = "ntbk";
		private readonly MetadataCache _mdc = MetadataCache.MdCache; // Theory has it that the model veriosn file was process already, so the version is current.

		internal override bool CanValidateFile(string pathToFile)
		{
			if (!FileUtils.CheckValidPathname(pathToFile, Extension))
				return false;

			if (Path.GetFileName(pathToFile) != "DataNotebook.ntbk")
				return false;

			return DoValidation(pathToFile) == null;
		}

		internal override void Do3WayMerge(MergeOrder mergeOrder)
		{
			if (mergeOrder == null) throw new ArgumentNullException("mergeOrder");

			// Add optional custom property information to MDC.
			FieldWorksMergeStrategyServices.AddCustomPropInfo(_mdc, mergeOrder, "Linguistics", 1);

			XmlMergeService.Do3WayMerge(mergeOrder,
				new FieldWorksAnthropologyMergeStrategy(mergeOrder.MergeSituation, _mdc),
				"header",
				"RnGenericRec", "guid", WritePreliminaryInformation);
		}

		internal override IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			return Xml2WayDiffService.ReportDifferences(repository, parent, child,
				"header",
				"RnGenericRec", "guid");
		}

		internal override IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			throw new NotImplementedException();
		}

		internal override string ValidateFile(string pathToFile, IProgress progress)
		{
			return DoValidation(pathToFile);
		}

		private static string DoValidation(string pathToFile)
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

		private static void WritePreliminaryInformation(XmlReader reader, XmlWriter writer)
		{
			reader.MoveToContent();
			writer.WriteStartElement("Anthropology");
			reader.Read();
		}
	}
}