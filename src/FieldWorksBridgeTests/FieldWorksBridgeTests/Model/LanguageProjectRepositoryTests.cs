using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FieldWorksBridge.Infrastructure;
using FieldWorksBridgeTests.Controller;
using NUnit.Framework;

namespace FieldWorksBridgeTests.Model
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

			// Remote collaboration enabled project
			var projectPath = Directory.CreateDirectory(Path.Combine(_baseFolderPath, "ZPI")).FullName;
			Directory.CreateDirectory(Path.Combine(projectPath, ".hg"));
			_dummyFolderPaths.Add(projectPath);
			File.WriteAllText(Path.Combine(projectPath, "ZPI.fwdata"), "");

			// Remote collaboration not enabled project
			projectPath = Directory.CreateDirectory(Path.Combine(_baseFolderPath, "NotEnabled")).FullName;
			_dummyFolderPaths.Add(projectPath);
			File.WriteAllText(Path.Combine(projectPath, "NotEnabled.fwdata"), "");

			_languageProjectRepository = new LanguageProjectRepository(new MockedLocator(new HashSet<string>(_baseFolderPaths)));
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
			Assert.AreEqual(2, projects.Count());
		}

		[Test]
		public void ZpiProjectIsChorusEnabled()
		{
			Assert.IsTrue(_languageProjectRepository.GetProject("ZPI").IsRemoteCollaborationEnabled);
		}

		[Test]
		public void NotEnabledProjectIsNotChorusEnabled()
		{
			Assert.IsFalse(_languageProjectRepository.GetProject("NotEnabled").IsRemoteCollaborationEnabled);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void NullPathSetThrows()
		{
			new LanguageProjectRepository(null);
		}

		[Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void NoPathsThrows()
		{
			new LanguageProjectRepository(new MockedLocator(new HashSet<string>()));
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void NullProjectNameThrows()
		{
			_languageProjectRepository.GetProject(null);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void EmptyProjectNameThrows()
		{
			_languageProjectRepository.GetProject(string.Empty);
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void NonExistantProjectNameThrows()
		{
			_languageProjectRepository.GetProject("NobodyHomeProject");
		}
	}
}
