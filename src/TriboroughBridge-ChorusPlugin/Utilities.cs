using System;
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
			configure(system.ProjectFolderConfiguration);
			return system;
		}

		public static string HgDataFolder(string path)
		{
			return Path.Combine(path, ".hg", "store", "data");
		}

		public static string LiftOffset(string path)
		{
			return Path.Combine(path, "OtherRepositories", "LIFT");
		}

		/// <summary>
		/// Move a repository (.hg folder and local workspace) from one location to another, even to another device).
		/// </summary>
		/// <remarks>After the move, the original location will not exist.</remarks>
		public static void MakeLocalCloneAndRemoveSourceParentFolder(string sourceFolder, string targetFolder)
		{
			MakeLocalClone(sourceFolder, targetFolder);

			Directory.Delete(Directory.GetParent(sourceFolder).FullName, true);
		}

		public static string ProjectsPath
		{
			get
			{
				return (string)Registry.LocalMachine
								   .OpenSubKey("software")
								   .OpenSubKey("SIL")
								   .OpenSubKey("FieldWorks")
								   .OpenSubKey("7.0")
								   .GetValue("ProjectsDir");
			}
		}

		public static void MakeLocalClone(string sourceFolder, string targetFolder)
		{
			if (!Directory.Exists(targetFolder))
				Directory.CreateDirectory(targetFolder);

			// Do a clone of the lift repo into the new home.
			var oldRepo = new HgRepository(sourceFolder, new NullProgress());
			oldRepo.CloneLocalWithoutUpdate(targetFolder);
			// Now copy the original hgrc file into the new location.
			File.Copy(Path.Combine(sourceFolder, ".hg", "hgrc"), Path.Combine(targetFolder, ".hg", "hgrc"), true);
			var newRepo = new HgRepository(targetFolder, new NullProgress());
			newRepo.Update();

			// Move the import failure notification file
			var roadblock = Path.Combine(sourceFolder, FailureFilename);
			if (File.Exists(roadblock))
				File.Copy(roadblock, Path.Combine(targetFolder, FailureFilename), true);
		}
	}
}
