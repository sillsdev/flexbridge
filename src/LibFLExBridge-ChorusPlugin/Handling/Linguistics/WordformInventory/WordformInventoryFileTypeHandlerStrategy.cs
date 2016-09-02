// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: license.rtf file)

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
using Palaso.IO;
using System.ComponentModel.Composition;

namespace LibFLExBridgeChorusPlugin.Handling.Linguistics.WordformInventory
{
	[Export(typeof(IFieldWorksFileHandler))]
	internal sealed class WordformInventoryFileTypeHandlerStrategy : IFieldWorksFileHandler
	{
		private IFieldWorksFileHandler AsIFieldWorksFileHandler
		{
			get { return this; }
		}

		#region Implementation of IFieldWorksFileHandler

		bool IFieldWorksFileHandler.CanValidateFile(string pathToFile)
		{
			return FileUtils.CheckValidPathname(pathToFile, AsIFieldWorksFileHandler.Extension)
				&& Path.GetFileNameWithoutExtension(pathToFile).StartsWith(FlexBridgeConstants.WordformInventory + "_");
		}

		string IFieldWorksFileHandler.ValidateFile(string pathToFile)
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

		IChangePresenter IFieldWorksFileHandler.GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			return FieldWorksChangePresenter.GetCommonChangePresenter(report, repository);
		}

		IEnumerable<IChangeReport> IFieldWorksFileHandler.Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			return Xml2WayDiffService.ReportDifferences(
				repository,
				parent,
				child,
				FlexBridgeConstants.Header,
				FlexBridgeConstants.WfiWordform, FlexBridgeConstants.GuidStr);
		}

		void IFieldWorksFileHandler.Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			FieldWorksCommonFileHandler.Do3WayMerge(mergeOrder, mdc, true);
		}

		string IFieldWorksFileHandler.Extension
		{
			get { return FlexBridgeConstants.Inventory; }
		}

		#endregion
	}
}