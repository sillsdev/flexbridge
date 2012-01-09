using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chorus.FileTypeHanders;
using Chorus.FileTypeHanders.xml;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPluginTests.BorrowedCode;
using NUnit.Framework;
using Palaso.IO;

namespace FLEx_ChorusPluginTests.Infrastructure.Handling.CustomProperties
{
	/// <summary>
	/// Test the FW custom property file handler.
	/// </summary>
	[TestFixture]
	public class FieldWorksCustomPropertyFileHandlerTests
	{
		private IChorusFileTypeHandler _fileHandler;
		private TempFile _ourFile;
		private TempFile _theirFile;
		private TempFile _commonFile;
		private MetadataCache _mdc;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_fileHandler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
							where handler.GetType().Name == "FieldWorksCommonFileHandler"
											  select handler).First();
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			_fileHandler = null;
		}

		[SetUp]
		public void TestSetup()
		{
			_mdc = MetadataCache.TestOnlyNewCache;
			FieldWorksTestServices.SetupTempFilesWithExstension(".CustomProperties", out _ourFile, out _commonFile, out _theirFile);
		}

		[TearDown]
		public void TestTearDown()
		{
			_mdc = null;
			FieldWorksTestServices.RemoveTempFiles(ref _ourFile, ref _commonFile, ref _theirFile);
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
			Assert.AreEqual(5, extensions.Count(), "Wrong number of extensions.");
			Assert.IsTrue(extensions.Contains(SharedConstants.CustomProperties));
		}

		[Test]
		public void ShouldNotBeAbleToValidateIncorrectFormatFile()
		{
			const string data = "<classdata />";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsFalse(_fileHandler.CanValidateFile(_ourFile.Path));
		}

		[Test]
		public void ShouldBeAbleToValidateInProperlyFormatedFile()
		{
			const string data = @"<AdditionalFields>
<CustomField name='Certified' class='WfiWordform' type='Boolean' />
</AdditionalFields>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(_fileHandler.CanValidateFile(_ourFile.Path));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFile()
		{
			using (var tempModelVersionFile = new TempFile("<classdata />"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, "someext");
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsNotNull(_fileHandler.ValidateFile(newpath, null));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldBeAbleToValidateFile()
		{
			const string data = @"<AdditionalFields>
<CustomField name='Certified' class='WfiWordform' type='Boolean' />
</AdditionalFields>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNull(_fileHandler.ValidateFile(_ourFile.Path, null));
		}

		[Test]
		public void Find2WayDifferencesShouldReportThreeChanges()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
<CustomField class='WfiWordform' key='WfiWordformGoner' name='Goner' type='Boolean' />
<CustomField class='WfiWordform' key='WfiWordformDirtball' name='Dirtball' type='Boolean' />
</AdditionalFields>";
			// One deletion, one change, and one insertion, and one unchanged.
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
<CustomField class='WfiWordform' key='WfiWordformDirtball' name='Dirtball' type='Integer' />
<CustomField class='WfiWordform' key='WfiWordformNewby' name='Newby' type='Boolean' />
</AdditionalFields>";
			using (var repositorySetup = new RepositorySetup("randy"))
			{
				repositorySetup.AddAndCheckinFile("fwtest.CustomProperties", parent);
				repositorySetup.ChangeFileAndCommit("fwtest.CustomProperties", child, "change it");
				var hgRepository = repositorySetup.Repository;
				var allRevisions = (from rev in hgRepository.GetAllRevisions()
									orderby rev.Number.LocalRevisionNumber
									select rev).ToList();
				var first = allRevisions[0];
				var second = allRevisions[1];
				var firstFiR = hgRepository.GetFilesInRevision(first).First();
				var secondFiR = hgRepository.GetFilesInRevision(second).First();
				var result = _fileHandler.Find2WayDifferences(firstFiR, secondFiR, hgRepository).ToList();
				Assert.AreEqual(3, result.Count);
			}
		}

		[Test]
		public void PropertyDeletedReported()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
<CustomField class='WfiWordform' key='WfiWordformGoner' name='Goner' type='Boolean' />
</AdditionalFields>";
			// One deletion, one change, and one insertion, and one unchanged.
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
</AdditionalFields>";
			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var listener = new ListenerForUnitTests();
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path, listener,
					null,
					"CustomField",
					"key");
				differ.ReportDifferencesToListener();
				listener.AssertExpectedConflictCount(0);
				listener.AssertExpectedChangesCount(1);
				listener.AssertFirstChangeType<XmlDeletionChangeReport>();
			}
		}

		[Test]
		public void PropertyChangedReported()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
