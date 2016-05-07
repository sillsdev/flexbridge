// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: license.rtf file)

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress;
using TriboroughBridge_ChorusPlugin;
using L10NSharp;
using LibTriboroughBridgeChorusPlugin;
using LibTriboroughBridgeChorusPlugin.Infrastructure;

namespace SIL.LiftBridge.Infrastructure.ActionHandlers
{
	/// <summary>
	/// This IBridgeActionTypeHandler implementation handles everything needed move a Lift repo
	/// from the older Lift Bridge location to the new Lift repo home within to the main Felx project folder structure.
	/// </summary>
	[Export(typeof (IBridgeActionTypeHandler))]
	internal sealed class MoveLiftActionHandler : IBridgeActionTypeHandler, IBridgeActionTypeHandlerCallEndWork
	{
		[Import]
		private FLExConnectionHelper _connectionHelper;
		private string _baseLiftDir;
		private const string MappingTag = "Mapping";
		private const string ProjectguidAttrTag = "projectguid";
		private const string RepositoryidentifierAttrTag = "repositoryidentifier";
		private const string MappingFilename = "LanguageProject_Repository_Map.xml";

		private static void RemoveElementAndSaveDoc(XDocument mappingDoc, XElement goner, string mappingDocPathname)
		{
			goner.Remove();
			mappingDoc.Save(mappingDocPathname);
		}

		#region IBridgeActionTypeHandler impl

		/// <summary>
		/// Start doing whatever is needed for the supported type of action.
		/// </summary>
		public void StartWorking(IProgress progress, Dictionary<string, string> options, ref string somethingForClient)
		{
			_baseLiftDir = Utilities.LiftOffset(Path.GetDirectoryName(options["-p"]));
			var fwLangProjGuid = options["-g"];
			var basePathForOldLiftRepos = Path.Combine(
						Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
						"LiftBridge");
			if (!Directory.Exists(basePathForOldLiftRepos))
			{
				return;
			}
			if (Directory.GetDirectories(basePathForOldLiftRepos).Length == 0)
			{
				Directory.Delete(basePathForOldLiftRepos, true);
				return;
			}
			var mappingDocPathname = Path.Combine(basePathForOldLiftRepos, MappingFilename);
			if (!File.Exists(mappingDocPathname))
			{
				return;
			}

			var mappingDoc = XDocument.Load(mappingDocPathname);
			if (!mappingDoc.Root.HasElements)
			{
				Directory.Delete(basePathForOldLiftRepos, true);
				return;
			}
			var removedElements = mappingDoc.Root.Elements(MappingTag)
				.Where(mapElement => mapElement.Attribute(ProjectguidAttrTag) == null || mapElement.Attribute(RepositoryidentifierAttrTag) == null).ToList();
			foreach (var goner in removedElements)
			{
				goner.Remove();
			}
			if (removedElements.Count > 0)
			{
				removedElements.Clear();
				mappingDoc.Save(mappingDocPathname);
			}

			string oldLiftFolder = null;
			foreach (var mapElement in mappingDoc.Root.Elements(MappingTag).ToList())
			{
				if (mapElement.Attribute(ProjectguidAttrTag).Value.ToLowerInvariant() != fwLangProjGuid.ToLowerInvariant())
					continue;

				var repoId = mapElement.Attribute(RepositoryidentifierAttrTag).Value;

				foreach (var directory in Directory.GetDirectories(basePathForOldLiftRepos).Where(directory => Directory.Exists(Path.Combine(directory, TriboroughBridge_ChorusPlugin.Utilities.hg))))
				{
					var repo = new HgRepository(directory, new NullProgress());
					if (repo.Identifier != repoId)
						continue;

					oldLiftFolder = directory;
					break;
				}
				if (oldLiftFolder == null)
					continue;

				RemoveElementAndSaveDoc(mappingDoc, mapElement, mappingDocPathname);
				break;
			}
			if (oldLiftFolder == null)
				return;

			var actualCloneResult = new ActualCloneResult();
			ObtainProjectStrategyLift.MakeLocalClone(oldLiftFolder, _baseLiftDir);
			actualCloneResult.ActualCloneFolder = _baseLiftDir;
			actualCloneResult.FinalCloneResult = FinalCloneResult.Cloned;

			// Update to the head of the desired branch, if possible.
			ObtainProjectStrategyLift.UpdateToTheCorrectBranchHeadIfPossible(_baseLiftDir, "LIFT" + options["-liftmodel"], actualCloneResult);
			if(actualCloneResult.FinalCloneResult != FinalCloneResult.Cloned)
			{
				MessageBox.Show(actualCloneResult.Message, LocalizationManager.GetString("LiftBridge_MoveFailed_Title",
																												 "Failed to update LiftBridge project.",
																												 "Title of error message shown when moving a LiftBridge repo to the FlexBridge location fails."));
				// The clone and update did not go smoothly.
				return;
			}
			var folderToZap = mappingDoc.Root.HasElements || Directory.GetDirectories(basePathForOldLiftRepos).Length > 1
								  ? oldLiftFolder
								  : basePathForOldLiftRepos;
			Directory.Delete(folderToZap, true);
			var otherRepoDir = Directory.GetParent(_baseLiftDir).FullName;
			if (!Directory.Exists(_baseLiftDir) && Directory.GetDirectories(_baseLiftDir).Length == 0)
				Directory.Delete(otherRepoDir);
		}

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		public ActionType SupportedActionType
		{
			get { return ActionType.MoveLift; }
		}

		#endregion IBridgeActionTypeHandler impl

		#region IBridgeActionTypeHandlerCallEndWork impl

		/// <summary>
		/// Perform ending work for the supported action.
		/// </summary>
		public void EndWork()
		{
			var liftPathname = Directory.Exists(_baseLiftDir)
				? Directory.GetFiles(_baseLiftDir, "*" + LiftUtilties.LiftExtension).FirstOrDefault()
				: null;
			_connectionHelper.SendLiftPathnameToFlex(liftPathname); // May send null, which is fine.
			_connectionHelper.SignalBridgeWorkComplete(false);
		}

		#endregion IBridgeActionTypeHandlerCallEndWork impl
	}
}