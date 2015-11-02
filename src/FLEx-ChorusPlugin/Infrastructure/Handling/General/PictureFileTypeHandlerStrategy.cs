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

namespace FLEx_ChorusPlugin.Infrastructure.Handling.General
{
	/// <summary>
	/// This class deals with files with extension of "pictures".
	/// There is only one of them with a fixed name.
	/// </summary>
	internal sealed class PictureFileTypeHandlerStrategy : IFieldWorksFileHandler
	{
		#region Implementation of IFieldWorksFileHandler

		public bool CanValidateFile(string pathToFile)
		{
			return FileUtils.CheckValidPathname(pathToFile, SharedConstants.pictures) &&
				   Path.GetFileName(pathToFile) == SharedConstants.FLExUnownedPicturesFilename;
		}

		public string ValidateFile(string pathToFile)
		{
			try
			{
				var doc = XDocument.Load(pathToFile);
				var root = doc.Root;
				if (root.Name.LocalName != SharedConstants.Pictures
					|| root.Element(SharedConstants.Header) != null)
				{
					return "Not a valid unowned picture file.";
				}

				var mdc = MetadataCache.MdCache;
				return root.Elements(SharedConstants.CmPicture)
					.Select(pictureElement => CmObjectValidator.ValidateObject(mdc, pictureElement)).FirstOrDefault(result => result != null);
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
				null,
				SharedConstants.CmPicture, SharedConstants.GuidStr);
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			FieldWorksCommonFileHandler.Do3WayMerge(mergeOrder, mdc, true);
		}

		public string Extension
		{
			get { return SharedConstants.pictures; }
		}

		#endregion
	}
}