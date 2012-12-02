using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using NUnit.Framework;
using TriboroughBridge_ChorusPlugin;

namespace TriboroughBridge_ChorusPluginTests
{
	[TestFixture]
	public class BridgeTrafficCopTests
	{
		private AggregateCatalog _catalog;
		private CompositionContainer _container;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			//An aggregate catalog that combines multiple catalogs
			_catalog = new AggregateCatalog();
			_catalog.Catalogs.Add(
				new DirectoryCatalog(Path.GetDirectoryName(Utilities.StripFilePrefix(typeof(BridgeTrafficCop).Assembly.CodeBase)),
				"*-ChorusPlugin.dll"));

			//Create the CompositionContainer with the parts in the catalog
			_container = new CompositionContainer(_catalog);

			//var options = ParseCommandLineArgs(args);
			//var bridgeTrafficCop = _container.GetExportedValue<BridgeTrafficCop>();
			//bridgeTrafficCop.StartWorking(options);
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			_container.Dispose();
			_catalog.Dispose();
		}

		[Test]
		public void HasRightNumberOfModels()
		{
			Assert.AreEqual(2, _container.GetExportedValue<BridgeTrafficCop>().Models.Count());
		}

		[Test]
		public void HasAllSupportedModels()
		{
			var trafficCop = _container.GetExportedValue<BridgeTrafficCop>();
			Assert.IsNotNull(trafficCop.GetModel(BridgeModelType.Lift));
			Assert.IsNotNull(trafficCop.GetModel(BridgeModelType.Flex));
		}
	}
}
