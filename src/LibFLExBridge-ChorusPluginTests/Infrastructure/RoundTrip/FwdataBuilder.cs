// Copyright (c) 2026 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using LibFLExBridgeChorusPlugin.Infrastructure;

namespace LibFLExBridgeChorusPluginTests.Infrastructure.RoundTrip
{
	/// <summary>
	/// Builds a small .fwdata document that the splitter and unifier accept.
	/// </summary>
	/// <remarks>
	/// A new builder already holds the stub objects every project needs: a LangProject that owns a LexDb and
	/// the other objects listed in <see cref="RequiredLangProjectOwnedObjects"/>. Add what a test cares about:
	/// <code>
	/// var fwdata = new FwdataBuilder()
	///     .AddCustomField("LexEntry", "Rank", "Integer")
	///     .AddUnownedObject("LexEntry", EntryGuid, new XElement("Custom", new XAttribute("name", "Rank"), new XAttribute("val", "3")))
	///     .AddOwnedObject(EntryGuid, "Senses", "LexSense", SenseGuid);
	/// </code>
	/// Property content is passed as <see cref="XElement"/>s (or anything else <see cref="XElement.Add(object)"/>
	/// takes), so any property the model knows can be added. Use <see cref="AddRt"/> for an object that
	/// needs full control.
	/// </remarks>
	internal sealed class FwdataBuilder
	{
		internal const string LangProjectGuid = "c1ed46ba-e382-11de-8a39-0800200c9a66";

		/// <summary>
		/// The objects LangProject must own for the splitter and unifier to round trip a project:
		/// (LangProject property, class of the owned object). LangProject must also have an AnalyzingAgents element.
		/// </summary>
		/// <remarks>
		/// Without LexDb, MorphologicalData, MsFeatureSystem, PhFeatureSystem, PhonologicalData or AnalyzingAgents
		/// the split throws. Every real project has those. Without GenreList, TextMarkupTags, TranslationTags or
		/// ResearchNotebook the split succeeds but the unify throws, because the unifier expects a list file or notebook that the
		/// splitter writes only when the object exists.
		/// </remarks>
		internal static readonly IReadOnlyList<KeyValuePair<string, string>> RequiredLangProjectOwnedObjects = new[]
		{
			new KeyValuePair<string, string>("GenreList", "CmPossibilityList"),
			new KeyValuePair<string, string>("LexDb", "LexDb"),
			new KeyValuePair<string, string>("MorphologicalData", "MoMorphData"),
			new KeyValuePair<string, string>("MsFeatureSystem", "FsFeatureSystem"),
			new KeyValuePair<string, string>("PhFeatureSystem", "FsFeatureSystem"),
			new KeyValuePair<string, string>("PhonologicalData", "PhPhonData"),
			new KeyValuePair<string, string>("ResearchNotebook", "RnResearchNbk"),
			new KeyValuePair<string, string>("TextMarkupTags", "CmPossibilityList"),
			new KeyValuePair<string, string>("TranslationTags", "CmPossibilityList")
		};

		private readonly List<XElement> _customFields = new List<XElement>();
		// The <rt> elements, in the order added.
		private readonly List<XElement> _objects = new List<XElement>();
		private readonly string _modelVersion;

		internal FwdataBuilder()
			: this(MetadataCache.MaximumModelVersion)
		{
		}

		internal FwdataBuilder(int modelVersion)
		{
			_modelVersion = modelVersion.ToString();
			AddRt(new XElement(FlexBridgeConstants.RtTag,
				new XAttribute(FlexBridgeConstants.Class, FlexBridgeConstants.LangProject),
				new XAttribute(FlexBridgeConstants.GuidStr, LangProjectGuid),
				new XElement(FlexBridgeConstants.AnalyzingAgents)));
			// Fixed guids keep the split files (for example, which Lexicon_NN.lexdb bucket a guid lands in) stable from run to run.
			var i = 0;
			foreach (var owned in RequiredLangProjectOwnedObjects)
				AddOwnedObject(LangProjectGuid, owned.Key, owned.Value, string.Format("00000000-0000-0000-0000-{0:x12}", ++i));
		}

