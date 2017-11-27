// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

namespace LibTriboroughBridgeChorusPlugin
{
	/// <summary>
	/// These are all of the expected command line options for the "-v" option,
	/// except for "Unknown", which is the never-to-be-used default enum value.
	/// </summary>
	internal enum ActionType
	{
		Unknown,

		LanguageForgeClone,

		LanguageForgeUpdateToLongHash,

		LanguageForgeAuxiliaryCommit,

		LanguageForgeGetChorusNotes,
		LanguageForgeWriteToChorusNotes,

		Obtain,
		ObtainLift,

		LanguageForgeSendReceive,
		SendReceive,
		SendReceiveLift,

		ViewNotes,
		ViewNotesLift,

		UndoExport, // Not supported yet.
		UndoExportLift,

		MoveLift,

		CheckForUpdates,
		AboutFlexBridge
	}
}