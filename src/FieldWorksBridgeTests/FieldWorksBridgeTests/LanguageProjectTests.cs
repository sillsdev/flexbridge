using System;
using System.IO;
using FieldWorksBridge.Model;
using NUnit.Framework;

namespace FieldWorksBridgeTests
{
	/// <summary>
	/// Test the LanguageProject class.
	/// </summary>
	[TestFixture]
	public class LanguageProjectTests
	{
		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void NullPathnameThrows()
		{
			new LanguageProject(null);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void EmptyPathnameThrows()
		{
			new LanguageProject(string.Empty);
		}

		[Test, ExpectedException(typeof(FileNotFoundException))]
		public void NonExistantFileThrows()
		{
			new LanguageProject("NobodyHome");
		}
	}
}