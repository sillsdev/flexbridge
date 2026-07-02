// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chorus.FileTypeHandlers.xml;
using Chorus.merge.xml.generic;
using LibFLExBridgeChorusPlugin;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.IO;
using SIL.Progress;
using System.Text.RegularExpressions;

namespace LibFLExBridgeChorusPluginTests.Handling.ConfigLayout
{
	[TestFixture]
	public class FieldWorksCustomLayoutTypeHandlerTests : BaseFieldWorksTypeHandlerTests
	{
		private TempFile _ourFile;
		private TempFile _theirFile;
		private TempFile _commonFile;

		[SetUp]
		public override void TestSetup()
		{
			base.TestSetup();
			FieldWorksTestServices.SetupTempFilesWithExtension("." + FlexBridgeConstants.fwlayout, out _ourFile, out _commonFile,
															   out _theirFile);
		}

		[TearDown]
		public override void TestTearDown()
		{
			base.TestTearDown();
			FieldWorksTestServices.RemoveTempFiles(ref _ourFile, ref _commonFile, ref _theirFile);
		}

		[Test]
		public void DescribeInitialContentsShouldHaveAddedForLabel()
		{
			var initialContents = FileHandler.DescribeInitialContents(null, null).ToList();
			Assert.AreEqual(1, initialContents.Count());
			var onlyOne = initialContents.First();
			Assert.AreEqual("Added", onlyOne.ActionLabel);
		}

		[Test]
		public void ExtensionOfKnownFileTypesShouldBefwlayout()
		{
			var extensions = FileHandler.GetExtensionsOfKnownTextFileTypes().ToArray();
			Assert.AreEqual(FieldWorksTestServices.ExpectedExtensionCount, extensions.Count(), "Wrong number of extensions.");
			Assert.IsTrue(extensions.Contains(FlexBridgeConstants.fwlayout));
		}

		[Test]
		public void ShouldBeAbleToValidateIncorrectFormatFileIfFilenameIsRight()
		{
			using (var tempModelVersionFile = new TempFile("<classdata />"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, FlexBridgeConstants.fwlayout);
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsTrue(FileHandler.CanValidateFile(newpath));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldBeAbleToValidateAProperlyFormattedFile()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<LayoutInventory>
  <layout class='CmLocation' type='jtview' name='publishStemLocation#Stem-612' version='19'>
	<part ref='NamePub' label='Name' before='' after=' ' visibility='never' ws='analysis' wsType='vernacular analysis'/>
  </layout>
</LayoutInventory>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
		}

		[Test]
		public void ShouldBeAbleToDoAllCanOperations()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<LayoutInventory>
  <layout class='CmLocation' type='jtview' name='publishStemLocation#Stem-612' version='19'>
	<part ref='NamePub' label='Name' before='' after=' ' visibility='never' ws='analysis' wsType='vernacular analysis'/>
  </layout>
</LayoutInventory>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanDiffFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanMergeFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanPresentFile(_ourFile.Path));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFile()
		{
			const string data = "<?xml version='1.0' encoding='utf-8'?><classdata />";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldBeAbleToValidateFileWithPartAndIndent()
		{
			const string data =
@"<?xml version=""1.0"" encoding=""utf-8""?>
  <LayoutInventory>
  <part ref=""HeavySummary"" param=""Summary"" collapsedLayout=""SummaryCollapsed"" expansion=""expanded"" menu=""mnuDataTree-Sense"" hotlinks=""mnuDataTree-Sense-Hotlinks"" notifyVirtual=""LexSenseOutline"">
	<indent>
		<part ref=""Exemplar"" visibility=""ifdata"" />
		<part ref=""ReversalEntries"" visibility=""ifdata"" />
	</indent>
  </part>
</LayoutInventory>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldBeAbleToValidateFile()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<LayoutInventory>
  <layout class='CmLocation' type='jtview' name='publishStemLocation#Stem-612' version='19'>
	<part ref='NamePub' label='Name' before='' after=' ' visibility='never' ws='analysis' wsType='vernacular analysis'/>
  </layout>
</LayoutInventory>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFileWithMissingClassAttributeForLayout()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<LayoutInventory>
  <layout type='jtview' name='publishStemLocation#Stem-612' version='19'>
	<part ref='NamePub' label='Name' before='' after=' ' visibility='never' ws='analysis' wsType='vernacular analysis'/>
  </layout>
</LayoutInventory>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFileWithMissingTypeAttributeForLayout()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<LayoutInventory>
  <layout class='CmLocation' name='publishStemLocation#Stem-612' version='19'>
	<part ref='NamePub' label='Name' before='' after=' ' visibility='never' ws='analysis' wsType='vernacular analysis'/>
  </layout>
</LayoutInventory>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFileWithMissingNameAttributeForLayout()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<LayoutInventory>
  <layout class='CmLocation' type='jtview' version='19'>
	<part ref='NamePub' label='Name' before='' after=' ' visibility='never' ws='analysis' wsType='vernacular analysis'/>
  </layout>
</LayoutInventory>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFileWithMissingRefAttributeForPart()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<LayoutInventory>
  <layout class='CmLocation' type='jtview' name='publishStemLocation#Stem-612' version='19'>
	<part label='Name' before='' after=' ' visibility='never' ws='analysis' wsType='vernacular analysis'/>
  </layout>
</LayoutInventory>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFileWithMissingNameAttributeForSublayout()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<LayoutInventory>
  <layout class='CmLocation' type='jtview' name='publishStemLocation#Stem-612' version='19'>
	<sublayout group='para' style='Dictionary-Normal'/>
  </layout>
</LayoutInventory>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFileWithMissingAttributeForGenerate()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<LayoutInventory>
  <layout class='CmLocation' type='jtview' name='publishStemLocation#Stem-612' version='19'>
	<generate class='LexExampleSentence' fieldType='mlstring' restrictions='customOnly' />
  </layout>
</LayoutInventory>";

			File.WriteAllText(_ourFile.Path, data.Replace("class='LexExampleSentence' ", null));
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));

