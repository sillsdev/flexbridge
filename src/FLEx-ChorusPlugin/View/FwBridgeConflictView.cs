using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using FLEx_ChorusPlugin.Controller;
using FLEx_ChorusPlugin.Model;

namespace FLEx_ChorusPlugin.View
{
	/// <summary>
	/// This class provides the view of the Conflicts that FLEx wants to see.
	/// </summary>
	internal partial class FwBridgeConflictView : Form
	{
		private IEnumerable<LanguageProject> _projects;

		internal FwBridgeConflictView()
		{
			InitializeComponent();
		}

		//private void _projectView_Load(object sender, EventArgs e)
		//{
		//    throw new NotImplementedException();
		//}
	}
}
