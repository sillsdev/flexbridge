using System;
using System.Collections.Generic;
using System.IO;
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
		public const string LiftRangesExtension = ".lift-ranges";
		public const string OtherRepositories = "OtherRepositories";
		public const string LIFT = "LIFT";
		private static Dictionary<string, string> _extantRepoIdentifiers;
		private static string _testingProjectsPath = null;

		public static Dictionary<string, string> ExtantRepoIdentifiers
		{
			get
			{
				if (_extantRepoIdentifiers == null)
					CacheExtantRepositoryIdentifiers(ProjectsPath);
				return _extantRepoIdentifiers;
			}
		}

		internal static void SetProjectsPathForTests(string testProjectsPath)
		{
			_testingProjectsPath = testProjectsPath;
		}

		internal static void ClearCacheForTests()
		{
			_extantRepoIdentifiers = null;
		}

		public static bool AlreadyHasLocalRepository(string fwProjectBaseDir, string repositoryLocation)
		{
			if (_extantRepoIdentifiers == null)
			{
				CacheExtantRepositoryIdentifiers(fwProjectBaseDir);
			}
			var repo = new HgRepository(repositoryLocation, new NullProgress());
			var identifier = repo.Identifier;

			// We don't really want to clone an empty repo (identifier == null).
			return identifier == null || _extantRepoIdentifiers.ContainsKey(identifier);
		}

		private static void CacheExtantRepositoryIdentifiers(string fwProjectBaseDir)
		{
			_extantRepoIdentifiers = new Dictionary<string, string>();

			foreach (var mainFwProjectFolder in Directory.GetDirectories(fwProjectBaseDir, "*", SearchOption.TopDirectoryOnly))
			{
				var hgfolder = Path.Combine(mainFwProjectFolder, BridgeTrafficCop.hg);
				if (Directory.Exists(hgfolder))
				{
					CheckForMatchingRepo(mainFwProjectFolder);
				}

				var otherRepoFolder = Path.Combine(mainFwProjectFolder, OtherRepositories);
				if (!Directory.Exists(otherRepoFolder))
					continue;

				foreach (var sharedFolder in Directory.GetDirectories(otherRepoFolder, "*", SearchOption.TopDirectoryOnly))
				{
					hgfolder = Path.Combine(sharedFolder, BridgeTrafficCop.hg);
					if (Directory.Exists(hgfolder))
					{
						CheckForMatchingRepo(sharedFolder);
					}
				}
			}
		}

		private static void CheckForMatchingRepo(string repoContainingFolder)
		{
			var repo = new HgRepository(repoContainingFolder, new NullProgress());
			var identifier = repo.Identifier;
			// Pathologically we may already have a duplicate. If so we can only record one name; just keep the last encountered.
			if (identifier != null)
				_extantRepoIdentifiers[identifier] = Path.GetFileName(repoContainingFolder);
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
			return Path.Combine(path, OtherRepositories, LIFT);
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
	}
}
