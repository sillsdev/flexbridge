using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Chorus;
using Chorus.UI.Notes;
using Chorus.UI.Notes.Browser;
using Palaso.Network;
using TriboroughBridge_ChorusPlugin.View;

namespace TriboroughBridge_ChorusPlugin.Controller
{
	[Export(typeof(IBridgeController))]
	internal class BridgeConflictController : IConflictController
	{
		[ImportMany]
		private IEnumerable<IConflictStrategy> Strategies { get; set; }
		private IConflictStrategy _currentStrategy;
		private IChorusUser _chorusUser;
		private MainBridgeForm _mainBridgeForm;
		private NotesBrowserPage _notesBrowser;
		private string _projectDir;
		private string _projectName;

		private void JumpToFlexObject(string url)
		{
			//// Flex expects the query to be UrlEncoded (I think so it can be used as a command line argument).
			var hostLength = url.IndexOf("?", StringComparison.InvariantCulture);
			if (hostLength < 0)
				return; // can't do it, not a valid FLEx url.

			var host = url.Substring(0, hostLength);
			// This should be fairly safe for a lift URL, since it won't have the "database=current" string in the query.
			// A lift URL will be something like:
			//		lift://foo.lift?type=entry&id=someguid&label=formforentry
			var originalQuery = url.Substring(hostLength + 1).Replace("database=current", "database=" + _projectName);
			var query = HttpUtilityFromMono.UrlEncode(originalQuery);

			// Instead of closing the conflict viewer we now need to fire this event to notify
			// the FLExConnectionHelper that we have a URL to jump to.
			if (JumpUrlChanged != null)
				JumpUrlChanged(this, new JumpEventArgs(host + "?" + query));
		}

		private IConflictStrategy GetCurrentStrategy(ControllerType controllerType)
		{
			return Strategies.FirstOrDefault(strategy => strategy.SupportedControllerAction == controllerType);
		}

		#region IBridgeController implementation

		public void InitializeController(MainBridgeForm mainForm, Dictionary<string, string> options, ControllerType controllerType)
		{
			_currentStrategy = GetCurrentStrategy(controllerType);
			_projectDir = _currentStrategy.GetProjectDir(options["-p"]);
			_projectName = _currentStrategy.GetProjectName(options["-p"]);
			_mainBridgeForm = mainForm;
			_mainBridgeForm.ClientSize = new Size(904, 510);

			_chorusUser = new ChorusUser(options["-u"]);
			ChorusSystem = Utilities.InitializeChorusSystem(_projectDir, _chorusUser.Name, _currentStrategy.ConfigureProjectFolders);
			ChorusSystem.EnsureAllNotesRepositoriesLoaded();

			ChorusSystem.NavigateToRecordEvent.Subscribe(JumpToFlexObject);

			_notesBrowser = ChorusSystem.WinForms.CreateNotesBrowser();
			var conflictHandler = _notesBrowser.MessageContentHandlerRepository.KnownHandlers.OfType<MergeConflictEmbeddedMessageContentHandler>()
						 .First();
			conflictHandler.HtmlAdjuster = AdjustConflictHtml;
			var viewer = new BridgeConflictView();
			_mainBridgeForm.Controls.Add(viewer);
			_mainBridgeForm.Text = viewer.Text;
			viewer.Dock = DockStyle.Fill;
			viewer.SetBrowseView(_notesBrowser);

			// Only used by FLEx, so how can it not be in use?
			//if (_currentLanguageProject.FieldWorkProjectInUse)
			//	viewer.EnableWarning();
			viewer.SetProjectName(_projectName);
		}

		/// <summary>
		/// A minimal init sufficient for testing AdjustConflictHtml.
		/// </summary>
		/// <param name="projectName"></param>
		/// <param name="projectDir"></param>
		internal void InitForAdjustConflict(string projectName, string projectDir)
		{
			_projectName = projectName;
			_projectDir = projectDir;
		}

		/// <summary>
		/// This method receives the HtmlDetails stored in a conflict, and returns adjusted HTML.
		/// Specifically it fixes URLs containing "database=current" to contain the real project name
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public string AdjustConflictHtml(string input)
		{
			return FixWsRuns(input).Replace(@"&amp;database=current&amp;", @"&amp;database=" + _projectName + @"&amp;");
		}

