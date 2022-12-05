// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;

namespace LibTriboroughBridgeChorusPlugin.Infrastructure.ActionHandlers
{
	/// <summary>
	/// Class used to convert a string representing an ActionType to an ActionType.
	/// </summary>
	internal static class StringToActionTypeConverter
	{
		// Support for Language Forge
		private const string Language_Forge_Send_Receive = "Language_Forge_Send_Receive";
		private const string Language_Forge_Clone = "Language_Forge_Clone";
		private const string Language_Forge_Update_To_Long_Hash = "Language_Forge_Update_To_Long_Hash";
		private const string Language_Forge_Auxiliary_Commit = "Language_Forge_Auxiliary_Commit";
		private const string Language_Forge_Get_Chorus_Notes = "Language_Forge_Get_Chorus_Notes";
		private const string Language_Forge_Write_To_Chorus_Notes = "Language_Forge_Write_To_Chorus_Notes";

		// Support for FLEx Bridge itself
		private const string about_flex_bridge = "about_flex_bridge";
		// Support for full FLEx project
		private const string obtain = "obtain";							// -p <$fwroot>
		private const string send_receive = "send_receive";				// -p <$fwroot>\foo\foo.fwdata
		private const string view_notes = "view_notes";					// -p <$fwroot>\foo\foo.fwdata
		private const string undo_export = "undo_export";				// Not supported (yet?).
		// Support for LIFT
		private const string obtain_lift = "obtain_lift";				// -p <$fwroot>\foo where 'foo' is the project folder name
		private const string send_receive_lift = "send_receive_lift";	// -p <$fwroot>\foo\foo.fwdata
		private const string view_notes_lift = "view_notes_lift";		// -p <$fwroot>\foo\foo.fwdata
		private const string undo_export_lift = "undo_export_lift";	// -p <$fwroot>\foo where 'foo' is the project folder name
		private const string move_lift = "move_lift";					// -p <$fwroot>\foo\foo.fwdata

		/// <summary>
		/// Get the <see cref="ActionType"/> represented by the <paramref name="actionType"/> parameter.
		/// </summary>
		/// <param name="actionType">The string representation of the <see cref="ActionType"/>.</param>
		/// <returns>The <see cref="ActionType"/> for the given <paramref name="actionType"/> parameter.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="actionType"/> has no corresponding <see cref="ActionType"/>.</exception>
		internal static ActionType GetActionType(string actionType)
		{
			switch (actionType)
			{
				case Language_Forge_Send_Receive:
					return ActionType.LanguageForgeSendReceive;
				case Language_Forge_Clone:
					return ActionType.LanguageForgeClone;
				case Language_Forge_Update_To_Long_Hash:
					return ActionType.LanguageForgeUpdateToLongHash;
				case Language_Forge_Auxiliary_Commit:
					return ActionType.LanguageForgeAuxiliaryCommit;
				case Language_Forge_Get_Chorus_Notes:
					return ActionType.LanguageForgeGetChorusNotes;
				case Language_Forge_Write_To_Chorus_Notes:
					return ActionType.LanguageForgeWriteToChorusNotes;

				case obtain:
					return ActionType.Obtain;
				case obtain_lift:
					return ActionType.ObtainLift;
				case send_receive:
					return ActionType.SendReceive;
				case send_receive_lift:
					return ActionType.SendReceiveLift;
				case view_notes:
					return ActionType.ViewNotes;
				case view_notes_lift:
					return ActionType.ViewNotesLift;
				case undo_export: // Not used.
					return ActionType.UndoExport;
				case undo_export_lift:
					return ActionType.UndoExportLift;
				case move_lift:
					return ActionType.MoveLift;
				case about_flex_bridge:
					return ActionType.AboutFlexBridge;
				default:
					throw new ArgumentOutOfRangeException("actionType", "Action type is not supported");
			}
		}
	}
}