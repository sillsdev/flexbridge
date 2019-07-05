// Copyright (c) 2019 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHandlers;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers.Mercurial;
using LibFLExBridgeChorusPlugin.Infrastructure;
using SIL.IO;

namespace LibFLExBridgeChorusPlugin.Handling.Common
{
	[Export(typeof(IFieldWorksFileHandler))]
	class ProjectLexiconSettingsStrategy : IFieldWorksFileHandler
	{
		public bool CanValidateFile(string pathToFile)
		{
			return PathHelper.CheckValidPathname(pathToFile, Extension);
		}

		public string ValidateFile(string pathToFile)
		{
			var document = XDocument.Load(pathToFile);
			if (document.Root?.Name != "ProjectLexiconSettings")
			{
				return "Not a lexicon settings file.";
			}

			var writingSystemsElem = document.Root.Element("WritingSystems");
			if (writingSystemsElem == null)
			{
				return "Lexicon settings file has no writing systems element.";
			}
			var wsIdSet = new HashSet<string>();
			foreach (var writingSystemElem in writingSystemsElem.Elements("WritingSystem"))
			{
				var idAttribute = writingSystemElem.Attribute("id");
				if (idAttribute == null)
				{
					return "Writing system found with no id.";
				}
				if (wsIdSet.Contains(idAttribute.Value))
				{
					return $"Duplicate writing system found: {idAttribute.Value}";
				}

				wsIdSet.Add(idAttribute.Value);
			}
			// No errors found
			return null;
		}

		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			return FieldWorksChangePresenter.GetCommonChangePresenter(report, repository);
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child,
			HgRepository repository)
		{
			return Xml2WayDiffService.ReportDifferences(repository, parent, child, null, "WritingSystem", "id");

		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			var merger = new XmlMerger(mergeOrder.MergeSituation) { EventListener = mergeOrder.EventListener };
			var rootStrategy = ElementStrategy.CreateSingletonElement();
			merger.MergeStrategies.SetStrategy("ProjectLexiconSettings", rootStrategy);
			var writingSystemsStrategy = ElementStrategy.CreateSingletonElement();
			writingSystemsStrategy.Premerger = new LexiconSettingsWritingSystemsPreMerger();
			merger.MergeStrategies.SetStrategy("WritingSystems", writingSystemsStrategy);
			var writingSystemStrategy = ElementStrategy.CreateForKeyedElement("id", false);
			writingSystemStrategy.IsAtomic = true;
			merger.MergeStrategies.SetStrategy("WritingSystem", writingSystemStrategy);
			var mergeResults = merger.MergeFiles(mergeOrder.pathToOurs, mergeOrder.pathToTheirs, mergeOrder.pathToCommonAncestor);
			// Write merged data
			File.WriteAllText(mergeOrder.pathToOurs, mergeResults.MergedNode.OuterXml, Encoding.UTF8);
		}

		private sealed class LexiconSettingsWritingSystemsPreMerger : IPremerger
		{
			public void Premerge(IMergeEventListener listener, ref XmlNode ours, XmlNode theirs, XmlNode ancestor)
			{
				SetAttrToFalseIfNotFound("projectSharing", ancestor);
				SetAttrToFalseIfNotFound("projectSharing", ours);
				SetAttrToFalseIfNotFound("projectSharing", theirs);
				SetAttrToFalseIfNotFound("addToSldr", ancestor);
				SetAttrToFalseIfNotFound("addToSldr", ours);
				SetAttrToFalseIfNotFound("addToSldr", theirs);
			}

			private void SetAttrToFalseIfNotFound(string attName, XmlNode toChange)
			{
				if (toChange == null)
					return;

				if (toChange.Attributes?[attName] == null)
				{
					((XmlElement)toChange).SetAttribute(attName, "false"); // The default value is false in SIL.Lexicon 
				}
			}
		}

		public string Extension => "plsx";
	}
}
