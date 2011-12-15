using Microsoft.Win32;

namespace FLEx_ChorusPlugin.Infrastructure
{
	internal static class FieldWorksProjectServices
	{
		internal static string ProjectsPath
		{
			get
			{
// ReSharper disable PossibleNullReferenceException
				return (string)Registry
								.LocalMachine
								.OpenSubKey("software")
								.OpenSubKey("SIL")
								.OpenSubKey("FieldWorks")
								.OpenSubKey("7.0")
								.GetValue("ProjectsDir");
// ReSharper restore PossibleNullReferenceException
			}
		}
	}
}