// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using Chorus.FileTypeHandlers;
using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;
using LibFLExBridgeChorusPlugin.Infrastructure;

namespace LibFLExBridgeChorusPlugin.Handling
{
	internal interface IFieldWorksFileHandler
	{
		bool CanValidateFile(string pathToFile);
		/// <summary>
		/// Validate the file, checking its objects against <paramref name="mdc"/>, which describes the file's project.
		/// Implementations must not change <paramref name="mdc"/>, since it may be shared.
		/// </summary>
		string ValidateFile(MetadataCache mdc, string pathToFile);
		IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository);
		IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository);
		void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder);
		string Extension { get; }
	}
}