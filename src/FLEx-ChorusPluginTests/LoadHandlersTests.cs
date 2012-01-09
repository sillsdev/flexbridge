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
			var handlerNames = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
							   select handler.GetType().Name).ToList();
			Assert.IsTrue(handlerNames.Contains("FieldWorksCommonFileHandler"));
			var unexpectedBridgeHandlers = new HashSet<string>
											{
												"FieldWorksCustomPropertyFileHandler",
												"FieldWorksModelVersionFileHandler",
												"FieldWorksFileHandler",
												"FieldWorksReversalTypeHandler"
											};
			foreach (var unexpectedBridgeHandler in unexpectedBridgeHandlers)
				Assert.IsFalse(handlerNames.Contains(unexpectedBridgeHandler));
		}
	}
}
