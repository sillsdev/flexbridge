using System.Windows.Forms;

namespace TheTurtle.View
{
	internal sealed partial class ProjectView : UserControl, IProjectView
	{
		internal ProjectView()
		{
			InitializeComponent();
		}

		#region Implementation of IProjectView

		public IExistingSystemView ExistingSystemView
		{
			get { return _existingSystemView; }
		}

		#endregion
	}
}
