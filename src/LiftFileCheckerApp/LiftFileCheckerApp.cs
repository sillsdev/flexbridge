// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using Chorus.merge.xml.generic;

namespace LiftFileCheckerApp
{
	/*
AddKeyedElementType("entry", "id", false);
AddKeyedElementType("sense", "id", true);
AddKeyedElementType("form", "lang", false); // in entries and in header in <field> element, which is not the same as the field element with the type attr.
AddKeyedElementType("gloss", "lang", false);
AddKeyedElementType("field", "type", false);
	*/
	public partial class LiftFileCheckerApp : Form
	{
		private string _liftPathname;

		public LiftFileCheckerApp()
		{
			InitializeComponent();
			_btnCheckfile.Enabled = false;
		}

		private void BrowseButtonClicked(object sender, EventArgs e)
		{
			_openFileDialog.ShowDialog(this);
			if (_openFileDialog.FileName != null)
			{
				_liftPathname = _openFileDialog.FileName;
				_btnCheckfile.Enabled = true;
			}
			else
			{
				_liftPathname = null;
			}
		}

		private void TestFileButtonClicked(object sender, EventArgs e)
		{
			var sb = new StringBuilder();
			var currentSet = new HashSet<string>();

			var liftDoc = XDocument.Load(_liftPathname);
#if !ORIGINAL
			//liftDoc.Root.Element("header").Remove();
			foreach (var entryElement in liftDoc.Root.Elements("entry").ToArray())
			{
				foreach (var gonerChild in entryElement.Elements().Where(child => child.Name.LocalName != "variant").ToArray())
					gonerChild.Remove();
				if (entryElement.Elements("variant").Count() < 2)
					entryElement.Remove();

				// Check variant elements
				currentSet.Clear();
				var duplicateVariantsAndCounts = new Dictionary<string, List<XElement>>();
				{
					foreach (var variantElement in entryElement.Elements("variant"))
					{
						var currentStr = variantElement.ToString();
						List<XElement> dups;
						if (!duplicateVariantsAndCounts.TryGetValue(currentStr, out dups))
						{
							duplicateVariantsAndCounts.Add(currentStr, new List<XElement>{variantElement});
						}
						else
						{
							dups.AddRange(from duplicateVariantsAndCountTempKvp in duplicateVariantsAndCounts
										  where XmlUtilities.AreXmlElementsEqual(duplicateVariantsAndCountTempKvp.Key, currentStr)
										  select variantElement);
						}
					}
				}
				foreach (var variantKvp in duplicateVariantsAndCounts.Where(variantKvp => variantKvp.Value.Count == 1))
				{
					currentSet.Add(variantKvp.Key);
				}
				foreach (var key in currentSet)
				{
					duplicateVariantsAndCounts[key][0].Remove();
					duplicateVariantsAndCounts.Remove(key);
				}
				if (duplicateVariantsAndCounts.Count > 0)
				{
					entryElement.Attributes().Where(attr => attr.Name.LocalName != "guid").Remove();
					entryElement.Add(new XAttribute("TOTALDUPVARIANTCOUNT", entryElement.Elements("variant").Count()));
					foreach (var dupList in duplicateVariantsAndCounts.Values)
					{
						for (var i = 1; i < dupList.Count; ++i)
						{
							dupList[i].Remove();
						}
						dupList[0].Add(new XAttribute("DUPVARCOUNT", dupList.Count));
					}
				}
			}
			foreach (var gonnerEntry in liftDoc.Root.Elements("entry").Where(entry => !entry.HasElements).ToArray())
				gonnerEntry.Remove();

			liftDoc.Root.Attributes().Remove();
			liftDoc.Root.Add(new XAttribute("ENTRIESWITHDUPVARCOUNT", liftDoc.Root.Elements("entry").Count()));

			liftDoc.Save(_liftPathname.Replace(".lift", "-variants-new.lift"));
#else
#if false
			// Check out header element.
			// For now, only work with root/header/fields/field (<form> elelments).
			foreach (var headerFieldElement in liftDoc.Root.Element("header").Element("fields").Elements("field"))
			{
				var fieldTagAttrValue = headerFieldElement.Attribute("tag").Value;
				currentSet.Clear();
				foreach (var headerFieldFormAttrValue in headerFieldElement.Elements("form").Select(formAltElement => formAltElement.Attribute("lang").Value))
				{
					if (currentSet.Contains(headerFieldFormAttrValue) || currentSet.Contains(headerFieldFormAttrValue.ToLowerInvariant()))
					{
						sb.AppendFormat("Found header field form element with duplicate 'lang' attribute '{0}' in header field with tag '{1}'", headerFieldFormAttrValue, fieldTagAttrValue);
						sb.AppendLine();
					}
					else
					{
						currentSet.Add(headerFieldFormAttrValue);
					}
				}
			}
#endif

			foreach (var entryElement in liftDoc.Root.Elements("entry"))
			{
				if (entryElement.Attribute("dateDeleted") != null)
					continue;

				var entryGuid = entryElement.Attribute("guid").Value;

#if false
				// 1. Check out "form' alts in:
				/*
<lexical-unit>
	<form
		lang="azj-Latn">
		<text>asqır</text>
	</form>
</lexical-unit>
				*/
				currentSet.Clear();
				var lexUnit = entryElement.Element("lexical-unit");
				if (lexUnit != null)
				{
					foreach (var formLang in entryElement.Element("lexical-unit").Elements("form").Select(formAltElement => formAltElement.Attribute("lang").Value))
					{
						if (currentSet.Contains(formLang) || currentSet.Contains(formLang.ToLowerInvariant()))
						{
							sb.AppendFormat("Found lexical-unit form element with duplicate 'lang' attribute '{0}' in entry with guid '{1}'", formLang, entryGuid);
							sb.AppendLine();
						}
						else
						{
							currentSet.Add(formLang);
						}
					}
				}

				// 2. Check out form alts in:
				/*
<citation>
	<form
		lang="azj-Latn">
		<text>asqırmaq</text>
	</form>
</citation>
				*/
				currentSet.Clear();
				var citElement = entryElement.Element("citation");
				if (citElement != null)
				{
					foreach (var formLang in entryElement.Element("citation").Elements("form").Select(formAltElement => formAltElement.Attribute("lang").Value))
					{
						if (currentSet.Contains(formLang) || currentSet.Contains(formLang.ToLowerInvariant()))
						{
							sb.AppendFormat("Found citation form element with duplicate 'lang' attribute '{0}' in entry with guid '{1}'", formLang, entryGuid);
							sb.AppendLine();
						}
						else
						{
							currentSet.Add(formLang);
						}
					}
				}

				// Check out dups in entry level:
#endif
				// Check variant elements
				currentSet.Clear();
				var duplicateVariantsAndCounts = new Dictionary<string, int>(StringComparer.InvariantCulture);
				{
					foreach (var variantElement in entryElement.Elements("variant"))
					{
						var currentStr = variantElement.ToString();
						if (!duplicateVariantsAndCounts.ContainsKey(currentStr))
						{
							duplicateVariantsAndCounts.Add(currentStr, 1);
							continue;
						}
						var duplicateVariantsAndCountsTemp = new Dictionary<string, int>(duplicateVariantsAndCounts);
						foreach (var duplicateVariantsAndCountTempKvp in duplicateVariantsAndCountsTemp)
						{
							var currentCount = duplicateVariantsAndCounts[duplicateVariantsAndCountTempKvp.Key];
							if (XmlUtilities.AreXmlElementsEqual(duplicateVariantsAndCountTempKvp.Key, currentStr))
							{
								duplicateVariantsAndCounts[duplicateVariantsAndCountTempKvp.Key] = currentCount + 1;
							}
						}
					}
				}
				foreach (var variantKvp in duplicateVariantsAndCounts.Where(variantKvp => variantKvp.Value == 1))
				{
					currentSet.Add(variantKvp.Key);
				}
				foreach (var key in currentSet)
					duplicateVariantsAndCounts.Remove(key);
				if (duplicateVariantsAndCounts.Count > 0)
				{
					sb.AppendFormat("Found duplicate variant element(s) in entry with guid '{0}'", entryGuid);
					sb.AppendLine();
					foreach (var variantKvp in duplicateVariantsAndCounts)
					{
						sb.AppendFormat("Duplicate variant element count '{0}' for:", variantKvp.Value);
						sb.AppendLine();
						sb.Append(variantKvp.Key);
						sb.AppendLine();
					}
				}

#if false
				// type attr is a key, so assume multiple entry field elements
				// Assume repeating <form> elments in the <field> element.
				/*
<field type="scientific-name">
<form lang="ru"><text>Rutilus rutilus</text></form>
</field>
				*/
				currentSet.Clear();
				foreach (var entryFieldElement in entryElement.Elements("field"))
				{
					var typeAttrValue = entryFieldElement.Attribute("type").Value;
					if (currentSet.Contains(typeAttrValue) || currentSet.Contains(typeAttrValue.ToLowerInvariant()))
					{
						sb.AppendFormat("Found field element with duplicate 'type' attribute '{0}' in entry with guid '{1}'", typeAttrValue, entryGuid);
						sb.AppendLine();
					}
					else
					{
						currentSet.Add(typeAttrValue);
					}

					// Now check for dup lang attrs on form elements.
					var fieldFormSet = new HashSet<string>();
					foreach (var fieldFormAttrValue in entryFieldElement.Elements("form").Select(formAltElement => formAltElement.Attribute("lang").Value))
					{
						if (fieldFormSet.Contains(fieldFormAttrValue) || fieldFormSet.Contains(fieldFormAttrValue.ToLowerInvariant()))
						{
							sb.AppendFormat("Found field element with duplicate 'lang' attribute in field of type '{0}' with a form 'lang' of '{1}' in entry with guid '{2}'", typeAttrValue, fieldFormAttrValue, entryGuid);
							sb.AppendLine();
						}
						else
						{
							fieldFormSet.Add(fieldFormAttrValue);
						}
					}
				}

				// Check out dup form lang attrs in label of illustration:
				// Assume:
				//	1. multiple <illustration> elements per entry,
				//	2. multiple <label> elements per <illustration> elemtn, and
				//	3. multiple <form> elements per <label> (Only testable keyed element.)
				/*
<illustration href="Rutilusrutilus38cm_2143x1060.JPG">
<label>
<form lang="azj-Latn"><text>külmə</text></form>
<form lang="en"><text>roach, common</text></form>
<form lang="ru"><text>плотва</text></form>
</label>
</illustration>
				*/
				foreach (var illustrationElement in entryElement.Elements("illustration"))
				{
					foreach (var labelElement in illustrationElement.Elements("label"))
					{
						currentSet.Clear();
						foreach (var labelFormAttrValue in labelElement.Elements("form").Select(formAltElement => formAltElement.Attribute("lang").Value))
						{
							if (currentSet.Contains(labelFormAttrValue) || currentSet.Contains(labelFormAttrValue.ToLowerInvariant()))
							{
								sb.AppendFormat("Found field element with duplicate 'lang' attribute in some label of some illustration with a 'lang' attribute of '{0}' in entry with guid '{1}'", labelFormAttrValue, entryGuid);
								sb.AppendLine();
							}
							else
							{
								currentSet.Add(labelFormAttrValue);
							}
						}
					}
				}

				// Check out duplicate sense ids (the sense id attr is what is used in the lift merge code for finding a matching sense.)
				// But a dup guid is just as bad, so report it, too. But then, a sense may not have a guid attr.
				currentSet.Clear();
				foreach (var senseElement in entryElement.Elements("sense"))
				{
					var senseId = senseElement.Attribute("id").Value;
					if (currentSet.Contains(senseId) || currentSet.Contains(senseId.ToLowerInvariant()))
					{
						sb.AppendFormat("Found sense element with duplicate id attribute '{0}' in entry with guid '{1}'", senseId, entryGuid);
						sb.AppendLine();
					}
					else
					{
						currentSet.Add(senseId);
					}

					// Check out duplicate glosses.
					/*
			<gloss
				lang="en">
				<text>to sneeze</text>
			</gloss>
			<gloss
				lang="ru">
				<text>чихать</text>
			</gloss>
					*/
					var glossSet = new HashSet<string>();
					foreach (var glossLangAttrValue in senseElement.Elements("gloss").Select(glossElement => glossElement.Attribute("lang").Value))
					{
						if (glossSet.Contains(glossLangAttrValue) || glossSet.Contains(glossLangAttrValue.ToLowerInvariant()))
						{
							sb.AppendFormat("Found gloss element with duplicate lang attribute '{0}' in sense with id '{1}' in entry with guid '{2}'", glossLangAttrValue, senseId, entryGuid);
							sb.AppendLine();
						}
						else
						{
							glossSet.Add(glossLangAttrValue);
						}
					}

					// Check out duplicate definition forms
					/*
			<definition>
				<form
					lang="en">
					<text>to sneeze</text>
				</form>
				<form
					lang="ru">
					<text>чихать</text>
				</form>
			</definition>
					*/
					var definitionFormsSet = new HashSet<string>();
					foreach (var definitionFormLangAttrValue in senseElement.Elements("definition").Elements("form").Select(glossElement => glossElement.Attribute("lang").Value))
					{
						if (definitionFormsSet.Contains(definitionFormLangAttrValue) || definitionFormsSet.Contains(definitionFormLangAttrValue.ToLowerInvariant()))
						{
							sb.AppendFormat("Found definition form element with duplicate lang attribute '{0}' in sense with id '{1}' in entry with guid '{2}'", definitionFormLangAttrValue, senseId, entryGuid);
							sb.AppendLine();
						}
						else
						{
							definitionFormsSet.Add(definitionFormLangAttrValue);
						}
					}

					// Check out examples.
					// Assumptions:
					//	1. There can be muiltiple examples.
					//	2. Each example can have multiple forms.
					//	3. Each example can have multiple translation elements each of which can have multiple form elements.
					// The assumptions may not hold, but they may flush out more dups.
					/*
<example>
<form lang="azj-Latn"><text></text></form>
<translation>
<form lang="en"><text></text></form>
</translation>
</example>
					*/
					foreach (var exampleElement in senseElement.Elements("example"))
					{
						var exampleFormsSet = new HashSet<string>();
						foreach (var exampleFormLangAttrValue in exampleElement.Elements("form").Select(exampleFormElement => exampleFormElement.Attribute("lang").Value))
						{
							if (exampleFormsSet.Contains(exampleFormLangAttrValue) || exampleFormsSet.Contains(exampleFormLangAttrValue.ToLowerInvariant()))
							{
								sb.AppendFormat("Found example form element with duplicate lang attribute '{0}' in some example in the sense with id '{1}' in entry with guid '{2}'", exampleFormLangAttrValue, senseId, entryGuid);
								sb.AppendLine();
							}
							else
							{
								exampleFormsSet.Add(exampleFormLangAttrValue);
							}
						}
						foreach (var exampleTranslationElement in exampleElement.Elements("translation"))
						{
							var exampleTranslationFormsSet = new HashSet<string>();
							foreach (var exampleTranslationFormLangAttrValue in exampleTranslationElement.Elements("form").Select(exampleTranlationFormElement => exampleTranlationFormElement.Attribute("lang").Value))
							{
								if (exampleTranslationFormsSet.Contains(exampleTranslationFormLangAttrValue) || exampleTranslationFormsSet.Contains(exampleTranslationFormLangAttrValue.ToLowerInvariant()))
								{
									sb.AppendFormat("Found example translation form element with duplicate lang attribute '{0}' in some example's translation in the sense with id '{1}' in entry with guid '{2}'", exampleTranslationFormLangAttrValue, senseId, entryGuid);
									sb.AppendLine();
								}
								else
								{
									exampleTranslationFormsSet.Add(exampleTranslationFormLangAttrValue);
								}
							}
						}
					}
				}
#endif
			}
			var results = sb.ToString();
			if (String.IsNullOrEmpty(results))
			{
				Console.WriteLine("No dups yet.");
			}
			else
			{
				Console.WriteLine("Found dups.");
				Console.Write(results);
			}
#endif
		}
	}
}
