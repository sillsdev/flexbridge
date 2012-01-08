using System;
using System.IO;
using System.Xml;
using Palaso.IO;

namespace FLEx_ChorusPlugin.Infrastructure
{
	internal static class FieldWorksAnthropologyValidationServices
	{
		private const string Extension = "ntbk";

		internal static bool CanValidateFile(string pathToFile)
		{
			if (!FileUtils.CheckValidPathname(pathToFile, Extension))
				return false;

			if (Path.GetFileName(pathToFile) != "DataNotebook.ntbk")
				return false;

			return DoValidation(pathToFile) == null;
		}

		private static string DoValidation(string pathToFile)
		{
			try
			{
				var settings = new XmlReaderSettings { ValidationType = ValidationType.None };
				using (var reader = XmlReader.Create(pathToFile, settings))
				{
					reader.MoveToContent();
					if (reader.LocalName == "Anthropology")
					{
						// It would be nice, if it could really validate it.
						while (reader.Read())
						{
						}
					}
					else
					{
						throw new InvalidOperationException("Not a FieldWorks data notebook file.");
					}
				}
			}
			catch (Exception e)
			{
				return e.Message;
			}
			return null;
		}
	}
}