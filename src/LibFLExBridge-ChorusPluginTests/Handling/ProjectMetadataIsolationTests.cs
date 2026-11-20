// Copyright (c) 2026 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Chorus.FileTypeHandlers;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibFLExBridgeChorusPluginTests.Infrastructure.RoundTrip;
using NUnit.Framework;
using SIL.Progress;

namespace LibFLExBridgeChorusPluginTests.Handling
{
	/// <summary>
	/// One process can split and validate several projects (LexBox's FwHeadless runs LfMergeBridge for many
	/// projects in one process). Each project's files must be checked against that project's own custom fields.
	/// </summary>
	[TestFixture]
	public class ProjectMetadataIsolationTests
	{
		private static readonly Guid EntryGuid = new Guid("c1ed46bc-e382-11de-8a39-0800200c9a66");

		private IChorusFileTypeHandler _fileHandler;

		[SetUp]
		public void Setup()
		{
			_fileHandler = FieldWorksTestServices.CreateChorusFileHandlers();
		}

		private static XElement IntegerRank(int value)
		{
			return new XElement("Custom", new XAttribute(FlexBridgeConstants.Name, "Rank"), new XAttribute("val", value));
		}

		private static XElement StringRank(string value)
		{
			return new XElement("Custom", new XAttribute(FlexBridgeConstants.Name, "Rank"),
				new XElement("Str", new XElement("Run", new XAttribute("ws", "en"), value)));
		}

		/// <summary>The basic properties validation requires on every LexEntry.</summary>
		private static object[] RequiredLexEntryProperties()
		{
			return new object[]
			{
				new XElement("DateCreated", new XAttribute("val", "2026-11-20 12:00:00.000")),
				new XElement("DateModified", new XAttribute("val", "2026-11-20 12:00:00.000")),
				new XElement("DoNotUseForParsing", new XAttribute("val", "False")),
				new XElement("HomographNumber", new XAttribute("val", "0"))
			};
		}

		/// <summary>A project whose LexEntry.Rank custom field holds integers.</summary>
		private static FwdataBuilder ProjectWithIntegerRank()
		{
			return new FwdataBuilder()
				.AddCustomField("LexEntry", "Rank", "Integer")
				.AddUnownedObject("LexEntry", EntryGuid, IntegerRank(3), RequiredLexEntryProperties());
		}

		/// <summary>A project whose LexEntry.Rank custom field holds strings.</summary>
		private static FwdataBuilder ProjectWithStringRank()
		{
			return new FwdataBuilder()
				.AddCustomField("LexEntry", "Rank", "String")
				.AddUnownedObject("LexEntry", EntryGuid, StringRank("first"), RequiredLexEntryProperties());
		}

		private static string LexiconFile(SplitUnifyHarness project)
		{
			return project.SplitFileContaining(EntryGuid);
		}

		private string Validate(string pathname)
		{
			return _fileHandler.ValidateFile(pathname, new NullProgress());
		}

		[Test]
		public void ValidateFile_AfterSplittingAnotherProject_UsesItsOwnCustomFieldTypes()
		{
			using (var stringProject = new SplitUnifyHarness(ProjectWithStringRank()))
			using (var integerProject = new SplitUnifyHarness(ProjectWithIntegerRank()))
			{
				stringProject.Split();
				integerProject.Split();

				Assert.That(Validate(LexiconFile(stringProject)), Is.Null, "the String project, split first");
				Assert.That(Validate(LexiconFile(integerProject)), Is.Null, "the Integer project, split last");
				Assert.That(Validate(LexiconFile(stringProject)), Is.Null, "the String project, validated again");
			}
		}

		[Test]
		public void ValidateFile_RelativePath_UsesTheProjectInTheCurrentDirectory()
		{
			// Chorus's CommitCop sets the current directory to the repository and validates relative paths.
			using (var stringProject = new SplitUnifyHarness(ProjectWithStringRank()))
			using (var integerProject = new SplitUnifyHarness(ProjectWithIntegerRank()))
			{
				stringProject.Split();
				integerProject.Split();

				var relativeLexiconPath = LexiconFile(stringProject).Substring(stringProject.ProjectFolder.Length + 1);
				var originalDirectory = Directory.GetCurrentDirectory();
				Directory.SetCurrentDirectory(stringProject.ProjectFolder);
				try
				{
					Assert.That(Validate(relativeLexiconPath), Is.Null);
				}
				finally
				{
					Directory.SetCurrentDirectory(originalDirectory);
				}
			}
		}

		[Test]
		public void ValidateFile_AfterCustomPropertiesFileChanges_UsesTheNewDefinition()
		{
			using (var project = new SplitUnifyHarness(ProjectWithIntegerRank()))
			{
				project.Split();
				Assert.That(Validate(LexiconFile(project)), Is.Null, "before the change");

				// As a pull that changed the field's type and its data would.
				project.EditSplitFile(FlexBridgeConstants.CustomPropertiesFilename, doc =>
					doc.Root.Elements(FlexBridgeConstants.CustomField).Single().Attribute("type").Value = "String");
				project.EditSplitObject(EntryGuid, entry => entry.Element("Custom").ReplaceWith(StringRank("first")));

				Assert.That(Validate(LexiconFile(project)), Is.Null, "after the change");
			}
		}

		[Test]
		public void ValidateFile_DataOfTheWrongType_IsStillReported()
		{
			using (var project = new SplitUnifyHarness(ProjectWithIntegerRank()))
			{
				project.Split();
				project.EditSplitObject(EntryGuid, entry => entry.Element("Custom").ReplaceWith(StringRank("first")));

				Assert.That(Validate(LexiconFile(project)), Is.Not.Null);
			}
		}
	}
}
