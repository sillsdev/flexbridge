namespace TriboroughBridge_ChorusPlugin
{
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