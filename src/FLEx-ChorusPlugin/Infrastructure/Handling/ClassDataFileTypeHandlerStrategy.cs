using System;
using System.Collections.Generic;
using System.Xml;
using Chorus.FileTypeHanders;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers.Mercurial;
using Palaso.IO;

namespace FLEx_ChorusPlugin.Infrastructure.Handling
{
	internal sealed class ClassDataFileTypeHandlerStrategy : IFieldWorksFileHandler
	{
		#region Implementation of IFieldWorksFileHandler

		public bool CanValidateFile(string pathToFile)
		{
			if (!FileUtils.CheckValidPathname(pathToFile, SharedConstants.ClassData))
				return false;

			return (ValidateFile(pathToFile) == null);
		}

		public string ValidateFile(string pathToFile)
		{
			try
			{
				var settings = new XmlReaderSettings { ValidationType = ValidationType.None };
				using (var reader = XmlReader.Create(pathToFile, settings))
				{
					reader.MoveToContent();
					if (reader.LocalName == "classdata")
					{
						// It would be nice, if it could really validate it.
						while (reader.Read())
						{
						}
					}
					else
					{
						throw new InvalidOperationException("Not a FieldWorks file.");
					}
				}
			}
			catch (Exception error)
			{
				return error.Message;
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
				null,
				SharedConstants.RtTag, SharedConstants.GuidStr);
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			mdc.AddCustomPropInfo(mergeOrder); // NB: Must be done before FieldWorksMergingStrategy is created.

			XmlMergeService.Do3WayMerge(mergeOrder,
				new FieldWorksMergingStrategy(mergeOrder.MergeSituation, mdc),
				null,
				SharedConstants.RtTag, SharedConstants.GuidStr, WritePreliminaryClassDataInformation);
		}

		public string Extension
		{
			get { return SharedConstants.ClassData; }
		}

		#endregion

		private static void WritePreliminaryClassDataInformation(XmlReader reader, XmlWriter writer)
		{
			reader.MoveToContent();
			writer.WriteStartElement("classdata");
			reader.Read();
		}
	}
}