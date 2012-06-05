using System;
using System.Xml;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Properties;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.Linguistics.Lexicon
{
	/// <summary>
	/// Context generator for LexEntry/Senses/ownseq/MorphoSyntaxAnalysis element.
	/// It contains a objsur element that refers to the
	/// LexEntry/MorphoSyntaxAnalyses/[SomeMSA]/PartOfSpeech/objsur
	/// that refers the part of speech item in the part of speech list
	/// that is in a different file.
	/// The part of speech is now called "Grammatical Info."
	/// </summary>
	class PosContextGenerator : IGenerateContextDescriptor, IGenerateContextDescriptorFromNode, IGenerateHtmlContext
	{
		private string GetLabel(XmlNode start)
		{
			return LexEntryName(start) + ' ' + GetLabelForPos(start);
		}

		string EntryLabel
		{
			get { return Resources.kLexEntryClassLabel; }
		}

		private string LexEntryName(XmlNode start)
		{
			//grab the form from the stem (if available) to give a user understandable message
			var form = start.SelectSingleNode("ancestor::LexEntry/LexemeForm//Form/AUni");
			return form == null
				? EntryLabel
				: EntryLabel + " \"" + form.InnerText + '"';
		}

		private string GetLabelForPos(XmlNode entry)
		{
			return "Grammatical Info.";
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
			string guid = ""; // guid of the MoStemMsa with the POS
			XmlNode pos = null; // reference to the part of speech node that changed
			if (mergeElement != null && mergeElement.Name == "MorphoSyntaxAnalysis")
			{
				guid = mergeElement.SelectSingleNode("objsur/@guid").Value; // should only be one and have a guid in a validated file
				pos = mergeElement.SelectSingleNode("../../preceding-sibling::MorphoSyntaxAnalyses/*[@guid = \"" + guid + "\"]/PartOfSpeech");
			}
			if (pos == null)
				throw new ArgumentException("Node is not valid for part of speech guid lookup."); // something not right!

			var posGuid = pos.SelectSingleNode("objsur/@guid").Value;
			return "<div class='guid'>" + "Guid of part of speech: " + posGuid  + "</div>";
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
