using System.Collections.Generic;
using Chorus.FileTypeHanders;
using Chorus.VcsDrivers.Mercurial;
using Chorus.merge;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.Scripture
{
	internal sealed class ScriptureTypeHandlerStrategy// : IFieldWorksFileHandler
	{
		#region Implementation of IFieldWorksFileHandler

		public bool CanValidateFile(string pathToFile)
		{
			throw new System.NotImplementedException();
		}

		public string ValidateFile(string pathToFile)
		{
			throw new System.NotImplementedException();
		}

		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			throw new System.NotImplementedException();
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			throw new System.NotImplementedException();
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			throw new System.NotImplementedException();
		}

		public string Extension
		{
			get { throw new System.NotImplementedException(); }
		}

		#endregion
	}
}