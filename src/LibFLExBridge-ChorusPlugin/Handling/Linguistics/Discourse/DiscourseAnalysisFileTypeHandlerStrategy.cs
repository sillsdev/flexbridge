// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Chorus.FileTypeHandlers;
using Chorus.VcsDrivers.Mercurial;
using Chorus.merge;
using LibFLExBridgeChorusPlugin.DomainServices;
using LibFLExBridgeChorusPlugin.Infrastructure;
using SIL.IO;
using System.ComponentModel.Composition;

namespace LibFLExBridgeChorusPlugin.Handling.Linguistics.Discourse
{
	[Export(typeof(IFieldWorksFileHandler))]
	internal sealed class DiscourseAnalysisFileTypeHandlerStrategy : IFieldWorksFileHandler
	{
		#region Implementation of IFieldWorksFileHandler

		public bool CanValidateFile(string pathToFile)
		{
			return PathHelper.CheckValidPathname(pathToFile, FlexBridgeConstants.DiscourseExt) &&
				   Path.GetFileName(pathToFile) == FlexBridgeConstants.DiscourseChartFilename;
		}

		public string ValidateFile(string pathToFile)
		{
			try
			{
				var doc = XDocument.Load(pathToFile);
				var root = doc.Root;
				if (root.Name.LocalName != FlexBridgeConstants.DiscourseRootFolder
					|| root.Element(FlexBridgeConstants.Header) == null
					|| root.Element(FlexBridgeConstants.Header).Element("DsDiscourseData") == null)
				{
					return "Not valid discourse file.";
				}

				var mdc = MetadataCache.MdCache;
				var result = CmObjectValidator.ValidateObject(mdc, root.Element(FlexBridgeConstants.Header).Element("DsDiscourseData"));
				if (result != null)
					return result;

				foreach (var element in root.Elements(FlexBridgeConstants.DsChart))
				{
					if (element.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant() == FlexBridgeConstants.EmptyGuid)
						return null;
					result = CmObjectValidator.ValidateObject(mdc, element);
					if (result != null)
						return result;
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
				FlexBridgeConstants.Header,
				FlexBridgeConstants.DsChart, FlexBridgeConstants.GuidStr);
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			FieldWorksCommonFileHandler.Do3WayMerge(mergeOrder, mdc, true);
		}

		public string Extension
		{
			get { return FlexBridgeConstants.DiscourseExt; }
		}

		#endregion
	}
}