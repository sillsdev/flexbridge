using System;
using System.IO;

namespace FieldWorksBridge.Model
{
	/// <summary>
	/// LanguageProject represents a FieldWorks language project
	/// (but is not the same class as is used in FielwdWorks).
	///
	/// A LanguageProject may, or may not, be enabled for remote collaboration use.
	/// If it is not ready for such use, it can be made ready.
	///
	/// The expectation is that the main data file to be used will have an extension of 'fwdata'
	/// and will be an xml file.
	/// </summary>
	internal sealed class LanguageProject
	{
		private readonly string _fwdataFile;

		internal LanguageProject(string fwdataFile)
		{
			if (string.IsNullOrEmpty(fwdataFile))
				throw new ArgumentNullException("fwdataFile");
			if (!File.Exists(fwdataFile))
				throw new FileNotFoundException("Cannot find the file.", fwdataFile);

			_fwdataFile = fwdataFile;
		}

		internal string Name
		{
			get { return Path.GetFileNameWithoutExtension(_fwdataFile); }
		}

		public bool IsRemoteCollaborationEnabled
		{
			get
			{
				return Directory.Exists(Path.Combine(Path.GetDirectoryName(_fwdataFile), ".hg"));
			}
		}
	}
}