<CustomField class='WfiWordform' key='WfiWordformDirtball' name='Dirtball' type='Boolean' />
</AdditionalFields>";
			// One deletion, one change, and one insertion, and one unchanged.
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
<CustomField class='WfiWordform' key='WfiWordformDirtball' name='Dirtball' type='Integer' />
</AdditionalFields>";
			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var listener = new ListenerForUnitTests();
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path, listener,
					null,
					"CustomField",
					"key");
				differ.ReportDifferencesToListener();
				listener.AssertExpectedConflictCount(0);
				listener.AssertExpectedChangesCount(1);
				listener.AssertFirstChangeType<XmlChangedRecordReport>();
			}
		}

		[Test]
		public void NoChangesReported()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
</AdditionalFields>";
			// One deletion, one change, and one insertion, and one unchanged.
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
</AdditionalFields>";
			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var listener = new ListenerForUnitTests();
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path, listener,
					null,
					"CustomField",
					"key");
				differ.ReportDifferencesToListener();
				listener.AssertExpectedConflictCount(0);
				listener.AssertExpectedChangesCount(0);
			}
		}

		[Test]
		public void WinnerAndLoserEachAddedNewElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
</AdditionalFields>";
			var ourContent = commonAncestor.Replace("</AdditionalFields>", "<CustomField class='WfiWordform' key='WfiWordformOurCertified' name='OurCertified' type='Boolean' /></AdditionalFields>");
			var theirContent = commonAncestor.Replace("</AdditionalFields>", "<CustomField class='WfiWordform' key='WfiWordformTheirCertified' name='TheirCertified' type='Boolean' /></AdditionalFields>");

			FieldWorksTestServices.DoMerge(
				_fileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string>
					{
						@"AdditionalFields/CustomField[@key=""WfiWordformCertified""]",
						@"AdditionalFields/CustomField[@key=""WfiWordformOurCertified""]",
						@"AdditionalFields/CustomField[@key=""WfiWordformTheirCertified""]"
					},
				null,
				0, new List<Type>(),
				2, new List<Type> {typeof (XmlAdditionChangeReport), typeof (XmlAdditionChangeReport)});
		}

		[Test]
		public void WinnerAddedNewElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
</AdditionalFields>";
			var ourContent = commonAncestor.Replace("</AdditionalFields>", "<CustomField class='WfiWordform' key='WfiWordformAttested' name='Attested' type='Boolean' /></AdditionalFields>");
			const string theirContent = commonAncestor;

			FieldWorksTestServices.DoMerge(
				_fileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@key=""WfiWordformCertified""]", @"AdditionalFields/CustomField[@key=""WfiWordformAttested""]" },
				null,
				0, new List<Type>(),
				1, new List<Type> { typeof(XmlAdditionChangeReport) });
		}

		[Test]
		public void LoserAddedNewElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
</AdditionalFields>";
			const string ourContent = commonAncestor;
			var theirContent = commonAncestor.Replace("</AdditionalFields>", "<CustomField class='WfiWordform' key='WfiWordformAttested' name='Attested' type='Boolean' /></AdditionalFields>");

			FieldWorksTestServices.DoMerge(
				_fileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@key=""WfiWordformCertified""]", @"AdditionalFields/CustomField[@key=""WfiWordformAttested""]" },
				null,
				0, new List<Type>(),
				1, new List<Type> { typeof(XmlAdditionChangeReport) });
		}

		[Test]
		public void WinnerDeletedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
<CustomField class='WfiWordform' key='WfiWordformAttested' name='Attested' type='Boolean' />
</AdditionalFields>";
			var ourContent = commonAncestor.Replace("<CustomField class='WfiWordform' key='WfiWordformAttested' name='Attested' type='Boolean' />", null);
			const string theirContent = commonAncestor;

			FieldWorksTestServices.DoMerge(
				_fileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@key=""WfiWordformCertified""]" },
				new List<string> { @"AdditionalFields/CustomField[@key=""WfiWordformAttested""]" },
				0, new List<Type>(),
				1, new List<Type> { typeof(XmlDeletionChangeReport) });
		}

		[Test]
		public void LoserDeletedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
