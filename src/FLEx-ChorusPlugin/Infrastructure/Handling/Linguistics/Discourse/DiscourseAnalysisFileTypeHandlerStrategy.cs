// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Chorus.FileTypeHanders;
using Chorus.VcsDrivers.Mercurial;
using Chorus.merge;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using SIL.IO;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.Linguistics.Discourse
{
	internal sealed class DiscourseAnalysisFileTypeHandlerStrategy : IFieldWorksFileHandler
	{
		#region Implementation of IFieldWorksFileHandler

		public bool CanValidateFile(string pathToFile)
		{
			return FileUtils.CheckValidPathname(pathToFile, SharedConstants.DiscourseExt) &&
				   Path.GetFileName(pathToFile) == SharedConstants.DiscourseChartFilename;
		}

		public string ValidateFile(string pathToFile)
		{
			try
			{
				var doc = XDocument.Load(pathToFile);
				var root = doc.Root;
				if (root.Name.LocalName != SharedConstants.DiscourseRootFolder
					|| root.Element(SharedConstants.Header) == null
					|| root.Element(SharedConstants.Header).Element("DsDiscourseData") == null)
				{
					return "Not valid discourse file.";
				}

				var mdc = MetadataCache.MdCache;
				var result = CmObjectValidator.ValidateObject(mdc, root.Element(SharedConstants.Header).Element("DsDiscourseData"));
				if (result != null)
					return result;

				foreach (var element in root.Elements(SharedConstants.DsChart))
				{
					if (element.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant() == SharedConstants.EmptyGuid)
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
				SharedConstants.Header,
				SharedConstants.DsChart, SharedConstants.GuidStr);
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			FieldWorksCommonFileHandler.Do3WayMerge(mergeOrder, mdc, true);
		}

		public string Extension
		{
			get { return SharedConstants.DiscourseExt; }
		}

		#endregion
	}
}