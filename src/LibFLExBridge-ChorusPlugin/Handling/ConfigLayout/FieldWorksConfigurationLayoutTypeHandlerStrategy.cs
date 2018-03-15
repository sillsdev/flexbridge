// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Chorus.FileTypeHandlers;
using Chorus.VcsDrivers.Mercurial;
using Chorus.merge;
using Chorus.merge.xml.generic;
using SIL.IO;
using LibFLExBridgeChorusPlugin.Infrastructure;
using System.ComponentModel.Composition;

namespace LibFLExBridgeChorusPlugin.Handling.ConfigLayout
{
	[Export(typeof(IFieldWorksFileHandler))]
	internal sealed class FieldWorksConfigurationLayoutTypeHandlerStrategy : IFieldWorksFileHandler
	{
		#region Implementation of IFieldWorksFileHandler

		public bool CanValidateFile(string pathToFile)
		{
			return PathHelper.CheckValidPathname(pathToFile, Extension);
		}

		public string ValidateFile(string pathToFile)
		{
			return FieldWorksConfigurationLayoutValidator.Validate(pathToFile);
		}

		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			return FieldWorksCustomLayoutChangePresenter.GetCommonChangePresenter(report, repository);
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			return Xml2WayDiffService.ReportDifferences(
				parent, CustomLayoutDataCollectorMethod.GetDataFromRevision(parent, repository),
				child, CustomLayoutDataCollectorMethod.GetDataFromRevision(child, repository));
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			var merger = new XmlMerger(mergeOrder.MergeSituation)
				{
					EventListener = mergeOrder.EventListener
				};
			CustomLayoutMergeStrategiesMethod.AddElementStrategies(merger.MergeStrategies);
			CustomLayoutMergeService.DoMerge(mergeOrder, merger);
		}

		public string Extension
		{
			get { return FlexBridgeConstants.fwlayout; }
		}

		#endregion
	}
}