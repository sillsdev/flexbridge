using System;
using System.Collections.Generic;
using System.Linq;
using FieldWorksBridge.Infrastructure;
using FieldWorksBridgeTests.Mocks;
using NUnit.Framework;

namespace FieldWorksBridgeTests.Model
{
	/// <summary>
	/// Test the LanguageProjectRepository class.
	/// </summary>
	[TestFixture]
	public class LanguageProjectRepositoryTests
	{
		private DummyFolderSystem _dummyFolderSystem;
		private LanguageProjectRepository _languageProjectRepository;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_dummyFolderSystem = new DummyFolderSystem();
			_languageProjectRepository = new LanguageProjectRepository(
				new MockedProjectPathLocator(
					new HashSet<string>(_dummyFolderSystem.BaseFolderPaths)));
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			_dummyFolderSystem.Dispose();
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

		[Test]
		public void NullPathSetThrows()
		{
			Assert.Throws<ArgumentNullException>(() => new LanguageProjectRepository(null));
		}

		[Test]
		public void NoPathsThrows()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => new LanguageProjectRepository(new MockedProjectPathLocator(new HashSet<string>())));
		}

		[Test]
		public void NullProjectNameThrows()
		{
			Assert.Throws<ArgumentNullException>(() => _languageProjectRepository.GetProject(null));
		}

		[Test]
		public void EmptyProjectNameThrows()
		{
			Assert.Throws<ArgumentNullException>(() => _languageProjectRepository.GetProject(string.Empty));
		}

		[Test]
		public void NonExistantProjectNameThrows()
		{
			Assert.Throws<InvalidOperationException>(() => _languageProjectRepository.GetProject("NobodyHomeProject"));
		}
	}
}
