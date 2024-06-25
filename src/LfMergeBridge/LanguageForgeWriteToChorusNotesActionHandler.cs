// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Chorus.notes;
using LibTriboroughBridgeChorusPlugin;
using LibTriboroughBridgeChorusPlugin.Infrastructure;
using SIL.Progress;
using SIL.Providers;
using FLEx_ChorusPlugin.Infrastructure.ActionHandlers;
using LfMergeBridge.LfMergeModel;

namespace LfMergeBridge
{
	/// <summary>
	/// This IBridgeActionTypeHandler implementation handles writing new notes (which probably came from comments on the Language Forge site) into the ChorusNotes system.
	/// </summary>
	[Export(typeof (IBridgeActionTypeHandler))]
	internal sealed class LanguageForgeWriteToChorusNotesActionHandler : IBridgeActionTypeHandler
	{
		public const string mainNotesFilenameStub = "Lexicon.fwstub";
		public const string chorusNotesExt = ".ChorusNotes";
		public const string mainNotesFilename = mainNotesFilenameStub + chorusNotesExt;
		public readonly string zeroGuidStr = Guid.Empty.ToString();
		public const string genericAuthorName = "Language Forge";

		internal string ProjectName { get; set; }
		internal string ProjectDir { get; set; }

		private IProgress Progress { get; set; }

		#region IBridgeActionTypeHandler impl

		/// <summary>
		/// Start doing whatever is needed for the supported type of action.
		/// </summary>
		void IBridgeActionTypeHandler.StartWorking(IProgress progress, Dictionary<string, string> options, ref string somethingForClient)
		{
			var pOption = options["-p"];
			ProjectName = Path.GetFileNameWithoutExtension(pOption);
			ProjectDir = Path.GetDirectoryName(pOption);
			Progress = progress;

			List<KeyValuePair<string, LfComment>> commentsFromLF;
			if (LfMergeBridge.ExtraInputData.TryGetValue(options, out var extraData) && extraData is WriteToChorusNotesInput inputData)
			{
				commentsFromLF = inputData.LfComments;
			}
			else
			{
				LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient,
					"No ExtraInputData passed, or it was the wrong type (should be WriteToChorusNotesInput). Aborting operation.");
				return;
			}

			AnnotationRepository[] annRepos = GetAnnotationRepositories(progress);
			AnnotationRepository primaryRepo = annRepos[0];

			// The LINQ-based approach in the following line does NOT work, because there can be duplicate keys for some reason.
			// Dictionary<string, Annotation> chorusAnnotationsByGuid = annRepos.SelectMany(repo => repo.GetAllAnnotations()).ToDictionary(ann => ann.Guid, ann => ann);
			// Instead we have to do it by hand:
			var chorusAnnotationsByGuid = new Dictionary<string, Annotation>();
			foreach (Annotation ann in annRepos.SelectMany(repo => repo.GetAllAnnotations()))
			{
				chorusAnnotationsByGuid[ann.Guid] = ann;
			}

			// We'll keep track of any comment IDs and reply IDs from LF that didn't have GUIDs when they were handed to us, and make sure
			// that LfMerge can assign the right GUIDs to the right comment and/or reply IDs.
			// Two dictionaries are needed, because comment IDs are Mongo ObjectId instances, whereas reply IDs are strings coming from PHP's so-called "uniqid" function.
			var commentIdsThatNeedGuids = new Dictionary<string,Guid>();
			var replyIdsThatNeedGuids = new Dictionary<string,Guid>();

			if (commentsFromLF == null)
				return;

			foreach (KeyValuePair<string, LfComment> kvp in commentsFromLF)
			{
				string lfAnnotationObjectId = kvp.Key;
				LfComment lfAnnotation = kvp.Value;
				if (lfAnnotation == null || lfAnnotation.IsDeleted)
				{
					if (lfAnnotation == null)
					{
						LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format("Skipping null annotation with MongoId {0}",
							lfAnnotationObjectId ?? "(null ObjectId)"));
					}
					else
					{
						LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, String.Format("Skipping deleted annotation {0} containing content \"{1}\"",
							lfAnnotation?.Guid?.ToString() ?? "(no guid)", lfAnnotation?.Content ?? "(no content)"));
					}
					continue;
				}
				string ownerGuid = lfAnnotation.Regarding?.TargetGuid ?? string.Empty;
				string ownerShortName = lfAnnotation.Regarding?.Word ?? "???";  // Match FLEx's behavior when short name can't be determined

