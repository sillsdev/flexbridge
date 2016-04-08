// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.IO;
using Chorus;
using Chorus.sync;
using LibTriboroughBridgeChorusPlugin;
using System.ComponentModel.Composition;

namespace LibFLExBridgeChorusPlugin.Infrastructure.ActionHandlers
{
	/// <summary>
	/// Implement send/receive functionality. The caller needs to provide a syncFunc that does the
	/// actual sending/receiving, e.g. by calling the appropriate Chorus functions.
	/// </summary>
	[Export]
	class SendReceiveAction
	{
		[Import]
		private IChorusSystem ChorusSystem { get; set; }

		[Import]
		private Func<IChorusSystem, SyncResults> SyncFunc { get; set; }

		public SyncResults Run(string fullProjectPath, string user)
		{
			var projectDir = Path.GetDirectoryName(fullProjectPath);
			var projectName = Path.GetFileNameWithoutExtension(fullProjectPath);

			ChorusSystem.Init(projectDir, user);

			FlexFolderSystem.ConfigureChorusProjectFolder(ChorusSystem.ProjectFolderConfiguration);

			var newlyCreated = false;
			if (ChorusSystem.Repository.Identifier == null)
			{
				// Write an empty custom prop file to get something in the default branch at rev 0.
				// The custom prop file will always exist and can be empty, so start it as empty (null).
				// This basic rev 0 commit will then allow for a roll back if the soon to follow main commit fails on a validation problem.
				FileWriterService.WriteCustomPropertyFile(Path.Combine(projectDir,
					FlexBridgeConstants.CustomPropertiesFilename), null);
				ChorusSystem.Repository.AddAndCheckinFile(Path.Combine(projectDir,
					FlexBridgeConstants.CustomPropertiesFilename));
				newlyCreated = true;
			}
			ChorusSystem.EnsureAllNotesRepositoriesLoaded();

			// Add the 'lock' file to keep FW apps from starting up at such an inopportune moment.
			var lockPathname = Path.Combine(projectDir, projectName + SharedConstants.FwXmlLockExtension);
			try
			{
				File.WriteAllText(lockPathname, string.Empty);

				// Do the Chorus business.
				var result = SyncFunc(ChorusSystem);
				if (!result.Cancelled)
				{
					if (newlyCreated && (!result.Succeeded || result.ErrorEncountered != null))
					{
						// Wipe out new repo, since something bad happened in S/R,
						// and we don't want to leave the user in a sad state (cf. LT-14751, LT-14957).
						BackOutOfRepoCreation(projectDir);
					}
					return result;
				}

				// User probably bailed out of S/R using the "X" to close the dlg.
				if (newlyCreated)
				{
					// Wipe out new repo, since the user cancelled without even trying the S/R,
					// and we don't want to leave the user in a sad state (cf. LT-14751, LT-14957).
					BackOutOfRepoCreation(projectDir);
				}
				return result;
			}
			finally
			{
				if (File.Exists(lockPathname))
					File.Delete(lockPathname);
			}
		}

		/// <summary>Removes .hg repo and other files and folders created by S/R Project</summary>
		/// <remarks>Directory.Delete throws if the directory does not exist; File.Delete does not.</remarks>
		private static void BackOutOfRepoCreation(string projectDir)
		{
			foreach (var subDir in new[] {".hg", "Anthropology", "General", "Linguistics"})
			{
				var fullPath = Path.Combine(projectDir, subDir);
				if (Directory.Exists(fullPath))
					Directory.Delete(fullPath, true);
			}
			File.Delete(Path.Combine(projectDir, "FLExProject.CustomProperties"));
			File.Delete(Path.Combine(projectDir, "FLExProject.ModelVersion"));
		}

	}
}

