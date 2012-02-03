using System;
using System.IO;
using System.Linq;
using Chorus.FileTypeHanders;
using Chorus.FileTypeHanders.xml;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPluginTests.BorrowedCode;
using FLEx_ChorusPluginTests.Properties;
using NUnit.Framework;
using Palaso.IO;

namespace FLEx_ChorusPluginTests.Infrastructure.Handling
{
	/// <summary>
	/// Test the FieldWorksFileHandler implementation of the IChorusFileTypeHandler interface.
	/// </summary>
	[TestFixture]
	public class FieldWorksFileDiffTests
	{
		private IChorusFileTypeHandler _fileHandler;
		private string _goodXmlPathname;
		private string _tempfilePathname;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_fileHandler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
							where handler.GetType().Name == "FieldWorksCommonFileHandler"
							   select handler).First();
			_tempfilePathname = Path.GetTempFileName();
			_goodXmlPathname = Path.ChangeExtension(_tempfilePathname, ".ClassData");
			File.WriteAllText(_goodXmlPathname, TestResources.kXmlHeading + Environment.NewLine + TestResources.kClassDataEmptyTag);
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			_fileHandler = null;
			if (File.Exists(_tempfilePathname))
				File.Delete(_tempfilePathname);
			if (File.Exists(_goodXmlPathname))
				File.Delete(_goodXmlPathname);
		}

		[Test]
		public void CannotDiffNonexistantFile()
		{
			Assert.IsFalse(_fileHandler.CanDiffFile("bogusPathname"));
		}

		[Test]
		public void CannotDiffNullFile()
		{
			Assert.IsFalse(_fileHandler.CanDiffFile(null));
		}

		[Test]
		public void CannotDiffEmptyStringFile()
		{
			Assert.IsFalse(_fileHandler.CanDiffFile(String.Empty));
		}

		[Test]
		public void CanDiffGoodFwXmlFile()
		{
			Assert.IsTrue(_fileHandler.CanDiffFile(_goodXmlPathname));
		}

		// This test is of a series that test:
		//	DONE: 1. No changes (Is this needed? If Hg can commit unchanged files, probably.)
		//	DONE: 2. <rt> element added to child.
		//	DONE: 3. <rt> element removed from child.
		//	DONE: 4. <rt> element changed in child.
		//	5. Custom field stuff.
		//	6. Model version same or different. (Child is lower number?)
		[Test]
		public void NewObjectInChildReported()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt guid='3d9b7d90-4a25-11df-9879-0800200c9a66'>
</rt>
<rt guid='3d9ba4a0-4a25-11df-9879-0800200c9a66'>
</rt>
</classdata>";
			// Third <rt> element (3d9ba4a1-4a25-11df-9879-0800200c9a66) is new.
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt guid='3d9b7d90-4a25-11df-9879-0800200c9a66'>
</rt>
<rt guid='3d9ba4a0-4a25-11df-9879-0800200c9a66'>
</rt>
<rt guid='3d9ba4a1-4a25-11df-9879-0800200c9a66'>
</rt>
</classdata>";
			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var listener = new ListenerForUnitTests();
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path, listener,
					null,
															 SharedConstants.RtTag,
															 SharedConstants.GuidStr);
				differ.ReportDifferencesToListener();
				listener.AssertExpectedChangesCount(1);
				listener.AssertFirstChangeType<XmlAdditionChangeReport>();
			}
		}

		[Test]
		public void GuidCaseDifferenceNotReported()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt guid='3d9b7d90-4a25-11df-9879-0800200c9a66'>
</rt>
</classdata>";
			// Third <rt> element (3d9ba4a1-4a25-11df-9879-0800200c9a66) is new.
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt guid='3D9B7D90-4A25-11DF-9879-0800200C9A66'>
</rt>
</classdata>";
			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var listener = new ListenerForUnitTests();
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path, listener,
					null,
															 SharedConstants.RtTag,
															 SharedConstants.GuidStr);

				// TODO: It would be nice if case on a guid didn't matter,
				// TODO: but that would require the basic XML diff code to know the id's attribute,
				// TODO: and that it was a guid.
				differ.ReportDifferencesToListener();
				listener.AssertExpectedChangesCount(1);
				listener.AssertFirstChangeType<XmlChangedRecordReport>();
			}
		}

		[Test]
		public void ObjectDeletedInChildReported()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt guid='3d9b7d90-4a25-11df-9879-0800200c9a66'>
