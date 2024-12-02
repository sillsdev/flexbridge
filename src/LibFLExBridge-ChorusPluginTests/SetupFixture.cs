using L10NSharp;
using NUnit.Framework;

namespace LibFLExBridgeChorusPluginTests
{
	[SetUpFixture]
	public class SetupFixture
	{
		[OneTimeSetUp]
		public void RunBeforeAnyTests()
		{
			LocalizationManager.StrictInitializationMode = false;
		}
	}
}
