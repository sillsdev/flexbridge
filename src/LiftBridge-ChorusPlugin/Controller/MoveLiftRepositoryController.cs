using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Chorus;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Controller;
using TriboroughBridge_ChorusPlugin.View;

namespace SIL.LiftBridge.Controller
{
	[Export(typeof(IBridgeController))]
	internal class MoveLiftRepositoryController : IMoveOldLiftRepositorController
	{
		[Import]
		public FLExConnectionHelper ConnectionHelper;
		private string _baseLiftDir;
		private string _fwLangProjGuid;
		private const string MappingTag = "Mapping";
		private const string ProjectguidAttrTag = "projectguid";
		private const string RepositoryidentifierAttrTag = "repositoryidentifier";
		private const string MappingFilename = "LanguageProject_Repository_Map.xml";

		public void Dispose()
		{
		}

		public void InitializeController(MainBridgeForm mainForm, Dictionary<string, string> options, ControllerType controllerType)
		{
			_baseLiftDir = Utilities.LiftOffset(Path.GetDirectoryName(options["-p"]));
			_fwLangProjGuid = options["-g"];
		}

		public ChorusSystem ChorusSystem
		{
			get { return null; }
		}

		public IEnumerable<ControllerType> SupportedControllerActions
		{
			get { return new List<ControllerType> { ControllerType.MoveLift }; }
		}

		public IEnumerable<BridgeModelType> SupportedModels
		{
			get { return new List<BridgeModelType> { BridgeModelType.Lift }; }
		}

		public void MoveRepoIfPresent()
		{
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
				if (mapElement.Attribute(ProjectguidAttrTag).Value.ToLowerInvariant() != _fwLangProjGuid.ToLowerInvariant())
					continue;

				var repoId = mapElement.Attribute(RepositoryidentifierAttrTag).Value;

				foreach (var directory in Directory.GetDirectories(basePathForOldLiftRepos).Where(directory => Directory.Exists(Path.Combine(directory, ".hg"))))
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

			Utilities.MakeLocalClone(oldLiftFolder, _baseLiftDir, new NullProgress());

			var folderToZap = mappingDoc.Root.HasElements || Directory.GetDirectories(basePathForOldLiftRepos).Length > 1
								  ? oldLiftFolder
								  : basePathForOldLiftRepos;
			Directory.Delete(folderToZap, true);
		}

		public void EndWork()
		{
			var liftPathname = Directory.Exists(_baseLiftDir)
				? Directory.GetFiles(_baseLiftDir, "*.lift").FirstOrDefault()
				: null;
			ConnectionHelper.SendLiftPathnameToFlex(liftPathname); // May send null, which is fine.
		}

		private static void RemoveElementAndSaveDoc(XDocument mappingDoc, XElement goner, string mappingDocPathname)
		{
			goner.Remove();
			mappingDoc.Save(mappingDocPathname);
		}
	}
}
