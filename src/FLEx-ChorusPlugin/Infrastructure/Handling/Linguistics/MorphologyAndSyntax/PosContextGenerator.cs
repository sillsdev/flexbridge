using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Properties;
using Palaso.Xml;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.Linguistics.MorphologyAndSyntax
{
	/// <summary>
	/// Context generator for LexEntry/Senses/ownseq/MorphoSyntaxAnalysis element.
	/// It contains a objsur element that refers to the
	/// LexEntry/MorphoSyntaxAnalyses/[SomeMSA]/[To/From]PartOfSpeech/objsur
	/// that refers the part of speech item in the part of speech list
	/// that is in a different file.
	/// The part of speech is now called "Grammatical Info.", by default, but now we look up the
	/// PartsOfSpeech.list file and load it in so we can get names for the parts of speech.
	/// </summary>
	internal sealed class PosContextGenerator : IGenerateContextDescriptor, IGenerateContextDescriptorFromNode, IGenerateHtmlContext
	{
		private string[] _posGuidStrs;
		private Dictionary<string, string> _guidPosNameDict;

		public bool IsListLoaded
		{
			get { return _guidPosNameDict != null && _guidPosNameDict.Count != 0; }
		}

		private string GetLabel(XmlNode start)
		{
			var result = LexEntryName(start) + Space;
			_posGuidStrs = FindPosGuidsInMergeElement(start);
			if (_posGuidStrs[0] == NotLoaded || _posGuidStrs[0] == Unknown)
				return result + DefaultNoName; // missing MorphoSyntaxAnalyses section or <Not sure> category

			var	hasTwoPos = SetTwoPosFlag(_posGuidStrs);
			if (hasTwoPos)
				return result + GetLabelForPos(_posGuidStrs[0]) + "/" + GetLabelForPos(_posGuidStrs[1]);
			return result + GetLabelForPos(_posGuidStrs[0]);
		}

		private bool SetTwoPosFlag(string[] posGuids)
		{
			return posGuids[1] != NotLoaded;
		}

		private const string Space = " ";
		private const string Colon = ":";
		private const string Unknown = "unknown";
		private const string NotLoaded = "not loaded";
		private const string DefaultNoName = "Grammatical Info.";

		static string EntryLabel
		{
			get { return Resources.kLexEntryClassLabel; }
		}

		private string LexEntryName(XmlNode start)
		{
			var entryNode = GetLexEntryNode(start);
			//grab the form from the stem (if available) to give a user understandable message
			var form = entryNode.SelectSingleNode("LexemeForm//Form/AUni");
			return form == null
				? EntryLabel
				: EntryLabel + " \"" + form.InnerText + "\"";
		}

		private string GetLabelForPos(string posGuid)
		{
			var result = DefaultNoName;
			if (!IsListLoaded)
				return result;

			string posName;
			if (_guidPosNameDict.TryGetValue(posGuid, out posName))
				result = posName;
			return result;
		}

		private static XmlNode GetLexEntryNode(XmlNode node)
		{
			return node.Name == SharedConstants.LexEntry ? node : GetLexEntryNode(node.ParentNode);
		}

		/// <summary>
		/// Generate a nice HTML representation of the data that is contained in the mergeElement.
		/// Three versions of the results of this method are compared, and the conflict details report
		/// shows two diffs (ancestor -> ours, ancestor -> theirs). Eventually we hope to be able to
		/// highlight the conflicting changes more boldly.
		/// The results may well display more than just the mergeElement, especially when GenerateContextDescriptor
		/// uses a parent element as the basis for finding the label of what changed. (The same element is passed
		/// to that method as to this for the "ours" case.) One option is to display a complete representation of
		/// the user-recognizable element that the context name is based on. Various defaults are also employed,
		/// to give answers as helpful as possible when we don't have a really pretty one created.
		/// </summary>
		public string HtmlContext(XmlNode mergeElement)
		{
			Debug.Assert(mergeElement != null && mergeElement.ParentNode != null && mergeElement.ParentNode.Name == SharedConstants.Msa);
			// We come in here once each for Ancestor, Ours and Theirs with different mergeElement,
			// so resist the temptation to skip this step if _posGuidStrs != null.
			_posGuidStrs = FindPosGuidsInMergeElement(mergeElement.ParentNode);
			var hasTwoPoses = SetTwoPosFlag(_posGuidStrs);
			var posNames = ConvertGuidStringsToPosNames();

			return string.Format("<div class='"+ SharedConstants.PartOfSpeech+ "'>{0}</div>",
				hasTwoPoses
					? string.Format("Cat: {0}/{1}",posNames[0], posNames[1])
					: string.Format("Cat: {0}", posNames[0]));
		}

		private string[] ConvertGuidStringsToPosNames()
		{
			var result = new string[2];
			result[0] = Unknown;
			if (_posGuidStrs[0] != Unknown && _posGuidStrs[0] != NotLoaded)
				result[0] = GetLabelForPos(_posGuidStrs[0]);

			result[1] = Unknown;
			if (_posGuidStrs[1] != Unknown && _posGuidStrs[1] != NotLoaded)
				result[1] = GetLabelForPos(_posGuidStrs[1]);

			return result;
		}

		private static string[] FindPosGuidsInMergeElement(XmlNode mergeElement)
		{
			var posGuids = new[] { NotLoaded, NotLoaded };
			if (mergeElement != null && mergeElement.Name == SharedConstants.Msa)
			{
				var objsurNode = mergeElement.SelectSingleNode(SharedConstants.Objsur); // Could be null.
				if (objsurNode != null)
				{
					var msaGuid = objsurNode.Attributes[SharedConstants.GuidStr].Value; // guid of the Msa with one or two POSes
					var entryNode = GetLexEntryNode(mergeElement);
					var msa = entryNode.SelectSingleNode(SharedConstants.Msas + "/*[@guid = \"" + msaGuid + "\"]");
					if (msa == null)
						return posGuids; // Can't do anything without a MorphoSyntaxAnalyses section!
					if (msa.Name == SharedConstants.MoDerivAffMsa)
					{
						posGuids[0] = GetPosGuidOrUnknown(msa, SharedConstants.FromPartOfSpeech);
						posGuids[1] = GetPosGuidOrUnknown(msa, SharedConstants.ToPartOfSpeech);
					}
					else
					{
						//All others have 'PartOfSpeech' property.
						posGuids[0] = GetPosGuidOrUnknown(msa, SharedConstants.PartOfSpeech);
					}
				}
			}
			return posGuids;
		}

		private static string GetPosGuidOrUnknown(XmlNode node, string propertyName)
		{
			var propNode = node.SelectSingleNode(propertyName);
			if (propNode == null)
				return Unknown;
			var objsurNode = propNode.SelectSingleNode(SharedConstants.Objsur);
			return objsurNode == null ? Unknown : objsurNode.Attributes[SharedConstants.GuidStr].Value;
		}

		public string HtmlContextStyles(XmlNode mergeElement)
		{
			return "div.alternative {margin-left:  0.25in} div.ws {margin-left:  0.25in} div.property {margin-left:  0.25in} div.checksum {margin-left:  0.25in}";
		}

		public ContextDescriptor GenerateContextDescriptor(string mergeElement, string filePath)
		{
			var doc = new XmlDocument();
			doc.LoadXml(mergeElement);
			return GenerateContextDescriptor(doc.DocumentElement, filePath);
		}

		public ContextDescriptor GenerateContextDescriptor(XmlNode mergeElement, string filePath)
		{
			LoadPosList();
			return FieldWorksMergeStrategyServices.GenerateContextDescriptor(filePath,
																			 FieldWorksMergeStrategyServices.GetGuid(mergeElement),
																			 GetLabel(mergeElement));
		}

		public void LoadPosList()
		{
			if (IsListLoaded)
				return;
			// The broken-up fwdata file should have a PartsOfSpeech list stored under Linguistics/MorphologyAndSyntax.
			var posListDir = Path.Combine(SharedConstants.Linguistics, SharedConstants.MorphologyAndSyntax);
			if (!Directory.Exists(posListDir))
				return; // If we can't find the list, we'll just report "Grammatical Info."
			var posListFullFilename = Path.Combine(posListDir, SharedConstants.PartsOfSpeechFilename);
			var possible = File.Exists(posListFullFilename);
			if (!possible)
				return;
			var doc = new XmlDocument();
			doc.Load(posListFullFilename);
			LoadPosList(doc.DocumentElement);
		}

		public void LoadPosList(XmlNode data)
		{
			_guidPosNameDict = new Dictionary<string, string>();
			// In the CmPossibilityList we want all the nodes under Possibilities. They are 'ownseq' nodes of class PartOfSpeech,
			// even when embedded in SubPossibilities.
			const string xpathSearch = "descendant::Possibilities//" + SharedConstants.Ownseq+"[@"+SharedConstants.Class+"=\""+
									   SharedConstants.PartOfSpeech+"\"]";
			foreach (XmlNode posNode in data.SelectNodes(xpathSearch))
			{
				var guidStr = posNode.GetStringAttribute("guid");
				var stringIdentifier = ProcessNameAndAbbrevNodes(posNode);
				if (stringIdentifier.Length > 0) // If we can't find either an Abbreviation or a Name, don't add it!
					_guidPosNameDict.Add(guidStr, stringIdentifier);
			}
		}

		private string ProcessNameAndAbbrevNodes(XmlNode posNode)
		{
			// Check for POS Name(s)
			var nameOrAbbrev = posNode.SelectSingleNode("Name");
			var result = PullOutStrings(nameOrAbbrev);
			if (result.Length > 0)
				return result;
			// Didn't find Name, try Abbreviation(s)
			nameOrAbbrev = posNode.SelectSingleNode("Abbreviation");
			result = PullOutStrings(nameOrAbbrev);
			return result;
		}

		private string PullOutStrings(XmlNode nameOrAbbrev)
		{
			var labels = string.Empty;
			var auniList = nameOrAbbrev.SelectNodes(SharedConstants.AUni);
			if (auniList == null)
				return labels;

			// If we have more than one ws, concatenate them with colons
			// e.g. Noun:Nombre:Nom
			foreach (XmlElement auniStr in auniList)
			{
				if (string.IsNullOrEmpty(auniStr.InnerText))
					continue;
				labels += (labels == string.Empty) ? auniStr.InnerText : Colon + auniStr.InnerText;
			}
			return labels;
		}
	}
}
