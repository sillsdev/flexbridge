// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Chorus.FileTypeHandlers;
using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;
using LibFLExBridgeChorusPlugin.Infrastructure;
using System.ComponentModel.Composition;

namespace LibFLExBridgeChorusPlugin.Handling
{
	[Export]
	internal sealed class UnknownFileTypeHandlerStrategy : IFieldWorksFileHandler
	{
		#region Implementation of IFieldWorksFileHandler

		public bool CanValidateFile(string pathToFile)
		{
			return false;
		}

		public string ValidateFile(string pathToFile)
		{
			throw new NotSupportedException("'ValidateFile' method is not supported for unknown file types.");
		}

		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			throw new NotSupportedException("'GetChangePresenter' method is not supported for unknown file types.");
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			throw new NotSupportedException("'Find2WayDifferences' method is not supported for unknown file types.");
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			throw new NotSupportedException("'Do3WayMerge' method is not supported for unknown file types.");
		}

		public string Extension
		{
			get { throw new NotSupportedException("'Extension' property is not supported for unknown file types."); }
		}

		#endregion
	}
}