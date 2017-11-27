// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Text;
using System.Xml;
using Chorus.FileTypeHandlers;
using Chorus.FileTypeHandlers.xml;
using Chorus.VcsDrivers.Mercurial;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibFLExBridgeChorusPlugin.Infrastructure;

namespace LibFLExBridgeChorusPlugin.Handling
{
	/// <summary>
	/// Class that is a FieldWorks Presenter, which is part of Chorus' MVP system.
	/// </summary>
	/// <remarks>
	/// This class will need to be broken up at some some. Maybe one for each context, or file extension.
	/// </remarks>
	internal sealed class FieldWorksChangePresenter : IChangePresenter
	{
		private readonly IXmlChangeReport _report;

		internal FieldWorksChangePresenter(IXmlChangeReport report)
		{
			_report = report;
		}

		internal static IChangePresenter GetCommonChangePresenter(IChangeReport report, HgRepository repository)
		{
			var xmlChangeReport = report as IXmlChangeReport;
			if (xmlChangeReport != null)
				return new FieldWorksChangePresenter(xmlChangeReport);

			if (report is ErrorDeterminingChangeReport)
				return (IChangePresenter)report;

			return new DefaultChangePresenter(report, repository);
		}

		private XmlNode FirstNonNullNode
		{
			get { return _report.ChildNode ?? _report.ParentNode; }
		}

		private static void GetRawXmlRendering(StringBuilder builder, XmlNode node)
		{
			builder.AppendFormat("<p><pre>{0}</pre></p>",
								 XmlUtilities.GetXmlForShowingInHtml(node.OuterXml));
		}

		#region Implementation of IChangePresenter

		string IChangePresenter.GetDataLabel()
		{
			var firstNode = FirstNonNullNode;
			var nodeLocalName = firstNode.LocalName;
			string label;
			switch (nodeLocalName)
			{
				case FlexBridgeConstants.RtTag:
					label = firstNode.Attributes[FlexBridgeConstants.Class].Value;
					break;
				case FlexBridgeConstants.CustomField:
					label = "Custom property";
					break;
				default:
					label = nodeLocalName; // Good start, but...
					break;
			}
			return label;
		}

		string IChangePresenter.GetActionLabel()
		{
			return ((IChangeReport)_report).ActionLabel;
		}

		string IChangePresenter.GetHtml(string style, string styleSheet)
		{
			var builder = new StringBuilder();
			builder.Append("<html><head>" + styleSheet + "</head>");

			switch (_report.GetType().Name)
			{
				default:
					builder.Append("Don't know how to display a report of type: " + _report.GetType().Name + "<p/>");
					break;
				case "XmlAdditionChangeReport":
					var additionChangeReport = (XmlAdditionChangeReport)_report;
					builder.Append("Added the following object:<p/>");
					switch (style)
					{
						case "normal": // Fall through for now.
						//builder.Append(GetHtmlForEntry(additionChangeReport.ChildNode));
						//break;
						case "raw":
							GetRawXmlRendering(builder, additionChangeReport.ChildNode);
							break;
						default:
							return String.Empty;
					}
					break;
				case "XmlDeletionChangeReport":
					var deletionChangeReport = (XmlDeletionChangeReport)_report;
					builder.Append("Deleted the following object:<p/>");
					switch (style)
					{
						case "normal": // Fall through for now.
						//builder.Append(GetHtmlForEntry(deletionChangeReport.ParentNode));
						//break;
						case "raw":
							GetRawXmlRendering(builder, deletionChangeReport.ParentNode);
							break;
						default:
							return String.Empty;
					}
					break;
				case "XmlChangedRecordReport":
					// TODO: run the diff again at lower levels to get the place(s) that actually changed.
					var changedRecordReport = (XmlChangedRecordReport)_report;
					switch (style.ToLower())
					{
						default:
							break;
						case "normal": // Fall through for now.
						//var original = GetHtmlForEntry(changedRecordReport.ParentNode);
						//// XmlUtilities.GetXmlForShowingInHtml("<entry>" + r.ParentNode.InnerXml + "</entry>");
						//var modified = GetHtmlForEntry(changedRecordReport.ChildNode);
						//// XmlUtilities.GetXmlForShowingInHtml("<entry>" + r.ChildNode.InnerXml + "</entry>");
						//var m = new Rainbow.HtmlDiffEngine.Merger(original, modified);
						//builder.Append(m.merge());
						//break;
						case "raw":

							builder.Append("<h3>From</h3>");
							GetRawXmlRendering(builder, changedRecordReport.ParentNode);
							builder.Append("<h3>To</h3>");
							GetRawXmlRendering(builder, changedRecordReport.ChildNode);
							break;
					}
					break;
			}
			builder.Append("</html>");
			return builder.ToString();
		}

		string IChangePresenter.GetTypeLabel()
		{
			return "FLEx data object";
		}

		string IChangePresenter.GetIconName()
		{
			return "file";
		}

		#endregion
	}
}