// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using Chorus.FileTypeHandlers;
using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;
using LibFLExBridgeChorusPlugin.Infrastructure;
using System.ComponentModel.Composition;

namespace LibFLExBridgeChorusPlugin.Handling
{
	/// <summary>
	/// File handler strategy for unknown file types
	/// </summary>
	/// <remarks>In contrast to the other file type handlers we export the concrete type
	/// instead of the interface. The reason is that in FieldWorksCommonFileHandler we use this
	/// class as a fallback which requires that we can explicitly access the type.</remarks>
	[Export]
	internal sealed class UnknownFileTypeHandlerStrategy : IFieldWorksFileHandler
	{
		#region Implementation of IFieldWorksFileHandler

		bool IFieldWorksFileHandler.CanValidateFile(string pathToFile)
		{
			return false;
		}

		string IFieldWorksFileHandler.ValidateFile(string pathToFile)
		{
			throw new NotSupportedException("'ValidateFile' method is not supported for unknown file types.");
		}

		IChangePresenter IFieldWorksFileHandler.GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			throw new NotSupportedException("'GetChangePresenter' method is not supported for unknown file types.");
		}

		IEnumerable<IChangeReport> IFieldWorksFileHandler.Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			throw new NotSupportedException("'Find2WayDifferences' method is not supported for unknown file types.");
		}

		void IFieldWorksFileHandler.Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			throw new NotSupportedException("'Do3WayMerge' method is not supported for unknown file types.");
		}

		string IFieldWorksFileHandler.Extension
		{
			get { throw new NotSupportedException("'Extension' property is not supported for unknown file types."); }
		}

		#endregion
	}
}