			File.WriteAllText(_ourFile.Path, data.Replace("fieldType='mlstring' ", null));
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));

			File.WriteAllText(_ourFile.Path, data.Replace("restrictions='customOnly'", null));
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void SampleDiff()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<LayoutInventory>
  <layout class='CmLocation' type='jtview' name='publishStemLocation#Stem-612' version='19'>
	<generate class='LexExampleSentence' fieldType='mlstring' restrictions='customOnly' />
  </layout>
</LayoutInventory>";

			var child = parent.Replace("19", "20");

			using (var repositorySetup = new RepositorySetup("randy"))
			{
				repositorySetup.AddAndCheckinFile("Sample." + FlexBridgeConstants.fwlayout, parent);
				repositorySetup.ChangeFileAndCommit("Sample." + FlexBridgeConstants.fwlayout, child, "change it");
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
				var onlyReport = result[0];
				Assert.IsInstanceOf<XmlChangedRecordReport>(onlyReport);
				Assert.AreEqual(firstFiR.FullPath, onlyReport.PathToFile);
			}
		}

		[Test]
		public void Diff_RecordTypeLayoutsAreMatchedByChoiceGuid_LT19237()
		{
			// LT-19237 (two-way diff path): the Data Notebook emits one <layout> per record type,
			// all sharing class="RnGenericRec" type="detail" name="Normal" and differing only by
			// choiceGuid. The diff data collector keyed layouts on class+type+name alone, so two
			// record-type layouts collided and ToDictionary threw a duplicate-key ArgumentException
			// when a user viewed history/change reports. The key now includes choiceGuid (matching
			// the merge fix), so each record-type layout is diffed against its own counterpart.
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<LayoutInventory>
  <layout class='RnGenericRec' type='detail' name='Normal' choiceGuid='08e4d456-ce03-4bc1-9231-38caca76b80f' version='25'>
	<part ref='Title' visibility='always' />
	<part ref='Hypothesis' visibility='ifdata' />
  </layout>
  <layout class='RnGenericRec' type='detail' name='Normal' choiceGuid='B7EA5156-EA5E-11DE-9F9C-0013722F8DEC' version='25'>
	<part ref='Title' visibility='always' />
	<part ref='SeeAlso' visibility='always' />
  </layout>
</LayoutInventory>";

			// Edit ONLY the 08e4d456 layout: Hypothesis visibility ifdata->always. ('ifdata' occurs
			// only in that layout, so the B7EA5156 layout is left untouched.)
			var child = parent.Replace("ifdata", "always");

			using (var repositorySetup = new RepositorySetup("randy"))
			{
				repositorySetup.AddAndCheckinFile("RnGenericRec." + FlexBridgeConstants.fwlayout, parent);
				repositorySetup.ChangeFileAndCommit("RnGenericRec." + FlexBridgeConstants.fwlayout, child, "change it");
				var hgRepository = repositorySetup.Repository;
				var allRevisions = (from rev in hgRepository.GetAllRevisions()
									orderby rev.Number.LocalRevisionNumber
									select rev).ToList();
				var firstFiR = hgRepository.GetFilesInRevision(allRevisions[0]).First();
				var secondFiR = hgRepository.GetFilesInRevision(allRevisions[1]).First();

				// Before the fix this call threw ArgumentException (duplicate key) instead of returning.
				var result = FileHandler.Find2WayDifferences(firstFiR, secondFiR, hgRepository).ToList();

				Assert.AreEqual(1, result.Count);
				var onlyReport = result[0];
				Assert.IsInstanceOf<XmlChangedRecordReport>(onlyReport);
				Assert.AreEqual(firstFiR.FullPath, onlyReport.PathToFile);
				// The change is attributed to the edited record type (08e4d456), not the other layout.
				var changedLayout = ((XmlChangedRecordReport)onlyReport).ChildNode;
				Assert.AreEqual("08e4d456-ce03-4bc1-9231-38caca76b80f", changedLayout.Attributes["choiceGuid"].Value);
			}
		}

		[Test]
		public void SampleMergeWithNoConflicts()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<LayoutInventory>
  <layout class='CmLocation' type='jtview' name='publishStemLocation#Stem-612' version='19'>
	<generate class='LexExampleSentence' fieldType='mlstring' restrictions='customOnly' />
  </layout>