		/// <summary>The guid of the stub LexDb that LangProject owns.</summary>
		internal string LexDbGuid
		{
			get { return OwnedGuid(LangProjectGuid, FlexBridgeConstants.LexDb); }
		}

		/// <summary>
		/// Declare a custom field in the AdditionalFields element.
		/// <paramref name="type"/> is the fwdata type name, such as "Integer", "String", "MultiUnicode" or "OC".
		/// </summary>
		internal FwdataBuilder AddCustomField(string className, string name, string type, params object[] extraAttributes)
		{
			_customFields.Add(new XElement(FlexBridgeConstants.CustomField,
				new XAttribute(FlexBridgeConstants.Name, name),
				new XAttribute(FlexBridgeConstants.Class, className),
				new XAttribute("type", type),
				extraAttributes));
			return this;
		}

		/// <summary>Add an object with no owner, such as a LexEntry.</summary>
		internal FwdataBuilder AddUnownedObject(string className, string guid, params object[] content)
		{
			return AddRt(NewRt(className, guid, content));
		}

		/// <summary>
		/// Add an object owned by <paramref name="ownerGuid"/> (already added) in its <paramref name="owningProperty"/>.
		/// The owner gets an &lt;objsur t='o'/&gt; in that property, after any it already has.
		/// </summary>
		internal FwdataBuilder AddOwnedObject(string ownerGuid, string owningProperty, string className, string guid, params object[] content)
		{
			var owner = GetRt(ownerGuid);
			var property = owner.Element(owningProperty);
			if (property == null)
			{
				property = new XElement(owningProperty);
				owner.Add(property);
			}
			property.Add(new XElement(FlexBridgeConstants.Objsur,
				new XAttribute(FlexBridgeConstants.GuidStr, guid),
				new XAttribute("t", "o")));

			var rt = NewRt(className, guid, content);
			rt.Add(new XAttribute(FlexBridgeConstants.OwnerGuid, ownerGuid));
			return AddRt(rt);
		}

		/// <summary>Add an &lt;rt&gt; element as is. The caller handles ownerguid and the owner's objsur.</summary>
		internal FwdataBuilder AddRt(XElement rt)
		{
			var guid = rt.Attribute(FlexBridgeConstants.GuidStr).Value;
			if (_objects.Any(obj => SameGuid(obj, guid)))
				throw new ArgumentException("An object with guid " + guid + " was already added.", "rt");
			_objects.Add(rt);
			return this;
		}

		/// <summary>The &lt;rt&gt; element with the given guid, for a test that wants to change a stub object.</summary>
		internal XElement GetRt(string guid)
		{
			var rt = _objects.FirstOrDefault(obj => SameGuid(obj, guid));
			if (rt == null)
				throw new ArgumentException("No object with guid " + guid + " has been added.", "guid");
			return rt;
		}

		internal XDocument Build()
		{
			var root = new XElement("languageproject", new XAttribute("version", _modelVersion));
			if (_customFields.Count > 0)
				root.Add(new XElement(FlexBridgeConstants.AdditionalFieldsTag, _customFields.Select(cf => new XElement(cf))));
			root.Add(_objects.Select(rt => new XElement(rt)));
			return new XDocument(new XDeclaration("1.0", "utf-8", null), root);
		}

		public override string ToString()
		{
			var doc = Build();
			return doc.Declaration + Environment.NewLine + doc;
		}

		private string OwnedGuid(string ownerGuid, string owningProperty)
		{
			return GetRt(ownerGuid).Element(owningProperty).Element(FlexBridgeConstants.Objsur).Attribute(FlexBridgeConstants.GuidStr).Value;
		}

		private static XElement NewRt(string className, string guid, object[] content)
		{
			return new XElement(FlexBridgeConstants.RtTag,
				new XAttribute(FlexBridgeConstants.Class, className),
				new XAttribute(FlexBridgeConstants.GuidStr, guid),
				content);
		}

		private static bool SameGuid(XElement rt, string guid)
		{
			return string.Equals(rt.Attribute(FlexBridgeConstants.GuidStr).Value, guid, StringComparison.OrdinalIgnoreCase);
		}
	}
}
