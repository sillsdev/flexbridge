using System;
using System.IO;
using Palaso.IO;

namespace FLEx_ChorusPlugin.Infrastructure.DomainServices
{
	/// <summary>
	/// Handle the FieldWorks model version file.
	/// </summary>
	/// <remarks>
	/// This file uses JSON data in the form: {"modelversion": #####}
	/// </remarks>
	internal static class FieldWorksModelVersionValidationServices
	{
		private const string Extension = "ModelVersion";

		internal static bool CanValidateFile(string pathToFile)
		{
			if (!FileUtils.CheckValidPathname(pathToFile, Extension))
				return false;

			return DoValidation(pathToFile) == null;
		}

		private static string DoValidation(string pathToFile)
		{
			try
			{
				// Uses JSON: {"modelversion": #####}
				var data = File.ReadAllText(pathToFile);
				var splitData = SplitData(data);
				if (splitData.Length == 2 && splitData[0] == "\"modelversion\"" && int.Parse(splitData[1].Trim()) >= 7000000)
					return null;
				return "Not a valid JSON model version file.";
			}
			catch (Exception e)
			{
				return e.Message;
			}
		}

		private static string[] SplitData(string data)
		{
			return data.Split(new[] { "{", ":", "}" }, StringSplitOptions.RemoveEmptyEntries);
		}
	}
}