</LayoutInventory>";

			var ourContent = commonAncestor.Replace("19", "20");
			const string theirContent = commonAncestor;

			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				null, null,
				0, new List<Type>(),
				1, new List<Type> { typeof(XmlAttributeChangedReport) });
			Assert.IsTrue(results.Contains("20"));
		}

		[Test]
		public void SampleMergeWithConflicts()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<LayoutInventory>
  <layout class='CmLocation' type='jtview' name='publishStemLocation#Stem-612' version='19'>
	<generate class='LexExampleSentence' fieldType='mlstring' restrictions='customOnly' />
  </layout>
</LayoutInventory>";

			var ourContent = commonAncestor.Replace("19", "20");
			var theirContent = commonAncestor.Replace("19", "21");

			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				null, null,
				1, new List<Type> { typeof(BothEditedAttributeConflict) },
				0, new List<Type>());
			Assert.IsTrue(results.Contains("20"));
			Assert.IsFalse(results.Contains("combinedkey"));
		}

		[Test]
		public void Merge_RecordTypeLayoutsAreMatchedByChoiceGuid_LT19237()
		{
			// LT-19237: The Data Notebook writes one <layout> per record type, and every one of
			// them shares class="RnGenericRec" type="detail" name="Normal"; they are distinguished
			// ONLY by the choiceGuid attribute. Before this fix the layout merge key was
			// {class, type, name}, so two record-type layouts were indistinguishable to the merger.
			// When two users had the layouts in a different order and each edited a DIFFERENT
			// record type's layout, the merger cross-matched them, fabricating spurious conflicts,
			// losing most fields from one layout and duplicating the other.
			// The GUIDs, the reversed ordering, and the edited fields below are a trimmed subset of
			// the real Nukak reproduction attached to the ticket.
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<LayoutInventory>
  <layout class='RnGenericRec' type='detail' name='Normal' choiceGuid='08e4d456-ce03-4bc1-9231-38caca76b80f' version='25'>
	<part ref='Title' visibility='always' />
	<part ref='Hypothesis' visibility='ifdata' />
	<part ref='SeeAlso' visibility='always' />
	<part ref='ExternalMaterials' visibility='always' />
  </layout>
  <layout class='RnGenericRec' type='detail' name='Normal' choiceGuid='B7EA5156-EA5E-11DE-9F9C-0013722F8DEC' version='25'>
	<part ref='Title' visibility='always' />
	<part ref='Hypothesis' visibility='ifdata' />
	<part ref='SeeAlso' visibility='always' />
	<part ref='ExternalMaterials' visibility='always' />
	<part ref='Custom' param='Esquema de Materiales Culturales' />
  </layout>
</LayoutInventory>";

			// OURS: the two record-type layouts are in REVERSED order, and only the
			// 08e4d456 layout is edited (Hypothesis visibility ifdata->always, custom field added).
			const string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<LayoutInventory>
  <layout class='RnGenericRec' type='detail' name='Normal' choiceGuid='B7EA5156-EA5E-11DE-9F9C-0013722F8DEC' version='25'>
	<part ref='Title' visibility='always' />
	<part ref='Hypothesis' visibility='ifdata' />
	<part ref='SeeAlso' visibility='always' />
	<part ref='ExternalMaterials' visibility='always' />
	<part ref='Custom' param='Esquema de Materiales Culturales' />
  </layout>
  <layout class='RnGenericRec' type='detail' name='Normal' choiceGuid='08e4d456-ce03-4bc1-9231-38caca76b80f' version='25'>
	<part ref='Title' visibility='always' />
	<part ref='Hypothesis' visibility='always' />
	<part ref='SeeAlso' visibility='always' />
	<part ref='ExternalMaterials' visibility='always' />
	<part ref='Custom' param='Esquema de Materiales Culturales' />
  </layout>
</LayoutInventory>";

			// THEIRS: ancestor order; only the B7EA5156 layout is edited
			// (SeeAlso & ExternalMaterials always->ifdata, custom field gains a visibility).
			const string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<LayoutInventory>
  <layout class='RnGenericRec' type='detail' name='Normal' choiceGuid='08e4d456-ce03-4bc1-9231-38caca76b80f' version='25'>
	<part ref='Title' visibility='always' />
	<part ref='Hypothesis' visibility='ifdata' />
	<part ref='SeeAlso' visibility='always' />
	<part ref='ExternalMaterials' visibility='always' />
  </layout>
  <layout class='RnGenericRec' type='detail' name='Normal' choiceGuid='B7EA5156-EA5E-11DE-9F9C-0013722F8DEC' version='25'>
	<part ref='Title' visibility='always' />
	<part ref='Hypothesis' visibility='ifdata' />
	<part ref='SeeAlso' visibility='ifdata' />
	<part ref='ExternalMaterials' visibility='ifdata' />
	<part ref='Custom' param='Esquema de Materiales Culturales' visibility='ifdata' />
  </layout>
</LayoutInventory>";

			var matchesExactlyOne = new List<string>
			{
				// Exactly one layout survives per record type (the bug duplicated one and mangled the other).
				"LayoutInventory/layout[@choiceGuid='08e4d456-ce03-4bc1-9231-38caca76b80f']",
				"LayoutInventory/layout[@choiceGuid='B7EA5156-EA5E-11DE-9F9C-0013722F8DEC']",
				// Our edit landed in the 08e4d456 layout...
				"LayoutInventory/layout[@choiceGuid='08e4d456-ce03-4bc1-9231-38caca76b80f']/part[@ref='Hypothesis' and @visibility='always']",
				// ...and did NOT bleed into the B7EA5156 layout.
				"LayoutInventory/layout[@choiceGuid='B7EA5156-EA5E-11DE-9F9C-0013722F8DEC']/part[@ref='Hypothesis' and @visibility='ifdata']",
				// Their edits landed in the B7EA5156 layout...
				"LayoutInventory/layout[@choiceGuid='B7EA5156-EA5E-11DE-9F9C-0013722F8DEC']/part[@ref='SeeAlso' and @visibility='ifdata']",
				// ...and did NOT bleed into the 08e4d456 layout.
				"LayoutInventory/layout[@choiceGuid='08e4d456-ce03-4bc1-9231-38caca76b80f']/part[@ref='SeeAlso' and @visibility='always']"
			};

			// A correct merge of edits to DIFFERENT record-type layouts has NO conflicts.
			// (The change-report expectations are pinned from the observed correct merge output.)
			// Each side's edits are applied (none lost): our 08e4d456 layout has one attribute
			// change (Hypothesis) plus one added part (Custom); their B7EA5156 layout has three
			// changes (SeeAlso, ExternalMaterials, Custom visibility). Atomic <part> edits surface
			// as XmlChangedRecordReport; the added <part> as XmlAdditionChangeReport.
			var expectedChanges = new List<Type>
			{
				typeof(XmlChangedRecordReport),
				typeof(XmlChangedRecordReport),
				typeof(XmlChangedRecordReport),
				typeof(XmlChangedRecordReport),
				typeof(XmlAdditionChangeReport)
			};

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				matchesExactlyOne, null,
				0, new List<Type>(),
				expectedChanges.Count, expectedChanges);
		}

		[Test]
		public void SampleMergeWithMissingAncestor()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<LayoutInventory>
  <layout class='CmLocation' type='jtview' name='publishStemLocation#Stem-612' version='19'>
	<generate class='LexExampleSentence' fieldType='mlstring' restrictions='customOnly' />
  </layout>
