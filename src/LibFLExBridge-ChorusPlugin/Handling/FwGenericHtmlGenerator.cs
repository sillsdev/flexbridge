// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Chorus.merge.xml.generic;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibTriboroughBridgeChorusPlugin;

namespace LibFLExBridgeChorusPlugin.Handling
{
	/// <summary>
	/// This class provides a default way of showing the XML of an element from the Chorus representation of FW data in HTML.
	/// Basically it uses Property names as labels, nested divs for indentation, and leaves out class level info.
	/// All reference information is compressed into a single checksum, since with only guids available, all we can really
	/// usefully tell the user is that some references changed.
	/// A few special cases make it prettier.
	/// </summary>
	internal sealed class FwGenericHtmlGenerator
	{
		public string MakeHtml(XmlNode input)
		{
			var sb = new StringBuilder();
			ProcessNode(sb, input);
			return sb.ToString();
		}

		private void ProcessNode(StringBuilder sb, XmlNode input)
		{
			if (SpecialHandling(sb, input))
				return;
			if (HandleRefProp(sb, input))
				return;
			var sb2 = new StringBuilder();
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
			var wantLevel = !SkipNodeLevel(input);
			if (wantLevel)
			{
				AppendElementlabelDiv(sb, input);
			}
			sb.Append(inner);
			if (wantLevel)
				sb.Append("</div>");
		}

		/// <summary>
		/// If this node is some sort of reference property (identified by having objsur, refsur, or refcol children), output a checksum div.
		/// These are generated so that a subsequent stage can diff the HTML of different objects and highlight the
		/// checksums that differ. However, the result is an ugly thing we don't want users to see; so still later code
		/// (in BridgeConflictController.AdjustConflictHtml) cleans them up and just shows a list of attributes with problems.
		/// </summary>
		/// <param name="sb"></param>
		/// <param name="input"></param>
		/// <returns>true if the node is a ref prop (and has been handled)</returns>
		private bool HandleRefProp(StringBuilder sb, XmlNode input)
		{
			var firstChild = input.FirstChild as XmlElement;
			if (firstChild == null || !WantChecksum(firstChild))
				return false;
			var checksumInput = new MemoryStream();
			var encoder = SHA1.Create();
			AddChecksum(checksumInput, input);
			if (checksumInput.Length > 0) // paranoia
			{
				checksumInput.Seek(0, SeekOrigin.Begin);
				var hash = encoder.ComputeHash(checksumInput);
				var checksum = Convert.ToBase64String(hash);
				sb.Append("<div class='checksum'>" + input.Name + ": " + checksum + "</div>");
			}
			return true;
		}

		/// <summary>
		/// Append the appropriate div header for displaying a property.
		/// </summary>
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
		private bool SpecialHandling(StringBuilder sb, XmlNode input)
		{
			switch (input.Name)
			{
				case FlexBridgeConstants.AUni:
				case FlexBridgeConstants.AStr:
					sb.Append("<div class='ws'>");
					var ws = XmlUtilities.GetOptionalAttributeString(input, FlexBridgeConstants.Ws);
					if (!string.IsNullOrEmpty(ws))
					{
						sb.Append("<span class=\"ws\">");
						sb.Append(ws);
						sb.Append("</span>: ");
					}
					sb.Append(input.InnerText);
					sb.Append("</div>");
					return true;
				case FlexBridgeConstants.Str:
					sb.Append(input.InnerText); // for now ignore all markup
					return true;
				case "BeginOffset":
				case "ParseIsCurrent":
					// Various cases of properties that are not relevant to the end user, so leave them out.
					return true;
				default:
					var val = XmlUtilities.GetOptionalAttributeString(input, FlexBridgeConstants.Val);
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
		private static bool SkipNodeLevel(XmlNode node)
		{
			if (node.Attributes[FlexBridgeConstants.GuidStr] != null)
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
			return input.Name == FlexBridgeConstants.Refseq || input.Name == FlexBridgeConstants.Objsur || input.Name == FlexBridgeConstants.Refcol;
		}

		private static void AddChecksumData(Stream s, XmlNode input)
		{
			var guid = XmlUtilities.GetOptionalAttributeString(input, "guid");
			if (!string.IsNullOrEmpty(guid))
			{
				var bytes = LibTriboroughBridgeSharedConstants.Utf8.GetBytes(guid);
				s.Write(bytes, 0, bytes.Length);
			}
		}

		internal string ChecksumLabel
		{
			get { return "Checksum of links to other objects: "; }
		}
	}
}
