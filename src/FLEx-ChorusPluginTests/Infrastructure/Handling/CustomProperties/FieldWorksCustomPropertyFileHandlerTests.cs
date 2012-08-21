using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Chorus.FileTypeHanders;
using Chorus.FileTypeHanders.xml;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using LibChorus.TestUtilities;
using NUnit.Framework;
using Palaso.IO;
using Palaso.Progress.LogBox;

namespace FLEx_ChorusPluginTests.Infrastructure.Handling.CustomProperties
{
	/// <summary>
	/// Test the FW custom property file handler.
	/// </summary>
	[TestFixture]
	public class FieldWorksCustomPropertyFileHandlerTests : BaseFieldWorksTypeHandlerTests
	{
		private TempFile _ourFile;
		private TempFile _theirFile;
		private TempFile _commonFile;

		[SetUp]
		public void TestSetup()
		{
			FieldWorksTestServices.SetupTempFilesWithName(SharedConstants.CustomPropertiesFilename, out _ourFile, out _commonFile, out _theirFile);
		}

		[TearDown]
		public void TestTearDown()
		{
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
		public void ExtensionOfKnownFileTypesShouldBeCustomProperties()
		{
			var extensions = FileHandler.GetExtensionsOfKnownTextFileTypes().ToArray();
			Assert.AreEqual(FieldWorksTestServices.ExpectedExtensionCount, extensions.Count(), "Wrong number of extensions.");
			Assert.IsTrue(extensions.Contains(SharedConstants.CustomProperties));
		}

		[Test]
		public void ShouldBeAbleToValidateIncorrectFormatFile()
		{
			const string data = "<classdata />";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
		}

		[Test]
		public void ShouldNotBeAbleToValidateIncorrectFormatFile()
		{
			const string data = "<classdata />";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldBeAbleToValidateImproperlyFormattedFile()
		{
			const string data = @"<AdditionalFields>
<CustomField name='Certified' class='WfiWordform' key='WfiWordformCertified' type='Boolean' />
</AdditionalFields>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
		}

		[Test]
		public void ShouldBeAbleToValidateFileWithOnlyRequiredAttributes()
		{
			const string data = @"<AdditionalFields>
<CustomField name='Certified' class='WfiWordform' key='WfiWordformCertified' type='Boolean' />
</AdditionalFields>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldBeAbleToValidateFileWithAllPossibleAttributes()
		{
			const string data = @"<AdditionalFields>
<CustomField name='Certified' class='WfiWordform' key='WfiWordformCertified' type='Boolean' destclass='LexSense' wsSelector='0' helpString='WhatHelp' listRoot='mylist' label='MyLabel' />
</AdditionalFields>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFileWithUnknownAttribute()
		{
			const string data = @"<AdditionalFields>
<CustomField name='Certified' class='WfiWordform' key='WfiWordformCertified' type='Boolean' unknown='mystery' />
</AdditionalFields>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFileWithNonameAttribute()
		{
			const string data = @"<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' type='Boolean' />
</AdditionalFields>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFileWithNoClassAttribute()
		{
			const string data = @"<AdditionalFields>
<CustomField name='Certified' key='WfiWordformCertified' type='Boolean' />
</AdditionalFields>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFileWithNoKeyAttribute()
		{
			const string data = @"<AdditionalFields>
<CustomField name='Certified' class='WfiWordform' type='Boolean' />
</AdditionalFields>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFileWithNoTypeAttribute()
		{
			const string data = @"<AdditionalFields>
<CustomField name='Certified' class='WfiWordform' key='WfiWordformCertified' />
</AdditionalFields>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFileWithKeyAttrMisMatch()
		{
			const string data = @"<AdditionalFields>
<CustomField name='Certified' class='WfiWordform' key='LexEntryCertified' type='Boolean' />
</AdditionalFields>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
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
				repositorySetup.AddAndCheckinFile(SharedConstants.CustomPropertiesFilename, parent);
				repositorySetup.ChangeFileAndCommit(SharedConstants.CustomPropertiesFilename, child, "change it");
				var hgRepository = repositorySetup.Repository;
				var allRevisions = (from rev in hgRepository.GetAllRevisions()
									orderby rev.Number.LocalRevisionNumber
									select rev).ToList();
				var first = allRevisions[0];
				var second = allRevisions[1];
				var firstFiR = hgRepository.GetFilesInRevision(first).First();
				var secondFiR = hgRepository.GetFilesInRevision(second).First();
				var result = FileHandler.Find2WayDifferences(firstFiR, secondFiR, hgRepository).ToList();
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
				FileHandler,
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
				0, new List<Type>());
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
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@key=""WfiWordformCertified""]", @"AdditionalFields/CustomField[@key=""WfiWordformAttested""]" },
				null,
				0, new List<Type>(),
				0, new List<Type>());
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
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@key=""WfiWordformCertified""]", @"AdditionalFields/CustomField[@key=""WfiWordformAttested""]" },
				null,
				0, new List<Type>(),
				0, new List<Type>());
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
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@key=""WfiWordformCertified""]" },
				new List<string> { @"AdditionalFields/CustomField[@key=""WfiWordformAttested""]" },
				0, new List<Type>(),
				0, new List<Type>());
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
				FileHandler,
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
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@key=""WfiWordformCertified""]" },
				new List<string> { @"AdditionalFields/CustomField[@key=""WfiWordformAttested""]" },
				0, new List<Type>(),
				0, new List<Type>());
		}

