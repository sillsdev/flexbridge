using System;
using System.Collections.Generic;
using System.IO;
using Chorus.FileTypeHanders;
using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;
using FLEx_ChorusPlugin.Contexts.Anthropology;
using FLEx_ChorusPlugin.Contexts.General;
using FLEx_ChorusPlugin.Contexts.Linguistics.Reversals;
using FLEx_ChorusPlugin.Infrastructure.CustomProperties;
using FLEx_ChorusPlugin.Infrastructure.ModelVersion;
using Palaso.IO;
using Palaso.Progress.LogBox;

namespace FLEx_ChorusPlugin.Infrastructure
{
	internal sealed class FieldWorksCommonFileHandler : IChorusFileTypeHandler
	{
		private readonly Dictionary<string, FieldWorksInternalFieldWorksFileHandlerBase> _handlers = new Dictionary<string, FieldWorksInternalFieldWorksFileHandlerBase>
			{
				{"ntbk", new FieldWorksAnthropologyTypeHandler()},
				{"ClassData", new FieldWorksFileHandler()},
				{"reversal", new FieldWorksReversalTypeHandler()},
				{"CustomProperties", new FieldWorksCustomPropertyFileHandler()},
				{"ModelVersion", new FieldWorksModelVersionFileHandler()}
			};

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

			var handler = GetHandlerFromPathname(pathToFile);
			return handler != null && handler.CanValidateFile(pathToFile);
		}

		public void Do3WayMerge(MergeOrder mergeOrder)
		{
			if (mergeOrder == null) throw new ArgumentNullException("mergeOrder");

			GetHandlerFromPathname(mergeOrder.pathToOurs).Do3WayMerge(mergeOrder);
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			return GetHandlerFromPathname(child.FullPath).Find2WayDifferences(parent, child, repository);
		}

		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			return GetHandlerFromPathname(report.PathToFile).GetChangePresenter(report, repository);
		}

		public string ValidateFile(string pathToFile, IProgress progress)
		{
			if (string.IsNullOrEmpty(pathToFile))
				return "No Pathname";
			if (!File.Exists(pathToFile))
				return "File does not exist.";

			var handler = GetHandlerFromPathname(pathToFile);
			return handler == null ? "No handler for file" : handler.ValidateFile(pathToFile, progress);
		}

		public IEnumerable<IChangeReport> DescribeInitialContents(FileInRevision fileInRevision, TempFile file)
		{
			return new IChangeReport[] { new DefaultChangeReport(fileInRevision, "Added") };
		}

		public IEnumerable<string> GetExtensionsOfKnownTextFileTypes()
		{
			return _handlers.Keys;
		}

		public uint MaximumFileSize
		{
			get { return int.MaxValue; }
		}

		#endregion

		private FieldWorksInternalFieldWorksFileHandlerBase GetHandlerFromPathname(string pathname)
		{
			var extension = Path.GetExtension(pathname).Substring(1);
			FieldWorksInternalFieldWorksFileHandlerBase result;
			_handlers.TryGetValue(extension, out result);
			return result;
		}
	}
}