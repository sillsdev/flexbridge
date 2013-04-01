using System;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace TheTurtle
{
	internal static class TheTurtleUtilities
	{
		public static string ProjectsPath
		{
			get
			{
				using (var hkcu = Registry.CurrentUser)
				{
					var hkcuPath = GetFwPath(hkcu, "ProjectsDir");
					if (hkcuPath != null)
						return hkcuPath;
				}

				using (var hklm = Registry.LocalMachine)
				{
					return GetFwPath(hklm, "ProjectsDir");
				}
			}
		}

		public static string FwAssemblyPath
		{
			get
			{
				using (var hkcu = Registry.CurrentUser)
				{
					var hkcuPath = GetFwPath(hkcu, "FwExeDir");
					if (hkcuPath != null)
						return GetDevOffset(hkcuPath);
					hkcuPath = GetFwPath(hkcu, "RootCodeDir");
					if (hkcuPath != null)
						return GetDevOffset(hkcuPath);
				}

				using (var hklm = Registry.LocalMachine)
				{
					var hklmPath = GetFwPath(hklm, "FwExeDir");
					if (hklmPath != null)
						return GetDevOffset(hklmPath);
					return GetDevOffset(GetFwPath(hklm, "RootCodeDir"));
				}
			}
		}

		private static string GetDevOffset(string path)
		{
			if (path.EndsWith("DistFiles"))
			{
				var baseDir = Path.GetDirectoryName(path);
				var fixitExe = Directory.GetFiles(Path.Combine(baseDir, "Output"), "FixFwData.exe", SearchOption.AllDirectories).First();
				return Path.GetDirectoryName(fixitExe);
			}
			return path;
		}

		private static string GetFwPath(RegistryKey registryKey, string property)
		{
			using (var softwareSubKey = registryKey.OpenSubKey("Software"))
			{
				if (softwareSubKey == null)
					return null;
				using (var silKey = softwareSubKey.OpenSubKey("SIL"))
				{
					if (silKey == null)
						return null;
					using (var fwKey = silKey.OpenSubKey("FieldWorks"))
					{
						if (fwKey == null)
							return null;
						using (var version8Key = fwKey.OpenSubKey("8"))
						{
							if (version8Key == null)
							{
								using (var version7Key = fwKey.OpenSubKey("7.0"))
								{
									return version7Key == null ? null : GetValue(version7Key, property);
								}
							}
							return GetValue(version8Key, property);
						}
					}
				}
			}
		}

		private static string GetValue(RegistryKey registryKey, string value)
		{
			var result = registryKey.GetValue(value) as string;
			if (result == null || result.Trim() == String.Empty)
				return null;
			if (result.Length > 3)
				result = result.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			return result;
		}
	}
}
