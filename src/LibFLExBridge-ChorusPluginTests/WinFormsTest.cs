using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace LibFLExBridgeChorusPluginTests
{
	[TestFixture]
	public class WinFormsTest
	{
		[Test]
		public void TestForAbsenceOfWindowsForms()
		{
			// See: https://stackoverflow.com/questions/2241961/how-to-get-all-types-in-a-referenced-assembly
			var formsNamespaceAssembly = Assembly.GetExecutingAssembly().GetReferencedAssemblies().
				Where(assemblyName => assemblyName.FullName.Contains("Windows.Forms")).ToList();
			Assert.IsEmpty(formsNamespaceAssembly, "LibFlexBridge-ChorusPlugin should not reference Windows.Forms. See Readme.md");
		}
	}
}
