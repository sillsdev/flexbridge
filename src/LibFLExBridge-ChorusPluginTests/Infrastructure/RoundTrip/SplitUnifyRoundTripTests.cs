// Copyright (c) 2026 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Xml.Linq;
using LibFLExBridgeChorusPlugin.Infrastructure;
using NUnit.Framework;

namespace LibFLExBridgeChorusPluginTests.Infrastructure.RoundTrip
{
	/// <summary>
	/// Split a small project, optionally change the split files, unify, and check the rejoined .fwdata.
	/// See <see cref="SplitUnifyHarness"/> for how to add a test for a new edge case.
	/// </summary>
	[TestFixture]
	public class SplitUnifyRoundTripTests
	{
		private static readonly Guid EntryGuid = new Guid("c1ed46bc-e382-11de-8a39-0800200c9a66");
		private static readonly Guid SenseGuid = new Guid("c1ed46bd-e382-11de-8a39-0800200c9a66");
		private static readonly Guid SubsenseGuid = new Guid("c1ed46be-e382-11de-8a39-0800200c9a66");

		private static XElement CustomInteger(string name, int value)
		{
			return new XElement("Custom", new XAttribute(FlexBridgeConstants.Name, name), new XAttribute("val", value));
		}

		[Test]
		public void RoundTrip_MinimalProject_KeepsObjectsAndCustomFields()
		{
			var fwdata = new FwdataBuilder()
				.AddCustomField("LexEntry", "Rank", "Integer")
				.AddCustomField("LexSense", "Notes", "MultiUnicode", new XAttribute("wsSelector", "-1"))
				.AddUnownedObject("LexEntry", EntryGuid);

			using (var project = new SplitUnifyHarness(fwdata))
			{
				var rejoined = project.RoundTrip();

				FwdataAssert.SameObjects(project.Original, rejoined);
				FwdataAssert.SameCustomFields(project.Original, rejoined);
			}
		}

		[Test]
		public void RoundTrip_MinimalProject_KeepsObjectContents()
		{
			var fwdata = new FwdataBuilder()
				.AddCustomField("LexEntry", "Rank", "Integer")
				.AddUnownedObject("LexEntry", EntryGuid, CustomInteger("Rank", 3))
				.AddOwnedObject(EntryGuid, "Senses", "LexSense", SenseGuid);

			using (var project = new SplitUnifyHarness(fwdata))
			{
				FwdataAssert.SameObjectContents(project.Original, project.RoundTrip());
			}
		}

		[Test]
		public void RoundTrip_LexEntryCustomFieldValue_Survives()
		{
			var fwdata = new FwdataBuilder()
				.AddCustomField("LexEntry", "Rank", "Integer")
				.AddUnownedObject("LexEntry", EntryGuid, CustomInteger("Rank", 3));

			using (var project = new SplitUnifyHarness(fwdata))
			{
				var entry = SplitUnifyHarness.ObjectsByGuid(project.RoundTrip())[EntryGuid];

				var custom = entry.Element("Custom");
				Assert.That(custom, Is.Not.Null);
				Assert.That(custom.Attribute(FlexBridgeConstants.Name).Value, Is.EqualTo("Rank"));
				Assert.That(custom.Attribute("val").Value, Is.EqualTo("3"));
			}
		}

		[Test]
		public void RoundTrip_NestedOwnedObjects_KeepTheirOwners()
		{
			var fwdata = new FwdataBuilder()
				.AddUnownedObject("LexEntry", EntryGuid)
				.AddOwnedObject(EntryGuid, "Senses", "LexSense", SenseGuid)
				.AddOwnedObject(SenseGuid, "Senses", "LexSense", SubsenseGuid);

			using (var project = new SplitUnifyHarness(fwdata))
			{
				var objects = SplitUnifyHarness.ObjectsByGuid(project.RoundTrip());

				Assert.That(new Guid(objects[SenseGuid].Attribute(FlexBridgeConstants.OwnerGuid).Value), Is.EqualTo(EntryGuid));
				Assert.That(new Guid(objects[SubsenseGuid].Attribute(FlexBridgeConstants.OwnerGuid).Value), Is.EqualTo(SenseGuid));
				Assert.That(SplitUnifyHarness.GuidOf(objects[EntryGuid].Element("Senses").Element(FlexBridgeConstants.Objsur)),
					Is.EqualTo(SenseGuid));
				Assert.That(SplitUnifyHarness.GuidOf(objects[SenseGuid].Element("Senses").Element(FlexBridgeConstants.Objsur)),
					Is.EqualTo(SubsenseGuid));
			}
		}

		[Test]
		public void Unify_EditedSplitObject_ReachesFwdata()
		{
			var fwdata = new FwdataBuilder()
				.AddCustomField("LexEntry", "Rank", "Integer")
				.AddUnownedObject("LexEntry", EntryGuid, CustomInteger("Rank", 3));

			using (var project = new SplitUnifyHarness(fwdata))
			{
				project.Split();
				project.EditSplitObject(EntryGuid, entry => entry.Element("Custom").Attribute("val").Value = "7");
				var entryAfter = SplitUnifyHarness.ObjectsByGuid(project.Unify())[EntryGuid];

				Assert.That(entryAfter.Element("Custom").Attribute("val").Value, Is.EqualTo("7"));
			}
		}
	}
}
