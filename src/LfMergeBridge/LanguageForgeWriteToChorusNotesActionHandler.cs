// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: license.rtf file)

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;
using System.Text;
using Chorus;
using Chorus.merge;
using Chorus.notes;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Infrastructure;
using LibTriboroughBridgeChorusPlugin;
using LibTriboroughBridgeChorusPlugin.Infrastructure;
using LibFLExBridgeChorusPlugin.Infrastructure;
using Palaso.Progress;

namespace FLEx_ChorusPlugin.Infrastructure.ActionHandlers
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
		public const string zeroGuidStr = "00000000-0000-0000-0000-000000000000";
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

			string inputFilename = options[LfMergeBridge.LfMergeBridgeUtilities.serializedCommentsFromLfMerge];
			string data = File.ReadAllText(inputFilename);

			List<KeyValuePair<string, SerializableLfComment>> commentsFromLF = LfMergeBridge.LfMergeBridgeUtilities.DecodeJsonFile<List<KeyValuePair<string, SerializableLfComment>>>(inputFilename);
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
			var commentIdsThatNeedGuids = new Dictionary<string,string>();
			var replyIdsThatNeedGuids = new Dictionary<string,string>();

			foreach (KeyValuePair<string, SerializableLfComment> kvp in commentsFromLF)
			{
				string lfAnnotationObjectId = kvp.Key;
				SerializableLfComment lfAnnotation = kvp.Value;
				if (lfAnnotation == null || lfAnnotation.IsDeleted)
				{
					if (lfAnnotation == null)
					{
						LfMergeBridge.LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, String.Format("Skipping null annotation with MongoId {0}",
							lfAnnotationObjectId ?? "(null ObjectId)"));
					}
					else
					{
						// We don't have a C# 6 compiler in our build infrastructure, so we have to do this the hard(er) way.
						string guidForLog = lfAnnotation == null ? "(no guid)" :
							lfAnnotation.Guid == null ? "(no guid)" :
							lfAnnotation.Guid.ToString();
						string contentForLog = lfAnnotation == null ? "(no content)" : lfAnnotation.Content ?? "(no content)";
						LfMergeBridge.LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, String.Format("Skipping deleted annotation {0} containing content \"{1}\"",
							guidForLog, contentForLog));
						// The easy way would have been able to skip creating guidForLog and contentForLog; that would have looked like:
						// LfMergeBridge.LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, String.Format("Skipping deleted annotation {0} containing content \"{1}\"",
						// 	lfAnnotation?.Guid ?? "(no guid)", lfAnnotation?.Content ?? "(no content)"));
					}
					continue;
				}
				// TODO: Once we're compiling with C# 6, use the ?. syntax below instead of the more lengthy (== null) syntax that we're currently using.
				// string ownerGuid = lfAnnotation.Regarding?.TargetGuid ?? string.Empty;
				// string ownerShortName = lfAnnotation.Regarding?.Word ?? "???";  // Match FLEx's behavior when short name can't be determined
				string ownerGuid = (lfAnnotation.Regarding == null) ? string.Empty : lfAnnotation.Regarding.TargetGuid;
				string ownerShortName = (lfAnnotation.Regarding == null) ? "???" : lfAnnotation.Regarding.Word;  // Match FLEx's behavior when short name can't be determined

				Annotation chorusAnnotation;
				if (lfAnnotation.Guid != null && chorusAnnotationsByGuid.TryGetValue(lfAnnotation.Guid, out chorusAnnotation) && chorusAnnotation != null)
				{
					SetChorusAnnotationMessagesFromLfReplies(chorusAnnotation, lfAnnotation, lfAnnotationObjectId, replyIdsThatNeedGuids, commentIdsThatNeedGuids);
				}
				else
				{
					Annotation newAnnotation = CreateAnnotation(lfAnnotation.Content, lfAnnotation.Guid, lfAnnotation.AuthorNameAlternate, lfAnnotation.Status, ownerGuid, ownerShortName);
					SetChorusAnnotationMessagesFromLfReplies(newAnnotation, lfAnnotation, lfAnnotationObjectId, replyIdsThatNeedGuids, commentIdsThatNeedGuids);
					primaryRepo.AddAnnotation(newAnnotation);
				}
			}

			LfMergeBridge.LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, String.Format("New comment ID->Guid mappings: {0}",
				String.Join(";", commentIdsThatNeedGuids.Select(kv => String.Format("{0}={1}", kv.Key, kv.Value)))));
			LfMergeBridge.LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, String.Format("New reply ID->Guid mappings: {0}",
				String.Join(";", replyIdsThatNeedGuids.Select(kv => String.Format("{0}={1}", kv.Key, kv.Value)))));

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
			if (lfStatus == SerializableLfComment.Resolved)
			{
				return Chorus.notes.Annotation.Closed;
			}
			else
			{
				return Chorus.notes.Annotation.Open;
			}
		}

		private void SetChorusAnnotationMessagesFromLfReplies(Annotation chorusAnnotation, SerializableLfComment annotationInfo, string annotationObjectId, Dictionary<string,string> uniqIdsThatNeedGuids, Dictionary<string,string> commentIdsThatNeedGuids)
		{
			// Any LF comments that do NOT yet have GUIDs need them set from the corresponding Chorus annotation
			if (String.IsNullOrEmpty(annotationInfo.Guid) && !String.IsNullOrEmpty(annotationObjectId))
			{
				commentIdsThatNeedGuids[annotationObjectId] = chorusAnnotation.Guid;
			}

			if (annotationInfo.Replies == null || annotationInfo.Replies.Count <= 0)
			{
				return;  // Nothing, or nothing else, to do!
			}

			var chorusMsgGuids = new HashSet<string>(chorusAnnotation.Messages.Select(msg => msg.Guid).Where(s => ! string.IsNullOrEmpty(s) && s != zeroGuidStr));
			string statusToSet = LfStatusToChorusStatus(annotationInfo.Status);
			// If we're in this function, the Chorus annotation already contains the text of the LF annotation's comment,
			// so the only thing we need to go through are the replies.
			foreach (SerializableLfCommentReply reply in annotationInfo.Replies)
			{
				if (reply.IsDeleted || chorusMsgGuids.Contains(reply.Guid))
				{
					continue;
				}
				Message newChorusMsg = chorusAnnotation.AddMessage(reply.AuthorNameAlternate, statusToSet, reply.Content);
				if ((string.IsNullOrEmpty(reply.Guid) || reply.Guid == zeroGuidStr) && ! string.IsNullOrEmpty(reply.UniqId))
				{
					uniqIdsThatNeedGuids[reply.UniqId] = newChorusMsg.Guid;
				}
			}
			// Since LF allows changing a comment's status without adding any replies, it's possible we haven't updated the Chorus status yet at this point.
			// But first, check for a special case. Oten, the Chorus annotation's status will be blank, which corresponds to "open" in LfMerge. We don't want
			// to add a blank message just to change the Chorus status from "" (empty string) to "open", so we need to detect this situation specially.
			if (String.IsNullOrEmpty(chorusAnnotation.Status) && statusToSet == Chorus.notes.Annotation.Open)
			{
				// No need for new status here
			}
			else if (statusToSet != chorusAnnotation.Status)
			{
				// LF doesn't keep track of who clicked on the "Resolved" or "Todo" buttons, so we have to be vague about authorship
				chorusAnnotation.SetStatus(genericAuthorName, statusToSet);
			}
		}

		private AnnotationRepository[] GetAnnotationRepositories(IProgress progress)
		{
			AnnotationRepository[] projectRepos = AnnotationRepository.CreateRepositoriesFromFolder(ProjectDir, progress).ToArray();
			// Order of these repos doesn't matter, *except* that we want the "main" repo to be first in the array
			if (projectRepos.Length <= 0)
			{
				var primaryRepo = MakePrimaryAnnotationRepository();
				return new AnnotationRepository[] { primaryRepo };
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
			string fname = Path.Combine(ProjectDir, mainNotesFilenameStub);
			EnsureFileExists(fname, "This is a stub file to provide an attachment point for " + mainNotesFilename);
			return AnnotationRepository.FromFile("id", fname, new NullProgress());
		}

		private void EnsureFileExists(string filename, string contentToCreateFileWith)
		{
			if (!File.Exists(filename))
			{
				using (var writer = new StreamWriter(filename, false, Encoding.UTF8))
				{
					writer.WriteLine(contentToCreateFileWith);
				}
			}
		}

		private Annotation CreateAnnotation(string content, string guidStr, string author, string status, string ownerGuidStr, string ownerShortName)
		{
			Guid guid;
			if (Guid.TryParse(guidStr, out guid))
			{
				if (guid == Guid.Empty)
				{
					guid = Guid.NewGuid();
				}
			}
			else
			{
				guid = Guid.NewGuid();
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
