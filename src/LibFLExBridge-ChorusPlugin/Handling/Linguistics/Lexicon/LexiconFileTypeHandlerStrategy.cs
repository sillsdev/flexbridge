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

namespace LibFLExBridgeChorusPlugin.Handling.Linguistics.Lexicon
{
	[Export(typeof(IFieldWorksFileHandler))]
	internal sealed class LexiconFileTypeHandlerStrategy : IFieldWorksFileHandler
	{
		#region Implementation of IFieldWorksFileHandler

		public bool CanValidateFile(string pathToFile)
		{
			return PathHelper.CheckValidPathname(pathToFile, FlexBridgeConstants.Lexdb)
				&& Path.GetFileNameWithoutExtension(pathToFile).StartsWith(FlexBridgeConstants.Lexicon + "_");
		}

		public string ValidateFile(string pathToFile)
		{
			try
			{
				var doc = XDocument.Load(pathToFile);
				var root = doc.Root;
				if (root.Name.LocalName != FlexBridgeConstants.Lexicon)
				{
					return "Not valid lexicon file";
				}

				var mdc = MetadataCache.MdCache;
				var header = root.Element(FlexBridgeConstants.Header);
				if (header != null)
				{
					var lexDb = header.Element(FlexBridgeConstants.LexDb);
					if (lexDb == null)
					{
						return "Not valid lexicon file";
					}
					var result = CmObjectValidator.ValidateObject(mdc, lexDb);
					if (result != null)
						return result;
				}

				foreach (var entryElement in root.Elements(FlexBridgeConstants.LexEntry))
				{
					if (entryElement.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant() == FlexBridgeConstants.EmptyGuid)
						return null;
					var result = CmObjectValidator.ValidateObject(mdc, entryElement);
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
				FlexBridgeConstants.LexEntry, FlexBridgeConstants.GuidStr);
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			FieldWorksCommonFileHandler.Do3WayMerge(mergeOrder, mdc, true);
		}

		public string Extension
		{
			get { return FlexBridgeConstants.Lexdb; }
		}

		#endregion
	}
}