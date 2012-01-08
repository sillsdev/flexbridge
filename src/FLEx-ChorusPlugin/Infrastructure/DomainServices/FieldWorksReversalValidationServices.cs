using System;
using System.Xml;
using Palaso.IO;

namespace FLEx_ChorusPlugin.Infrastructure.DomainServices
{
	///<summary>
	/// Handler for '.reversal' extension for FieldWorks reversal files.
	///</summary>
	internal static class FieldWorksReversalValidationServices
	{
		private const string Extension = "reversal";

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
				var settings = new XmlReaderSettings { ValidationType = ValidationType.None };
				using (var reader = XmlReader.Create(pathToFile, settings))
				{
					reader.MoveToContent();
					if (reader.LocalName == "Reversal")
					{
						// It would be nice, if it could really validate it.
						while (reader.Read())
						{
						}
					}
					else
					{
						throw new InvalidOperationException("Not a FieldWorks reversal file.");
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
