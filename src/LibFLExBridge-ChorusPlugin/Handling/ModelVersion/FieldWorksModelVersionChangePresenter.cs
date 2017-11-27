// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Text;
using Chorus.FileTypeHandlers;

namespace LibFLExBridgeChorusPlugin.Handling.ModelVersion
{
	///<summary>
	///</summary>
	internal sealed class FieldWorksModelVersionChangePresenter : IChangePresenter
	{
		private readonly FieldWorksModelVersionChangeReport _report;

		/// <summary>
		/// Constructor
		/// </summary>
		internal FieldWorksModelVersionChangePresenter(FieldWorksModelVersionChangeReport report)
		{
			_report = report;
		}

		#region Implementation of IChangePresenter

		string IChangePresenter.GetDataLabel()
		{
			return "FDO model version";
		}

		string IChangePresenter.GetActionLabel()
		{
			return _report.ActionLabel;
		}

		string IChangePresenter.GetHtml(string style, string styleSheet)
		{
			var builder = new StringBuilder();
			builder.Append("<html><head>" + styleSheet + "</head>");

			switch (_report.GetType().Name)
			{
				default:
					builder.Append("Don't know how to display a report of type: " + _report.GetType().Name + " <p/>");
					break;
				case "FieldWorksModelVersionAdditionChangeReport":
					var additionChangeReport = (FieldWorksModelVersionAdditionChangeReport)_report;
					builder.AppendFormat("Added the version file for the following model version: {0} <p/>", additionChangeReport.NewModelVersion);
					break;
				case "FieldWorksModelVersionUpdatedReport":
					var changedRecordReport = (FieldWorksModelVersionUpdatedReport)_report;
					builder.AppendFormat("Updated the model version number from {0} to {1}. <p/>", changedRecordReport.OldModelVersion, changedRecordReport.NewModelVersion);
					break;
			}
			builder.Append("</html>");
			return builder.ToString();
		}

		string IChangePresenter.GetTypeLabel()
		{
			return "FieldWorks data model version";
		}

		string IChangePresenter.GetIconName()
		{
			return "file";
		}

		#endregion
	}
}
