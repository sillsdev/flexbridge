// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.IO;
using System.Linq;
using Chorus.merge;
using LibFLExBridgeChorusPlugin;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibFLExBridgeChorusPlugin.Handling.ModelVersion;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.IO;
using SIL.Progress;

namespace LibFLExBridgeChorusPluginTests.Handling.ModelVersion
{
	/// <summary>
	/// Test the FW model version file handler
	/// </summary>
	[TestFixture]
	public class FieldWorksModelVersionFileHandlerTests : BaseFieldWorksTypeHandlerTests
	{
		private TempFile _ourFile;
		private TempFile _theirFile;
		private TempFile _commonFile;

		[SetUp]
		public override void TestSetup()
		{
			base.TestSetup();
			FieldWorksTestServices.SetupTempFilesWithName(FlexBridgeConstants.ModelVersionFilename, out _ourFile, out _commonFile, out _theirFile);
		}

		[TearDown]
		public override void TestTearDown()
		{
			base.TestTearDown();
			FieldWorksTestServices.RemoveTempFilesAndParentDir(ref _ourFile, ref _commonFile, ref _theirFile);
		}

		[Test]
		public void DescribeInitialContentsShouldHaveAddedForLabel()
		{
			var initialContents = FileHandler.DescribeInitialContents(null, null);
			Assert.AreEqual(1, initialContents.Count());
			var onlyOne = initialContents.First();
			Assert.AreEqual("Added", onlyOne.ActionLabel);
		}

		[Test]
		public void ExtensionOfKnownFileTypesShouldBeCustomProperties()
		{
			var extensions = FileHandler.GetExtensionsOfKnownTextFileTypes().ToArray();
			Assert.AreEqual(FieldWorksTestServices.ExpectedExtensionCount, extensions.Count(), "Wrong number of extensions.");
			Assert.IsTrue(extensions.Contains(FlexBridgeConstants.ModelVersion));
		}

		[Test]
		public void ShouldBeAbleToValidateIncorrectFormatFile()
		{
			File.WriteAllText(_ourFile.Path, "<classdata />");
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
		}

