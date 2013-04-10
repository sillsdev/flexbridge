namespace TriboroughBridge_ChorusPlugin
{
	/// <summary>
	/// These are all of the expected command line options for the "-v" option,
	/// except for "Unknown", which is the never-to-be-used default enum value.
	/// </summary>
	public enum ActionType
	{
		Unknown,

		Obtain,
		ObtainLift,

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