using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FieldWorksBridge.Model;
using FieldWorksBridge.View;

namespace FieldWorksBridgeTests.Mocks
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

		public bool EnableSendReceive { set; internal get; }

		#endregion
	}
}