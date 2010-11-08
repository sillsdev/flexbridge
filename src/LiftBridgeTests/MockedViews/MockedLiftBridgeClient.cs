using LiftBridgeCore;

namespace LiftBridgeTests.MockedViews
{
	/// <summary>
	/// Class that simulates a LiftBridge client
	/// </summary>
	internal class MockedLiftBridgeClient
	{
		private readonly ILiftBridge _liftBridgeController;
		internal bool HandledBasicLexiconImport { get; private set; }
		internal bool HandledImportLexicon { get; private set; }
		internal bool HandledExportLexicon { get; private set; }

		internal MockedLiftBridgeClient(ILiftBridge liftBridgeController)
		{
			_liftBridgeController = liftBridgeController;
			_liftBridgeController.BasicLexiconImport += LiftBridgeControllerBasicLexiconImport;
			_liftBridgeController.ImportLexicon += LiftBridgeControllerImportLexicon;
			_liftBridgeController.ExportLexicon += LiftBridgeControllerExportLexicon;
			HandledBasicLexiconImport = false;
			HandledImportLexicon = false;
			HandledExportLexicon = false;
		}

		void LiftBridgeControllerExportLexicon(object sender, LiftBridgeEventArgs e)
		{
			HandledExportLexicon = true;
		}

		void LiftBridgeControllerImportLexicon(object sender, LiftBridgeEventArgs e)
		{
			HandledImportLexicon = true;
		}

		void LiftBridgeControllerBasicLexiconImport(object sender, LiftBridgeEventArgs e)
		{
			HandledBasicLexiconImport = true;
		}
	}
}