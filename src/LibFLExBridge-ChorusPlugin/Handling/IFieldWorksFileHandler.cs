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
		string ValidateFile(string pathToFile);
		IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository);
		IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository);
		void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder);
		string Extension { get; }
	}
}