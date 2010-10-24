using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Chorus;
using FieldWorksBridge.Infrastructure;
using FieldWorksBridge.Model;
using FieldWorksBridge.View;

namespace FieldWorksBridgeTests.Controller
{
	internal class MockedView : UserControl, IFwBridgeView
	{
		private ChorusSystem _chorusSystem;
		private IEnumerable<LanguageProject> _projects;

		#region Implementation of IFwBridgeView

		public event ProjectSelectedEventHandler ProjectSelected;
		public event SynchronizeProjectEventHandler SynchronizeProject;

		public IEnumerable<LanguageProject> Projects
		{
			set { _projects = value; }
		}

		public ChorusSystem SyncSystem
		{
			set { _chorusSystem = value; }
		}

		internal void RaiseProjectSelected()
		{
			ProjectSelected(this, new ProjectEventArgs(_projects.First()));
		}

		internal void RaiseSynchronizeProject()
		{
			SynchronizeProject(this, new SynchronizeEventArgs(_chorusSystem));
		}

		#endregion
	}

	internal class MockedLocator : IProjectPathLocator
	{
		private readonly HashSet<string> _baseFolderPaths;

		internal MockedLocator(HashSet<string> baseFolderPaths)
		{
			_baseFolderPaths = baseFolderPaths;
		}

		#region Implementation of IProjectPathLocator

		public HashSet<string> BaseFolderPaths
		{
			get { return new HashSet<string>(_baseFolderPaths); }
		}

		#endregion
	}
}