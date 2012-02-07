using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Chorus.FileTypeHanders;
using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using Palaso.IO;
using Palaso.Progress.LogBox;

namespace FLEx_ChorusPlugin.Infrastructure.Handling
{
	internal sealed class FieldWorksCommonFileHandler : IChorusFileTypeHandler
	{
		private readonly IFieldWorksFileHandler _unknownFileTypeHandler;
		private readonly Dictionary<string, IFieldWorksFileHandler> _knownHandlers;

		internal FieldWorksCommonFileHandler()
		{
			_knownHandlers = new Dictionary<string, IFieldWorksFileHandler>();
			var fbAssembly = Assembly.GetExecutingAssembly();
			var fileHandlerTypes = (fbAssembly.GetTypes().Where(typeof(IFieldWorksFileHandler).IsAssignableFrom)).ToList();
			foreach (var fileHandlerType in fileHandlerTypes)
			{
				if (fileHandlerType.IsInterface)
					continue;
				var constInfo = fileHandlerType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
				if (constInfo == null)
					continue; // It does need at least one public or non-public default constructor.
				var instance = (IFieldWorksFileHandler)constInfo.Invoke(BindingFlags.Public | BindingFlags.NonPublic, null, null, null);
				if (fileHandlerType.Name == "UnknownFileTypeHandlerStrategy")
					_unknownFileTypeHandler = instance;
				else
					_knownHandlers.Add(instance.Extension, instance);
			}
		}

		private IFieldWorksFileHandler GetHandlerfromExtension(string extension)
		{
			IFieldWorksFileHandler handler;
			_knownHandlers.TryGetValue(extension, out handler);
			return handler ?? _unknownFileTypeHandler;
		}

		#region Implementation of IChorusFileTypeHandler

		public bool CanDiffFile(string pathToFile)
		{
			return CanValidateFile(pathToFile);
		}

		public bool CanMergeFile(string pathToFile)
		{
			return CanValidateFile(pathToFile);
		}

		public bool CanPresentFile(string pathToFile)
		{
			return CanValidateFile(pathToFile);
		}

		public bool CanValidateFile(string pathToFile)
		{
			if (string.IsNullOrEmpty(pathToFile))
				return false;
			if (!File.Exists(pathToFile))
				return false;
			var extension = Path.GetExtension(pathToFile);
			if (string.IsNullOrEmpty(extension))
				return false;
			if (extension[0] != '.')
				return false;

			var handler = GetHandlerfromExtension(extension.Substring(1));
			return handler.CanValidateFile(pathToFile);
		}

		public void Do3WayMerge(MergeOrder mergeOrder)
		{
			if (mergeOrder == null)
				throw new ArgumentNullException("mergeOrder");

			var extension = FileWriterService.GetExtensionFromPathname(mergeOrder.pathToOurs);
			GetHandlerfromExtension(extension).Do3WayMerge(MetadataCache.MdCache, mergeOrder);
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			if (parent == null)
				throw new ArgumentNullException("parent"); // Parent seems not be optional in Chorus usage.
			if (child == null)
				throw new ArgumentNullException("child");
			if (repository == null)
				throw new ArgumentNullException("repository");

			var extension = FileWriterService.GetExtensionFromPathname(child.FullPath);
			return GetHandlerfromExtension(extension).Find2WayDifferences(parent, child, repository);
		}

		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			if (report == null)
				throw new ArgumentNullException("report");
			if (repository == null)
				throw new ArgumentNullException("repository");

			var extension = FileWriterService.GetExtensionFromPathname(report.PathToFile);
			return GetHandlerfromExtension(extension).GetChangePresenter(report, repository);
		}

		public string ValidateFile(string pathToFile, IProgress progress)
		{
			if (progress == null)
				throw new ArgumentNullException("progress");

			if (string.IsNullOrEmpty(pathToFile))
				return "No file to work with.";
			if (!File.Exists(pathToFile))
				return "File does not exist.";
			var extension = Path.GetExtension(pathToFile);
			if (string.IsNullOrEmpty(extension))
				return "File has no extension.";
			if (extension[0] != '.')
				return "File has no extension.";

			var handler = GetHandlerfromExtension(extension.Substring(1));
			return handler.ValidateFile(pathToFile);
		}

		public IEnumerable<IChangeReport> DescribeInitialContents(FileInRevision fileInRevision, TempFile file)
		{
			// Skip check, since DefaultChangeReport doesn't require it.
			//if (fileInRevision == null)
			//    throw new ArgumentNullException("fileInRevision");

			// Not used here, so don't fret if it is null.
			//if (file == null)
			//    throw new ArgumentNullException("file");

			return new IChangeReport[] { new DefaultChangeReport(fileInRevision, "Added") };
		}

		public IEnumerable<string> GetExtensionsOfKnownTextFileTypes()
		{
			return new List<string>
			{
				// Common
				SharedConstants.ModelVersion,
				SharedConstants.CustomProperties,
				SharedConstants.Style,
				SharedConstants.List,

				// Old style
				SharedConstants.ClassData,

				// Scripture
				SharedConstants.ArchivedDraft,
				SharedConstants.ImportSetting,
				SharedConstants.Srs,
				SharedConstants.Trans,

				// Anthropology
				SharedConstants.Ntbk,

				// Linguistics
				SharedConstants.Reversal,
				SharedConstants.Lexdb, // The lexicon only added one new extension "lexdb", as the lists are already taken care of.
				SharedConstants.TextInCorpus, // Text corpus only added one new extension "textincorpus", as the list is already taken care of.
				SharedConstants.Inventory, // inventory
				SharedConstants.DiscourseExt // discourse
			};
		}

		public uint MaximumFileSize
		{
			get { return int.MaxValue; }
		}

		#endregion
	}
}