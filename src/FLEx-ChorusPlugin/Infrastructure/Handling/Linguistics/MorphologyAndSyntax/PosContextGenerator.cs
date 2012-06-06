using System.Xml;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Properties;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.Linguistics.MorphologyAndSyntax
{
	/// <summary>
	/// Context generator for LexEntry/Senses/ownseq/MorphoSyntaxAnalysis element.
	/// It contains a objsur element that refers to the
	/// LexEntry/MorphoSyntaxAnalyses/[SomeMSA]/[To/From]PartOfSpeech/objsur
	/// that refers the part of speech item in the part of speech list
	/// that is in a different file.
	/// The part of speech is now called "Grammatical Info."
	/// </summary>
	internal sealed class PosContextGenerator : IGenerateContextDescriptor, IGenerateContextDescriptorFromNode, IGenerateHtmlContext
	{
		private string GetLabel(XmlNode start)
		{
			return LexEntryName(start) + ' ' + GetLabelForPos();
		}

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
				: EntryLabel + " \"" + form.InnerText + '"';
		}

		private static string GetLabelForPos()
		{
			return "Grammatical Info.";
		}

		private static XmlNode GetLexEntryNode(XmlNode node)
		{
			return node.Name == "LexEntry" ? node : GetLexEntryNode(node.ParentNode);
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
			var hasTwoPoses = false;
			var posGuids = new string[2]; // reference to the part of speech node(s) that changed
			if (mergeElement != null && mergeElement.Name == "MorphoSyntaxAnalysis")
			{
				var entryNode = GetLexEntryNode(mergeElement);
				var objsurNode = mergeElement.SelectSingleNode("objsur"); // Could be null.
				if (objsurNode != null)
				{
					var msaGuid = objsurNode.Attributes["guid"].Value; // guid of the Msa with one or two POSes
					var msa = entryNode.SelectSingleNode("MorphoSyntaxAnalyses/*[@guid = \"" + msaGuid + "\"]");
					if (msa.Name == "MoDerivAffMsa")
					{
						hasTwoPoses = true;
						posGuids[0] = GetPosGuidOrUnknown(msa, "FromPartOfSpeech");
						posGuids[1] = GetPosGuidOrUnknown(msa, "ToPartOfSpeech");
					}
					else
					{
						//All others have 'PartOfSpeech' property.
						posGuids[0] = GetPosGuidOrUnknown(msa, "PartOfSpeech");
					}
				}
			}

			return string.Format("<div class='guid'>{0}</div>",
				hasTwoPoses
					? string.Format("Guids: {0}/{1}", posGuids[0], posGuids[1])
					: string.Format("Guid of part of speech: {0}", posGuids[0]));
		}

		private static string GetPosGuidOrUnknown(XmlNode node, string propertyName)
		{
			var propNode = node.SelectSingleNode(propertyName);
			if (propNode == null)
				return "unknown";
			var objsurNode = propNode.SelectSingleNode("objsur");
			return objsurNode == null ? "unknown" : objsurNode.Attributes["guid"].Value;
		}

		public string HtmlContextStyles(XmlNode mergeElement)
		{
			return "div.alternative {margin-left:  0.25in} div.ws {margin-left:  0.25in} div.property {margin-left:  0.25in} div.checksum {margin-left:  0.25in}";
		}

		public ContextDescriptor GenerateContextDescriptor(string mergeElement, string filePath)
		{
			//throw new NotImplementedException();
			var doc = new XmlDocument();
			doc.LoadXml(mergeElement);
			return GenerateContextDescriptor(doc.DocumentElement, filePath);
		}

		public ContextDescriptor GenerateContextDescriptor(XmlNode mergeElement, string filePath)
		{
			return FieldWorksMergeStrategyServices.GenerateContextDescriptor(filePath,
																			 FieldWorksMergeStrategyServices.GetGuid(mergeElement),
																			 GetLabel(mergeElement));
		}
	}
}
