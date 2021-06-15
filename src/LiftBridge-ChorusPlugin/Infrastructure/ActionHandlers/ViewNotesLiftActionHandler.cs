// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Windows.Forms;
using Chorus;
using Chorus.FileTypeHandlers.lift;
using Chorus.UI.Notes;
using Chorus.UI.Notes.Browser;
using LibTriboroughBridgeChorusPlugin;
using LibTriboroughBridgeChorusPlugin.Infrastructure;
using SIL.Network;
using SIL.Progress;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Infrastructure;
using TriboroughBridge_ChorusPlugin.View;

namespace SIL.LiftBridge.Infrastructure.ActionHandlers
{
	/// <summary>
	/// This IBridgeActionTypeHandler implementation handles everything needed for viewing the notes of a Lift repo.
	/// </summary>
	[Export(typeof(IBridgeActionTypeHandler))]
	internal sealed class ViewNotesLiftActionHandler : IBridgeActionTypeHandler, IBridgeActionTypeHandlerShowWindow
	{
		[Import]
		private FLExConnectionHelper _connectionHelper;
		private IChorusUser _chorusUser;
		private ChorusSystem _chorusSystem;
		private NotesBrowserPage _notesBrowser;
		private string _fwProjectFolder;
		private Form _mainForm;

		public event JumpEventHandler JumpUrlChanged;

		/// <summary>
		/// Currently no adjustments required for LIFT.
		/// </summary>
		private static string AdjustConflictHtml(string input)
		{
			return input;
		}

		private void JumpToLiftObject(string url)
		{
			// TODO REVIEW JohnT(RandyR): This one needs to be modified for use by lift data, but for use by FLEx.
			//// Flex expects the query to be UrlEncoded (I think so it can be used as a command line argument).
			var hostLength = url.IndexOf("?", StringComparison.InvariantCulture);
			if (hostLength < 0)
				return; // can't do it, not a valid FLEx url.

			var host = url.Substring(0, hostLength);
			// This should be fairly safe for a lift URL, since it won't have the "database=current" string in the query.
			// A lift URL will be something like:
			//		lift://foo.lift?type=entry&id=someguid&label=formforentry
			var originalQuery = url.Substring(hostLength + 1).Replace("database=current", "database=" + LiftUtilties.GetLiftProjectName(_fwProjectFolder));
			var query = HttpUtility.UrlEncode(originalQuery);

			// Instead of closing the conflict viewer we now need to fire this event to notify
			// the FLExConnectionHelper that we have a URL to jump to.
			if (JumpUrlChanged != null)
				JumpUrlChanged(this, new JumpEventArgs(host + "?" + query));
		}

		#region IBridgeActionTypeHandler impl

		/// <summary>
		/// Start doing whatever is needed for the supported type of action.
		/// </summary>
		void IBridgeActionTypeHandler.StartWorking(IProgress progress, Dictionary<string, string> options, ref string somethingForClient)
		{
			_fwProjectFolder = Path.GetDirectoryName(options["-p"]);

			_mainForm = new MainBridgeForm
				{
					ClientSize = new Size(904, 510)
				};
			_chorusUser = new ChorusUser(options["-u"]);
			_chorusSystem = TriboroughBridgeUtilities.InitializeChorusSystem(TriboroughBridgeUtilities.LiftOffset(_fwProjectFolder), _chorusUser.Name, LiftFolder.AddLiftFileInfoToFolderConfiguration);
			_chorusSystem.EnsureAllNotesRepositoriesLoaded();

			_notesBrowser = _chorusSystem.WinForms.CreateNotesBrowser();
			var conflictHandler = _notesBrowser.MessageContentHandlerRepository.KnownHandlers.OfType<MergeConflictEmbeddedMessageContentHandler>().First();

			_chorusSystem.NavigateToRecordEvent.Subscribe(JumpToLiftObject);
			conflictHandler.HtmlAdjuster = AdjustConflictHtml;
			if (_connectionHelper != null)
				JumpUrlChanged += _connectionHelper.SendJumpUrlToFlex;

			var viewer = new BridgeConflictView();
			_mainForm.Controls.Add(viewer);
			_mainForm.Text = viewer.Text;
			viewer.Dock = DockStyle.Fill;
			viewer.SetBrowseView(_notesBrowser);

			// Only used by FLEx, so how can it not be in use?
			//if (_currentLanguageProject.FieldWorkProjectInUse)
			//	viewer.EnableWarning();
			viewer.SetProjectName(LiftUtilties.GetLiftProjectName(_fwProjectFolder));
		}

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		ActionType IBridgeActionTypeHandler.SupportedActionType
		{
			get { return ActionType.ViewNotesLift; }
		}

		#endregion IBridgeActionTypeHandler impl

		#region Implementation of IBridgeActionTypeHandlerShowWindow

		/// <summary>
		/// Get the main window for the application.
		/// </summary>
		Form IBridgeActionTypeHandlerShowWindow.MainForm
		{
			get { return _mainForm; }
		}

		#endregion Implementation of IBridgeActionTypeHandlerShowWindow

		#region Implementation of IDisposable

		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. m_isDisposed is true)
		/// </summary>
		~ViewNotesLiftActionHandler()
		{
			Dispose(false);
			// The base class finalizer is called automatically.
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing,
		/// or resetting unmanaged resources.
		/// </summary>
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
				if (_mainForm != null)
				{
					_mainForm.Dispose();
				}
				if (_notesBrowser != null)
				{
					_notesBrowser.Dispose();
				}
				if (_chorusSystem != null)
				{
					_chorusSystem.Dispose();
				}
				if (_connectionHelper != null)
				{
					JumpUrlChanged -= _connectionHelper.SendJumpUrlToFlex;
				}
				if (_chorusSystem != null)
				{
					_chorusSystem.Dispose();
				}
			}
			_connectionHelper = null;
			_mainForm = null;
			_chorusUser = null;
			_notesBrowser = null;

			IsDisposed = true;
		}

		#endregion Implementation of IDisposable
	}
}
