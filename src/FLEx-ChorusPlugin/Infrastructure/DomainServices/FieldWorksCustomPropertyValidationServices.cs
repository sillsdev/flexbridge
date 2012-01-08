using System;
using System.Linq;
using System.Xml.Linq;
using Palaso.IO;

namespace FLEx_ChorusPlugin.Infrastructure.DomainServices
{
	/// <summary>
	/// Handle the FieldWorks custom properties file
	/// </summary>
	internal static class FieldWorksCustomPropertyValidationServices
	{
		private const string Extension = "CustomProperties";

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
				var doc = XDocument.Load(pathToFile);
				var root = doc.Root;
				if (root.Name.LocalName != SharedConstants.OptionalFirstElementTag || root.Elements("CustomField").Count() == 0)
					return "Not valid custom properties file";

				return null;
			}
			catch (Exception e)
			{
				return e.Message;
			}
		}
	}
}
