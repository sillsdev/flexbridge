using System;
using System.Windows.Forms;

namespace SIL.LiftBridge.View
{
	internal interface ILiftBridgeView : IDisposable
	{
		/// <summary>
		/// Show the view with the given parent form and the given title.
		/// </summary>
		void Show(Form parent, string title);

		/// <summary>
		/// Make <paramref name="activeView"/> the active control.
		/// </summary>
		/// <remarks>
		/// <paramref name="activeView"/> must be a Control (UserControl),
		/// since the expectation is that the ILiftBridgeView will replace its current control (if any)
		/// with <paramref name="activeView"/>.
		/// </remarks>
		void ActivateView(IActiveView activeView);
	}
}