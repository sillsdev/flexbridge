// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Xml;
using Chorus.sync;
using Chorus.VcsDrivers.Mercurial;
using FLEx_ChorusPlugin.Infrastructure;
using Palaso.Progress;
using Palaso.Xml;
using LibFLExBridgeChorusPlugin.Infrastructure;

namespace RepositoryUtility
{
	/// <summary>
	/// A minimalist implementation of the ISychronizerAdjunct interface that only deals with the branch name.
	///
	/// This implementation can handle getting the bracnh name for Lift and FW repos. It will need to be revised,
	/// if Lift ever goes to a new model, or if the repo util app ever supports types of Chorus repos,
	/// beyond Lift and FW project repos.
	/// </summary>
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
				switch (_repoType)
				{
					case RepositoryUtilityForm.RepoType.FLEx:
						return FieldWorksProjectServices.GetVersionNumber(_dataPathname);
					case RepositoryUtilityForm.RepoType.LIFT:
						return GetLiftBranchName();
					default:
						throw new InvalidOperationException("Repository type not supported.");
				}
			}
		}

		public bool WasUpdated { get { return false; } }

		#endregion ISychronizerAdjunct impl
	}
}
