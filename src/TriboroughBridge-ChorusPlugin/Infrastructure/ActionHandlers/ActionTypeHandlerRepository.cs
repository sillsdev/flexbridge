// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace TriboroughBridge_ChorusPlugin.Infrastructure.ActionHandlers
{
	/// <summary>
	/// This is the central repository (collection) of IBridgeActionTypeHandler implementations.
	///
	/// When an implementation of the IBridgeActionTypeHandler is marked for export. MEF then makes sure
	/// it is included in this class.
	///
	/// Depending on the command line option for "-v", the startup code selects a matching hanlder to process
	/// the action specified in the "-v" option.
	/// </summary>
	[Export(typeof(ActionTypeHandlerRepository))]
	public class ActionTypeHandlerRepository
	{
		internal const string obtain = "obtain";						// -p <$fwroot>
		internal const string obtain_lift = "obtain_lift";				// -p <$fwroot>\foo where 'foo' is the project folder name
		internal const string send_receive = "send_receive";			// -p <$fwroot>\foo\foo.fwdata
		internal const string send_receive_lift = "send_receive_lift";	// -p <$fwroot>\foo\foo.fwdata
		internal const string view_notes = "view_notes";				// -p <$fwroot>\foo\foo.fwdata
		internal const string view_notes_lift = "view_notes_lift";		// -p <$fwroot>\foo\foo.fwdata
		internal const string undo_export = "undo_export";				// Not supported.
		internal const string undo_export_lift = "undo_export_lift";	// -p <$fwroot>\foo where 'foo' is the project folder name
		internal const string move_lift = "move_lift";					// -p <$fwroot>\foo\foo.fwdata
		internal const string check_for_updates = "check_for_updates";	// -p <$fwroot>\foo where 'foo' is the project folder name
		internal const string about_flex_bridge = "about_flex_bridge";	// -p <$fwroot>\foo where 'foo' is the project folder name

		private static readonly Dictionary<string, ActionType> VOptionToActionTypeMap = new Dictionary<string, ActionType>
			{
				{obtain, ActionType.Obtain},
				{obtain_lift, ActionType.ObtainLift},
				{send_receive, ActionType.SendReceive},
				{send_receive_lift, ActionType.SendReceiveLift},
				{view_notes, ActionType.ViewNotes},
				{view_notes_lift, ActionType.ViewNotesLift},
				{undo_export, ActionType.UndoExport},
				{undo_export_lift, ActionType.UndoExportLift},
				{move_lift, ActionType.MoveLift},
				{check_for_updates, ActionType.CheckForUpdates},
				{about_flex_bridge, ActionType.AboutFlexBridge}
			};

		[ImportMany]
		public IEnumerable<IBridgeActionTypeHandler> Handlers { get; private set; }

		public IBridgeActionTypeHandler GetHandler(Dictionary<string, string> commandLineArgs)
		{
			return Handlers.First(handler => handler.SupportedActionType == VOptionToActionTypeMap[commandLineArgs["-v"]]);
		}
	}
}