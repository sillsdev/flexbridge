using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FieldWorksBridge.Infrastructure;
using FieldWorksBridge.Model;
using NUnit.Framework;

namespace FieldWorksBridgeTests
{
	/// <summary>
	/// Test the LanguageProjectRepository class.
	/// </summary>
	[TestFixture]
	public class LanguageProjectRepositoryTests
	{
		private string _baseFolderPath;
		private readonly List<string> _dummyFolderPaths = new List<string>();
		private readonly HashSet<string> _baseFolderPaths = new HashSet<string>();
		private LanguageProjectRepository _languageProjectRepository;

		/// <summary>
		/// Set up some dummy folders.
		/// </summary>
		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_baseFolderPath = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "Projects")).FullName;
			_baseFolderPaths.Add(_baseFolderPath);

			var projectPath = Directory.CreateDirectory(Path.Combine(_baseFolderPath, "ZPI")).FullName;
			Directory.CreateDirectory(Path.Combine(projectPath, ".hg"));
			_dummyFolderPaths.Add(projectPath);
			File.WriteAllText(Path.Combine(projectPath, "ZPI.fwdata"), "");

			_languageProjectRepository = new LanguageProjectRepository(_baseFolderPaths);
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			Directory.Delete(_baseFolderPath, true);
		}

		[Test]
		public void EnsureCorrectNumberOfProjects()
		{
			var projects = _languageProjectRepository.AllLanguageProjects;
			Assert.AreEqual(1, projects.Count());
		}
	}
}
