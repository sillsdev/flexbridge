using System;
using System.Collections.Generic;
using System.Xml;
using Chorus.FileTypeHanders;
using Chorus.FileTypeHanders.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers.Mercurial;
using FLEx_ChorusPlugin.Contexts.General;
using FLEx_ChorusPlugin.Infrastructure;
using Palaso.IO;
using Palaso.Progress.LogBox;

namespace FLEx_ChorusPlugin.Contexts.Linguistics.Reversals
{
	///<summary>
	/// Handler for '.reversal' extension for FieldWorks reversal files.
	///</summary>
	internal sealed class FieldWorksReversalTypeHandler : FieldWorksInternalFieldWorksFileHandlerBase
	{
		private const string Extension = "reversal";
		private readonly MetadataCache _mdc = MetadataCache.MdCache; // Theory has it that the model veriosn file was process already, so the version is current.

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
			if (mergeOrder == null) throw new ArgumentNullException("mergeOrder");

			// Add optional custom property information to MDC.
			FieldWorksMergeStrategyServices.AddCustomPropInfo(_mdc, mergeOrder, "Linguistics", 1);

			XmlMergeService.Do3WayMerge(mergeOrder,
				new FieldWorksReversalMergeStrategy(mergeOrder.MergeSituation, _mdc),
				"header",
				"ReversalIndexEntry", "guid", WritePreliminaryInformation);
		}

		internal override IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			return Xml2WayDiffService.ReportDifferences(repository, parent, child,
				"header",
				"ReversalIndexEntry", "guid");
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
				var settings = new XmlReaderSettings { ValidationType = ValidationType.None };
				using (var reader = XmlReader.Create(pathToFile, settings))
				{
					reader.MoveToContent();
					if (reader.LocalName == "Reversal")
					{
						// It would be nice, if it could really validate it.
						while (reader.Read())
						{
						}
					}
					else
					{
						throw new InvalidOperationException("Not a FieldWorks reversal file.");
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
			writer.WriteStartElement("Reversal");
			reader.Read();
		}
	}
}
