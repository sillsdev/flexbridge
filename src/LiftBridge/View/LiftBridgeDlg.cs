using System.Windows.Forms;

namespace SIL.LiftBridge.View
{
	internal sealed partial class LiftBridgeDlg : ILiftBridgeView
	{
		internal LiftBridgeDlg()
		{
			InitializeComponent();
		}

		#region Implementation of ILiftBridgeView

		/// <summary>
		/// Show the view with the given parent form and the given title.
		/// </summary>
		public void Show(Form parent, string title)
		{
			Text = title;
			ShowDialog(parent);
		}

		/// <summary>
		/// Make <paramref name="activeView"/> the active control.
		/// </summary>
		/// <remarks>
		/// <paramref name="activeView"/> must be a Control (UserControl),
		/// since the expectation is that the ILiftBridgeView will replace its current control (if any)
		/// with <paramref name="activeView"/>.
		/// </remarks>
		public void ActivateView(IActiveView activeView)
		{
			SuspendLayout();
			if (Controls.Count > 0)
			{
				var oldControl = Controls[0];
				Controls.Clear();
				oldControl.Dispose();
			}
			var newControl = (Control)activeView;
			Controls.Add(newControl);
			newControl.Dock = DockStyle.Fill;
			ResumeLayout(true);
		}

		#endregion Implementation of ILiftBridgeView
	}
}
