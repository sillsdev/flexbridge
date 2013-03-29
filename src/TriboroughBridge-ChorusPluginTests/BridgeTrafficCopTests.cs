using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using LibChorus.TestUtilities;
using NUnit.Framework;
using Palaso.TestUtilities;
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
				new DirectoryCatalog(
					Path.GetDirectoryName(Utilities.StripFilePrefix(typeof(BridgeTrafficCop).Assembly.CodeBase)),
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
			Assert.IsNotNull(trafficCop.Models.Select(model => model.ModelType == BridgeModelType.Flex).First());
			Assert.IsNotNull(trafficCop.Models.Select(model => model.ModelType == BridgeModelType.Lift).First());
		}

		[Test, Category("UnknownMonoIssue")]
		public void undo_export_lift_RevertsModifiedFiles_RemovesNewFiles_AndLeavesTombstone()
		{
			using (var mainProjectFolder = new TemporaryFolder("Projects"))
			using (var fooProjectFolder = new TemporaryFolder(mainProjectFolder, "foo"))
			using (var otherRepositoriesFolder = new TemporaryFolder(fooProjectFolder, Utilities.OtherRepositories))
			using (var repo = new RepositorySetup("Randy", Path.Combine(otherRepositoriesFolder.Path, "foo_LIFT")))
			{
				var repoFile = Path.Combine(repo.ProjectFolder.Path, "foo" + Utilities.LiftExtension);
				repo.AddAndCheckinFile(repoFile, "original stuff");
				File.WriteAllText(repoFile, "changed stuff");

				var newFile = Path.Combine(repo.ProjectFolder.Path, "new.txt");
				File.WriteAllText(newFile, "new stuff");

				var failureNotificationFile = Path.Combine(repo.ProjectFolder.Path, Utilities.FailureFilename);
				File.WriteAllText(failureNotificationFile, "standard");

				var options = new Dictionary<string, string>
					{
						{"-v", "undo_export_lift"},
						{"-u", "Randy"},
						{"-p", fooProjectFolder.Path }
					};
				var trafficCop = _container.GetExportedValue<BridgeTrafficCop>();
				bool showWindow;
				var result = trafficCop.StartWorking(options, out showWindow);
				Assert.IsFalse(result);
				Assert.IsFalse(showWindow);
				Assert.AreEqual("original stuff", File.ReadAllText(repoFile));
				Assert.IsFalse(File.Exists(newFile));
				Assert.IsTrue(File.Exists(failureNotificationFile));
			}
		}
	}
}
