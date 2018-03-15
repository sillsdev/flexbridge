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
using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;
using SIL.IO;
using LibFLExBridgeChorusPlugin.DomainServices;
using LibFLExBridgeChorusPlugin.Infrastructure;
using System.ComponentModel.Composition;

namespace LibFLExBridgeChorusPlugin.Handling.Anthropology
{
	[Export(typeof(IFieldWorksFileHandler))]
	internal sealed class NtbkFileTypeHandlerStrategy : IFieldWorksFileHandler
	{
		#region Implementation of IFieldWorksFileHandler

		public bool CanValidateFile(string pathToFile)
		{
			return PathHelper.CheckValidPathname(pathToFile, FlexBridgeConstants.Ntbk) &&
				   Path.GetFileName(pathToFile) == FlexBridgeConstants.DataNotebookFilename;
		}

		public string ValidateFile(string pathToFile)
		{
			try
			{
				var doc = XDocument.Load(pathToFile);
				var root = doc.Root;
				if (root.Name.LocalName != FlexBridgeConstants.Anthropology)
					return "Not a FieldWorks data notebook file.";
				var header = root.Element(FlexBridgeConstants.Header);
				var result = CmObjectValidator.ValidateObject(MetadataCache.MdCache, header.Element("RnResearchNbk"));
				if (result != null)
					return result;
				foreach (var record in root.Elements(FlexBridgeConstants.RnGenericRec))
				{
					if (record.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant() == FlexBridgeConstants.EmptyGuid)
						return null;
					result = CmObjectValidator.ValidateObject(MetadataCache.MdCache, record);
					if (result != null)
						return result;
				}
			}
			catch (Exception e)
			{
				return e.Message;
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
				FlexBridgeConstants.Header,
				FlexBridgeConstants.RnGenericRec, FlexBridgeConstants.GuidStr);
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			FieldWorksCommonFileHandler.Do3WayMerge(mergeOrder, mdc, true);
		}

		public string Extension
		{
			get { return FlexBridgeConstants.Ntbk; }
		}

		#endregion
	}
}