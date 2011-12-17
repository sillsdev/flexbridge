using System;
using System.IO;
using System.Linq;
using Chorus.FileTypeHanders;
using Chorus.merge;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.ModelVersion;
using NUnit.Framework;
using Palaso.IO;

namespace FLEx_ChorusPluginTests.Infrastructure.ModelVersion
{
	/// <summary>
	/// Test the FW model version file handler
	/// </summary>
	[TestFixture]
	public class FieldWorksModelVersionFileHandlerTests
	{
		private IChorusFileTypeHandler _fileHandler;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_fileHandler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
											  where handler.GetType().Name == "FieldWorksModelVersionFileHandler"
											  select handler).First();
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			_fileHandler = null;
		}

		[Test]
		public void DescribeInitialContentsShouldHaveAddedForLabel()
		{
			var initialContents = _fileHandler.DescribeInitialContents(null, null);
			Assert.AreEqual(1, initialContents.Count());
			var onlyOne = initialContents.First();
			Assert.AreEqual("Added", onlyOne.ActionLabel);
		}

		[Test]
		public void ExtensionOfKnownFileTypesShouldBeCustomProperties()
		{
			var extensions = _fileHandler.GetExtensionsOfKnownTextFileTypes().ToArray();
			Assert.AreEqual(1, extensions.Count(), "Wrong number of extensions.");
			Assert.AreEqual("ModelVersion", extensions[0]);
		}

		[Test]
		public void ShouldNotBeAbleToValidateIncorrectFormatFile()
		{
			using (var tempModelVersionFile = new TempFile("<classdata />"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, "ModelVersion");
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsFalse(_fileHandler.CanValidateFile(newpath));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldBeAbleToValidateInProperlyFormatedFile()
		{
			using (var tempModelVersionFile = new TempFile("{\"modelversion\": 7000037}"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, "ModelVersion");
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsTrue(_fileHandler.CanValidateFile(newpath));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldNotBeAbleToValidateFile()
		{
			using (var tempModelVersionFile = new TempFile("<classdata />"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, "ModelVersion");
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsNotNull(_fileHandler.ValidateFile(newpath, null));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldBeAbleToValidateFile()
		{
			using (var tempModelVersionFile = new TempFile("{\"modelversion\": 7000037}"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, "ModelVersion");
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsNull(_fileHandler.ValidateFile(newpath, null));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldMergeTheirModelNumber()
		{
			const string commonData = "{\"modelversion\": 7000044}";
			const string ourData = "{\"modelversion\": 7000045}";
			const string theirData = "{\"modelversion\": 7000046}";
			using (var commonTempFile = new TempFile("Common.ModelVersion"))
			using (var ourTempFile = new TempFile("Our.ModelVersion"))
			using (var theirTempFile = new TempFile("Their.ModelVersion"))
			{
				File.WriteAllText(commonTempFile.Path, commonData);
				File.WriteAllText(ourTempFile.Path, ourData);
				File.WriteAllText(theirTempFile.Path, theirData);

				var listener = new ListenerForUnitTests();
				var mergeOrder = new MergeOrder(ourTempFile.Path, commonTempFile.Path, theirTempFile.Path, new NullMergeSituation())
					{ EventListener = listener };
				_fileHandler.Do3WayMerge(mergeOrder);
				var mergedData = File.ReadAllText(ourTempFile.Path);
				Assert.AreEqual(theirData, mergedData);
				listener.AssertExpectedConflictCount(0);
				listener.AssertExpectedChangesCount(1);
				listener.AssertFirstChangeType<FieldWorksModelVersionUpdatedReport>();
			}
		}

		[Test]
		public void ShouldMergeOurModelNumber()
		{
			const string commonData = "{\"modelversion\": 7000044}";
			const string ourData = "{\"modelversion\": 7000046}";
			const string theirData = "{\"modelversion\": 7000045}";
			using (var commonTempFile = new TempFile("Common.ModelVersion"))
			using (var ourTempFile = new TempFile("Our.ModelVersion"))
			using (var theirTempFile = new TempFile("Their.ModelVersion"))
			{
				File.WriteAllText(commonTempFile.Path, commonData);
				File.WriteAllText(ourTempFile.Path, ourData);
				File.WriteAllText(theirTempFile.Path, theirData);

				var listener = new ListenerForUnitTests();
				var mergeOrder = new MergeOrder(ourTempFile.Path, commonTempFile.Path, theirTempFile.Path, new NullMergeSituation())
					{EventListener = listener};
				_fileHandler.Do3WayMerge(mergeOrder);
				var mergedData = File.ReadAllText(ourTempFile.Path);
				Assert.AreEqual(ourData, mergedData);
				listener.AssertExpectedConflictCount(0);
				listener.AssertExpectedChangesCount(1);
				listener.AssertFirstChangeType<FieldWorksModelVersionUpdatedReport>();
			}
		}

		[Test]
		public void BothDidSameUpgrade()
		{
			//var mdc = MetadataCache.TestOnlyNewCache;
			const string commonData = "{\"modelversion\": 7000044}";
			const string ourData = "{\"modelversion\": 7000046}";
			const string theirData = "{\"modelversion\": 7000046}";
			using (var commonTempFile = new TempFile("Common.ModelVersion"))
			using (var ourTempFile = new TempFile("Our.ModelVersion"))
			using (var theirTempFile = new TempFile("Their.ModelVersion"))
			{
				File.WriteAllText(commonTempFile.Path, commonData);
				File.WriteAllText(ourTempFile.Path, ourData);
				File.WriteAllText(theirTempFile.Path, theirData);

				var listener = new ListenerForUnitTests();
				var mergeOrder = new MergeOrder(ourTempFile.Path, commonTempFile.Path, theirTempFile.Path, new NullMergeSituation()) { EventListener = listener };
				_fileHandler.Do3WayMerge(mergeOrder);
				var mergedData = File.ReadAllText(ourTempFile.Path);
				Assert.AreEqual(ourData, mergedData);
				listener.AssertExpectedConflictCount(0);
				listener.AssertExpectedChangesCount(1);
				listener.AssertFirstChangeType<FieldWorksModelVersionUpdatedReport>();
			}
		}

		[Test]
		public void ShouldRejectOurDowngrade()
		{
			const string commonData = "{\"modelversion\": 7000010}";
			const string ourData = "{\"modelversion\": 7000009}";
			const string theirData = "{\"modelversion\": 7000010}";
			using (var commonTempFile = new TempFile("Common.ModelVersion"))
			using (var ourTempFile = new TempFile("Our.ModelVersion"))
			using (var theirTempFile = new TempFile("Their.ModelVersion"))
			{
				File.WriteAllText(commonTempFile.Path, commonData);
				File.WriteAllText(ourTempFile.Path, ourData);
				File.WriteAllText(theirTempFile.Path, theirData);

				var listener = new ListenerForUnitTests();
				var mergeOrder = new MergeOrder(ourTempFile.Path, commonTempFile.Path, theirTempFile.Path, new NullMergeSituation()) { EventListener = listener };
				Assert.Throws<InvalidOperationException>(() => _fileHandler.Do3WayMerge(mergeOrder));
			}
		}

		[Test]
		public void ShouldRejectTheirDowngrade()
		{
			const string commonData = "{\"modelversion\": 7000010}";
			const string ourData = "{\"modelversion\": 7000010}";
			const string theirData = "{\"modelversion\": 7000009}";
			using (var commonTempFile = new TempFile("Common.ModelVersion"))
			using (var ourTempFile = new TempFile("Our.ModelVersion"))
			using (var theirTempFile = new TempFile("Their.ModelVersion"))
			{
				File.WriteAllText(commonTempFile.Path, commonData);
				File.WriteAllText(ourTempFile.Path, ourData);
				File.WriteAllText(theirTempFile.Path, theirData);

				var listener = new ListenerForUnitTests();
				var mergeOrder = new MergeOrder(ourTempFile.Path, commonTempFile.Path, theirTempFile.Path, new NullMergeSituation()) { EventListener = listener };
				Assert.Throws<InvalidOperationException>(() => _fileHandler.Do3WayMerge(mergeOrder));
			}
		}

		[Test]
		public void ShouldHaveNoChanges()
		{
			const string commonData = "{\"modelversion\": 7000002}";
			const string ourData = "{\"modelversion\": 7000002}";
			const string theirData = "{\"modelversion\": 7000002}";
			using (var commonTempFile = new TempFile("Common.ModelVersion"))
			using (var ourTempFile = new TempFile("Our.ModelVersion"))
			using (var theirTempFile = new TempFile("Their.ModelVersion"))
			{
				File.WriteAllText(commonTempFile.Path, commonData);
				File.WriteAllText(ourTempFile.Path, ourData);
				File.WriteAllText(theirTempFile.Path, theirData);

				var listener = new ListenerForUnitTests();
				var mergeOrder = new MergeOrder(ourTempFile.Path, commonTempFile.Path, theirTempFile.Path, new NullMergeSituation()) { EventListener = listener };
				_fileHandler.Do3WayMerge(mergeOrder);
				var mergedData = File.ReadAllText(ourTempFile.Path);
				Assert.AreEqual(ourData, mergedData);
				listener.AssertExpectedConflictCount(0);
				listener.AssertExpectedChangesCount(0);
			}
		}

		[Test]
		public void Find2WayDifferencesShouldReportOneAddition()
		{
			const string parent = "{\"modelversion\": 7000002}";
			using (var repositorySetup = new RepositorySetup("randy"))
			{
				repositorySetup.AddAndCheckinFile("fwtest.ModelVersion", parent);
				var hgRepository = repositorySetup.Repository;
				var allRevisions = (from rev in hgRepository.GetAllRevisions()
									orderby rev.Number.LocalRevisionNumber
									select rev).ToList();
				var first = allRevisions[0];
				var firstFiR = hgRepository.GetFilesInRevision(first).First();
				var result = _fileHandler.Find2WayDifferences(null, firstFiR, hgRepository).ToList();
				Assert.AreEqual(1, result.Count);
				Assert.IsInstanceOf(typeof(FieldWorksModelVersionAdditionChangeReport), result[0]);
			}
		}

		[Test]
		public void Find2WayDifferencesShouldReportOneChange()
		{
			const string parent = "{\"modelversion\": 7000000}";
			// One change.
			const string child = "{\"modelversion\": 7000002}";
			using (var repositorySetup = new RepositorySetup("randy"))
			{
				repositorySetup.AddAndCheckinFile("fwtest.ModelVersion", parent);
				repositorySetup.ChangeFileAndCommit("fwtest.ModelVersion", child, "change it");
				var hgRepository = repositorySetup.Repository;
				var allRevisions = (from rev in hgRepository.GetAllRevisions()
									orderby rev.Number.LocalRevisionNumber
									select rev).ToList();
				var first = allRevisions[0];
				var second = allRevisions[1];
				var firstFiR = hgRepository.GetFilesInRevision(first).First();
				var secondFiR = hgRepository.GetFilesInRevision(second).First();
				var result = _fileHandler.Find2WayDifferences(firstFiR, secondFiR, hgRepository).ToList();
				Assert.AreEqual(1, result.Count);
				Assert.IsInstanceOf(typeof(FieldWorksModelVersionUpdatedReport), result[0]);
			}
		}
	}
}