using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Chorus.FileTypeHanders;
using Chorus.VcsDrivers.Mercurial;
using Chorus.merge;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using Palaso.IO;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.General
{
	/// <summary>
	/// This class deals with files with extension of "annotation".
	/// There is only one of them with a fixed name.
	/// </summary>
	internal sealed class AnnotationFileTypeHandlerStrategy : IFieldWorksFileHandler
	{
		#region Implementation of IFieldWorksFileHandler

		public bool CanValidateFile(string pathToFile)
		{
			return FileUtils.CheckValidPathname(pathToFile, SharedConstants.Annotation) &&
				   Path.GetFileName(pathToFile) == SharedConstants.FLExAnnotationsFilename;
		}

		public string ValidateFile(string pathToFile)
		{
			try
			{
				var doc = XDocument.Load(pathToFile);
				var root = doc.Root;
				if (root.Name.LocalName != SharedConstants.Annotations
					|| root.Element(SharedConstants.Header) != null
					|| !root.Elements(SharedConstants.CmAnnotation).Any())
				{
					return "Not a valid annotations file.";
				}

				return root.Elements(SharedConstants.CmAnnotation)
					.Select(filterElement => CmObjectValidator.ValidateObject(MetadataCache.MdCache, filterElement)).FirstOrDefault(result => result != null);
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
				SharedConstants.CmAnnotation, SharedConstants.GuidStr);
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			FieldWorksCommonFileHandler.Do3WayMerge(mergeOrder, mdc, true);
		}

		public string Extension
		{
			get { return SharedConstants.Annotation; }
		}

		#endregion
	}
}