// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Chorus.FileTypeHandlers;
using Chorus.VcsDrivers.Mercurial;
using Chorus.merge;
using LibFLExBridgeChorusPlugin.DomainServices;
using LibFLExBridgeChorusPlugin.Infrastructure;
using SIL.IO;
using System.ComponentModel.Composition;

namespace LibFLExBridgeChorusPlugin.Handling.Linguistics.WordformInventory
{
	[Export(typeof(IFieldWorksFileHandler))]
	internal sealed class WordformInventoryFileTypeHandlerStrategy : IFieldWorksFileHandler
	{
		#region Implementation of IFieldWorksFileHandler

		public bool CanValidateFile(string pathToFile)
		{
			return PathHelper.CheckValidPathname(pathToFile, FlexBridgeConstants.Inventory)
				&& Path.GetFileNameWithoutExtension(pathToFile).StartsWith(FlexBridgeConstants.WordformInventory + "_");
		}

		public string ValidateFile(string pathToFile)
		{
			try
			{
				var doc = XDocument.Load(pathToFile);
				var root = doc.Root;
				if (root.Name.LocalName != FlexBridgeConstants.WordformInventoryRootFolder // "Inventory"
					|| (root.Element(FlexBridgeConstants.Header) != null && !root.Element(FlexBridgeConstants.Header).Elements("PunctuationForm").Any()))
				{
					return "Not valid inventory file";
				}

				// Header is optional, but if present it must have at least one PunctuationForm.
				var header = root.Element(FlexBridgeConstants.Header);
				if (header != null)
				{
					foreach (var punctResult in header.Elements("PunctuationForm").Select(punctForm => CmObjectValidator.ValidateObject(MetadataCache.MdCache, punctForm)).Where(punctResult => punctResult != null))
					{
						return punctResult;
					}
				}
				foreach (var record in root.Elements(FlexBridgeConstants.WfiWordform))
				{
					if (record.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant() == FlexBridgeConstants.EmptyGuid)
						return null;
					var result = CmObjectValidator.ValidateObject(MetadataCache.MdCache, record);
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
			return Xml2WayDiffService.ReportDifferences(
				repository,
				parent,
				child,
				FlexBridgeConstants.Header,
				FlexBridgeConstants.WfiWordform, FlexBridgeConstants.GuidStr);
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			FieldWorksCommonFileHandler.Do3WayMerge(mergeOrder, mdc, true);
		}

		public string Extension
		{
			get { return FlexBridgeConstants.Inventory; }
		}

		#endregion
	}
}