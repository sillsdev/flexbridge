using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress;
using SIL.LiftBridge.Controller;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Infrastructure;

namespace SIL.LiftBridge.Infrastructure.ActionHandlers
{
	[Export(typeof (IBridgeActionTypeHandler))]
	internal sealed class MoveLiftActionHandler : IBridgeActionTypeHandler
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

		public bool StartWorking(Dictionary<string, string> options)
		{
			_baseLiftDir = Utilities.LiftOffset(Path.GetDirectoryName(options["-p"]));
			var fwLangProjGuid = options["-g"];
			var basePathForOldLiftRepos = Path.Combine(
						Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
						"LiftBridge");
			if (!Directory.Exists(basePathForOldLiftRepos))
			{
				return false;
			}
			if (Directory.GetDirectories(basePathForOldLiftRepos).Length == 0)
			{
				Directory.Delete(basePathForOldLiftRepos, true);
				return false;
			}
			var mappingDocPathname = Path.Combine(basePathForOldLiftRepos, MappingFilename);
			if (!File.Exists(mappingDocPathname))
			{
				return false;
			}

			var mappingDoc = XDocument.Load(mappingDocPathname);
			if (!mappingDoc.Root.HasElements)
			{
				Directory.Delete(basePathForOldLiftRepos, true);
				return false;
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

				foreach (var directory in Directory.GetDirectories(basePathForOldLiftRepos).Where(directory => Directory.Exists(Path.Combine(directory, Utilities.hg))))
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
				return false;

			LiftObtainProjectStrategy.MakeLocalClone(oldLiftFolder, _baseLiftDir);

			var folderToZap = mappingDoc.Root.HasElements || Directory.GetDirectories(basePathForOldLiftRepos).Length > 1
								  ? oldLiftFolder
								  : basePathForOldLiftRepos;
			Directory.Delete(folderToZap, true);
			var otherRepoDir = Directory.GetParent(_baseLiftDir).FullName;
			if (!Directory.Exists(_baseLiftDir) && Directory.GetDirectories(_baseLiftDir).Length == 0)
				Directory.Delete(otherRepoDir);

			return false;
		}

		public void EndWork()
		{
			var liftPathname = Directory.Exists(_baseLiftDir)
				? Directory.GetFiles(_baseLiftDir, "*" + Utilities.LiftExtension).FirstOrDefault()
				: null;
			_connectionHelper.SendLiftPathnameToFlex(liftPathname); // May send null, which is fine.
			_connectionHelper.SignalBridgeWorkComplete(false);
		}

		public ActionType SupportedActionType
		{
			get { return ActionType.MoveLift; }
		}

		public Form MainForm
		{
			get { throw new NotSupportedException("The Move Lift handler has no window"); }
		}

		#endregion IBridgeActionTypeHandler impl

		#region IDisposable impl

		public void Dispose()
		{ /* Do nothing */ }

		#endregion IDisposable impl
	}
}