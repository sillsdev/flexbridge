using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TheTurtle.Model;
using TheTurtleTests.Mocks;

namespace TheTurtleTests.Model
{
	/// <summary>
	/// Test the LanguageProjectRepository class.
	/// </summary>
	[TestFixture]
	public class LanguageProjectRepositoryTests
	{
		private DummyFolderSystem _dummyFolderSystem;
		private LanguageProjectRepository _languageProjectRepository;

		[SetUp]
		public void TestSetup()
		{
			_dummyFolderSystem = new DummyFolderSystem();
			_languageProjectRepository = new LanguageProjectRepository(
				new MockedProjectPathLocator(
					new HashSet<string>(_dummyFolderSystem.BaseFolderPaths)));
		}

		[TearDown]
		public void TestTearDown()
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
		public void NullProjectNameHasNullReturnedProject()
		{
			Assert.IsNull(_languageProjectRepository.GetProject(null));
		}

		[Test]
		public void EmptyProjectNameHasNullReturnedProject()
		{
			Assert.IsNull(_languageProjectRepository.GetProject(string.Empty));
		}

		[Test]
		public void NonExistantProjectNameHasNullReturnedProject()
		{
			Assert.IsNull(_languageProjectRepository.GetProject("NobodyHomeProject"));
		}
	}
}
