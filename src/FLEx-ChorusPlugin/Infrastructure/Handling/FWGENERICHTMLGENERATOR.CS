using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Chorus.merge.xml.generic;

namespace FLEx_ChorusPlugin.Infrastructure.Handling
{
	/// <summary>
	/// This class provides a default way of showing the XML of an element from the Chorus representation of FW data in HTML.
	/// Basically it uses Property names as labels, nested divs for indentation, and leaves out class level info.
	/// All reference information is compressed into a single checksum, since with only guids available, all we can really
	/// usefully tell the user is that some references changed.
	/// A few special cases make it prettier.
	/// </summary>
	internal class FwGenericHtmlGenerator
	{
		public string MakeHtml(XmlNode input)
		{
			var sb = new StringBuilder();
			ProcessNode(sb, input);
			var checksumInput = new MemoryStream();
			var encoder = SHA1.Create();
			AddChecksum(checksumInput, input);
			if (checksumInput.Length > 0)
			{
				checksumInput.Seek(0, SeekOrigin.Begin);
				var hash = encoder.ComputeHash(checksumInput);
				var checksum = Convert.ToBase64String(hash);
				sb.Append("<div class='checksum'>" + ChecksumLabel + checksum + "</div>");
			}
			return sb.ToString();
		}

		private void ProcessNode(StringBuilder sb, XmlNode input)
		{
			if (SpecialHandling(sb, input))
				return;
			StringBuilder sb2 = new StringBuilder();
			foreach (var child in input.ChildNodes)
			{
				var xmlText = child as XmlText;
				if (xmlText != null)
				{
					sb2.Append((xmlText).InnerText);
					continue;
				}
				var xmlNode = child as XmlNode;
				if (xmlNode != null)
					ProcessNode(sb2, xmlNode);
			}
			var inner = sb2.ToString();
			if (string.IsNullOrEmpty(inner))
				return;
			bool wantLevel = !SkipNodeLevel(input);
			if (wantLevel)
			{
				AppendElementlabelDiv(sb, input);
			}
			sb.Append(inner);
			if (wantLevel)
				sb.Append("</div>");
		}

		/// <summary>
		/// Append the appropriate div header for displaying a property.
		/// </summary>
		/// <param name="sb"></param>
		/// <param name="input"></param>
		private static void AppendElementlabelDiv(StringBuilder sb, XmlNode input)
		{
			sb.Append("<div class='property'>");
			sb.Append(input.Name);
			sb.Append(": ");
		}

		/// <summary>
		/// Return true (after appending anything desired to builder) if this node requires special handling.
		/// Eventually may make this virtual to allow customization
		/// </summary>
		/// <param name="sb"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		private bool SpecialHandling(StringBuilder sb, XmlNode input)
		{
			switch (input.Name)
			{
				case "AUni":
				case "AStr":
					sb.Append("<div class='ws'>");
					var ws = XmlUtilities.GetOptionalAttributeString(input, "ws");
					if (!string.IsNullOrEmpty(ws))
					{
						sb.Append(ws);
						sb.Append(": ");
					}
					sb.Append(input.InnerText);
					sb.Append("</div>");
					return true;
				case "Str":
					sb.Append(input.InnerText); // for now ignore all markup
					return true;
				case "BeginOffset":
				case "ParseIsCurrent":
					// Various cases of properties that are not relevant to the end user, so leave them out.
					return true;
				default:
					var val = XmlUtilities.GetOptionalAttributeString(input, "val");
					if (!string.IsNullOrEmpty(val))
					{
						AppendElementlabelDiv(sb, input);
						sb.Append(val);
						sb.Append("</div>");
						return true;
					}
					return false;
			}
		}

		/// <summary>
		/// Return true if we should skip outputting a div and label for this level, but still process its children.
		/// Enhance JohnT: could be made virtual method of generic baseclass, or use a configured list of attributes,
		/// or use a strategy.
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		bool SkipNodeLevel(XmlNode node)
		{
			if (node.Attributes["guid"] != null)
				return true;
			return false;
		}

		private void AddChecksum(Stream s, XmlNode input)
		{
			if (WantChecksum(input))
				AddChecksumData(s, input);
			foreach (var node in input.ChildNodes)
			{
				var xmlNode = node as XmlNode;
				if (xmlNode != null)
					AddChecksum(s, xmlNode);
			}
		}

		private bool WantChecksum(XmlNode input)
		{
			return input.Name == SharedConstants.Refseq || input.Name == SharedConstants.Objsur || input.Name == SharedConstants.Refcol;
		}

		private void AddChecksumData(Stream s, XmlNode input)
		{
			string guid = XmlUtilities.GetOptionalAttributeString(input, "guid");
			if (!string.IsNullOrEmpty(guid))
			{
				var bytes = Encoding.UTF8.GetBytes(guid);
				s.Write(bytes, 0, bytes.Length);
			}
		}

		public string ChecksumLabel
		{
			get { return "Checksum of links to other objects: "; }
		}
	}
}
