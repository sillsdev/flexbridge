// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using Palaso.Network;

namespace LibFLExBridgeChorusPlugin.Handling
{
	#region FwLinkArgs class
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// provides a message object specifically for asking a FieldWorks application
	/// to do various navigation activities.
	///
	/// This is a cut-down version of {FieldWorks}\src\common\fwutils\FwLinkArgs.cs,
	/// removing anything that depends on other FW stuff, except that the constants for
	/// the names of TE and FLEx had to be duplicated here.
	///
	/// It is unfortunate that it had to be duplicated here, but we wanted to avoid
	/// making FlexBridge depend on FLEx DLLs, especially one that has moved since 7.0.
	///
	/// Here is a summary of how the different pieces of linking work together.
	///
	/// For internal links, such as history, LinkListener creates an FwLinkArgs, and may store
	/// it (e.g., in the history stack). The current path for this in FLEx is that
	/// RecordView.UpdateContextHistory broadcasts AddContextToHistory with a new FwLinkArgs
	/// based on the current toolname and Guid. An instance of LinkListener is in the current
	/// list of colleagues and implements OnAddContextToHistory to update the history.
	/// LinkListener also implements OnHistoryBack (and forward), which call FollowActiveLink
	/// passing an FwLinkArgs to switch the active window.
	///
	/// For links which may address a different project, we use a subclass of FwLinkArgs,
	/// FwAppArgs, which adds information about the application and project. One way these
	/// get created is LinkListener.OnCopyLocationAsHyperlink. The resulting text may be
	/// pasted in other applications which understand hyperlinks, or in FLEx using the Paste
	/// Hyperlink command.
	///
	/// When such a link is clicked, execution begins in VwBaseVc.DoHotLinkAction. This executes
	/// an OS-level routine which tries to interpret the hyperlink. To make this work there
	/// must be a registry entry (created by the installer, but developers must do it by hand).
	/// The key HKEY_CLASSES_ROOT\silfw\shell\open\command must contain as its default value
	/// a string which is the path to FieldWorks.exe in quotes, followed by " %1". For example,
	///
	///     "C:\ww\Output\Debug\FieldWorks.exe" %1
	///
	/// In addition, the silfw key must have a default value of "URL:SILFW Protocol" and another
	/// string value called "URL Protocol" (no value). (Don't ask me why...Alistair may know.)
	///
	/// This initially results in a new instance of FieldWorks being started up with the URL
	/// as the command line. FieldWorks.Main() reconstitutes the FwAppArgs from the command-line
	/// data, and after a few checks passes the arguments to LaunchProject. If there isn't
	/// already an instance of FieldWorks running on that project, the new instance starts up
	/// on the appropriate app and project. The FwAppArgs created from the command line, with
	/// the tool and object, are passed to the application when created (currently only
	/// FwXApp does something with the passed-in FwAppArgs).
	///
	/// If there is already an instance running, this is detected in TryFindExistingProcess,
	/// which uses inter-process communication to invoke HandleOpenProjectRequest on each
	/// running instance of FieldWorks. This method is implemented in RemoteRequest,
	/// and if the project matches calls FieldWorks.KickOffAppFromOtherProcess. This takes
	/// various paths depending on which app is currently running. It often ends up activating
	/// a window and calling app.HandleIncomingLink() passing the FwAppArgs. This method,
	/// currently only implemented in FwXApp, activates the right tool and object.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	[Serializable]
	public class FwLinkArgs
	{
		#region Constants
		/// <summary>Internet Access Protocol identifier that indicates that this is a FieldWorks link</summary>
		public const string kSilScheme = "silfw";
		/// <summary>Indicates that this link should be handled by the local computer</summary>
		public const string kLocalHost = "localhost";
		/// <summary>Command-line argument: URL string for a FieldWorks link</summary>
		public const string kLink = "link";
		/// <summary>Command-line argument: App-specific tool name for a FieldWorks link</summary>
		public const string kTool = "tool";
		/// <summary>Command-line argument: FDO object guid for a FieldWorks link</summary>
		public const string kGuid = "guid";
		/// <summary>Command-line argument: FDO object field tag for a FieldWorks link</summary>
		public const string kTag = "tag";
		/// <summary>Fieldworks link prefix</summary>
		public const string kFwUrlPrefix = kSilScheme + "://" + kLocalHost + "/" + kLink + "?";
		#endregion

		#region Member variables
		/// <summary></summary>
		protected string _toolName = string.Empty;
		/// <summary></summary>
		protected string _tag = string.Empty;
		private readonly List<Property> _propertyTableEntries = new List<Property>();

