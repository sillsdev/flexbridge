using System.Windows.Forms;

namespace SIL.LiftBridge
{
	public interface ILiftBridgeImportExport
	{
		/// <summary>
		/// Export the FieldWorks lexicon into the LIFT file.
		/// The file may, or may not, exist.
		/// </summary>
		/// <returns>True, if successful, otherwise false.</returns>
		bool ExportLexicon(Form parentForm);

		/// <summary>
		/// Import the LIFT file into FieldWorks.
		/// </summary>
		/// <returns>True, if successful, otherwise false.</returns>
		bool ImportLexicon(Form parentForm);

		/// <summary>
		/// Do a basic 'safe' import, where entries in FLEx that are not
		/// in the Lift file are not removed.
		/// </summary>
		/// <returns>True, if successful, otherwise false.</returns>
		bool DoBasicImport(Form parentForm);

		/// <summary>
		/// Gets or sets the LIFT file's pathname.
		/// </summary>
		string LiftPathname { get; set; }
	}
}