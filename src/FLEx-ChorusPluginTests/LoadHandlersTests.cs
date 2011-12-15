using System.Collections.Generic;
using System.Linq;
using Chorus.FileTypeHanders;
using NUnit.Framework;

namespace FLEx_ChorusPluginTests
{
	[TestFixture]
	public class LoadHandlersTests
	{
		[Test]
		public void EnsureHandlersAreLoaded()
		{
			// As long as each individual handler is being tested, then this is not needed,
			// since the individual test gets the right handler from the collection.
			var handlerNames = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
							   select handler.GetType().Name).ToList();
			var expectedBridgeHandlers = new HashSet<string>
											{
												"FieldWorksCustomPropertyFileHandler",
												"FieldWorksModelVersionFileHandler",
												"FieldWorksFileHandler",
												"FieldWorksReversalTypeHandler"
											};
			foreach (var expectedBridgeHandler in expectedBridgeHandlers)
				Assert.IsTrue(handlerNames.Contains(expectedBridgeHandler));
		}
	}
}
