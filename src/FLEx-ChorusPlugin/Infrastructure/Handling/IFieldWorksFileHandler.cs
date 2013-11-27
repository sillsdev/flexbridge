// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Chorus.FileTypeHanders;
using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;

namespace FLEx_ChorusPlugin.Infrastructure.Handling
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