<CustomField class='WfiWordform' key='WfiWordformAttested' name='Attested' type='Boolean' />
</AdditionalFields>";
			const string ourContent = commonAncestor;
			var theirContent = commonAncestor.Replace("<CustomField class='WfiWordform' key='WfiWordformAttested' name='Attested' type='Boolean' />", null);

			FieldWorksTestServices.DoMerge(
				_fileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@key=""WfiWordformCertified""]" },
				new List<string> { @"AdditionalFields/CustomField[@key=""WfiWordformAttested""]" },
				0, new List<Type>(),
				0, new List<Type>());
		}

		[Test]
		public void WinnerAndLoserBothDeletedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
<CustomField class='WfiWordform' key='WfiWordformAttested' name='Attested' type='Boolean' />
</AdditionalFields>";
			var ourContent = commonAncestor.Replace("<CustomField class='WfiWordform' key='WfiWordformAttested' name='Attested' type='Boolean' />", null);
			var theirContent = commonAncestor.Replace("<CustomField class='WfiWordform' key='WfiWordformAttested' name='Attested' type='Boolean' />", null);

			FieldWorksTestServices.DoMerge(
				_fileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@key=""WfiWordformCertified""]" },
				new List<string> { @"AdditionalFields/CustomField[@key=""WfiWordformAttested""]" },
				0, new List<Type>(),
				1, new List<Type> { typeof(XmlDeletionChangeReport) });
		}

		[Test]
		public void WinnerAndLoserBothMadeSameChangeToElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
</AdditionalFields>";
			var ourContent = commonAncestor.Replace("Boolean", "Integer");
			var theirContent = commonAncestor.Replace("Boolean", "Integer");

			FieldWorksTestServices.DoMerge(
				_fileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@type=""Integer""]" },
				new List<string> { @"AdditionalFields/CustomField[@type=""Boolean""]" },
				0, new List<Type>(),
				1, new List<Type> { typeof(XmlChangedRecordReport) });
		}

		[Test]
		public void WinnerAndLoserBothChangedElementButInDifferentWays()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
</AdditionalFields>";
			var ourContent = commonAncestor.Replace("Boolean", "Integer");
			var theirContent = commonAncestor.Replace("Boolean", "Binary");

			FieldWorksTestServices.DoMerge(
				_fileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@type=""Integer""]" },
				new List<string> { @"AdditionalFields/CustomField[@type=""Binary""]" },
				1, new List<Type> { typeof(BothEditedAttributeConflict) },
				0, new List<Type>());
		}

		[Test]
		public void WinnerChangedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
</AdditionalFields>";
			var ourContent = commonAncestor.Replace("Boolean", "Integer");
			const string theirContent = commonAncestor;

			FieldWorksTestServices.DoMerge(
				_fileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@type=""Integer""]" },
				null,
				0, new List<Type>(),
				1, new List<Type> { typeof(XmlChangedRecordReport) });
		}

		[Test]
		public void LoserChangedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
</AdditionalFields>";
			const string ourContent = commonAncestor;
			var theirContent = commonAncestor.Replace("Boolean", "Integer");

			FieldWorksTestServices.DoMerge(
				_fileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@type=""Integer""]" },
				null,
				0, new List<Type>(),
				1, new List<Type> { typeof(XmlChangedRecordReport) });
		}

		[Test]
		public void WinnerEditedButLoserDeletedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
<CustomField class='WfiWordform' key='WfiWordformAttested' name='Attested' type='Binary' />
</AdditionalFields>";
			var ourContent = commonAncestor.Replace("Binary", "Integer");
			var theirContent = commonAncestor.Replace("<CustomField class='WfiWordform' key='WfiWordformAttested' name='Attested' type='Binary' />", null);

			FieldWorksTestServices.DoMerge(
				_fileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@key=""WfiWordformCertified""]", @"AdditionalFields/CustomField[@key=""WfiWordformAttested""]" },
				null,
				1, new List<Type> { typeof(EditedVsRemovedElementConflict) },
				0, new List<Type>());
		}

		[Test]
		public void WinnerDeletedButLoserEditedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
<CustomField class='WfiWordform' key='WfiWordformAttested' name='Attested' type='Binary' />
</AdditionalFields>";
			var ourContent = commonAncestor.Replace("<CustomField class='WfiWordform' key='WfiWordformAttested' name='Attested' type='Binary' />", null);
			var theirContent = commonAncestor.Replace("Binary", "Integer");

			FieldWorksTestServices.DoMerge(
				_fileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@key=""WfiWordformCertified""]", @"AdditionalFields/CustomField[@key=""WfiWordformAttested""]" },
				null,
				1, new List<Type> { typeof(RemovedVsEditedElementConflict) },
				0, new List<Type>());
		}
	}
}
