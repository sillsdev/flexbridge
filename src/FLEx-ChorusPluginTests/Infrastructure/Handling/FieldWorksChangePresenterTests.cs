using System;
using System.IO;
using System.Linq;
using System.Xml;
using Chorus.FileTypeHanders;
using Chorus.FileTypeHanders.xml;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Infrastructure.Handling;
using FLEx_ChorusPluginTests.BorrowedCode;
using FLEx_ChorusPluginTests.Properties;
using NUnit.Framework;

namespace FLEx_ChorusPluginTests.Infrastructure.Handling
{
	/// <summary>
	/// Test the FieldWorks implementation of the IChangePresenter interface.
	/// </summary>
	[TestFixture]
	public class FieldWorksChangePresenterTests
	{
		private IChorusFileTypeHandler _fileHandler;
		private IChangePresenter _changePresenter;
		private string _goodXmlPathname;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_fileHandler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
							where handler.GetType().Name == "FieldWorksCommonFileHandler"
							   select handler).First();
			_changePresenter = new FieldWorksChangePresenter(
				new XmlChangedRecordReport(
					null,
					null,
					null,
					null));
			_goodXmlPathname = Path.ChangeExtension(Path.GetTempFileName(), ".ClassData");
			File.WriteAllText(_goodXmlPathname, TestResources.kXmlHeading + Environment.NewLine + TestResources.kClassDataEmptyTag);
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			_fileHandler = null;
			_changePresenter = null;
			if (File.Exists(_goodXmlPathname))
				File.Delete(_goodXmlPathname);
		}

		[Test]
		public void CannotPresentFileForNonExtantFile()
		{
			Assert.IsFalse(_fileHandler.CanPresentFile("bogusPathname"));
		}

		[Test]
		public void CannotPresentFileForNullPathname()
		{
			Assert.IsFalse(_fileHandler.CanPresentFile(null));
		}

		[Test]
		public void CannotPresentFileForEmptyStringPathname()
		{
			Assert.IsFalse(_fileHandler.CanPresentFile(string.Empty));
		}

		[Test]
		public void CannotPresentFileForNonfwPathname()
		{
			Assert.IsFalse(_fileHandler.CanPresentFile("bogus.txt"));
		}

		[Test]
		public void CanPresentGoodFwXmlFile()
		{
			Assert.IsTrue(_fileHandler.CanPresentFile(_goodXmlPathname));
		}

		[Test]
		public void GetChangePresenterHasThreePresentations()
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
<classdata version='7000016'>
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
				const string stylsheet = @"<style type='text/css'><!--

BODY { font-family: verdana,arial,helvetica,sans-serif; font-size: 12px;}

span.langid {color: 'gray'; font-size: xx-small;position: relative;
	top: 0.3em;
}

span.fieldLabel {color: 'gray'; font-size: x-small;}

div.entry {color: 'blue';font-size: x-small;}

td {font-size: x-small;}

span.en {
color: 'green';
}
span.es {
color: 'green';
}
span.fr {
color: 'green';
}
span.tpi {
color: 'purple';
}

--></style>";
				repositorySetup.AddAndCheckinFile("fwtest.ClassData", parent);
				repositorySetup.ChangeFileAndCommit("fwtest.ClassData", child, "change it");
				var hgRepository = repositorySetup.Repository;
				var allRevisions = (from rev in hgRepository.GetAllRevisions()
									orderby rev.Number.LocalRevisionNumber
									select rev).ToList();
				foreach (var report in _fileHandler.Find2WayDifferences(
					hgRepository.GetFilesInRevision(allRevisions[0]).First(),
					hgRepository.GetFilesInRevision(allRevisions[1]).First(),
					hgRepository))
				{
					IChangePresenter presenter;
					string normalHtml;
					string rawHtml;
					switch (report.GetType().Name)
					{
						case "XmlDeletionChangeReport":
							presenter = _fileHandler.GetChangePresenter(report, hgRepository);
							normalHtml = presenter.GetHtml("normal", stylsheet);
							rawHtml = presenter.GetHtml("raw", stylsheet);
							Assert.AreEqual(normalHtml, rawHtml);
							break;
						case "XmlChangedRecordReport":
							presenter = _fileHandler.GetChangePresenter(report, hgRepository);
							normalHtml = presenter.GetHtml("normal", stylsheet);
							rawHtml = presenter.GetHtml("raw", stylsheet);
							Assert.AreEqual(normalHtml, rawHtml);
							break;
						case "XmlAdditionChangeReport":
							presenter = _fileHandler.GetChangePresenter(report, hgRepository);
							normalHtml = presenter.GetHtml("normal", stylsheet);
							rawHtml = presenter.GetHtml("raw", stylsheet);
							Assert.AreEqual(normalHtml, rawHtml);
							break;
					}
				}
			}
		}

		[Test]
		public void GetDataLabelIsLexEntry()
		{
			var doc = new XmlDocument();
			doc.AppendChild(doc.CreateElement("classdata"));
			var changePresenter = new FieldWorksChangePresenter(
				new XmlChangedRecordReport(
					null,
					null,
					null,
					XmlUtilities.GetDocumentNodeFromRawXml(@"<rt guid='3d9ba4a5-4a25-11df-9879-0800200c9a66' class='LexEntry'/>", doc)));
			Assert.AreEqual("LexEntry", changePresenter.GetDataLabel());
		}

		[Test]
		public void GetActionLabelIsChanged()
		{
			var doc = new XmlDocument();
			doc.AppendChild(doc.CreateElement("classdata"));
			var changePresenter = new FieldWorksChangePresenter(
				new XmlChangedRecordReport(
					null,
					null,
					null,
					XmlUtilities.GetDocumentNodeFromRawXml(@"<rt guid='3d9ba4a5-4a25-11df-9879-0800200c9a66' class='LexEntry'/>", doc)));
			Assert.AreEqual("Changed", changePresenter.GetActionLabel());
		}

		[Test]
		public void GetHtmlHasNullInput()
		{
			Assert.Throws<NullReferenceException>(() => _changePresenter.GetHtml(null, null));
		}

		[Test]
		public void GetTypeLabelIsFieldWorksDataObject()
		{
			var doc = new XmlDocument();
			doc.AppendChild(doc.CreateElement("classdata"));
			var changePresenter = new FieldWorksChangePresenter(
				new XmlChangedRecordReport(
					null,
					null,
					null,
					XmlUtilities.GetDocumentNodeFromRawXml(@"<rt guid='3d9ba4a5-4a25-11df-9879-0800200c9a66' class='LexEntry'/>", doc)));
			Assert.AreEqual("FieldWorks data object", changePresenter.GetTypeLabel());
		}

		[Test]
		public void GetIconNameIsFile()
		{
			Assert.AreEqual("file", _changePresenter.GetIconName());
		}
	}
}