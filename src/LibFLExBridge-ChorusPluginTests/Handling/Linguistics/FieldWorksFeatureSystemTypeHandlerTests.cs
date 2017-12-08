// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.IO;
using System.Linq;
using System.Xml.Linq;
using Chorus.merge;
using LibChorus.TestUtilities;
using LibFLExBridgeChorusPlugin.Handling.Linguistics;
using LibFLExBridgeChorusPlugin.Infrastructure;
using NUnit.Framework;
using SIL.IO;
using SIL.Progress;

namespace LibFLExBridgeChorusPluginTests.Handling.Linguistics
{
	[TestFixture]
	public class FieldWorksFeatureSystemTypeHandlerTests : BaseFieldWorksTypeHandlerTests
	{
		private TempFile _ourFile;
		private TempFile _theirFile;
		private TempFile _commonFile;

		[SetUp]
		public override void TestSetup()
		{
			base.TestSetup();
			FieldWorksTestServices.SetupTempFilesWithName(FlexBridgeConstants.MorphAndSynFeaturesFilename, out _ourFile,
														  out _commonFile, out _theirFile);
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
			var initialContents = FileHandler.DescribeInitialContents(null, null).ToList();
			Assert.AreEqual(1, initialContents.Count);
			var onlyOne = initialContents.First();
			Assert.AreEqual("Added", onlyOne.ActionLabel);
		}

		[Test]
		public void ExtensionOfKnownFileTypesShouldBeFeatsys()
		{
			var extensions = FileHandler.GetExtensionsOfKnownTextFileTypes().ToArray();
			Assert.AreEqual(FieldWorksTestServices.ExpectedExtensionCount, extensions.Count(), "Wrong number of extensions.");
			Assert.IsTrue(extensions.Contains(FlexBridgeConstants.Featsys));
		}

		[Test]
		public void ShouldNotBeAbleToValidateIncorrectFormatFile()
		{
			using (var tempModelVersionFile = new TempFile("<classdata />"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, FlexBridgeConstants.Featsys);
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsTrue(FileHandler.CanValidateFile(newpath));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldBeAbleToValidateInProperlyFormattedFile()
		{
			const string data =
@"<FeatureSystem>
<FsFeatureSystem guid='fff03918-9674-4401-8bb1-efe6502985a7' />
</FeatureSystem>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
		}

		[Test]
		public void ShouldBeAbleToDoAllCanOperations()
		{
			const string data =
@"<FeatureSystem>
<FsFeatureSystem guid='fff03918-9674-4401-8bb1-efe6502985a7' />
</FeatureSystem>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanDiffFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanMergeFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanPresentFile(_ourFile.Path));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFile1()
		{
			const string data = "<classdata />";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFile2()
		{
			const string data =
@"<FeatureSystem>
<header>
</header>
</FeatureSystem>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFile3()
		{
			const string data =
@"<FeatureSystem>
<FsFeatureSystem guid='fff03918-9674-4401-8bb1-efe6502985a7' />
<FsFeatureSystem guid='fff03918-9674-4401-8bb1-efe6502985a8' />
</FeatureSystem>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldBeAbleToValidateFile1()
		{
			const string data =
@"<FeatureSystem>
<FsFeatureSystem guid='fff03918-9674-4401-8bb1-efe6502985a7' />
</FeatureSystem>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void MergeChangedFiles_TwoChildrenInFeaturesNodeConsistent()
		{
			const string user1 =
@"<?xml version='1.0' encoding='utf-8'?>
<FeatureSystem>   
	<FsFeatureSystem   
		guid='d6d5a99e-ea5e-11de-994c-0013722f8dec'>   
		<Features>   
			<FsClosedFeature
				guid='00e226d2-743b-4e71-8663-98224b36596d'>
				<ShowInGloss val='False' />
			</FsClosedFeature>
			<FsComplexFeature
				guid='55706aa1-2381-45a6-bba2-ea489bb4a636'>
			</FsComplexFeature>
		</Features>
	</FsFeatureSystem>
</FeatureSystem>";

			const string user2 =
@"<?xml version='1.0' encoding='utf-8'?>
<FeatureSystem>   
	<FsFeatureSystem   
		guid='d6d5a99e-ea5e-11de-994c-0013722f8dec'>   
		<Features>   
			<FsClosedFeature
				guid='00e226d2-743b-4e71-8663-98224b36596d'>
				<ShowInGloss val='True' />
			</FsClosedFeature>
			<FsComplexFeature
				guid='55706aa1-2381-45a6-bba2-ea489bb4a636'>
			</FsComplexFeature>
		</Features>
	</FsFeatureSystem>
</FeatureSystem>";

			const string ancestor =
@"<?xml version='1.0' encoding='utf-8'?>
<FeatureSystem>   
	<FsFeatureSystem   
		guid='d6d5a99e-ea5e-11de-994c-0013722f8dec'>   
		<Features>   
			<FsClosedFeature
				guid='00e226d2-743b-4e71-8663-98224b36596d'>
			</FsClosedFeature>
			<FsComplexFeature
				guid='55706aa1-2381-45a6-bba2-ea489bb4a636'>
			</FsComplexFeature>
		</Features>
	</FsFeatureSystem>
</FeatureSystem>";

			File.WriteAllText(_ourFile.Path, user1);
			File.WriteAllText(_theirFile.Path, user2);
			File.WriteAllText(_commonFile.Path, ancestor);
			var mdc = MetadataCache.TestOnlyNewCache;
			var eventListener = new ListenerForUnitTests();
			var mergeOrder = new MergeOrder(_ourFile.Path, _commonFile.Path, _theirFile.Path, new NullMergeSituation())
			{
				EventListener = eventListener
			};
			var handlerStrategy = new FeatureSystemFileTypeHandlerStrategy();
			handlerStrategy.Do3WayMerge(mdc, mergeOrder);
			var doc = XDocument.Load(_ourFile.Path);
			var featureElement = doc.Root.Element("FsFeatureSystem").Element("Features");
			Assert.AreEqual(2, featureElement.Nodes().Count());
			Assert.AreEqual("False", featureElement.Element("FsClosedFeature").Element("ShowInGloss").Attribute("val").Value);
		}
	}
}