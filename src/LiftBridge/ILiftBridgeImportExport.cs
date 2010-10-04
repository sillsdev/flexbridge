namespace SIL.LiftBridge
{
	public interface ILiftBridgeImportExport
	{
		/// <summary>
		/// Export the FieldWorks lexicon into the LIFT file.
		/// The file may, or may not, exist.
		/// </summary>
		void ExportLexicon();

		/// <summary>
		/// Import the LIFT file into FieldWorks.
		/// </summary>
		void ImportLexicon();

		/// <summary>
		/// Gets or sets the LIFT file's pathname.
		/// </summary>
		string LiftPathname { get; set; }
	}
}