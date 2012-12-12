using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress;
using Palaso.Xml;

namespace TriboroughBridge_ChorusPlugin
{
	/// <summary>
	/// Provide some services needed by the model in support of the older Lift Bridge location.
	/// </summary>
	internal static class OldStyleLiftProjectServices
	{
		private const string MappingTag = "Mapping";
		private const string ProjectguidAttrTag = "projectguid";
		private const string RepositoryidentifierAttrTag = "repositoryidentifier";

		internal static void MoveRepositories()
		{
			var basePath = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
					"LiftBridge");
			if (!Directory.Exists(basePath))
				return;

			var guidToLiftFolderMap = CacheLiftProjectInfo(basePath);
			if (guidToLiftFolderMap.Count > 0)
				TryCloningLiftRepoIntoFlexland(guidToLiftFolderMap);

			Directory.Delete(basePath, true);
		}

		private static void TryCloningLiftRepoIntoFlexland(Dictionary<string, string> guidToLiftFolderMap)
		{
			var flexProjectsFolder = Utilities.ProjectsPath;
			if (!Directory.Exists(flexProjectsFolder))
				return;

			foreach (var flexProjectPath in Directory.GetDirectories(flexProjectsFolder))
			{
				if (guidToLiftFolderMap.Count == 0)
					break;

				var newHome = Utilities.LiftOffset(flexProjectPath);
				var safeLocation = MoveTempFileToSafetyIfNeeded(newHome);
				if (Directory.Exists(Path.Combine(newHome, ".hg")))
					continue; // Already has a lift repo.

				var fwDataFiles = Directory.GetFiles(flexProjectPath, "*.fwdata");
				//if (fwDataFiles.Length == 0)
				//	fwDataFiles = Directory.GetFiles(flexProjectPath, "*.fwdb"); // Nice thought, but we can't get at the data.
				if (fwDataFiles.Length == 0)
					continue; // Odd case where there is no file at all, or it is a DB4o system.

				// Have to do it the hard way.
				string liftFolder = null;
				using (var fastSplitter = new FastXmlElementSplitter(fwDataFiles[0]))
				{
					var searchedForAttrs = new HashSet<string> { "guid", "class" };
					bool foundOptionalFirstElement;
					foreach (var record in fastSplitter.GetSecondLevelElementStrings("AdditionalFields", "rt", out foundOptionalFirstElement))
					{
						if (foundOptionalFirstElement)
						{
							foundOptionalFirstElement = false;
							continue;
						}
						var classAndGuidData = XmlUtils.GetAttributes(record, searchedForAttrs);
						if (classAndGuidData["class"] != "LangProject")
							continue;

						var fwLangProjGuid = classAndGuidData["guid"].ToLowerInvariant();
						if (guidToLiftFolderMap.TryGetValue(fwLangProjGuid, out liftFolder))
						{
							guidToLiftFolderMap.Remove(fwLangProjGuid);
							break;
						}
					}
				}

				if (liftFolder == null)
					continue;

				Utilities.MakeLocalClone(liftFolder, newHome);
				MoveTempFileBackIfNeeded(safeLocation, newHome);
			}
		}

		private static string MoveTempFileToSafetyIfNeeded(string liftBaseFolder)
		{
			if (Directory.Exists(Path.Combine(liftBaseFolder, ".hg")))
				return null;

			string newTempPathname = null;
			var tempFile = Directory.GetFiles(liftBaseFolder, "*.tmp").FirstOrDefault();
			if (tempFile != null)
			{
				newTempPathname = Path.Combine(Path.GetTempPath(), Path.GetFileName(tempFile));
				File.Copy(tempFile, newTempPathname, true);
				Directory.Delete(Directory.GetParent(liftBaseFolder).FullName, true);
			}

			return newTempPathname;
		}

		private static void MoveTempFileBackIfNeeded(string tempPathname, string newHome)
		{
			if (tempPathname == null)
				return;

			var newPathname = Path.Combine(newHome, Path.GetFileName(tempPathname));
			File.Copy(tempPathname, newPathname, true);
			File.Delete(tempPathname);
		}

		private static Dictionary<string, string> CacheLiftProjectInfo(string basePath)
		{
			var guidToLiftFolderMap = new Dictionary<string, string>();

			var mappingDocPathname = Path.Combine(basePath, "LanguageProject_Repository_Map.xml");
			if (!File.Exists(mappingDocPathname))
				return guidToLiftFolderMap;

			var mappingDoc = XDocument.Load(mappingDocPathname);
			if (!mappingDoc.Root.HasElements)
				return guidToLiftFolderMap;

			foreach (var mapElement in mappingDoc.Root.Elements(MappingTag))
			{
				var repoId = mapElement.Attribute(RepositoryidentifierAttrTag).Value;
				var actualPath = (from directory in Directory.GetDirectories(basePath)
								  where Directory.Exists(Path.Combine(directory, ".hg"))
								  let repo = new HgRepository(directory, new NullProgress())
								  where repo.Identifier == repoId
								  select directory).FirstOrDefault();
				if (actualPath == null)
					continue;
				guidToLiftFolderMap.Add(mapElement.Attribute(ProjectguidAttrTag).Value.ToLowerInvariant(), actualPath);
			}
			return guidToLiftFolderMap;
		}
	}
}