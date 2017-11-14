// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: license.rtf file)

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
		private IFieldWorksFileHandler AsIFieldWorksFileHandler
		{
			get { return this; }
		}

		#region Implementation of IFieldWorksFileHandler

		bool IFieldWorksFileHandler.CanValidateFile(string pathToFile)
		{
			return FileUtils.CheckValidPathname(pathToFile, AsIFieldWorksFileHandler.Extension);
		}

		string IFieldWorksFileHandler.ValidateFile(string pathToFile)
		{
			return FieldWorksConfigurationLayoutValidator.Validate(pathToFile);
		}

		IChangePresenter IFieldWorksFileHandler.GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			return FieldWorksCustomLayoutChangePresenter.GetCommonChangePresenter(report, repository);
		}

		IEnumerable<IChangeReport> IFieldWorksFileHandler.Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			return Xml2WayDiffService.ReportDifferences(
				parent, CustomLayoutDataCollectorMethod.GetDataFromRevision(parent, repository),
				child, CustomLayoutDataCollectorMethod.GetDataFromRevision(child, repository));
		}

		void IFieldWorksFileHandler.Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			var merger = new XmlMerger(mergeOrder.MergeSituation)
				{
					EventListener = mergeOrder.EventListener
				};
			CustomLayoutMergeStrategiesMethod.AddElementStrategies(merger.MergeStrategies);
			CustomLayoutMergeService.DoMerge(mergeOrder, merger);
		}

		string IFieldWorksFileHandler.Extension
		{
			get { return FlexBridgeConstants.fwlayout; }
		}

		#endregion
	}
}
