// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Chorus.FileTypeHandlers;
using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;
using LibFLExBridgeChorusPlugin.DomainServices;
using LibFLExBridgeChorusPlugin.Infrastructure;
using SIL.IO;
using System.ComponentModel.Composition;

namespace LibFLExBridgeChorusPlugin.Handling.Linguistics.Reversal
{
	[Export(typeof(IFieldWorksFileHandler))]
	internal sealed class ReversalFileTypeHandlerStrategy : IFieldWorksFileHandler
	{
		#region Implementation of IFieldWorksFileHandler

		public bool CanValidateFile(string pathToFile)
		{
			return PathHelper.CheckValidPathname(pathToFile, FlexBridgeConstants.Reversal);
		}

		public string ValidateFile(string pathToFile)
		{
			try
			{
				var doc = XDocument.Load(pathToFile);
				var root = doc.Root;
				if (root.Name.LocalName != "Reversal")
					return "Not a FieldWorks reversal file.";
				var header = root.Element(FlexBridgeConstants.Header);
				var result = CmObjectValidator.ValidateObject(MetadataCache.MdCache, header.Element("ReversalIndex"));
				if (result != null)
					return result;
				foreach (var record in root.Elements(FlexBridgeConstants.ReversalIndexEntry))
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
				FlexBridgeConstants.ReversalIndexEntry, FlexBridgeConstants.GuidStr);
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			FieldWorksCommonFileHandler.Do3WayMerge(mergeOrder, mdc, true);
		}

		public string Extension
		{
			get { return FlexBridgeConstants.Reversal; }
		}

		#endregion
	}
}