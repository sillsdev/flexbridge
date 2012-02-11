using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress.LogBox;

namespace SIL.LiftBridge.Model
{
	/// <summary>
	/// Provide some services needed by the model.
	/// </summary>
	internal static class LiftProjectServices
	{
		internal static readonly string MappingPathname = Path.Combine(BasePath, "LanguageProject_Repository_Map.xml");
		internal const string MappingsRootTag = "Mappings";
		private const string MappingTag = "Mapping";
		private const string ProjectguidAttrTag = "projectguid";
		internal const string RepositoryidentifierAttrTag = "repositoryidentifier";

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

		internal static void ClearRepoIdentifier(Guid langProjId)
		{
			// If, sor some reason, the map file has the id, but the folder does not exist, then clear the id, so it can be reset.
			if (langProjId == Guid.Empty)
				return; // I don't think it can be, but....

		}

		internal static void StoreIdentifiers(Guid langProjId, string repositoryIdentifier)
		{
			if (langProjId == Guid.Empty)
				return; // Don't bother with older FLEx versions, since they don't know how to give us the LP guid.

			if (repositoryIdentifier != null)
				repositoryIdentifier =  repositoryIdentifier.Trim();
			var doc = GetMappingDoc();
			var root = doc.Element(MappingsRootTag);
			var mapForProject = GetMapForProject(langProjId, root);
			if (mapForProject == null)
			{
				mapForProject = new XElement(MappingTag,
											 new XAttribute(ProjectguidAttrTag, langProjId.ToString()),
											 string.IsNullOrEmpty(repositoryIdentifier)
												? null
												: new XAttribute(RepositoryidentifierAttrTag, repositoryIdentifier));
				root.Add(mapForProject);
			}
			else
			{
				// Have mapForProject
				var repoIdAttr = mapForProject.Attribute(RepositoryidentifierAttrTag);
				if (repoIdAttr == null)
				{
					// repositoryidentifier may be null on first use, so write out what is in project.
					if (!string.IsNullOrEmpty(repositoryIdentifier))
						mapForProject.Add(new XAttribute(RepositoryidentifierAttrTag, repositoryIdentifier));
				}
				else
				{
					// Has repoIdAttr.
					if (string.IsNullOrEmpty(repoIdAttr.Value))
					{
						// Not sure how it could have attr, with no value, but...
						if (string.IsNullOrEmpty(repositoryIdentifier))
							repoIdAttr.Remove();
						else
							repoIdAttr.Value = repositoryIdentifier;
					}
					else
					{
						// But, if repoIdAttr has a value, then it better be the same as the one in project.
						if (!string.IsNullOrEmpty(repositoryIdentifier) && repoIdAttr.Value.Trim() != repositoryIdentifier)
						{
							throw new InvalidOperationException(string.Format("There is a mis-match in the stored repository identifier and the current project identifier. Expected: '{0}' Actual: '{1}'", repoIdAttr.Value.Trim(), repositoryIdentifier));
						}
						//else
						//{
						//    // Do nothing, since they are the same.
						//}
					}
				}
			}
			doc.Save(MappingPathname);
		}

		internal static XElement GetMapForProject(Guid langProjId, XContainer root)
		{
			var childElements = root.Elements(MappingTag);
			var mapForProject = (!childElements.Any())
									? null
									: (from mapping in childElements
									   where mapping.Attribute(ProjectguidAttrTag).Value == langProjId.ToString()
									   select mapping).FirstOrDefault(); // Still will be null, if there is no matching LP id.
			return mapForProject; // May be null, which is fine.
		}

		internal static XDocument GetMappingDoc()
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
			var mapForProject = (from mapping in doc.Root.Elements()
								 where mapping.Attribute(ProjectguidAttrTag).Value.ToLowerInvariant() == languageProjectId.ToString().ToLowerInvariant()
								 select mapping).FirstOrDefault();
			if (mapForProject == null)
				return null;
			var repoIdAttr = mapForProject.Attribute(RepositoryidentifierAttrTag);
			return repoIdAttr == null
				? null
				: (string.IsNullOrEmpty(repoIdAttr.Value)
					? null
					: repoIdAttr.Value);
		}
	}
}