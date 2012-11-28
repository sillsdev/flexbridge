using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress;

namespace FLExBridge
{
	/// <summary>
	/// Provide some services needed by the model in support of the older Lift Bridge location.
	/// </summary>
	internal static class OldStyleLiftProjectServices
	{
		private const string MappingsRootTag = "Mappings";
		private const string MappingTag = "Mapping";
		private const string ProjectguidAttrTag = "projectguid";
		private const string RepositoryidentifierAttrTag = "repositoryidentifier";

		internal static void MoveRepositoryIfPossible(Guid languageProjectId, string newHome)
		{
			if (Directory.Exists(newHome))
				return; // Somebody already lives there.

			var repoId = GetRepositoryIdentifier(languageProjectId);
			if (repoId == null)
				return; // Nobody home.

			var pathToOldProject = PathToProject(repoId, null);
			if (pathToOldProject == null)
				return; // Nobody home.

			ClearRepoIdentifier(languageProjectId);

			// Move everything in the "pathToOldProject" folder to "newHome".
			Directory.Move(pathToOldProject, newHome);
		}

		private static string MappingPathname
		{
			get
			{
				return Path.Combine(BasePath, "LanguageProject_Repository_Map.xml");
			}
		}

		private static string BasePath
		{
			get
			{
				var basePath = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
					"LiftBridge");

				if (!Directory.Exists(basePath))
					Directory.CreateDirectory(basePath);

				return basePath;
			}
		}

		private static string PathToProject(string repoId, string defaultPath)
		{
			// Check for matching repo.
			foreach (var directory in from directory in Directory.GetDirectories(BasePath)
									  where Directory.Exists(Path.Combine(directory, ".hg"))
									  let repo = new HgRepository(directory, new NullProgress())
									  where repo.Identifier == repoId
									  select directory)
			{
				return directory;
			}
			return defaultPath;
		}

		private static void ClearRepoIdentifier(Guid langProjId)
		{
			// If, for some reason, the map file has the id, but the folder does not exist, then clear the id, so it can be reset.
			if (langProjId == Guid.Empty)
				return; // I don't think it can be, but....

			var doc = GetMappingDoc();
			var element = GetMapForProject(langProjId, doc);
			element.Remove();
			doc.Save(MappingPathname);
		}

		private static XElement GetMapForProject(Guid langProjId, XContainer root)
		{
			var childElements = root.Elements(MappingTag).ToList();
			var mapForProject = (!childElements.Any())
									? null
									: (from mapping in childElements
									   where mapping.Attribute(ProjectguidAttrTag).Value == langProjId.ToString()
									   select mapping).FirstOrDefault(); // Still will be null, if there is no matching LP id.
			return mapForProject; // May be null, which is fine.
		}

		private static XDocument GetMappingDoc()
		{
			XDocument doc;
			if (!File.Exists(MappingPathname))
			{
				doc = new XDocument(
					new XDeclaration("1.0", "utf-8", "yes"),
					new XElement(MappingsRootTag));
				doc.Save(MappingPathname);
			}
			else
			{
				doc = XDocument.Load(MappingPathname);
			}
			return doc;
		}

		internal static string GetRepositoryIdentifier(Guid languageProjectId)
		{
			if (languageProjectId == Guid.Empty)
				return null;

			var doc = GetMappingDoc();
			var mapForProject = (from mapping in doc.Root.Elements(MappingTag)
								 where mapping.Attribute(ProjectguidAttrTag).Value.ToLowerInvariant() == languageProjectId.ToString().ToLowerInvariant()
								 select mapping).FirstOrDefault();
			if (mapForProject == null)
				return null;
			var repoIdAttr = mapForProject.Attribute(RepositoryidentifierAttrTag);
			return repoIdAttr == null
					   ? null
					   : (String.IsNullOrEmpty(repoIdAttr.Value)
							  ? null
							  : repoIdAttr.Value);
		}
	}
}