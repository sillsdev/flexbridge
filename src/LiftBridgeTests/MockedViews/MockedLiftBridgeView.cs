using System.Windows.Forms;
using SIL.LiftBridge.View;

namespace LiftBridgeTests.MockedViews
{
	internal class MockedLiftBridgeView : ILiftBridgeView
	{
		private IActiveView _activeView;

		internal string Title { get; private set; }
		internal Form MainForm { get; private set; }

		#region Implementation of IDisposable

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose()
		{
			//MainForm = null;
			//Title = null;
		}

		#endregion

		#region Implementation of ILiftBridgeView

		/// <summary>
		/// Show the view with the given parent form and the given title.
		/// </summary>
		public void Show(Form parent, string title)
		{
			MainForm = parent;
			Title = title;
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
			_activeView = activeView;
		}

		#endregion
	}
}