</LayoutInventory>";

			var ourContent = commonAncestor.Replace("19", "20");
			var theirContent = commonAncestor.Replace("19", "21");

			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				null, "",
				_theirFile, theirContent,
				null, null,
				1, new List<Type> { typeof(BothAddedAttributeConflict) },
				4, new List<Type> { typeof(XmlAttributeBothAddedReport), typeof(XmlAttributeBothAddedReport), typeof(XmlAttributeBothAddedReport), typeof(XmlBothAddedSameChangeReport) });
			Assert.IsTrue(results.Contains("20"));
			Assert.IsFalse(results.Contains("combinedkey"));

		}

		[Test]
		public void SampleMergeWithEmptyAncestor()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<LayoutInventory>
  <layout class='CmLocation' type='jtview' name='publishStemLocation#Stem-612' version='19'>
	<generate class='LexExampleSentence' fieldType='mlstring' restrictions='customOnly' />
  </layout>
</LayoutInventory>";

			var ourContent = commonAncestor.Replace("19", "20");
			var theirContent = commonAncestor.Replace("19", "21");

			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, "",
				_theirFile, theirContent,
				null, null,
				1, new List<Type> { typeof(BothAddedAttributeConflict) },
				4, new List<Type> { typeof(XmlAttributeBothAddedReport), typeof(XmlAttributeBothAddedReport), typeof(XmlAttributeBothAddedReport), typeof(XmlBothAddedSameChangeReport) });
			Assert.IsTrue(results.Contains("20"));
			Assert.IsFalse(results.Contains("combinedkey"));

		}

		[Test]
		public void SampleMergeWithPartAndIndent()
		{
			const string commonAncestor =
@"<?xml version=""1.0"" encoding=""utf-8""?>
  <LayoutInventory>
  <part ref=""HeavySummary"" param=""Summary"" collapsedLayout=""SummaryCollapsed"" expansion=""expanded"" menu=""mnuDataTree-Sense"" hotlinks=""mnuDataTree-Sense-Hotlinks"" notifyVirtual=""LexSenseOutline"">
	<indent>
		<part ref=""Exemplar"" visibility=""ifdata"" />
		<part ref=""ReversalEntries"" visibility=""ifdata"" />
	</indent>
  </part>
</LayoutInventory>";
			const string ourContent =
@"<?xml version=""1.0"" encoding=""utf-8""?>
  <LayoutInventory>
  <part ref=""HeavySummary"" param=""Summary"" collapsedLayout=""SummaryCollapsed"" expansion=""expanded"" menu=""mnuDataTree-Sense"" hotlinks=""mnuDataTree-Sense-Hotlinks"" notifyVirtual=""LexSenseOutline"">
	<indent>
		<part ref=""ReversalEntries"" visibility=""ifdata"" />
		<part ref=""Exemplar"" visibility=""ifdata"" />
	</indent>
  </part>
</LayoutInventory>";

			const string theirContent = commonAncestor;

			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				null, null,
				0, new List<Type>(),
				1, new List<Type> { typeof(XmlChangedRecordReport) });
			string normalizedResults = Regex.Replace(results, @"\s", "");
			string normalizedOurContent = Regex.Replace(ourContent, @"\s", "");
			Assert.AreEqual(normalizedResults, normalizedOurContent);
		}

	}
}