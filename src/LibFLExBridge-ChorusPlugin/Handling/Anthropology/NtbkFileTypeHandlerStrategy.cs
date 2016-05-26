// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: license.rtf file)

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Chorus.FileTypeHandlers;
using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;
using Palaso.IO;
using LibFLExBridgeChorusPlugin.DomainServices;
using LibFLExBridgeChorusPlugin.Infrastructure;
using System.ComponentModel.Composition;

namespace LibFLExBridgeChorusPlugin.Handling.Anthropology
{
	[Export(typeof(IFieldWorksFileHandler))]
	internal sealed class NtbkFileTypeHandlerStrategy : IFieldWorksFileHandler
	{
		private IFieldWorksFileHandler AsIFieldWorksFileHandler
		{
			get { return this; }
		}

		#region Implementation of IFieldWorksFileHandler

		bool IFieldWorksFileHandler.CanValidateFile(string pathToFile)
		{
			return FileUtils.CheckValidPathname(pathToFile, AsIFieldWorksFileHandler.Extension) &&
				   Path.GetFileName(pathToFile) == FlexBridgeConstants.DataNotebookFilename;
		}

		string IFieldWorksFileHandler.ValidateFile(string pathToFile)
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

		IChangePresenter IFieldWorksFileHandler.GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			return FieldWorksChangePresenter.GetCommonChangePresenter(report, repository);
		}

		IEnumerable<IChangeReport> IFieldWorksFileHandler.Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			return Xml2WayDiffService.ReportDifferences(repository, parent, child,
				FlexBridgeConstants.Header,
				FlexBridgeConstants.RnGenericRec, FlexBridgeConstants.GuidStr);
		}

		void IFieldWorksFileHandler.Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			FieldWorksCommonFileHandler.Do3WayMerge(mergeOrder, mdc, true);
		}

		string IFieldWorksFileHandler.Extension
		{
			get { return FlexBridgeConstants.Ntbk; }
		}

		#endregion
	}
}