		[Test]
		public void WinnerAndLoserBothMadeSameChangeToAttribute()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
</AdditionalFields>";
			var ourContent = commonAncestor.Replace("Boolean", "Integer");
			var theirContent = commonAncestor.Replace("Boolean", "Integer");

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@type=""Integer""]" },
				new List<string> { @"AdditionalFields/CustomField[@type=""Boolean""]" },
				0, new List<Type>(),
				0, new List<Type>());
		}

		[Test]
		public void WinnerAndLoserBothChangedAttributeButInDifferentWays()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
</AdditionalFields>";
			var ourContent = commonAncestor.Replace("Boolean", "Integer");
			var theirContent = commonAncestor.Replace("Boolean", "Binary");

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@type=""Integer""]" },
				new List<string> { @"AdditionalFields/CustomField[@type=""Binary""]" },
				1, new List<Type> { typeof(BothEditedTheSameAtomicElement) },
				0, new List<Type>());
		}

		[Test]
		public void WinnerChangedAttribute()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
</AdditionalFields>";
			var ourContent = commonAncestor.Replace("Boolean", "Integer");
			const string theirContent = commonAncestor;

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@type=""Integer""]" },
				null,
				0, new List<Type>(),
				0, new List<Type>());
		}

		[Test]
		public void LoserChangedAttribute()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
</AdditionalFields>";
			const string ourContent = commonAncestor;
			var theirContent = commonAncestor.Replace("Boolean", "Integer");

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@type=""Integer""]" },
				null,
				0, new List<Type>(),
				0, new List<Type>());
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
				FileHandler,
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
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@key=""WfiWordformCertified""]", @"AdditionalFields/CustomField[@key=""WfiWordformAttested""]" },
				null,
				1, new List<Type> { typeof(RemovedVsEditedElementConflict) },
				0, new List<Type>());
		}

		[Test]
		public void CustomFileHasKeyAttributeForEachCustomProperty()
		{
			const string originalCustomData =
@"<AdditionalFields>
	<CustomField class='LexEntry' destclass='7' listRoot='53241fd4-72ae-4082-af55-6b659657083c' name='Tone' type='RC' />
	<CustomField class='LexSense' name='Paradigm' type='String' wsSelector='-2' />
	<CustomField class='WfiWordform' name='Certified' type='Boolean' />
</AdditionalFields>";

			var tempPathname = Path.Combine(Path.GetTempPath(), SharedConstants.CustomPropertiesFilename);
			FileWriterService.WriteCustomPropertyFile(MetadataCache.TestOnlyNewCache, Path.GetTempPath(), Encoding.UTF8.GetBytes(originalCustomData));
			using (var tempFile = TempFile.TrackExisting(tempPathname))
			{
				var doc = XDocument.Load(tempFile.Path);
				Assert.IsTrue(doc.Root.Name.LocalName == "AdditionalFields");
				Assert.AreEqual(3, doc.Root.Elements().Count());
				foreach (var customPropertyDeclaration in doc.Root.Elements())
				{
					Assert.IsNotNull(customPropertyDeclaration.Attribute("key"));
				}
			}
		}
	}
}
