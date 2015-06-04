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

namespace FLEx_ChorusPlugin.Infrastructure.Handling.Linguistics.Lexicon
{
	internal sealed class LexiconFileTypeHandlerStrategy : IFieldWorksFileHandler
	{
		#region Implementation of IFieldWorksFileHandler

		public bool CanValidateFile(string pathToFile)
		{
			return FileUtils.CheckValidPathname(pathToFile, SharedConstants.Lexdb)
				&& Path.GetFileNameWithoutExtension(pathToFile).StartsWith(SharedConstants.Lexicon + "_");
		}

		public string ValidateFile(string pathToFile)
		{
			try
			{
				var doc = XDocument.Load(pathToFile);
				var root = doc.Root;
				if (root.Name.LocalName != SharedConstants.Lexicon)
				{
					return "Not valid lexicon file";
				}

				var mdc = MetadataCache.MdCache;
				var header = root.Element(SharedConstants.Header);
				if (header != null)
				{
					var lexDb = header.Element(SharedConstants.LexDb);
					if (lexDb == null)
					{
						return "Not valid lexicon file";
					}
					var result = CmObjectValidator.ValidateObject(mdc, lexDb);
					if (result != null)
						return result;
				}

				foreach (var entryElement in root.Elements(SharedConstants.LexEntry))
				{
					if (entryElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant() == SharedConstants.EmptyGuid)
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
				SharedConstants.Header,
				SharedConstants.LexEntry, SharedConstants.GuidStr);
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			FieldWorksCommonFileHandler.Do3WayMerge(mergeOrder, mdc, true);
		}

		public string Extension
		{
			get { return SharedConstants.Lexdb; }
		}

		#endregion
	}
}