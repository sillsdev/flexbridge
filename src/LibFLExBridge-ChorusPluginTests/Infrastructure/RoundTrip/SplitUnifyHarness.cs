// Copyright (c) 2026 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LibFLExBridgeChorusPlugin.DomainServices;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibTriboroughBridgeChorusPlugin;
using SIL.Progress;
using SIL.TestUtilities;

namespace LibFLExBridgeChorusPluginTests.Infrastructure.RoundTrip
{
	/// <summary>
	/// Runs <see cref="FLExProjectSplitter.PushHumptyOffTheWall"/> and <see cref="FLExProjectUnifier.PutHumptyTogetherAgain"/>
	/// on a .fwdata file in a temporary folder.
	/// </summary>
	/// <remarks>
	/// To add a test for a new split/unify edge case:
	/// <list type="number">
	/// <item>Describe the smallest project that shows the case with a <see cref="FwdataBuilder"/>.</item>
	/// <item>Create a harness from it in a using block. The harness starts and ends with a fresh MetadataCache,
	/// because the cache is process-wide and split and unify both add the project's custom fields to it.</item>
	/// <item>Call <see cref="Split"/>, then change the split files if the case needs it (as a merge would):
	/// <see cref="EditSplitObject"/> for one object found by guid, or <see cref="EditSplitFile"/> for a whole file,
	/// such as <see cref="FlexBridgeConstants.CustomPropertiesFilename"/>.</item>
	/// <item>Call <see cref="Unify"/>, which returns the rejoined .fwdata, and assert on it.
	/// <see cref="FwdataAssert"/> compares objects by guid; <see cref="ObjectsByGuid"/> gives direct access.</item>
	/// </list>
	/// <code>
	/// using (var project = new SplitUnifyHarness(new FwdataBuilder().AddUnownedObject("LexEntry", EntryGuid)))
	/// {
	///     project.Split();
	///     project.EditSplitObject(EntryGuid, entry => entry.Add(new XElement("HomographNumber", new XAttribute("val", "2"))));
	///     var rejoined = project.Unify();
	///     Assert.That(SplitUnifyHarness.ObjectsByGuid(rejoined)[EntryGuid].Element("HomographNumber"), Is.Not.Null);
	/// }
	/// </code>
	/// </remarks>
	internal sealed class SplitUnifyHarness : IDisposable
	{
		private readonly TemporaryFolder _projectFolder;

		internal SplitUnifyHarness(FwdataBuilder fwdata)
			: this(fwdata.ToString())
		{
		}

		internal SplitUnifyHarness(string fwdataContents)
		{
			MetadataCache = MetadataCache.TestOnlyNewCache;
			_projectFolder = new TemporaryFolder("SplitUnifyHarness_" + Guid.NewGuid().ToString("N"));
			FwdataPathname = Path.Combine(_projectFolder.Path, "Proj" + LibTriboroughBridgeSharedConstants.FwXmlExtension);
			File.WriteAllText(FwdataPathname, fwdataContents);
			Original = XDocument.Parse(fwdataContents);
		}

		/// <summary>The cache split and unify use. It is reset when the harness is created and disposed.</summary>
		internal MetadataCache MetadataCache { get; private set; }

		internal string ProjectFolder
		{
			get { return _projectFolder.Path; }
		}

		internal string FwdataPathname { get; private set; }

		/// <summary>The .fwdata the harness started with.</summary>
		internal XDocument Original { get; private set; }

		internal void Split()
		{
			FLExProjectSplitter.PushHumptyOffTheWall(new NullProgress(), false, FwdataPathname);
		}

		/// <summary>Rejoin the split files into the .fwdata file and return it.</summary>
		internal XDocument Unify()
		{
			FLExProjectUnifier.PutHumptyTogetherAgain(new NullProgress(), false, FwdataPathname);
			return ReadFwdata();
		}

		/// <summary>Split, then unify with no changes in between.</summary>
		internal XDocument RoundTrip()
		{
			Split();
			return Unify();
		}

		internal XDocument ReadFwdata()
		{
			return XDocument.Load(FwdataPathname);
		}

		/// <summary>The full pathname of a split file, given its path relative to the project folder.</summary>
		internal string SplitFilePathname(string relativePath)
		{
			return Path.Combine(ProjectFolder, relativePath);
		}

		/// <summary>The XML files the splitter wrote (everything but the .fwdata file and the model version file).</summary>
		internal IEnumerable<string> SplitFiles()
		{
			return Directory.GetFiles(ProjectFolder, "*", SearchOption.AllDirectories)
				.Where(pathname => pathname != FwdataPathname
					&& Path.GetFileName(pathname) != FlexBridgeConstants.ModelVersionFilename)
				.OrderBy(pathname => pathname, StringComparer.OrdinalIgnoreCase);
		}

		/// <summary>Load a split file, let <paramref name="edit"/> change it, and save it.</summary>
		internal void EditSplitFile(string relativePath, Action<XDocument> edit)
		{
			var pathname = SplitFilePathname(relativePath);
			var doc = XDocument.Load(pathname);
			edit(doc);
			doc.Save(pathname);
		}

		/// <summary>
		/// Find the split file holding the object with <paramref name="guid"/>, let <paramref name="edit"/> change
		/// the object's element, and save the file. In split files an object's element is named for its class
		/// (for example &lt;LexEntry guid='...'&gt;), or is &lt;ownseq class='...' guid='...'&gt; in an owning sequence.
		/// </summary>
		internal void EditSplitObject(Guid guid, Action<XElement> edit)
		{
			var pathname = SplitFileContaining(guid);
			var doc = XDocument.Load(pathname);
			edit(FindByGuid(doc, guid).Single());
			doc.Save(pathname);
		}

		/// <summary>The split file holding the object with <paramref name="guid"/>.</summary>
		internal string SplitFileContaining(Guid guid)
		{
			var pathnames = SplitFiles().Where(pathname => FindByGuid(XDocument.Load(pathname), guid).Any()).ToList();
			if (pathnames.Count != 1)
				throw new InvalidOperationException(string.Format("Expected one split file to hold {0}, but found {1}: {2}",
					guid, pathnames.Count, string.Join(", ", pathnames)));
			return pathnames[0];
		}

		/// <summary>The &lt;rt&gt; elements of a .fwdata document, keyed by guid.</summary>
		internal static Dictionary<Guid, XElement> ObjectsByGuid(XDocument fwdata)
		{
			return fwdata.Root.Elements(FlexBridgeConstants.RtTag)
				.ToDictionary(GuidOf);
		}

		public void Dispose()
		{
			_projectFolder.Dispose();
			MetadataCache = MetadataCache.TestOnlyNewCache;
		}

		/// <summary>The guid of an element with a guid attribute, such as an &lt;rt&gt; or &lt;objsur&gt;.</summary>
		internal static Guid GuidOf(XElement element)
		{
			return new Guid(element.Attribute(FlexBridgeConstants.GuidStr).Value);
		}

		private static IEnumerable<XElement> FindByGuid(XDocument doc, Guid guid)
		{
			// <objsur> elements refer to an object; they are not the object.
			return doc.Descendants()
				.Where(element => element.Name.LocalName != FlexBridgeConstants.Objsur
					&& element.Attribute(FlexBridgeConstants.GuidStr) != null
					&& GuidOf(element) == guid);
		}
	}
}
