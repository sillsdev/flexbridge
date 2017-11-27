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

namespace LibFLExBridgeChorusPlugin.Handling.ConfigLayout
{
	/// <summary>
	/// FieldWorks Presenter for a custom layout file.
	/// </summary>
	internal sealed class FieldWorksCustomLayoutChangePresenter : IChangePresenter
	{
		private readonly IXmlChangeReport _report;

		private FieldWorksCustomLayoutChangePresenter(IXmlChangeReport report)
		{
			_report = report;
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

		internal static IChangePresenter GetCommonChangePresenter(IChangeReport report, HgRepository repository)
		{
			var xmlChangeReport = report as IXmlChangeReport;
			if (xmlChangeReport != null)
				return new FieldWorksCustomLayoutChangePresenter(xmlChangeReport);

			if (report is ErrorDeterminingChangeReport)
				return (IChangePresenter)report;

			return new DefaultChangePresenter(report, repository);
		}

		#region Implementation of IChangePresenter

		string IChangePresenter.GetDataLabel()
		{
			return FirstNonNullNode.LocalName;
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
					builder.Append("Added the following:<p/>");
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
					builder.Append("Deleted the following:<p/>");
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
			return "FLEx custom layout element";
		}

		string IChangePresenter.GetIconName()
		{
			return "file";
		}

		#endregion
	}
}