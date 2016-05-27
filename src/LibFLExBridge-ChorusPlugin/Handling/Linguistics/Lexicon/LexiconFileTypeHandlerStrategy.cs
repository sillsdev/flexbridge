// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: license.rtf file)

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Chorus.FileTypeHandlers;
using Chorus.VcsDrivers.Mercurial;
using Chorus.merge;
using LibFLExBridgeChorusPlugin.DomainServices;
using LibFLExBridgeChorusPlugin.Infrastructure;
using Palaso.IO;
using System.ComponentModel.Composition;

namespace LibFLExBridgeChorusPlugin.Handling.Linguistics.Lexicon
{
	[Export(typeof(IFieldWorksFileHandler))]
	internal sealed class LexiconFileTypeHandlerStrategy : IFieldWorksFileHandler
	{
		private IFieldWorksFileHandler AsIFieldWorksFileHandler
		{
			get { return this; }
		}

		#region Implementation of IFieldWorksFileHandler

		bool IFieldWorksFileHandler.CanValidateFile(string pathToFile)
		{
			return FileUtils.CheckValidPathname(pathToFile, AsIFieldWorksFileHandler.Extension)
				&& Path.GetFileNameWithoutExtension(pathToFile).StartsWith(FlexBridgeConstants.Lexicon + "_");
		}

		string IFieldWorksFileHandler.ValidateFile(string pathToFile)
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

		IChangePresenter IFieldWorksFileHandler.GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			return FieldWorksChangePresenter.GetCommonChangePresenter(report, repository);
		}

		IEnumerable<IChangeReport> IFieldWorksFileHandler.Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			return Xml2WayDiffService.ReportDifferences(repository, parent, child,
				FlexBridgeConstants.Header,
				FlexBridgeConstants.LexEntry, FlexBridgeConstants.GuidStr);
		}

		void IFieldWorksFileHandler.Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			FieldWorksCommonFileHandler.Do3WayMerge(mergeOrder, mdc, true);
		}

		string IFieldWorksFileHandler.Extension
		{
			get { return FlexBridgeConstants.Lexdb; }
		}

		#endregion
	}
}