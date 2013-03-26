using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Chorus;
using Chorus.VcsDrivers.Mercurial;
using Chorus.sync;
using Microsoft.Win32;
using Palaso.Progress;

namespace TriboroughBridge_ChorusPlugin
{
	public static class Utilities
	{
		public const string FailureFilename = "FLExImportFailure.notice";
		public const string FwXmlExtension = "." + FwXmlExtensionNoPeriod;
		public const string FwXmlExtensionNoPeriod = "fwdata";
		public const string FwLockExtension = ".lock";
		public const string FwXmlLockExtension = FwXmlExtension + FwLockExtension;
		public const string FwDB4oExtension = "." + FwDB4oExtensionNoPeriod;
		public const string FwDB4oExtensionNoPeriod = "fwdb";
		public const string LiftExtension = ".lift";
		public const string FlexExtension = "_model_version";
		public const string LiftRangesExtension = ".lift-ranges";
		public const string OtherRepositories = "OtherRepositories";
		public const string LIFT = "LIFT";
		private static string _testingProjectsPath;
		private const string TipKey = "+_tip_+";

		internal static void SetProjectsPathForTests(string testProjectsPath)
		{
			_testingProjectsPath = testProjectsPath;
		}

		/// <summary>
		/// Strips file URI prefix from the beginning of a file URI string, and keeps
		/// a beginning slash if in Linux.
		/// eg "file:///C:/Windows" becomes "C:/Windows" in Windows, and
		/// "file:///usr/bin" becomes "/usr/bin" in Linux.
		/// Returns the input unchanged if it does not begin with "file:".
		///
		/// Does not convert the result into a valid path or a path using current platform
		/// path separators.
		/// fileString does not neet to be a valid URI. We would like to treat it as one
		/// but since we import files with file URIs that can be produced by other
		/// tools (eg LIFT) we can't guarantee that they will always be valid.
		///
		/// File URIs, and their conversation to paths, are more complex, with hosts,
		/// forward slashes, and escapes, but just stripping the file URI prefix is
		/// what's currently needed.
		/// Different places in code need "file://', or "file:///" removed.
		///
		/// See uri.LocalPath, http://en.wikipedia.org/wiki/File_URI , and
		/// http://blogs.msdn.com/b/ie/archive/2006/12/06/file-uris-in-windows.aspx .
		/// </summary>
		public static string StripFilePrefix(string fileString)
		{
			if (String.IsNullOrEmpty(fileString))
				return fileString;

			var prefix = Uri.UriSchemeFile + ":";

			if (!fileString.StartsWith(prefix))
				return fileString;

			var path = fileString.Substring(prefix.Length);
			// Trim any number of beginning slashes
			path = path.TrimStart('/');
			// Prepend slash on Linux
			if (IsUnix)
				path = '/' + path;

			return path;
		}

		public static XElement CreateFromBytes(byte[] xmlData)
		{
			using (var memStream = new MemoryStream(xmlData))
			{
				// This loads the MemoryStream as Utf8 xml. (I checked.)
				return XElement.Load(memStream);
			}
		}

		/// <summary>
		/// Returns <c>true</c> if we're running on Unix, otherwise <c>false</c>.
		/// </summary>
		public static bool IsUnix
		{
			get { return Environment.OSVersion.Platform == PlatformID.Unix; }
		}

		/// <summary>
		/// Creates and initializes the ChorusSystem for use in FLExBridge
		/// </summary>
		public static ChorusSystem InitializeChorusSystem(string directoryName, string user, Action<ProjectFolderConfiguration> configure)
		{
			var system = new ChorusSystem(directoryName);
			system.Init(user);
			if (configure != null)
				configure(system.ProjectFolderConfiguration);
			return system;
		}

		public static string HgDataFolder(string path)
		{
			return Path.Combine(path, BridgeTrafficCop.hg, "store", "data");
		}

		public static string LiftOffset(string path)
		{
			return Path.Combine(path, OtherRepositories, Path.GetFileName(path) + "_" + LIFT);
		}

		public static string ProjectsPath
		{
			get
			{
				if (_testingProjectsPath != null)
					return _testingProjectsPath;

				var rootDir = ((string) Registry.LocalMachine
												.OpenSubKey("software")
												.OpenSubKey("SIL")
												.OpenSubKey("FieldWorks")
												.OpenSubKey("7.0")
												.GetValue("ProjectsDir")).Trim();
				if (rootDir.Length > 3)
					rootDir = rootDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				return rootDir;
			}
		}

		public static void MakeLocalClone(string sourceFolder, string targetFolder)
		{
			var parentFolder = Directory.GetParent(targetFolder).FullName;
			if (!Directory.Exists(parentFolder))
				Directory.CreateDirectory(parentFolder);

			// Do a clone of the lift repo into the new home.
			var oldRepo = new HgRepository(sourceFolder, new NullProgress());
			oldRepo.CloneLocalWithoutUpdate(targetFolder);

			// Now copy the original hgrc file into the new location.
			File.Copy(Path.Combine(sourceFolder, BridgeTrafficCop.hg, "hgrc"), Path.Combine(targetFolder, BridgeTrafficCop.hg, "hgrc"), true);

			// Move the import failure notification file, if it exists.
			var roadblock = Path.Combine(sourceFolder, FailureFilename);
			if (File.Exists(roadblock))
				File.Copy(roadblock, Path.Combine(targetFolder, FailureFilename), true);

			var newRepo = new HgRepository(targetFolder, new NullProgress());
			newRepo.Update();
		}

		public static bool FolderIsEmpty(string folder)
		{
			return Directory.GetDirectories(folder).Length == 0 && Directory.GetFiles(folder).Length == 0;
		}

		public static bool UpdateToDesiredBranchHead(string repoPath, string desiredBranch)
		{
			bool desiredIsAtTip;
			var desiredRevision = GetDesiredBranchHead(repoPath, desiredBranch, out desiredIsAtTip);

			if (desiredRevision == null)
				return false;

			if (desiredIsAtTip)
				return true;

			try
			{
				var repo = new HgRepository(repoPath, new NullProgress());
				repo.Update(desiredRevision.Number.LocalRevisionNumber);
			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}

		public static Revision GetDesiredBranchHead(string repoPath, string desiredBranch, out bool desiredIsAtTip)
		{
			Revision desiredRevision;
			var allBranchHeads = CollectAllNewestBranchHeads(repoPath);
			allBranchHeads.TryGetValue(desiredBranch, out desiredRevision);

			desiredIsAtTip = desiredRevision != null && desiredRevision.Number.Hash == allBranchHeads[TipKey].Number.Hash;

			return desiredRevision; // Will be null, if it is not in the repo.
		}

		private static Dictionary<string, Revision> CollectAllNewestBranchHeads(string repoPath)
		{
			var retval = new Dictionary<string, Revision>();

			var repo = new HgRepository(repoPath, new NullProgress());
			var tip = repo.GetTip();
			retval.Add(TipKey, tip);
			foreach (var head in repo.GetHeads())
			{
				var branch = head.Branch;
				if (retval.ContainsKey(branch))
				{
					// Use the higher rev number since it has more than one head of the same branch.
					var extantRevNumber = Int32.Parse(retval[branch].Number.LocalRevisionNumber);
					var currentRevNumber = Int32.Parse(head.Number.LocalRevisionNumber);
					if (currentRevNumber > extantRevNumber)
					{
						// Use the newer head of a branch.
						retval[branch] = head;
					}
				}
				else
				{
					// New branch, so add it.
					retval.Add(branch, head);
				}
			}

			return retval;
		}
	}
}