		[Test]
		public void ShouldBeAbleToValidateInProperlyFormattedFile()
		{
			File.WriteAllText(_ourFile.Path, "{\"modelversion\": 7000037}");
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFile()
		{
			const string ourData = "<classdata />";
			File.WriteAllText(_ourFile.Path, ourData);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFileWithVersionNumberTooLow()
		{
			const string ourData = "{\"modelversion\": 6999999}";
			File.WriteAllText(_ourFile.Path, ourData);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFileWithNonIntegerVersionNumber()
		{
			const string ourData = "{\"modelversion\": cat}";
			File.WriteAllText(_ourFile.Path, ourData);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldBeAbleToValidateFile()
		{
			const string ourData = "{\"modelversion\": 7000037}";
			File.WriteAllText(_ourFile.Path, ourData);
			Assert.IsNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldMergeTheirModelNumber()
		{
			const string commonData = "{\"modelversion\": 7000044}";
			const string ourData = "{\"modelversion\": 7000045}";
			const string theirData = "{\"modelversion\": 7000046}";

			File.WriteAllText(_commonFile.Path, commonData);
			File.WriteAllText(_ourFile.Path, ourData);
			File.WriteAllText(_theirFile.Path, theirData);

			var listener = new ListenerForUnitTests();
			var mergeOrder = new MergeOrder(_ourFile.Path, _commonFile.Path, _theirFile.Path, new NullMergeSituation()) { EventListener = listener };
			FileHandler.Do3WayMerge(mergeOrder);
			var mergedData = File.ReadAllText(_ourFile.Path);
			Assert.AreEqual(theirData, mergedData);
			listener.AssertExpectedConflictCount(0);
			listener.AssertExpectedChangesCount(1);
			listener.AssertFirstChangeType<FieldWorksModelVersionUpdatedReport>();
		}

		[Test]
		public void ShouldMergeOurModelNumber()
		{
			const string commonData = "{\"modelversion\": 7000044}";
			const string ourData = "{\"modelversion\": 7000046}";
			const string theirData = "{\"modelversion\": 7000045}";

			File.WriteAllText(_commonFile.Path, commonData);
			File.WriteAllText(_ourFile.Path, ourData);
			File.WriteAllText(_theirFile.Path, theirData);

			var listener = new ListenerForUnitTests();
			var mergeOrder = new MergeOrder(_ourFile.Path, _commonFile.Path, _theirFile.Path, new NullMergeSituation()) { EventListener = listener };
			FileHandler.Do3WayMerge(mergeOrder);
			var mergedData = File.ReadAllText(_ourFile.Path);
			Assert.AreEqual(ourData, mergedData);
			listener.AssertExpectedConflictCount(0);
			listener.AssertExpectedChangesCount(1);
			listener.AssertFirstChangeType<FieldWorksModelVersionUpdatedReport>();
		}

		[Test]
		public void BothDidSameUpgrade()
		{
			const string commonData = "{\"modelversion\": 7000044}";
			const string ourData = "{\"modelversion\": 7000046}";
			const string theirData = "{\"modelversion\": 7000046}";

			File.WriteAllText(_commonFile.Path, commonData);
			File.WriteAllText(_ourFile.Path, ourData);
			File.WriteAllText(_theirFile.Path, theirData);

			var listener = new ListenerForUnitTests();
			var mergeOrder = new MergeOrder(_ourFile.Path, _commonFile.Path, _ourFile.Path, new NullMergeSituation()) { EventListener = listener };
			FileHandler.Do3WayMerge(mergeOrder);
			var mergedData = File.ReadAllText(_ourFile.Path);
			Assert.AreEqual(ourData, mergedData);
			listener.AssertExpectedConflictCount(0);
			listener.AssertExpectedChangesCount(1);
			listener.AssertFirstChangeType<FieldWorksModelVersionUpdatedReport>();
		}

		[Test]
		public void ShouldRejectOurDowngrade()
		{
			const string commonData = "{\"modelversion\": 7000010}";
			const string ourData = "{\"modelversion\": 7000009}";
			const string theirData = "{\"modelversion\": 7000010}";

			File.WriteAllText(_commonFile.Path, commonData);
			File.WriteAllText(_ourFile.Path, ourData);
			File.WriteAllText(_theirFile.Path, theirData);

			var listener = new ListenerForUnitTests();
			var mergeOrder = new MergeOrder(_ourFile.Path, _commonFile.Path, _theirFile.Path, new NullMergeSituation()) { EventListener = listener };
			Assert.Throws<InvalidOperationException>(() => FileHandler.Do3WayMerge(mergeOrder));
		}

		[Test]
		public void ShouldRejectTheirDowngrade()
		{
			const string commonData = "{\"modelversion\": 7000010}";
			const string ourData = "{\"modelversion\": 7000010}";
			const string theirData = "{\"modelversion\": 7000009}";

			File.WriteAllText(_commonFile.Path, commonData);
			File.WriteAllText(_ourFile.Path, ourData);
			File.WriteAllText(_theirFile.Path, theirData);

			var listener = new ListenerForUnitTests();
			var mergeOrder = new MergeOrder(_ourFile.Path, _commonFile.Path, _theirFile.Path, new NullMergeSituation()) { EventListener = listener };
			Assert.Throws<InvalidOperationException>(() => FileHandler.Do3WayMerge(mergeOrder));
		}

		[Test]
		public void ShouldHaveNoChanges()
		{
			const string commonData = "{\"modelversion\": 7000037}";
			const string ourData = "{\"modelversion\": 7000037}";
			const string theirData = "{\"modelversion\": 7000037}";

				File.WriteAllText(_commonFile.Path, commonData);
				File.WriteAllText(_ourFile.Path, ourData);
				File.WriteAllText(_theirFile.Path, theirData);

				var listener = new ListenerForUnitTests();
				var mergeOrder = new MergeOrder(_ourFile.Path, _commonFile.Path, _theirFile.Path, new NullMergeSituation()) { EventListener = listener };
				FileHandler.Do3WayMerge(mergeOrder);
				var mergedData = File.ReadAllText(_ourFile.Path);
				Assert.AreEqual(ourData, mergedData);
				listener.AssertExpectedConflictCount(0);
				listener.AssertExpectedChangesCount(0);
		}

		[Test]
		public void Find2WayDifferencesShouldReportOneChange()
		{
			const string parent = "{\"modelversion\": 7000000}";
			// One change.
			const string child = "{\"modelversion\": 7000002}";
			using (var repositorySetup = new RepositorySetup("randy"))
			{
				repositorySetup.AddAndCheckinFile("FLExProject.ModelVersion", parent);
				repositorySetup.ChangeFileAndCommit("FLExProject.ModelVersion", child, "change it");
				var hgRepository = repositorySetup.Repository;
				var allRevisions = (from rev in hgRepository.GetAllRevisions()
									orderby rev.Number.LocalRevisionNumber
									select rev).ToList();
				var first = allRevisions[0];
				var second = allRevisions[1];
				var firstFiR = hgRepository.GetFilesInRevision(first).First();
				var secondFiR = hgRepository.GetFilesInRevision(second).First();
				var result = FileHandler.Find2WayDifferences(firstFiR, secondFiR, hgRepository).ToList();
				Assert.AreEqual(1, result.Count);
				Assert.IsInstanceOf(typeof(FieldWorksModelVersionUpdatedReport), result[0]);
			}
		}
	}
}