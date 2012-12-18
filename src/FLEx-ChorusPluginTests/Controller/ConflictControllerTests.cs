using System.IO;
using System.Windows.Forms;
using FLEx_ChorusPlugin.Controller;
using FLEx_ChorusPluginTests.Mocks;
using NUnit.Framework;

namespace FLEx_ChorusPluginTests.Controller
{
#if notyet
	/// <summary>
	/// Test the conflict controller with mocked implementations of the various view interfaces.
	/// </summary>
	[TestFixture]
	public class ConflictControllerTests
	{
		private FlexBridgeConflictController _realController; // Well, 'real' minus references to Forms mostly.
		private DummyFolderSystem _dummyFolderSystem;
		private Form _mockedConflictView;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_dummyFolderSystem = new DummyFolderSystem();

			_mockedConflictView = new MockedConflictView();

			_realController = new TestConflictController(_mockedConflictView);
			_realController.InitController("Louis XIV", GetDummyFilePath());
		}

		private string GetDummyFilePath()
		{
			var path = Path.Combine(_dummyFolderSystem.BaseFolderPath, "ZPI");
			return Path.Combine(path, "ZPI" + Utilities.FwXmlExtension);
		}

		[TestFixtureTearDown]
		public void FixtureTeardown()
		{
			_mockedConflictView.Dispose();
			_mockedConflictView = null;
			_dummyFolderSystem.Dispose();
			_dummyFolderSystem = null;
			_realController.Dispose();
			_realController = null;
		}

		[Test]
		public void EnsureLanguageProjectExists()
		{
			Assert.IsNotNull(_realController.CurrentProject);
		}

		[Test]
		public void EnsureChorusSystemExists()
		{
			Assert.IsNotNull(_realController.ChorusSystem);
		}
	}
#endif
}