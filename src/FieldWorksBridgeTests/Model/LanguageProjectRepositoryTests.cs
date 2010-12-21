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

		[Test, ExpectedException(typeof(ArgumentNullException)), Category("SkipOnTeamCity")]
		public void NullPathSetThrows()
		{
			new LanguageProjectRepository(null);
		}

		[Test, ExpectedException(typeof(ArgumentOutOfRangeException)), Category("SkipOnTeamCity")]
		public void NoPathsThrows()
		{
			new LanguageProjectRepository(new MockedProjectPathLocator(new HashSet<string>()));
		}

		[Test, ExpectedException(typeof(ArgumentNullException)), Category("SkipOnTeamCity")]
		public void NullProjectNameThrows()
		{
			_languageProjectRepository.GetProject(null);
		}

		[Test, ExpectedException(typeof(ArgumentNullException)), Category("SkipOnTeamCity")]
		public void EmptyProjectNameThrows()
		{
			_languageProjectRepository.GetProject(string.Empty);
		}

		[Test, ExpectedException(typeof(InvalidOperationException)), Category("SkipOnTeamCity")]
		public void NonExistantProjectNameThrows()
		{
			_languageProjectRepository.GetProject("NobodyHomeProject");
		}
	}
}
