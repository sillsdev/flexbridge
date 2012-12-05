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
			TryCloningLiftRepoIntoFlexland(guidToLiftFolderMap);

			Directory.Delete(basePath, true);
		}

		private static void TryCloningLiftRepoIntoFlexland(Dictionary<string, string> guidToLiftFolderMap)
		{
			if (guidToLiftFolderMap.Count == 0)
				return;
			var flexProjectsFolder = Utilities.ProjectsPath;
			if (!Directory.Exists(flexProjectsFolder))
				return;

			foreach (var flexProjectPath in Directory.GetDirectories(flexProjectsFolder))
			{
				var newHome = Utilities.LiftOffset(flexProjectPath);
				if (Directory.Exists(newHome))
					continue; // Already has a lift repo.

				var fwDataFiles = Directory.GetFiles(flexProjectPath, "*.fwdata");
				//if (fwDataFiles.Length == 0)
				//	fwDataFiles = Directory.GetFiles(flexProjectPath, "*.fwdb"); // Nice thought, but we can't get at the data.
				if (fwDataFiles.Length == 0)
					continue; // Odd case where there is no file at all, or it is a DB4o system.

				// Have to do it the hard way.
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
						string liftFolder;
						if (!guidToLiftFolderMap.TryGetValue(fwLangProjGuid, out liftFolder))
							continue;

						Utilities.MakeLocalClone(liftFolder, newHome);
						break;
					}
				}
			}
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