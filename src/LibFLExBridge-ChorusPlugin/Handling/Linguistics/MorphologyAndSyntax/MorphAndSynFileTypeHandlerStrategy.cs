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

namespace LibFLExBridgeChorusPlugin.Handling.Linguistics.MorphologyAndSyntax
{
	[Export(typeof(IFieldWorksFileHandler))]
	internal sealed class MorphAndSynFileTypeHandlerStrategy : IFieldWorksFileHandler
	{
		#region Implementation of IFieldWorksFileHandler

		public bool CanValidateFile(string pathToFile)
		{
			return PathHelper.CheckValidPathname(pathToFile, FlexBridgeConstants.Morphdata) &&
				   Path.GetFileName(pathToFile) == FlexBridgeConstants.MorphAndSynDataFilename;
		}

		public string ValidateFile(string pathToFile)
		{
			try
			{
				var doc = XDocument.Load(pathToFile);
				var root = doc.Root;
				if (root.Name.LocalName != FlexBridgeConstants.MorphAndSynData
					|| (root.Element(FlexBridgeConstants.Header) != null)
					|| root.Elements(FlexBridgeConstants.MoMorphData).Count() != 1)
				{
					return "Not valid morphology data file";
				}

				return CmObjectValidator.ValidateObject(MetadataCache.MdCache, root.Element(FlexBridgeConstants.MoMorphData));
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
			return Xml2WayDiffService.ReportDifferences(
				repository,
				parent,
				child,
				null,
				FlexBridgeConstants.MoMorphData, FlexBridgeConstants.GuidStr);
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			FieldWorksCommonFileHandler.Do3WayMerge(mergeOrder, mdc, true);
		}

		public string Extension
		{
			get { return FlexBridgeConstants.Morphdata; }
		}

		#endregion
	}
}