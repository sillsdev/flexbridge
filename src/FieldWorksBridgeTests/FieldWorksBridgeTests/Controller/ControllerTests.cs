using System.Collections.Generic;
using FieldWorksBridge.Controller;
using FieldWorksBridge.Infrastructure;
using NUnit.Framework;

namespace FieldWorksBridgeTests.Controller
{
	/// <summary>
	/// Test the controller with a mocked implementation of IFwBridgeView.
	/// </summary>
	[TestFixture]
	public class ControllerTests
	{
		private MockedView _view;
		private MockedLocator _locator;
		private FwBridgeController _controller;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_view = new MockedView();
			_locator = new MockedLocator(new HashSet<string> {FieldWorksProjectServices.ProjectsPath});
			_controller = new FwBridgeController(_view, _locator);
		}

		[TestFixtureTearDown]
		public void FixtureTeardown()
		{
			_controller.Dispose();
		}

		[Test]
		public void EnsureMainhFormExists()
		{
			Assert.IsNotNull(_controller.MainForm);
		}
	}
}