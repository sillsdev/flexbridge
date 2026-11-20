// Copyright (c) 2026 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.IO;

namespace LibFLExBridgeChorusPlugin.Infrastructure
{
	/// <summary>
	/// Finds the <see cref="MetadataCache"/> for a file being validated.
	/// </summary>
	/// <remarks>
	/// Chorus creates the file handler itself and only gives it a file path, so validation has to work out
	/// the project from the path. A project's metadata depends only on its .ModelVersion and .CustomProperties
	/// files, so the cache for the most recently used project is kept, along with the text of those two files.
	/// It is reused while the project folder and both files are unchanged, and rebuilt otherwise.
	/// Comparing file contents (not timestamps) keeps it right when a Send/Receive changes the files.
	/// Only one project is kept, so a long-lived process that handles many projects does not grow.
	/// </remarks>
	internal static class ProjectMetadataCaches
	{
		private static readonly object Lock = new object();
		private static string _projectFolder;
		private static string _modelVersionText;
		private static string _customPropertiesText;
		private static MetadataCache _cache;

		/// <summary>
		/// Get the cache for the project holding <paramref name="pathToFile"/>.
		/// A file that is not in a split project (no .ModelVersion file in its folder or above) gets a cache for the base model.
		/// </summary>
		/// <remarks>
		/// The returned cache may be shared with other callers, so callers must not change it
		/// (no upgrading, and no adding custom properties).
		/// </remarks>
		internal static MetadataCache ForFile(string pathToFile)
		{
			var projectFolder = MetadataCache.FindProjectFolder(pathToFile);
			if (projectFolder == null)
				return new MetadataCache();

			var modelVersionText = MetadataCache.ReadAllTextIfExists(Path.Combine(projectFolder, FlexBridgeConstants.ModelVersionFilename));
			var customPropertiesText = MetadataCache.ReadAllTextIfExists(Path.Combine(projectFolder, FlexBridgeConstants.CustomPropertiesFilename));
			lock (Lock)
			{
				if (_cache != null
					&& string.Equals(_projectFolder, projectFolder, StringComparison.OrdinalIgnoreCase)
					&& _modelVersionText == modelVersionText
					&& _customPropertiesText == customPropertiesText)
				{
					return _cache;
				}

				var cache = MetadataCache.LoadFromProjectFiles(modelVersionText, customPropertiesText);
				_projectFolder = projectFolder;
				_modelVersionText = modelVersionText;
				_customPropertiesText = customPropertiesText;
				_cache = cache;
				return cache;
			}
		}
	}
}
