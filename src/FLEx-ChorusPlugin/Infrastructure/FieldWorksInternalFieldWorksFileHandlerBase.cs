using System.Collections.Generic;
using Chorus.FileTypeHanders;
using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;
using Palaso.IO;
using Palaso.Progress.LogBox;

namespace FLEx_ChorusPlugin.Infrastructure
{
	internal abstract class FieldWorksInternalFieldWorksFileHandlerBase
	{
		internal abstract bool CanValidateFile(string pathToFile);

		internal abstract void Do3WayMerge(MergeOrder mergeOrder);

		internal abstract IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository);

		internal abstract IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository);

		internal abstract string ValidateFile(string pathToFile, IProgress progress);
	}
}