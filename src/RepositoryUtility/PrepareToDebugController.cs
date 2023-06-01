// Copyright (c) 2010-2023 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System.Linq;

using Chorus;

namespace RepositoryUtility
{
	internal class PrepareToDebugController
	{
		private PrepareToDebugModel Model;
		private ChorusSystem ChorusSystem;
		private IPrepareToDebugMergeView View;
		public PrepareToDebugController(IPrepareToDebugMergeView view, PrepareToDebugModel model, ChorusSystem chorusSystem)
		{
			view.SetController(this);
			view.UpdateParentList(model.MergeParents);
			view.SetMergeCommit(model.MergeCommitToDebug);
			view.UpdateOkEnabledValue(false);
			View = view;
			Model = model;
			ChorusSystem = chorusSystem;
		}

		public void SetMergeCommit(string mergeCommitId)
		{
			Model.MergeCommitToDebug = mergeCommitId;
			var revision = ChorusSystem.Repository.GetRevision(mergeCommitId);
			View.UpdateParentList(revision.Parents.Select(p => p.Hash));
		}

		public void SetParentToInitFrom(string parentCommitId)
		{
			Model.ParentToInitFrom = parentCommitId;
			View.UpdateOkEnabledValue(!string.IsNullOrEmpty(parentCommitId));
		}

		public void StripAndRunMerge()
		{
			if (ChorusSystem.Repository.Execute(90, "strip", Model.MergeCommitToDebug, "--force").ExitCode == 0)
			{
				ChorusSystem.Repository.Update(Model.ParentToInitFrom);
				ChorusSystem.Repository.Merge(ChorusSystem.Repository.PathToRepo,
					Model.MergeParents.First(r => r != Model.ParentToInitFrom));
			}
		}
	}
}