				Annotation chorusAnnotation;
				if (lfAnnotation.Guid != null && chorusAnnotationsByGuid.TryGetValue(lfAnnotation.Guid.ToString(), out chorusAnnotation) && chorusAnnotation != null)
				{
					SetChorusAnnotationMessagesFromLfReplies(chorusAnnotation, lfAnnotation, lfAnnotationObjectId, replyIdsThatNeedGuids, commentIdsThatNeedGuids);
				}
				else
				{
					Annotation newAnnotation = CreateAnnotation(lfAnnotation.Content, lfAnnotation.Guid?.ToString(), lfAnnotation.AuthorNameAlternate, lfAnnotation.Status, ownerGuid, ownerShortName);
					SetChorusAnnotationMessagesFromLfReplies(newAnnotation, lfAnnotation, lfAnnotationObjectId, replyIdsThatNeedGuids, commentIdsThatNeedGuids);
					primaryRepo.AddAnnotation(newAnnotation);
				}
			}

			var response = new WriteToChorusNotesResponse {
				CommentIdsThatNeedGuids = commentIdsThatNeedGuids,
				ReplyIdsThatNeedGuids = replyIdsThatNeedGuids,
			};

			LfMergeBridge.ExtraOutputData.Remove(options); // Ensure Add() doesn't throw if there was already leftover data there
			LfMergeBridge.ExtraOutputData.Add(options, response);

			SaveReposIfNeeded(annRepos, progress);
		}

		private void SaveReposIfNeeded(IEnumerable<AnnotationRepository> repos, IProgress progress)
		{
			foreach (var repo in repos)
			{
				repo.SaveNowIfNeeded(progress);
			}
		}

		private string LfStatusToChorusStatus(string lfStatus)
		{
			return lfStatus == SerializableLfComment.Resolved ? Annotation.Closed : Annotation.Open;
		}

		private void SetChorusAnnotationMessagesFromLfReplies(Annotation chorusAnnotation, LfComment annotationInfo,
			string annotationObjectId, Dictionary<string,Guid> uniqIdsThatNeedGuids, Dictionary<string,Guid> commentIdsThatNeedGuids)
		{
			// Any LF comments that do NOT yet have GUIDs need them set from the corresponding Chorus annotation
			if (annotationInfo.Guid == null && !string.IsNullOrEmpty(annotationObjectId))
			{
				commentIdsThatNeedGuids[annotationObjectId] = Guid.Parse(chorusAnnotation.Guid);
			}

			string statusToSet = LfStatusToChorusStatus(annotationInfo.Status);
			if (annotationInfo.Replies.Count <= 0 && chorusAnnotation.Status == statusToSet)
			{
				return;  // Nothing, or nothing else, to do!
			}

			var chorusMsgGuids = new HashSet<string>(chorusAnnotation.Messages.Select(msg => msg.Guid).Where(s => ! string.IsNullOrEmpty(s) && s != zeroGuidStr));
			// If we're in this function, the Chorus annotation already contains the text of the LF annotation's comment,
			// so the only thing we need to go through are the replies.
			foreach (LfCommentReply reply in annotationInfo.Replies)
			{
				if (reply.IsDeleted || chorusMsgGuids.Contains(reply.Guid?.ToString()))
				{
					continue;
				}
				Message newChorusMsg = chorusAnnotation.AddMessage(reply.AuthorNameAlternate, statusToSet, reply.Content);
				if ((reply.Guid == null || reply.Guid == Guid.Empty) && ! string.IsNullOrEmpty(reply.UniqId))
				{
					uniqIdsThatNeedGuids[reply.UniqId] = Guid.Parse(newChorusMsg.Guid);
				}
			}
			// Since LF allows changing a comment's status without adding any replies, it's possible we haven't updated the Chorus status yet at this point.
			// But first, check for a special case. Often, the Chorus annotation's status will be blank, which corresponds to "open" in LfMerge. We don't want
			// to add a blank message just to change the Chorus status from "" (empty string) to "open", so we need to detect this situation specially.
			if (string.IsNullOrEmpty(chorusAnnotation.Status) && statusToSet == Annotation.Open)
			{
				// No need for new status here
			}
			else if (StatusChangeFromLF(chorusAnnotation, annotationInfo.StatusGuid, statusToSet))
			{
				// LF doesn't keep track of who clicked on the "Resolved" or "Todo" buttons, so we have to be vague about authorship
				chorusAnnotation.SetStatus(genericAuthorName, statusToSet);
				annotationInfo.StatusGuid = string.IsNullOrEmpty(chorusAnnotation.StatusGuid) ? Guid.Empty : Guid.Parse(chorusAnnotation.StatusGuid);
			}
		}

		private static bool StatusChangeFromLF(Annotation chorusAnnotation, Guid? statusGuid, string statusToSet)
		{
			if (statusGuid == null || statusGuid == Guid.Empty)
				return false;

			var guidStr = statusGuid.ToString();
			foreach (var message in chorusAnnotation.Messages)
			{
				if (message.Guid == guidStr)
					return statusToSet != message.Status;
			}
			return true;
		}

		private AnnotationRepository[] GetAnnotationRepositories(IProgress progress)
		{
			AnnotationRepository[] projectRepos = AnnotationRepository.CreateRepositoriesFromFolder(ProjectDir, progress).ToArray();
			// Order of these repos doesn't matter, *except* that we want the "main" repo to be first in the array
			if (projectRepos.Length <= 0)
			{
				var primaryRepo = MakePrimaryAnnotationRepository();
				return new [] { primaryRepo };
			}
			else
			{
				int idx = Array.FindIndex(projectRepos, repo => repo.AnnotationFilePath.Contains(mainNotesFilename));
				if (idx < 0)
				{
					var primaryRepo = MakePrimaryAnnotationRepository();
					var result = new AnnotationRepository[projectRepos.Length + 1];
					result[0] = primaryRepo;
					Array.Copy(projectRepos, 0, result, 1, projectRepos.Length);
					return result;
				}
				else if (idx == 0)
				{
					return projectRepos;
				}
				else
				{
					// Since order of the other repos doesn't matter, just swap the primary into first position
					var primaryRepo = projectRepos[idx];
					projectRepos[idx] = projectRepos[0];
					projectRepos[0] = primaryRepo;
					return projectRepos;
				}
			}
		}

		private AnnotationRepository MakePrimaryAnnotationRepository()
		{
			return AnnotationRepository.FromFile("id",
				Path.Combine(ProjectDir, mainNotesFilenameStub), new NullProgress());
		}

		private Annotation CreateAnnotation(string content, string guidStr, string author, string status, string ownerGuidStr, string ownerShortName)
		{
			Guid guid;
			if (guidStr == null || !Guid.TryParse(guidStr, out guid) || guid == Guid.Empty)
			{
				guid = GuidProvider.Current.NewGuid();
			}
			if (string.IsNullOrEmpty(author))
			{
				author = genericAuthorName;
			}
			var result = new Annotation("question", MakeFlexRefURL(ownerGuidStr, ownerShortName), guid, "ignored");
			result.AddMessage(author, LfStatusToChorusStatus(status), content);
			return result;
		}

		private static string MakeFlexRefURL(string guidStr, string shortName)
		{
			return string.Format("silfw://localhost/link?app=flex&database=current&server=&tool=default&guid={0}&tag=&id={0}&label={1}", guidStr, shortName);
		}

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		ActionType IBridgeActionTypeHandler.SupportedActionType
		{
			get { return ActionType.LanguageForgeWriteToChorusNotes; }
		}

		#endregion IBridgeActionTypeHandler impl
	}
}
