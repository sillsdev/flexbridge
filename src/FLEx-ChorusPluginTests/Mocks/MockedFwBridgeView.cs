using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FLEx_ChorusPlugin.Model;
using FLEx_ChorusPlugin.View;

namespace FLEx_ChorusPluginTests.Mocks
{
	internal class MockedFwBridgeView : UserControl, IFwBridgeView
	{
		private readonly IProjectView _mockedProjectView = new MockedProjectView();

		internal void RaiseProjectSelected(LanguageProject selectedProject)
		{
			ProjectSelected(this, new ProjectEventArgs(selectedProject));
		}

		internal void RaiseSynchronizeProject()
		{
			SynchronizeProject(this, new EventArgs());
		}

		#region Implementation of IFwBridgeView

		public event ProjectSelectedEventHandler ProjectSelected;
		public event SynchronizeProjectEventHandler SynchronizeProject;

		public IEnumerable<LanguageProject> Projects { set; internal get; }

		public IProjectView ProjectView
		{
			get { return _mockedProjectView; }
		}

		public void EnableSendReceiveControls(bool makeWarningsVisible)
		{
			WarningsAreVisible = makeWarningsVisible;
		}

		internal bool WarningsAreVisible { set; get; }

		#endregion
	}
}