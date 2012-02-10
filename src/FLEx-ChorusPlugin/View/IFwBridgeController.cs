using System.Windows.Forms;
using Chorus;
using FLEx_ChorusPlugin.Model;

namespace FLEx_ChorusPlugin.View
{
	internal interface IFwBridgeController
	{
		Form MainForm { get; }
		ChorusSystem ChorusSystem { get; }
		LanguageProject CurrentProject { get; }
	}
}