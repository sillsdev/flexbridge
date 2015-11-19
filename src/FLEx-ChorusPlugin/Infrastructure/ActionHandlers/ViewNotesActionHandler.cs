// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

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
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Infrastructure;
using TriboroughBridge_ChorusPlugin.View;
using LibFLExBridgeChorusPlugin.Infrastructure;

namespace FLEx_ChorusPlugin.Infrastructure.ActionHandlers
{
	/// <summary>
	/// This IBridgeActionTypeHandler implementation handles everything needed for viewing the notes of a Flex repo.
	/// </summary>
	[Export(typeof (IBridgeActionTypeHandler))]
	internal sealed class ViewNotesActionHandler : IBridgeActionTypeHandler, IBridgeActionTypeHandlerShowWindow
	{
		[Import]
		private FLExConnectionHelper _connectionHelper;

		private IChorusUser _chorusUser;
		private ChorusSystem _chorusSystem;
		private NotesBrowserPage _notesBrowser;

		public event JumpEventHandler JumpUrlChanged;

		internal string ProjectName { get; set; }
		internal string ProjectDir { get; set; }

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
			var originalQuery = url.Substring(hostLength + 1).Replace("database=current", "database=" + ProjectName);
			var query = HttpUtilityFromMono.UrlEncode(originalQuery);

			// Instead of closing the conflict viewer we now need to fire this event to notify
			// the FLExConnectionHelper that we have a URL to jump to.
			if (JumpUrlChanged != null)
				JumpUrlChanged(this, new JumpEventArgs(host + "?" + query));
		}

		/// <summary>
		/// This method receives the HtmlDetails stored in a conflict, and returns adjusted HTML.
		/// Specifically it fixes URLs containing "database=current" to contain the real project name,
		/// replaces WS identifiers with pretty names, and replaces reference property checksums
		/// with a nice list of reference properties that have conflicts (if any).
		/// </summary>
		internal string AdjustConflictHtml(string input)
		{
			var projectNameForUrl = HttpUtilityFromMono.UrlEncode(ProjectName);
			return FixChecksums(FixWsRuns(input)).Replace(@"&amp;database=current&amp;", @"&amp;database=" + projectNameForUrl + @"&amp;");
		}

		/// <summary>
		/// We're looking for <div class="checksum">Propname: checksum</div>, with the checksum possibly surrounded
		/// by spans with a style. If the span is present, the named property was changed differently in the two versions.
		/// We want to replace all such divisions in any given parent with a single
		/// <div class="checksum">There were changes to related objects in Propname, Propname...</div>
		/// </summary>
		private static string FixChecksums(string input)
		{
			// XML parser won't handle this obsolete html entity.
			var root = XElement.Parse(input.Replace("&nbsp;", "&#160;"), LoadOptions.PreserveWhitespace);
			foreach (var parentDiv in DivisionsWithChecksumChildren(root))
			{
				var checksumChildren = (from child in parentDiv.Elements("div")
										where child.Attribute("class") != null && child.Attribute("class").Value == "checksum"
										select child).ToList();
				var conflictPropnames = (checksumChildren.Where(child => child.Element("span") != null)
														 .Select(ExtractPropName)).ToList(); // This part may not work on Mono, so if not, use the non-method cahin option, below.
				// .Select(child => ExtractPropName(child))).ToList();
				var needToRemove = checksumChildren.Skip(1).ToList(); // all but first will surely be removed
				var firstChild = checksumChildren.First();
				if (conflictPropnames.Any())
				{
					firstChild.RemoveNodes();
					firstChild.Add("There were changes to related objects in " + string.Join(", ", conflictPropnames));
				}
				else
				{
					needToRemove.Add(firstChild); // get rid of it too, no conflicts to report.
				}
				foreach (var goner in needToRemove)
				{
					// This is not perfect...there may be trailing white space in a non-empty previous node that is
					// really formatting in front of goner...but it cleans up most of the junk. And currently
					// it is, I believe, only test data that actually has extra white space; for real data
					// we just generate one long line.
					var before = goner.PreviousNode;
					if (before is XText && ((XText)before).Value.Trim() == "")
						before.Remove();
					goner.Remove();
				}
			}
			return root.ToString(SaveOptions.DisableFormatting);
		}

		// Find all div elements with at least one direct child that is a div with class checksum
		private static IEnumerable<XElement> DivisionsWithChecksumChildren(XElement root)
		{
			return from div in root.Descendants("div")
				   where (from child in div.Elements("div")
						  where child.Attribute("class") != null && child.Attribute("class").Value == "checksum"
						  select child).Any()
				   select div;
		}

