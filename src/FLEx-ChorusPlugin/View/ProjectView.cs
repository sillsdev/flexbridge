using System;
using System.Windows.Forms;

namespace FLEx_ChorusPlugin.View
{
	internal sealed partial class ProjectView : UserControl, IProjectView
	{
		internal ProjectView()
		{
			InitializeComponent();
		}

		private void ResetViews(Control disabledView, Control enabledView)
		{
			SuspendLayout();
			disabledView.Visible = false;

			enabledView.Visible = true;
			enabledView.Dock = DockStyle.Fill;
			ResumeLayout(true);
		}

		#region Implementation of IProjectView

		IExistingSystemView IProjectView.ExistingSystemView
		{
			get { return _existingSystemView; }
		}

		IStartupNewView IProjectView.StartupNewView
		{
			get { return _startupNewView; }
		}

		void IProjectView.ActivateView(IActiveProjectView activeView)
		{
			if (activeView == _existingSystemView)
				ResetViews(_startupNewView, _existingSystemView);
			else if (activeView == _startupNewView)
				ResetViews(_existingSystemView, _startupNewView);
			else
				throw new InvalidOperationException("Unrecognized view class.");
		}

		#endregion
	}
}
