// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Xml;
using Chorus.sync;
using Chorus.VcsDrivers.Mercurial;
using FLEx_ChorusPlugin.Infrastructure;
using Palaso.Progress;
using Palaso.Xml;

namespace RepositoryUtility
{
	internal sealed class RepositoryUtilitySychronizerAdjunct : ISychronizerAdjunct
	{
		private readonly string _dataPathname;
		private readonly RepositoryUtilityForm.RepoType _repoType = RepositoryUtilityForm.RepoType.None;

		internal RepositoryUtilitySychronizerAdjunct(string dataPathname, RepositoryUtilityForm.RepoType repoType)
		{
			_dataPathname = dataPathname;
			_repoType = repoType;
		}

		private string GetLiftBranchName()
		{
			const string LIFT = @"LIFT";
			using (var reader = XmlReader.Create(_dataPathname, CanonicalXmlSettings.CreateXmlReaderSettings()))
			{
				reader.MoveToContent();
				reader.MoveToAttribute(@"version");
				var liftVersionString = reader.Value;
				return (liftVersionString == @"0.13") ? @"default" : LIFT + reader.Value;
			}
		}

		#region ISychronizerAdjunct impl

		public void PrepareForInitialCommit(IProgress progress)
		{
			// Do nothing.
		}

		public void SimpleUpdate(IProgress progress, bool isRollback)
		{
			// Do nothing.
		}

		public void PrepareForPostMergeCommit(IProgress progress)
		{
			// Do nothing.
		}

		public void CheckRepositoryBranches(IEnumerable<Revision> branches, IProgress progress)
		{
			// Do nothing.
		}

		public string BranchName {
			get
			{
				return (_repoType == RepositoryUtilityForm.RepoType.LIFT) ? GetLiftBranchName() : FieldWorksProjectServices.GetVersionNumber(_dataPathname);
			}
		}

		public bool WasUpdated { get { return false; } }

		#endregion ISychronizerAdjunct impl
	}
}
