using System.Windows.Forms;

namespace FLEx_ChorusPlugin.View
{
	internal sealed partial class ProjectView : UserControl, IProjectView
	{
		internal ProjectView()
		{
			InitializeComponent();
		}

		#region Implementation of IProjectView

		IExistingSystemView IProjectView.ExistingSystemView
		{
			get { return _existingSystemView; }
		}

		void IProjectView.ActivateView(IActiveProjectView activeView)
		{
			SuspendLayout();

			_existingSystemView.Visible = true;

			ResumeLayout(true);
		}

		#endregion
	}
}
