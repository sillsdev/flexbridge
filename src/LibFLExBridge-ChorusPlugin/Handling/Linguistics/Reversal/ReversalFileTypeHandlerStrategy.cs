// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: license.rtf file)

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Chorus.FileTypeHandlers;
using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;
using LibFLExBridgeChorusPlugin.DomainServices;
using LibFLExBridgeChorusPlugin.Infrastructure;
using Palaso.IO;
using System.ComponentModel.Composition;

namespace LibFLExBridgeChorusPlugin.Handling.Linguistics.Reversal
{
	[Export(typeof(IFieldWorksFileHandler))]
	internal sealed class ReversalFileTypeHandlerStrategy : IFieldWorksFileHandler
	{
		private IFieldWorksFileHandler AsIFieldWorksFileHandler
		{
			get { return this; }
		}

		#region Implementation of IFieldWorksFileHandler

		bool IFieldWorksFileHandler.CanValidateFile(string pathToFile)
		{
			return FileUtils.CheckValidPathname(pathToFile, AsIFieldWorksFileHandler.Extension);
		}

		string IFieldWorksFileHandler.ValidateFile(string pathToFile)
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

		IChangePresenter IFieldWorksFileHandler.GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			return FieldWorksChangePresenter.GetCommonChangePresenter(report, repository);
		}

		IEnumerable<IChangeReport> IFieldWorksFileHandler.Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			return Xml2WayDiffService.ReportDifferences(repository, parent, child,
				FlexBridgeConstants.Header,
				FlexBridgeConstants.ReversalIndexEntry, FlexBridgeConstants.GuidStr);
		}

		void IFieldWorksFileHandler.Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			FieldWorksCommonFileHandler.Do3WayMerge(mergeOrder, mdc, true);
		}

		string IFieldWorksFileHandler.Extension
		{
			get { return FlexBridgeConstants.Reversal; }
		}

		#endregion
	}
}