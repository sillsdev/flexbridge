using System.Collections.Generic;
using System.Windows.Forms;
using TheTurtle.Model;
using TheTurtle.View;

namespace TheTurtleTests.Mocks
{
	internal class MockedTurtleView : UserControl, ITurtleView
	{
		private readonly IProjectView _mockedProjectView = new MockedProjectView();

		internal void RaiseProjectSelected(LanguageProject selectedProject)
		{
			if (ProjectSelected != null)
				ProjectSelected(this, new ProjectEventArgs(selectedProject));
		}

		#region Implementation of ITurtleView

		public event ProjectSelectedEventHandler ProjectSelected;

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