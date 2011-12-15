using System;
using System.Windows.Forms;

namespace FLEx_ChorusPlugin.View
{
	public partial class ProjectView : UserControl, IProjectView
	{
		public ProjectView()
		{
			InitializeComponent();
		}

		#region Implementation of IProjectView

		public IExistingSystemView ExistingSystemView
		{
			get { return _existingSystemView; }
		}

		public IStartupNewView StartupNewView
		{
			get { return _startupNewView; }
		}

		public void ActivateView(IActiveProjectView activeView)
		{
			if (activeView == _existingSystemView)
				ResetViews(_startupNewView, _existingSystemView);
			else if (activeView == _startupNewView)
				ResetViews(_existingSystemView, _startupNewView);
			else
				throw new InvalidOperationException("Unrecognized view class.");
		}

		private void ResetViews(Control disabledView, Control enabledView)
		{
			SuspendLayout();
			disabledView.Visible = false;

			enabledView.Visible = true;
			enabledView.Dock = DockStyle.Fill;
			ResumeLayout(true);
		}

		#endregion
	}
}
