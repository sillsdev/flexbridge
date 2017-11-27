// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Windows.Forms;

namespace TriboroughBridge_ChorusPlugin.Infrastructure
{
	/// <summary>
	/// This is one of the two optional interfaces an action handler may choose to implement,
	/// if it is appropriate to the needs one the main IBridgeActionTypeHandler interface implementation.
	///
	/// Add this interface, if the action handler needs to show a window. Otherwise, skip this one.
	/// </summary>
	internal interface IBridgeActionTypeHandlerShowWindow : IDisposable
	{
		/// <summary>
		/// Get the main window for the application.
		/// </summary>
		Form MainForm { get; }
	}
}