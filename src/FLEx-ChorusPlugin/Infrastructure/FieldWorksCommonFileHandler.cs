using System.Collections.Generic;
using Chorus.FileTypeHanders;
using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;
using Palaso.IO;
using Palaso.Progress.LogBox;

namespace FLEx_ChorusPlugin.Infrastructure
{
	internal sealed class FieldWorksCommonFileHandler : IChorusFileTypeHandler
	{
		private readonly HashSet<string> _extensions = new HashSet<string>
			{
				FieldWorksFileHandlerServices.ClassData,
				FieldWorksFileHandlerServices.Ntbk,
				FieldWorksFileHandlerServices.Reversal,
				FieldWorksFileHandlerServices.CustomProperties,
				FieldWorksFileHandlerServices.ModelVersion
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
			return FieldWorksFileHandlerServices.CanValidateFile(pathToFile);
		}

		public void Do3WayMerge(MergeOrder mergeOrder)
		{
			FieldWorksFileHandlerServices.Do3WayMerge(mergeOrder);
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			return FieldWorksFileHandlerServices.Find2WayDifferences(parent, child, repository);
		}

		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			return FieldWorksFileHandlerServices.GetChangePresenter(report, repository);
		}

		public string ValidateFile(string pathToFile, IProgress progress)
		{
			return FieldWorksFileHandlerServices.ValidateFile(pathToFile);
		}

		public IEnumerable<IChangeReport> DescribeInitialContents(FileInRevision fileInRevision, TempFile file)
		{
			return new IChangeReport[] { new DefaultChangeReport(fileInRevision, "Added") };
		}

		public IEnumerable<string> GetExtensionsOfKnownTextFileTypes()
		{
			return _extensions;
		}

		public uint MaximumFileSize
		{
			get { return int.MaxValue; }
		}

		#endregion
	}
}