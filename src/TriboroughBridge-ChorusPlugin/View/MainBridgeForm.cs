using System.ComponentModel.Composition;
using System.Windows.Forms;

namespace TriboroughBridge_ChorusPlugin.View
{
	/// <summary>
	/// Generic form used by any action handler that wants a form. Each is given this instance and expected to add any controls that might be needed, if any.
	/// </summary>
	[Export(typeof(MainBridgeForm))]
	public partial class MainBridgeForm : Form
	{
		public MainBridgeForm()
		{
			InitializeComponent();
		}
	}
}
