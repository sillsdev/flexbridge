// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Chorus.FileTypeHanders;
using Chorus.VcsDrivers.Mercurial;
using Chorus.merge;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using SIL.IO;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.Linguistics.WordformInventory
{
	internal sealed class WordformInventoryFileTypeHandlerStrategy : IFieldWorksFileHandler
	{
		#region Implementation of IFieldWorksFileHandler

		public bool CanValidateFile(string pathToFile)
		{
			return FileUtils.CheckValidPathname(pathToFile, SharedConstants.Inventory)
				&& Path.GetFileNameWithoutExtension(pathToFile).StartsWith(SharedConstants.WordformInventory + "_");
		}

		public string ValidateFile(string pathToFile)
		{
			try
			{
				var doc = XDocument.Load(pathToFile);
				var root = doc.Root;
				if (root.Name.LocalName != SharedConstants.WordformInventoryRootFolder // "Inventory"
					|| (root.Element(SharedConstants.Header) != null && !root.Element(SharedConstants.Header).Elements("PunctuationForm").Any()))
				{
					return "Not valid inventory file";
				}

				// Header is optional, but if present it must have at least one PunctuationForm.
				var header = root.Element(SharedConstants.Header);
				if (header != null)
				{
					foreach (var punctResult in header.Elements("PunctuationForm").Select(punctForm => CmObjectValidator.ValidateObject(MetadataCache.MdCache, punctForm)).Where(punctResult => punctResult != null))
					{
						return punctResult;
					}
				}
				foreach (var record in root.Elements(SharedConstants.WfiWordform))
				{
					if (record.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant() == SharedConstants.EmptyGuid)
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
				SharedConstants.Header,
				SharedConstants.WfiWordform, SharedConstants.GuidStr);
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			FieldWorksCommonFileHandler.Do3WayMerge(mergeOrder, mdc, true);
		}

		public string Extension
		{
			get { return SharedConstants.Inventory; }
		}

		#endregion
	}
}