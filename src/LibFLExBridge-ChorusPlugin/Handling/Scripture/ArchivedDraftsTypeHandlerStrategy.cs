// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: license.rtf file)

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml.Linq;
using Chorus.FileTypeHandlers;
using Chorus.VcsDrivers.Mercurial;
using Chorus.merge;
using LibFLExBridgeChorusPlugin.DomainServices;
using LibFLExBridgeChorusPlugin.Infrastructure;
using Palaso.IO;

namespace LibFLExBridgeChorusPlugin.Handling.Scripture
{
	[Export(typeof(IFieldWorksFileHandler))]
	internal sealed class ArchivedDraftsTypeHandlerStrategy : IFieldWorksFileHandler
	{
		#region Implementation of IFieldWorksFileHandler

		bool IFieldWorksFileHandler.CanValidateFile(string pathToFile)
		{
			return FileUtils.CheckValidPathname(pathToFile, FlexBridgeConstants.ArchivedDraft);
		}

		string IFieldWorksFileHandler.ValidateFile(string pathToFile)
		{
			try
			{
				var doc = XDocument.Load(pathToFile);
				var root = doc.Root;
				if (root.Name.LocalName != FlexBridgeConstants.ArchivedDrafts || !root.Elements(FlexBridgeConstants.ScrDraft).Any())
					return "Not valid archived draft file.";

				foreach (var result in root.Elements(FlexBridgeConstants.ScrDraft).Select(draft => CmObjectValidator.ValidateObject(MetadataCache.MdCache, root.Element(FlexBridgeConstants.ScrDraft))).Where(result => result != null))
				{
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
			return Xml2WayDiffService.ReportDifferences(repository, parent, child,
				null,
				FlexBridgeConstants.ScrDraft, FlexBridgeConstants.GuidStr);
		}

		void IFieldWorksFileHandler.Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			FieldWorksCommonFileHandler.Do3WayMerge(mergeOrder, mdc, true);
		}

		string IFieldWorksFileHandler.Extension
		{
			get { return FlexBridgeConstants.ArchivedDraft; }
		}

		#endregion
	}
}