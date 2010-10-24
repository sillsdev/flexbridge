using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FieldWorksBridge.Model;
using FieldWorksBridge.Properties;

namespace FieldWorksBridge.Infrastructure
{
	/// <summary>
	/// A repository that gets the available FieldWorks projects on a computer
	/// as represented by the class LanguageProject
	/// (not the FielwdWorks internal language project class).
	/// </summary>
	internal sealed class LanguageProjectRepository
	{
		private readonly HashSet<string> _baseFolderPaths;
		private readonly HashSet<LanguageProject> _projects = new HashSet<LanguageProject>();

		internal LanguageProjectRepository(IProjectPathLocator pathLocator)
		{
			if (pathLocator == null)
				throw new ArgumentNullException("pathLocator");
			if (pathLocator.BaseFolderPaths.Count == 0)
				throw new ArgumentOutOfRangeException("pathLocator", Resources.kNoPathsGiven);

			_baseFolderPaths = pathLocator.BaseFolderPaths;
			foreach (var fwdataFiles in
				_baseFolderPaths.SelectMany(baseFolderPath => Directory.
					GetDirectories(baseFolderPath).
					Select(dir => Directory.
						GetFiles(dir, "*.fwdata")).
						Where(fwdataFiles => fwdataFiles.Length > 0)))
			{
				_projects.Add(new LanguageProject(fwdataFiles[0]));
			}
		}

		/// <summary>
		/// Return all of the FieldWorks projects on a computer.
		/// </summary>
		internal IEnumerable<LanguageProject> AllLanguageProjects
		{
			get { return _projects; }
		}

		internal LanguageProject GetProject(string projectName)
		{
			if (string.IsNullOrEmpty(projectName))
				throw new ArgumentNullException("projectName");

			return (from project in _projects
					where project.Name == projectName
					select project).First();
		}
	}
}