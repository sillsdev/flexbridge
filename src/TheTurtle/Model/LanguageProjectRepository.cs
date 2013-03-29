using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using FLEx_ChorusPlugin.Properties;
using TriboroughBridge_ChorusPlugin;

namespace TheTurtle.Model
{
	/// <summary>
	/// A repository that gets the available FieldWorks projects on a computer
	/// as represented by the class LanguageProject
	/// (not the FielwdWorks internal language project class).
	/// </summary>
	[Export(typeof(LanguageProjectRepository))]
	internal sealed class LanguageProjectRepository
	{
		private readonly HashSet<string> _baseFolderPaths;
		private readonly HashSet<LanguageProject> _projects = new HashSet<LanguageProject>();

		[ImportingConstructor]
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
						GetFiles(dir, "*" + Utilities.FwXmlExtension)).
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