		private static string ExtractPropName(XElement elt)
		{
			var text = elt.Value;
			var index = text.IndexOf(':');
			return index < 0 ? "" /* paranoia */ : text.Substring(0, index);
		}

		/// <summary>
		/// Looks for something like <span class="ws">en-fonipa</span>,
		/// and we can find a corresponding writing system in the project and extract its user-friendly name,
		/// replace the run content with the user-friendly name.
		/// There may be another span wrapped around the name inside it, for example,
		/// <span class='ws'><span style='background: Yellow'>en-fonipa</span></span>
		/// </summary>
		private string FixWsRuns(string input)
		{
			var pattern = new Regex("<span class=\"ws\">(<span[^>]*>)?([^<]+)</span>");
			string current = input;
			if (ProjectDir == null)
				return input;
			var wspath = Path.Combine(ProjectDir, @"WritingSystemStore");
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
					// ignore any parsing errors, just can't improve this one
				}
			}
			return current;
		}

		private static void AddSpecialValue(XElement root, XNamespace ns, string eltName, List<string> collector)
		{
			var item = GetSpecialValue(root, ns, eltName);
			if (item != null)
				collector.Add(item);
		}

		private static string GetSpecialValue(XElement root, XNamespace ns, string eltName)
		{
			return (root.Elements("special")
						.Select(elt => elt.Element(ns + eltName))
						.Where(targetElt => targetElt != null)
						.Select(targetElt => targetElt.Attribute("value"))
						.Where(valueAttr => valueAttr != null)
						.Select(valueAttr => valueAttr.Value)).FirstOrDefault();
		}

		#region IBridgeActionTypeHandler impl

		/// <summary>
		/// Start doing whatever is needed for the supported type of action.
		/// </summary>
		public void StartWorking(Dictionary<string, string> commandLineArgs)
		{
			var pOption = commandLineArgs["-p"];
			ProjectName = Path.GetFileNameWithoutExtension(pOption);
			ProjectDir = Path.GetDirectoryName(pOption);

			MainForm = new MainBridgeForm
			{
				ClientSize = new Size(904, 510)
			};
			_chorusUser = new ChorusUser(commandLineArgs["-u"]);
			_chorusSystem = TriboroughBridge_ChorusPlugin.Utilities.InitializeChorusSystem(ProjectDir, _chorusUser.Name,
				FlexFolderSystem.ConfigureChorusProjectFolder);
			_chorusSystem.EnsureAllNotesRepositoriesLoaded();
			_notesBrowser = _chorusSystem.WinForms.CreateNotesBrowser();
			var conflictHandler = _notesBrowser.MessageContentHandlerRepository.KnownHandlers.OfType<MergeConflictEmbeddedMessageContentHandler>().First();

			// _currentStrategy.InitializeStrategy(ChorusSystem, conflictHandler);
			_chorusSystem.NavigateToRecordEvent.Subscribe(JumpToFlexObject);
			conflictHandler.HtmlAdjuster = AdjustConflictHtml;
			if (_connectionHelper != null)
				JumpUrlChanged += _connectionHelper.SendJumpUrlToFlex;

			var viewer = new BridgeConflictView();
			MainForm.Controls.Add(viewer);
			MainForm.Text = viewer.Text;
			viewer.Dock = DockStyle.Fill;
			viewer.SetBrowseView(_notesBrowser);

			// Only used by FLEx, so how can it not be in use?
			//if (_currentLanguageProject.FieldWorkProjectInUse)
			//	viewer.EnableWarning();
			viewer.SetProjectName(ProjectName);
		}

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		public ActionType SupportedActionType
		{
			get { return ActionType.ViewNotes; }
		}

		#endregion IBridgeActionTypeHandler impl

		#region IBridgeActionTypeHandlerShowWindow impl

		/// <summary>
		/// Get the main window for the application.
		/// </summary>
		public Form MainForm { get; private set; }

		#endregion IBridgeActionTypeHandlerShowWindow impl

		#region IDisposable impl

		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. m_isDisposed is true)
		/// </summary>
		~ViewNotesActionHandler()
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
			MainForm = null;
			_chorusUser = null;
			_notesBrowser = null;
			ProjectName = null;
			ProjectDir = null;

			IsDisposed = true;
		}

		#endregion IDisposable impl
	}
}