		protected string _guid;
		#endregion

		#region Properties
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// The name/path of the tool or view within the specific application. Will never be null.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string ToolName
		{
			get
			{
				if (_toolName == null)
					return "";
				return _toolName;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// The GUID of the object which is the target of this link.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public Guid TargetGuid
		{
			get
			{
				if (_guid == null)
					return Guid.Empty;
				Guid guid;
				if (Guid.TryParse(_guid, out guid))
					return guid;
				return Guid.NewGuid(); // For tests that don't use a real guid.
			}
			protected set { _guid = value.ToString().ToLowerInvariant(); }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Additional information to be included in the property table. Will never be null.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal List<Property> PropertyTableEntries
		{
			get { return _propertyTableEntries; }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// An additional tag to differentiate between other FwLinkArgs entries between the
		/// same core ApplicationName, Database, Guid values. Will never be null.
		/// (cf. LT-7847)
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string Tag
		{
			get
			{
				Debug.Assert(_tag != null);
				return _tag;
			}
		}
		#endregion  Properties

		#region Construction and Initialization

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see>
		///                                     <cref>T:FwLinkArgs</cref>
		///                                   </see> class.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected FwLinkArgs()
		{
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see>
		///                                     <cref>T:FwLinkArgs</cref>
		///                                   </see> class.
		/// </summary>
		/// <param name="toolName">Name/path of the tool or view within the specific application.</param>
		/// <param name="targetGuid">The GUID of the object which is the target of this link.</param>
		/// ------------------------------------------------------------------------------------
		public FwLinkArgs(string toolName, string targetGuid)
			: this(toolName, targetGuid, null)
		{
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see>
		///                                     <cref>T:FwLinkArgs</cref>
		///                                   </see> class.
		/// </summary>
		/// <param name="toolName">Name/path of the tool or view within the specific application.</param>
		/// <param name="targetGuid">The GUID of the object which is the target of this link.</param>
		/// <param name="tag">The tag.</param>
		/// ------------------------------------------------------------------------------------
		public FwLinkArgs(string toolName, string targetGuid, string tag)
			: this()
		{
			_toolName = toolName;
			_guid= targetGuid;
			_tag = tag ?? string.Empty;
		}

		#endregion

		#region overridden methods and helpers
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Some comparisons don't care about the content of the property table, so we provide a
		/// method similar to Equals but not quite as demanding.
		/// </summary>
		/// <param name="lnk">The link to compare.</param>
		/// ------------------------------------------------------------------------------------
		public virtual bool EssentiallyEquals(FwLinkArgs lnk)
		{
			if (lnk == null)
				return false;
			if (lnk == this)
				return true;
			if (lnk.ToolName != ToolName)
				return false;
			if (lnk.TargetGuid != TargetGuid)
				return false;
			// tag is optional, but if a tool uses it with content, it should consistently provide content.
			// therefore if one of these links does not have content, and the other does then
			// we'll assume they refer to the same link
			// (one link simply comes from a control with more knowledge then the other one)
			return lnk.Tag.Length == 0 || Tag.Length == 0 || lnk.Tag == Tag;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object"/> to compare with this instance.</param>
		/// <returns>
		/// 	<c>true</c> if the specified <see cref="T:System.Object"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="T:System.NullReferenceException">
		/// The <paramref name="obj"/> parameter is null.
		/// </exception>
		/// ------------------------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			FwLinkArgs link = obj as FwLinkArgs;
			if (link == null)
				return false;
			if (link == this)
				return true;
			//just compare the URLs
			return (ToString() == link.ToString());
		}

		public void AddProperty(string name, string val)
		{
			PropertyTableEntries.Add(new Property {Name=name, Value=val});
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Get a URL corresponding to this link
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents this instance.
		/// </returns>
		/// ------------------------------------------------------------------------------------
		public override string ToString()
		{
			UriBuilder uriBuilder = new UriBuilder(kSilScheme, kLocalHost);
			uriBuilder.Path = kLink;
			StringBuilder query = new StringBuilder();
			AddProperties(query);

			foreach (Property property in PropertyTableEntries)
				query.AppendFormat("&{0}={1}", property.Name, Encode(property.Value));

			//make it safe to represent as a url string (e.g., convert spaces)
			uriBuilder.Query = HttpUtilityFromMono.UrlEncode(query.ToString());

			return uriBuilder.Uri.AbsoluteUri;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds the properties as named arguments in a format that can be used to produce a
		/// URI query.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected virtual void AddProperties(StringBuilder bldr)
		{
			bldr.AppendFormat("{0}={1}&{2}={3}&{4}={5}", kTool, ToolName, kGuid,
				(TargetGuid == Guid.Empty) ? "null" : TargetGuid.ToString(), kTag, Tag);
		}
		#endregion

		#region Serialization
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Add type info to the parameter if it's not a string
		/// </summary>
		/// <param name="o">The o.</param>
		/// ------------------------------------------------------------------------------------
		protected string Encode(object o)
		{
			switch(o.GetType().ToString())
			{
				default: throw new ArgumentException("Don't know how to serialize type of " + o.GetType());
				case "System.Boolean":
					return "bool:" + o;
				case "System.String":
					return (String)o;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Use the explicit type tag to parse parameters into the right type
		/// </summary>
		/// <param name="value">The value.</param>
		/// ------------------------------------------------------------------------------------
		protected object Decode(string value)
		{
			if(value.IndexOf("bool:") > -1)
			{
				value = value.Substring(5);
				return bool.Parse(value);
			}
			return HttpUtilityFromMono.UrlDecode(value);
		}
		#endregion
	}
	#endregion

	#region FwAppArgs class
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Class representing the arguments necessary for starting up a FieldWorks application (from a URI).
	/// See the class comment on FwLinkArgs for details on how all the parts of hyperlinking work.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	[Serializable]
	public class FwAppArgs : FwLinkArgs
	{
		#region Command-line switch constants
		/// <summary>Command-line argument: The application to start (te or flex)</summary>
		private const string kApp = "app";
		/// <summary>Command-line argument: The project name (or file)</summary>
		private const string kProject = "db";
		/// <summary>URI argument: The project name (or file)</summary>
		private const string kProjectUri = "database";
		/// <summary>Command-line argument: The server name</summary>
		private const string kServer = "s";
		/// <summary>URI argument: The server name</summary>
		private const string kServerUri = "server";
		#endregion

		#region Member variables
		private string _database = string.Empty;
		private string _server = string.Empty;
		private string _appName = string.Empty;
		private string _appAbbrev = string.Empty;
		private string _dbType = string.Empty;
		private string _locale = string.Empty;
		private string _configFile = string.Empty;
		private string _backupFile = string.Empty;
		private string _restoreOptions = string.Empty;
		private string _chooseProjectFile = string.Empty;
		#endregion

		#region Properties
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the database/project name (possibly/probably a file path/name.
		/// Will never return null.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string Database
		{
			get
			{
				Debug.Assert(_database != null);
				return _database;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the server name (for a remote project).
		/// Will never return null.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string Server
		{
			get
			{
				Debug.Assert(_server != null);
				return _server;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the type of the database/backend. (e.g. XML, MySql, etc.) Will never return null.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string DatabaseType
		{
			get
			{
				Debug.Assert(_dbType != null);
				return _dbType;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the lowercase application name (either "translation editor" or "language
		/// explorer"). Will never return null.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string AppName
		{
			get
			{
				Debug.Assert(_appName != null);
				return _appName;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the application abbreviation (either "te" or "flex"). Will never return null.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string AppAbbrev
		{
			get
			{
				Debug.Assert(_appAbbrev != null);
				return _appAbbrev;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the locale for the user interface.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string Locale
		{
			get
			{
				Debug.Assert(_locale != null);
				return _locale;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Flag indicating whether to show help (if true, all other arguments can be ignored)
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool ShowHelp { get; private set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Flag indicating whether or not to hide the UI.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool NoUserInterface { get; private set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Flag indicating whether or not to run in a server mode for other applications.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool AppServerMode { get; private set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the config file. Will never return null.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string ConfigFile
		{
			get
			{
				Debug.Assert(_configFile != null);
				return _configFile;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the backup file used for a restore. Will never return null.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string BackupFile
		{
			get
			{
				Debug.Assert(_backupFile != null);
				return _backupFile;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the restore options user chose for creating RestoreProjectSettings. Will
		/// never return null.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string RestoreOptions
		{
			get
			{
				Debug.Assert(_restoreOptions != null);
				return _restoreOptions;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a value indicating whether this FwAppArgs also contains information pertaining
		/// to a link request.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool HasLinkInformation
		{
			get
			{
				bool hasInfo = !string.IsNullOrEmpty(ToolName);
				return hasInfo;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the full path of the file that the handle for a chosen project will be
		/// written to.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string ChooseProjectFile
		{
			get { return _chooseProjectFile; }
		}
		#endregion

		#region Constructor
		// NOTE: Don't make constructor overloads that take only varying numbers of string
		// parameters. That would conflict with the string params constructor causing a bunch
		// of stuff to break (which, thankfully, we have tests for)

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see>
		///                                     <cref>T:FwAppArgs</cref>
		///                                   </see> class.
		/// </summary>
		/// <param name="applicationNameOrAbbrev">Name or abbreviation of the application.</param>
		/// <param name="database">The name of the database.</param>
		/// <param name="server">The server (or null for a local database).</param>
		/// <param name="toolName">Name/path of the tool or view within the specific application.</param>
		/// <param name="targetGuid">The GUID of the object which is the target of this link.</param>
		/// ------------------------------------------------------------------------------------
		public FwAppArgs(string applicationNameOrAbbrev, string database, string server,
			string toolName, string targetGuid) : base(toolName, targetGuid)
		{
			ProcessArg(kApp, applicationNameOrAbbrev);
			ProcessArg(kProject, database);
			if (!string.IsNullOrEmpty(server))
				ProcessArg(kServer, server);
		}


		#endregion

		#region Overridden methods
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds the properties as named arguments in a format that can be used to produce a
		/// URI query.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override void AddProperties(StringBuilder bldr)
		{
			bldr.AppendFormat("{0}={1}&{2}={3}&{4}={5}&", kApp, AppAbbrev, kProjectUri,
				Database, kServerUri, Server);
			base.AddProperties(bldr);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Some comparisons don't care about the content of the property table, so we provide a
		/// method similar to Equals but not quite as demanding.
		/// </summary>
		/// <param name="lnk">The link to compare.</param>
		/// ------------------------------------------------------------------------------------
		public override bool EssentiallyEquals(FwLinkArgs lnk)
		{
			var appArgs = lnk as FwAppArgs;
			if (appArgs == null || !base.EssentiallyEquals(lnk))
				return false;
			return (appArgs.AppAbbrev == AppAbbrev && appArgs.Database == Database &&
				appArgs.Server == Server);
		}
		#endregion

		#region Public methods
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Clears any link information from this FwAppArgs.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void ClearLinkInformation()
		{
			_tag = string.Empty;
			_toolName = string.Empty;
			_guid = Guid.Empty.ToString().ToLowerInvariant();
		}
		#endregion

		#region Private helper methods

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Processes a URL argument represented by the given name and value pair.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void ProcessArg(string name, string value)
		{
			switch (name)
			{
				case kProject: _database = value; break;
				case kServer: _server = value; break;
				case kApp: SetAppNameAndAbbrev(value); break;
			}
		}

		/// <summary>
		/// The name of the Translation Editor folder (Even though this is the same as
		/// DirectoryFinder.ksTeFolderName and FwSubKey.TE, PLEASE do not use them interchangeably.
		/// Use the one that is correct for your context, in case they need to be changed later.)
		/// </summary>
		public const string ksTeAppName = "Translation Editor";
		/// <summary>The command-line abbreviation for Translation Editor</summary>
		public const string ksTeAbbrev = "TE";
		/// <summary>
		/// The name of the Language Explorer folder (Even though this is the same as
		/// DirectoryFinder.ksFlexFolderName and FwSubKey.LexText, PLEASE do not use them interchangeably.
		/// Use the one that is correct for your context, in case they need to be changed later.)
		/// </summary>
		public const string ksFlexAppName = "Language Explorer";
		/// <summary>The command-line abbreviation for the Language Explorer</summary>
		public const string ksFlexAbbrev = "FLEx";
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the application name and abbreviation.
		/// </summary>
		/// <param name="value">The URL or command-line parameter value that is supposed to
		/// match one of the know FW application names or abbreviations.</param>
		/// ------------------------------------------------------------------------------------
		private void SetAppNameAndAbbrev(string value)
		{
			var appNameOrAbbrev = value.ToLowerInvariant();
			if (appNameOrAbbrev == ksTeAppName.ToLowerInvariant() ||
				appNameOrAbbrev == ksTeAbbrev.ToLowerInvariant())
			{
				_appName = ksTeAppName.ToLowerInvariant();
				_appAbbrev = ksTeAbbrev.ToLowerInvariant();
			}
			else if (appNameOrAbbrev == ksFlexAppName.ToLowerInvariant() ||
				appNameOrAbbrev == ksFlexAbbrev.ToLowerInvariant())
			{
				_appName = ksFlexAppName.ToLowerInvariant();
				_appAbbrev = ksFlexAbbrev.ToLowerInvariant();
			}
			else
				ShowHelp = true;
		}
		#endregion
	}
	#endregion

	internal class Property
	{
		public string Name;
		public object Value;
	}
}
