// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Text;
using Chorus.notes;
using Chorus.Utilities;
using LibTriboroughBridgeChorusPlugin;
using LibTriboroughBridgeChorusPlugin.Infrastructure;
using SIL.Progress;
using FLEx_ChorusPlugin.Infrastructure.ActionHandlers;
using LfMergeBridge.LfMergeModel;

namespace LfMergeBridge
{
	/// <summary>
	/// This IBridgeActionTypeHandler implementation handles everything needed for viewing the notes of a Flex repo.
	/// </summary>
	[Export(typeof (IBridgeActionTypeHandler))]
	internal sealed class LanguageForgeGetChorusNotesActionHandler : IBridgeActionTypeHandler
	{
		public const string mainNotesFilenameStub = "Lexicon.fwstub";
		public const string chorusNotesExt = ".ChorusNotes";
		public const string mainNotesFilename = mainNotesFilenameStub + chorusNotesExt;
		public const string genericAuthorName = "Language Forge";
		internal string ProjectName { get; set; }
		internal string ProjectDir { get; set; }

		#region IBridgeActionTypeHandler impl

		/// <summary>
		/// Start doing whatever is needed for the supported type of action.
		/// </summary>
		void IBridgeActionTypeHandler.StartWorking(IProgress progress, Dictionary<string, string> options, ref string somethingForClient)
		{
			var pOption = options["-p"];
			ProjectName = Path.GetFileNameWithoutExtension(pOption);
			ProjectDir = Path.GetDirectoryName(pOption);

			List<LfComment> commentsFromLF;
			if (LfMergeBridge.ExtraInputData.TryGetValue(options, out var extraData) && extraData is GetChorusNotesInput inputData)
			{
				commentsFromLF = inputData.LfComments;
			}
			else
			{
				LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, "No ExtraInputData passed, or it was the wrong type (should be GetChorusNotesInput). Aborting operation.");
				return;
			}

			Dictionary<Guid, LfComment> commentsFromLFByGuid = commentsFromLF.Where(comment => comment.Guid != null).ToDictionary(comment => comment.Guid.Value);
			var knownReplyGuids = new HashSet<Guid>(commentsFromLF.Where(comment => comment.Replies != null).SelectMany(comment => comment.Replies.Where(reply => reply.Guid != null).Select(reply => reply.Guid.Value)));

			var lfComments = new List<LfComment>();
			var lfReplies = new List<Tuple<string, List<LfCommentReply>>>();
			var lfStatusChanges = new List<KeyValuePair<string, Tuple<string, string>>>();
			// TODO: See if we want to suppress progress messages here by using a NullProgress instance instead of the IProgress instance we were given...
			foreach (Annotation ann in GetAllAnnotations(progress, ProjectDir))
			{
				Guid? annGuid = null;
				if (ann.Guid != null && Guid.TryParse(ann.Guid, out var parsedGuid)) { annGuid = parsedGuid; }
				if (annGuid != null && commentsFromLFByGuid.ContainsKey(annGuid.Value))
				{
					var lfComment = commentsFromLFByGuid[annGuid.Value];
					// Known comment; only return new replies
					List<LfCommentReply> repliesNotYetInLf =
						ann
							.Messages
							.Skip(1)  // First message translates to the LF *comment*, while subsequent messages are *replies* in LF
							.Where(m => ! String.IsNullOrWhiteSpace(m.Text))
							.Where(m => m.Guid != null && Guid.TryParse(m.Guid, out var mGuid) && ! knownReplyGuids.Contains(mGuid))
							.Select(ReplyFromChorusMsg)
							.ToList();
					if (repliesNotYetInLf.Count > 0)
					{
						lfReplies.Add(new Tuple<string, List<LfCommentReply>>(ann.Guid, repliesNotYetInLf));
					}
					// But also need to check for status updates
					string chorusStatus = ChorusStatusToLfStatus(ann.Status);
					Guid annStatusGuid = Guid.Empty;
					if (ann.StatusGuid != null) { Guid.TryParse(ann.StatusGuid, out annStatusGuid); }
					if (chorusStatus != lfComment.Status || (lfComment.StatusGuid ?? Guid.Empty) != annStatusGuid)
					{
						lfStatusChanges.Add(new KeyValuePair<string, Tuple<string, string>>(lfComment.Guid?.ToString(), new Tuple<string, string>(chorusStatus, ann.StatusGuid)));
					}
				}
				else
				{
					// New comment: return all replies
					var msg = ann.Messages.FirstOrDefault();
					var lfComment = new LfComment {
						Guid = annGuid,
						// AuthorNameAlternate = msg?.Author ?? string.Empty,  // C# 6 syntax would be simpler if we could count on a C# 6 compiler everywhere
						AuthorNameAlternate = (msg == null) ? string.Empty : msg.Author,
						DateCreated = ann.Date,
						DateModified = ann.Date,
						// Content = msg?.Text ?? string.Empty,  // C# 6 syntax would be simpler if we could count on a C# 6 compiler everywhere
						Content = (msg == null) ? string.Empty : msg.Text,
						Status = ChorusStatusToLfStatus(ann.Status),
						StatusGuid = Guid.TryParse(ann.StatusGuid, out var annStatusGuid) ? annStatusGuid : Guid.Empty,
						Replies = new List<LfCommentReply>(ann.Messages.Skip(1).Where(m => !String.IsNullOrWhiteSpace(m.Text)).Select(ReplyFromChorusMsg)),
						IsDeleted = false,
						Regarding = new LfCommentRegarding {
							TargetGuid = ExtractGuidFromChorusRef(ann.RefStillEscaped),
							// Word and Meaning will be set in LfMerge, but set them to something vaguely sensible here as a fallback
							Word = ann.LabelOfThingAnnotated,
							Meaning = string.Empty
						}
					};
					lfComments.Add(lfComment);
				}
			}

