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
using SIL.IO;
using System.ComponentModel.Composition;

namespace LibFLExBridgeChorusPlugin.Handling.General
{
	/// <summary>
	/// This class deals with files with extension of "annotation".
	/// There is only one of them with a fixed name.
	/// </summary>
	[Export(typeof(IFieldWorksFileHandler))]
	internal sealed class AnnotationFileTypeHandlerStrategy : IFieldWorksFileHandler
	{
		#region Implementation of IFieldWorksFileHandler

		bool IFieldWorksFileHandler.CanValidateFile(string pathToFile)
		{
			return FileUtils.CheckValidPathname(pathToFile, FlexBridgeConstants.Annotation) &&
				   Path.GetFileName(pathToFile) == FlexBridgeConstants.FLExAnnotationsFilename;
		}

		string IFieldWorksFileHandler.ValidateFile(string pathToFile)
		{
			try
			{
				var doc = XDocument.Load(pathToFile);
				var root = doc.Root;
				if (root.Name.LocalName != FlexBridgeConstants.Annotations
					|| root.Element(FlexBridgeConstants.Header) != null
					|| !root.Elements(FlexBridgeConstants.CmAnnotation).Any())
				{
					return "Not a valid annotations file.";
				}

				return root.Elements(FlexBridgeConstants.CmAnnotation)
					.Select(filterElement => CmObjectValidator.ValidateObject(MetadataCache.MdCache, filterElement)).FirstOrDefault(result => result != null);
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
				null,
				FlexBridgeConstants.CmAnnotation, FlexBridgeConstants.GuidStr);
		}

		void IFieldWorksFileHandler.Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			FieldWorksCommonFileHandler.Do3WayMerge(mergeOrder, mdc, true);
		}

		string IFieldWorksFileHandler.Extension
		{
			get { return FlexBridgeConstants.Annotation; }
		}

		#endregion
	}
}
