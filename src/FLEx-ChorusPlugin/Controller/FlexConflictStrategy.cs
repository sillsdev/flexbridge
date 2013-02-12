using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Chorus.sync;
using FLEx_ChorusPlugin.Infrastructure;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Controller;

namespace FLEx_ChorusPlugin.Controller
{
	[Export(typeof(IConflictStrategy))]
	public class FlexConflictStrategy : IConflictStrategy, IInitConflictStrategy
	{
		#region IConflictStrategy impl

		public ControllerType SupportedControllerAction
		{
			get { return ControllerType.ViewNotes; }
		}

		public Action<ProjectFolderConfiguration> ConfigureProjectFolders
		{
			get { return FlexFolderSystem.ConfigureChorusProjectFolder; }
		}

		public string GetProjectName(string pOption)
		{
			return Path.GetFileNameWithoutExtension(pOption);
		}

		public string GetProjectDir(string pOption)
		{
			return Path.GetDirectoryName(pOption);
		}

		/// <summary>
		/// This method receives the HtmlDetails stored in a conflict, and returns adjusted HTML.
		/// Specifically it fixes URLs containing "database=current" to contain the real project name,
		/// replaces WS identifiers with pretty names, and replaces reference property checksums
		/// with a nice list of reference properties that have conflicts (if any).
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public string AdjustConflictHtml(string input)
		{
			return FixChecksums(FixWsRuns(input)).Replace(@"&amp;database=current&amp;", @"&amp;database=" + _projectName + @"&amp;");
		}

		/// <summary>
		/// We're looking for <div class="checksum">Propname: checksum</div>, with the checksum possibly surrounded
		/// by spans with a style. If the span is present, the named property was changed differently in the two versions.
		/// We want to replace all such divisions in any given parent with a single
		/// <div class="checksum">There were changes to related objects in Propname, Propname...</div>
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		string FixChecksums(string input)
		{
			var root = XElement.Parse(input, LoadOptions.PreserveWhitespace);
			foreach (var parentDiv in DivisionsWithChecksumChildren(root))
			{
				var checksumChildren = (from child in parentDiv.Elements("div")
										where child.Attribute("class") != null && child.Attribute("class").Value == "checksum"
										select child).ToList();
				var conflictPropnames = new List<string>();
				foreach (var child in checksumChildren)
				{
					if (child.Element("span") != null) // only use of spans in checksums is to mark conflicts
						conflictPropnames.Add(ExtractPropName(child));
				}
				var needToRemove = checksumChildren.Skip(1).ToList(); // all but first will surely be removed
				var firstChild = checksumChildren.First();
				if (conflictPropnames.Any())
				{
					firstChild.RemoveNodes();
					firstChild.Add("There were changes to related objects in " + string.Join(", ", conflictPropnames));
				}
				else
				{
					needToRemove.Add(firstChild); // get rid of it too, no conflicts to report.
				}
				foreach (var goner in needToRemove)
				{
					// This is not perfect...there may be trailing white space in a non-empty previous node that is
					// really formatting in front of goner...but it cleans up most of the junk. And currently
					// it is, I believe, only test data that actually has extra white space; for real data
					// we just generate one long line.
					var before = goner.PreviousNode;
					if (before is XText && ((XText)before).Value.Trim() == "")
						before.Remove();
					goner.Remove();
				}
			}
			return root.ToString(SaveOptions.DisableFormatting);
		}

		// Find all div elements with at least one direct child that is a div with class checksum
		IEnumerable<XElement> DivisionsWithChecksumChildren(XElement root)
		{
			return from div in root.Descendants("div")
				   where (from child in div.Elements("div")
						  where child.Attribute("class") != null && child.Attribute("class").Value == "checksum"
						  select child).Any()
				   select div;
		}

		string ExtractPropName(XElement elt)
		{
			var text = elt.Value;
			int index = text.IndexOf(':');
			if (index < 0)
				return ""; // paranoia
			return text.Substring(0, index);
		}

		/// <summary>
		/// Looks for something like <span class="ws">en-fonipa</span>,
		/// and we can find a corresponding writing system in the project and extract its user-friendly name,
		/// replace the run content with the user-friendly name.
		/// There may be another span wrapped around the name inside it, for example,
		/// <span class='ws'><span style='background: Yellow'>en-fonipa</span></span>
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		string FixWsRuns(string input)
		{
			var pattern = new Regex("<span class=\"ws\">(<span[^>]*>)?([^<]+)</span>");
			string current = input;
			if (_projectDir == null)
				return input;
			var wspath = Path.Combine(_projectDir, @"WritingSystemStore");
			if (!Directory.Exists(wspath))
				return input; // can't translate.
			Match match;
			int startAt = 0;
			while (startAt < current.Length && (match = pattern.Match(current, startAt)).Success)
			{
				var targetGroup = match.Groups[2];
				string ws = targetGroup.Value;
				startAt = match.Index + match.Length; // don't match this again, even if we can't improve it.
				string ldmlpath = Path.ChangeExtension(Path.Combine(wspath, ws), "ldml");
				if (!File.Exists(ldmlpath))
					continue; // can't improve this one
				try
				{
					var ldmlElt = XDocument.Load(ldmlpath).Root;
					var special = ldmlElt.Element(@"special");
					if (special == null)
						continue;
					XNamespace palaso = @"urn://palaso.org/ldmlExtensions/v1";
					var langName = GetSpecialValue(ldmlElt, palaso, "languageName");
					if (langName == null)
						continue;
					var mods = new List<string>();
					XNamespace fw = @"urn://fieldworks.sil.org/ldmlExtensions/v1";
					if (!targetGroup.Value.Contains("Zxxx")) // suppress "Code for unwritten documents"
						AddSpecialValue(ldmlElt, fw, "scriptName", mods);
					AddSpecialValue(ldmlElt, fw, "regionName", mods);
					AddSpecialValue(ldmlElt, fw, "variantName", mods);
					string niceName = langName;
					if (mods.Count > 0)
					{
						niceName += " (";
						niceName += string.Join(", ", mods.ToArray());
						niceName += ")";
					}
					var beforeMatch = targetGroup.Index;
					var matchLength = targetGroup.Length;
					current = current.Substring(0, beforeMatch) + niceName + current.Substring(beforeMatch + matchLength);
					startAt += niceName.Length - matchLength;
				}
				catch (XmlException)
				{
					continue; // ignore any parsing errors, just can't improve this one
				}
			}
			return current;
		}

		void AddSpecialValue(XElement root, XNamespace ns, string eltName, List<string> collector)
		{
			var item = GetSpecialValue(root, ns, eltName);
			if (item != null)
				collector.Add(item);
		}

		string GetSpecialValue(XElement root, XNamespace ns, string eltName)
		{
			foreach (var elt in root.Elements("special"))
			{
				var targetElt = elt.Element(ns + eltName);
				if (targetElt == null)
					continue;
				var valueAttr = targetElt.Attribute("value");
				if (valueAttr == null)
					continue;
				return valueAttr.Value;
			}
			return null;
		}

		#endregion IConflictStrategy impl

		private string _projectDir ;
		private string _projectName;

		public void SetProjectName(string name)
		{
			_projectName = name;
		}

		public void SetProjectDir(string name)
		{
			_projectDir = name;
		}
	}
}