			var response = new GetChorusNotesResponse {
				LfComments = lfComments,
				LfReplies = lfReplies,
				LfStatusChanges = lfStatusChanges,
			};
			// LfMergeBridge.ExtraOutputData.AddOrUpdate(options, response); // Not available in netstandard2.0
			LfMergeBridge.ExtraOutputData.Remove(options); // Available in netstandard2.0, does not throw if value does not exist
			LfMergeBridge.ExtraOutputData.Add(options, response); // Now this is guaranteed safe (would have thrown if previous value had not been removed)
		}

		private LfCommentReply ReplyFromChorusMsg(Message msg)
		{
			var reply = new LfCommentReply();
			reply.Guid = Guid.Parse(msg.Guid); // We already know it's parseable by now
			reply.AuthorNameAlternate = msg.Author;
			if (reply.AuthorInfo == null)
				reply.AuthorInfo = new LfAuthorInfo();
			reply.AuthorInfo.CreatedDate = msg.Date;
			reply.AuthorInfo.ModifiedDate = msg.Date;
			reply.Content = msg.Text;
			reply.IsDeleted = false;
			reply.UniqId = null; // This will be set in LfMerge.
			return reply;
		}

		private string ExtractGuidFromChorusRef(string refStillEscaped)
		{
			var repos = AnnotationRepository.CreateRepositoriesFromFolder(ProjectDir, new NullProgress());
			var repo = repos.First();
			// TODO: Delete the lines above
			return UrlHelper.GetValueFromQueryStringOfRef(refStillEscaped, "guid", string.Empty);
		}

		private string ChorusStatusToLfStatus(string status)
		{
			if (status == Chorus.notes.Annotation.Closed)
			{
				return SerializableLfComment.Resolved;
			}
			else
			{
				return SerializableLfComment.Open; // LfMerge will look at this and see if the Mongo DB contained "Todo".
			}
		}

		IEnumerable<Annotation> GetAllAnnotations(IProgress progress, string projectDir)
		{
			return from repo in AnnotationRepository.CreateRepositoriesFromFolder(projectDir, progress)
				from ann in repo.GetAllAnnotations()
				where ann.Messages.FirstOrDefault() != null  // Some annotations have been known to have NO messages; we skip those
				select ann;
		}

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		ActionType IBridgeActionTypeHandler.SupportedActionType
		{
			get { return ActionType.LanguageForgeGetChorusNotes; }
		}

		#endregion IBridgeActionTypeHandler impl
	}
}