</rt>
<rt guid='3d9ba4a0-4a25-11df-9879-0800200c9a66'>
</rt>
<rt guid='3d9ba4a1-4a25-11df-9879-0800200c9a66'>
</rt>
</classdata>";
			// Third <rt> element (3d9ba4a1-4a25-11df-9879-0800200c9a66) in parent is removed in child.
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt guid='3d9b7d90-4a25-11df-9879-0800200c9a66'>
</rt>
<rt guid='3d9ba4a0-4a25-11df-9879-0800200c9a66'>
</rt>
</classdata>";
			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var listener = new ListenerForUnitTests();
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path, listener,
					null,
															 SharedConstants.RtTag,
															 SharedConstants.GuidStr);
				differ.ReportDifferencesToListener();
				listener.AssertExpectedChangesCount(1);
				listener.AssertFirstChangeType<XmlDeletionChangeReport>();
			}
		}

		[Test]
		public void ObjectChangedInChildReported()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt guid='3d9b7d90-4a25-11df-9879-0800200c9a66' ownerguid='3d9ba4a2-4a25-11df-9879-0800200c9a66'>
</rt>
</classdata>";
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt guid='3d9b7d90-4a25-11df-9879-0800200c9a66' ownerguid='3d9ba4a3-4a25-11df-9879-0800200c9a66'>
</rt>
</classdata>";
			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var listener = new ListenerForUnitTests();
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path, listener,
					null,
															 SharedConstants.RtTag,
															 SharedConstants.GuidStr);
				differ.ReportDifferencesToListener();
				listener.AssertExpectedChangesCount(1);
				listener.AssertFirstChangeType<XmlChangedRecordReport>();
			}
		}

		[Test]
		public void NoChangesInChildReported()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt guid='3d9b7d90-4a25-11df-9879-0800200c9a66'>
</rt>
<rt guid='3d9ba4a0-4a25-11df-9879-0800200c9a66'>
</rt>
<rt guid='3d9ba4a1-4a25-11df-9879-0800200c9a66'>
</rt>
</classdata>";
			// <rt> elements reordered, but no changes in any of them.
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt guid='3d9ba4a1-4a25-11df-9879-0800200c9a66'>
</rt>
<rt guid='3d9ba4a0-4a25-11df-9879-0800200c9a66'>
</rt>
<rt guid='3d9b7d90-4a25-11df-9879-0800200c9a66'>
</rt>
</classdata>";
			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var listener = new ListenerForUnitTests();
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path, listener,
					null,
															 SharedConstants.RtTag,
															 SharedConstants.GuidStr);
				differ.ReportDifferencesToListener();
				listener.AssertExpectedChangesCount(0);
			}
		}

		[Test]
		public void Find2WayDifferencesReportedThreeChanges()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt guid='3d9ba4a4-4a25-11df-9879-0800200c9a66'>
</rt>
<rt guid='3d9b7d90-4a25-11df-9879-0800200c9a66'>
</rt>
<rt guid='3d9ba4a0-4a25-11df-9879-0800200c9a66'>
</rt>
<rt guid='3d9ba4a1-4a25-11df-9879-0800200c9a66'>
</rt>
<rt guid='3d9ba4a6-4a25-11df-9879-0800200c9a66' ownerguid='3d9ba4a7-4a25-11df-9879-0800200c9a66'>
</rt>
</classdata>";
			// One deletion, one change, one insertion, and three reordered, but not changed.
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt guid='3d9ba4a1-4a25-11df-9879-0800200c9a66'>
</rt>
<rt guid='3d9ba4a0-4a25-11df-9879-0800200c9a66'>
</rt>
<rt guid='3d9b7d90-4a25-11df-9879-0800200c9a66'>
</rt>
<rt guid='3d9ba4a5-4a25-11df-9879-0800200c9a66'>
</rt>
<rt guid='3d9ba4a6-4a25-11df-9879-0800200c9a66' ownerguid='3d9ba4a8-4a25-11df-9879-0800200c9a66'>
</rt>
</classdata>";
			using (var repositorySetup = new RepositorySetup("randy"))
			{
				repositorySetup.AddAndCheckinFile("fwtest.ClassData", parent);
				repositorySetup.ChangeFileAndCommit("fwtest.ClassData", child, "change it");
				var hgRepository = repositorySetup.Repository;
				var allRevisions = (from rev in hgRepository.GetAllRevisions()
									   orderby rev.Number.LocalRevisionNumber
									   select rev).ToList();
				var first = allRevisions[0];
				var second = allRevisions[1];
				var firstFiR = hgRepository.GetFilesInRevision(first).First();
				var secondFiR = hgRepository.GetFilesInRevision(second).First();
				var result = _fileHandler.Find2WayDifferences(firstFiR, secondFiR, hgRepository);
				Assert.AreEqual(3, result.Count());
			}
		}
	}
}