		/// <summary>
		/// Looks for something like <span class="ws">en-fonipa</span>,
		/// and we can find a corresponding writing system in the project and extract its user-friendly name,
		/// replace the run content with the user-friendly name.
		/// There may be another span wrapped around the name inside it, for example,
		/// <span class='ws'><span style='background: Yellow'>en-fonipa</span></span>
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		string FixWsRuns(string input)
		{
			var pattern = new Regex("<span class=\"ws\">(<span[^>]*>)?([^<]+)</span>");
			string current = input;
			if (_projectDir == null)
				return input;
			var wspath = Path.Combine(_projectDir, @"WritingSystemStore");
			if (!Directory.Exists(wspath))
				return input; // can't translate.
			Match match;
			int startAt = 0;
			while (startAt < current.Length && (match = pattern.Match(current, startAt)).Success)
			{
				var targetGroup = match.Groups[2];
				string ws = targetGroup.Value;
				startAt = match.Index + match.Length; // don't match this again, even if we can't improve it.
				string ldmlpath = Path.ChangeExtension(Path.Combine(wspath, ws), "ldml");
				if (!File.Exists(ldmlpath))
					continue; // can't improve this one
				try
				{
					var ldmlElt = XDocument.Load(ldmlpath).Root;
					var special = ldmlElt.Element(@"special");
					if (special == null)
						continue;
					XNamespace palaso = @"urn://palaso.org/ldmlExtensions/v1";
					var langName = GetSpecialValue(ldmlElt, palaso, "languageName");
					if (langName == null)
						continue;
					var mods = new List<string>();
					XNamespace fw = @"urn://fieldworks.sil.org/ldmlExtensions/v1";
					if (!targetGroup.Value.Contains("Zxxx")) // suppress "Code for unwritten documents"
						AddSpecialValue(ldmlElt, fw, "scriptName", mods);
					AddSpecialValue(ldmlElt, fw, "regionName", mods);
					AddSpecialValue(ldmlElt, fw, "variantName", mods);
					string niceName = langName;
					if (mods.Count > 0)
					{
						niceName += " (";
						niceName += string.Join(", ", mods.ToArray());
						niceName += ")";
					}
					var beforeMatch = targetGroup.Index;
					var matchLength = targetGroup.Length;
					current = current.Substring(0, beforeMatch) + niceName + current.Substring(beforeMatch + matchLength);
					startAt += niceName.Length - matchLength;
				}
				catch (XmlException)
				{
					continue; // ignore any parsing errors, just can't improve this one
				}
			}
			return current;
		}

		void AddSpecialValue(XElement root, XNamespace ns, string eltName, List<string> collector)
		{
			var item = GetSpecialValue(root, ns, eltName);
			if (item != null)
				collector.Add(item);
		}

		string GetSpecialValue(XElement root, XNamespace ns, string eltName)
		{
			foreach (var elt in root.Elements("special"))
			{
				var targetElt = elt.Element(ns + eltName);
				if (targetElt == null)
					continue;
				var valueAttr = targetElt.Attribute("value");
				if (valueAttr == null)
					continue;
				return valueAttr.Value;
			}
			return null;
		}

		public ChorusSystem ChorusSystem { get; private set; }

		public IEnumerable<ControllerType> SupportedControllerActions
		{
			get { return new List<ControllerType> { ControllerType.ViewNotes, ControllerType.ViewNotesLift }; }
		}

		public IEnumerable<BridgeModelType> SupportedModels
		{
			get { return new List<BridgeModelType> { BridgeModelType.Flex, BridgeModelType.Lift }; }
		}

		#endregion

		#region IConflictController implementation

		public event JumpEventHandler JumpUrlChanged;

		#endregion

		#region Implementation of IDisposable

		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. m_isDisposed is true)
		/// </summary>
		~BridgeConflictController()
		{
			Dispose(false);
			// The base class finalizer is called automatically.
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing,
		/// or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose()
		{
			Dispose(true);
			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

		private bool IsDisposed { get; set; }

		/// <summary>
		/// Executes in two distinct scenarios.
		///
		/// 1. If disposing is true, the method has been called directly
		/// or indirectly by a user's code via the Dispose method.
		/// Both managed and unmanaged resources can be disposed.
		///
		/// 2. If disposing is false, the method has been called by the
		/// runtime from inside the finalizer and you should not reference (access)
		/// other managed objects, as they already have been garbage collected.
		/// Only unmanaged resources can be disposed.
		/// </summary>
		/// <remarks>
		/// If any exceptions are thrown, that is fine.
		/// If the method is being done in a finalizer, it will be ignored.
		/// If it is thrown by client code calling Dispose,
		/// it needs to be handled by fixing the issue.
		/// </remarks>
		private void Dispose(bool disposing)
		{
			if (IsDisposed)
				return;

			if (disposing)
			{
				if (ChorusSystem  != null)
					ChorusSystem.Dispose();
			}
			_mainBridgeForm = null;
			ChorusSystem = null;

			IsDisposed = true;
		}

		#endregion
	}
}
