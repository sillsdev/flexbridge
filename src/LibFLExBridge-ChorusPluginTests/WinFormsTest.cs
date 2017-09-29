using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace LibFLExBridgeChorusPluginTests
{
	[TestFixture]
	public class WinFormsTest
	{
		[Test]
		public void TestForAbsenceOfWindowsForms()
		{
			// See: https://stackoverflow.com/questions/8499593/c-sharp-how-to-check-if-namespace-class-or-method-exists-in-c
			var formsNamespace = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
						from type in assembly.GetTypes()
						where assembly.FullName.Contains("Lib")
						where type.Namespace == "System.Windows.Forms"
						select type);
			Assert.IsEmpty(formsNamespace, "LibFlexBridge-ChorusPlugin should not reference Windows.Forms. See Readme.md");
		}
	}
}
