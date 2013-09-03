using System.ComponentModel.Composition;
using System.Windows.Forms;

namespace TheTurtle.View
{
	[Export(typeof(TheTurtleForm))]
	public partial class TheTurtleForm : Form
	{
		public TheTurtleForm()
		{
			InitializeComponent();
		}

		internal ITurtleView TurtleView { get { return _turtleView; }}
	}
}
