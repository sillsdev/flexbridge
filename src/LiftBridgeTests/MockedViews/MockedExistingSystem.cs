using Chorus;
using LiftBridgeCore;
using SIL.LiftBridge.Model;
using SIL.LiftBridge.View;

namespace LiftBridgeTests.MockedViews
{
	internal class MockedExistingSystem : IExistingSystemView
	{
		internal bool ExportLexiconIsWiredUp
		{
			get { return ExportLexicon != null; }
		}
		internal bool ImportLexiconIsWiredUp
		{
			get { return ImportLexicon != null; }
		}
		internal ChorusSystem ChorusSys { get; private set; }
		internal LiftProject Project { get; private set; }

		internal void SimulateSendReceiveWithChangesFromAfar()
		{
			ExportLexicon(this, new LiftBridgeEventArgs("exported.lift"));
			ImportLexicon(this, new LiftBridgeEventArgs(Project.LiftPathname));
		}

		internal void SimulateSendReceiveWithNoChangesFromAfar()
		{
			ExportLexicon(this, new LiftBridgeEventArgs("exported.lift"));
		}

		#region Implementation of IExistingSystemView

		public event ExportLexiconEventHandler ExportLexicon;
		public event ImportLexiconEventHandler ImportLexicon;

		public void SetSystem(ChorusSystem chorusSystem, LiftProject liftProject)
		{
			ChorusSys = chorusSystem;
			Project = liftProject;
		}

		#endregion
	}
}