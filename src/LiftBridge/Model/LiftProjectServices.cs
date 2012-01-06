using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress.LogBox;
using SIL.LiftBridge.Properties;

namespace SIL.LiftBridge.Model
{
	/// <summary>
	/// Provide some services needed by the model.
	/// </summary>
	internal static class LiftProjectServices
	{
		internal static string BasePath
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

		internal static string PathToProject(LiftProject project)
		{
			var pathToProj = Path.Combine(BasePath, project.LiftProjectName);
			var repoId = project.RepositoryIdentifier; // May be null.

			if (!String.IsNullOrEmpty(repoId))
			{
				// Check for matching repo.
				foreach (var directory in from directory in Directory.GetDirectories(BasePath)
										  where Directory.Exists(Path.Combine(directory, ".hg"))
										  let repo = new HgRepository(directory, new NullProgress())
										  where repo.Identifier == repoId
										  select directory)
				{
					pathToProj = directory;
					break;
				}
			}

			if (!Directory.Exists(pathToProj))
				Directory.CreateDirectory(pathToProj);

			return pathToProj;
		}

		internal static bool ProjectIsShared(LiftProject project)
		{
			return Directory.Exists(PathToMercurialFolder(project));
		}

		internal static string PathToFirstLiftFile(LiftProject project)
		{
			var liftFiles = Directory.GetFiles(PathToProject(project), "*.lift").ToList();
			return liftFiles.Count == 0 ? null : GetMainLiftFile(liftFiles);
		}

		internal static string PathToMercurialFolder(LiftProject project)
		{
			return Path.Combine(PathToProject(project), ".hg");
		}

		private static string GetMainLiftFile(IEnumerable<string> liftFiles)
		{
			return (from file in liftFiles
						where HasOnlyOneDot(file)
						select file).FirstOrDefault();
		}

		private static bool HasOnlyOneDot(string file)
		{
			return file.IndexOf(".") == file.LastIndexOf(".");
		}

		internal static void StoreIdentifiers(Guid langProjId, string repositoryIdentifier)
		{
			var mappingPathname = Path.Combine(BasePath, "LanguageProject_Repository_Map.xml");

			if (!File.Exists(mappingPathname))
				File.WriteAllText(mappingPathname, Resources.kBasicMapFleContents);

			var doc = XDocument.Load(mappingPathname);
			var root = doc.Root;

			var mapForProject = (from mapping in root.Elements()
								 where mapping.Attribute("projectguid").Value.ToLowerInvariant() == langProjId.ToString().ToLowerInvariant()
								 select mapping).FirstOrDefault();
			if (mapForProject == null)
			{
				mapForProject = new XElement("Mapping",
											 new XAttribute("projectguid", langProjId.ToString().ToLowerInvariant()),
											 string.IsNullOrEmpty(repositoryIdentifier)
												? null
												: new XAttribute("repositoryidentifier", repositoryIdentifier.ToLowerInvariant()));
				root.Add(mapForProject);
			}
			else
			{
				var repoIdAttr = mapForProject.Attribute("repositoryidentifier");
				if (repoIdAttr == null)
				{
					// repositoryidentifier may be null on first use, so write out what is in project.
					if (!string.IsNullOrEmpty(repositoryIdentifier))
						mapForProject.Add(new XAttribute("repositoryidentifier", repositoryIdentifier));
				}
				else
				{
					// But, if repositoryidentifier has a value, then it better be the same as the one in project.
					if (repoIdAttr.Value.ToLowerInvariant() != repositoryIdentifier.ToLowerInvariant())
						throw new InvalidOperationException("There is a mis-match in the stored repository identifier and the current project identifier.");
				}
			}
			doc.Save(mappingPathname);
		}

		internal static string GetRepositoryIdentifier(Guid languageProjectId)
		{
			var mappingPathname = Path.Combine(BasePath, "LanguageProject_Repository_Map.xml");

			if (!File.Exists(mappingPathname))
				File.WriteAllText(mappingPathname, Resources.kBasicMapFleContents);

			var doc = XDocument.Load(mappingPathname);
			var mapForProject = (from mapping in doc.Root.Elements()
								 where mapping.Attribute("projectguid").Value.ToLowerInvariant() == languageProjectId.ToString().ToLowerInvariant()
								 select mapping).FirstOrDefault();
			if (mapForProject == null)
				return null;
			var repoIdAttr = mapForProject.Attribute("repositoryidentifier");
			return repoIdAttr == null
				? null
				: (string.IsNullOrEmpty(repoIdAttr.Value)
					? null
					: repoIdAttr.Value);
		}
	}
}