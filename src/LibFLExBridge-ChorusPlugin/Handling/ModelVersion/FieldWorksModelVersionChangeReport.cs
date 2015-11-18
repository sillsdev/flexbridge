﻿// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;

namespace LibFLExBridgeChorusPlugin.Handling.ModelVersion
{
	/// <summary>
	/// Abstract base class for FW Model version change reports
	/// </summary>
	internal abstract class FieldWorksModelVersionChangeReport : ChangeReport
	{
		protected FieldWorksModelVersionChangeReport(FileInRevision parent, FileInRevision child, int newModelVersion)
			: base(parent, child)
		{
			NewModelVersion = newModelVersion;
		}

		///<summary>
		/// Constructor
		///</summary>
		protected FieldWorksModelVersionChangeReport(FileInRevision fileInRevision, int newModelVersion)
			:base(null, fileInRevision)
		{
			NewModelVersion = newModelVersion;
		}

		protected FieldWorksModelVersionChangeReport(string childPath, int newModelVersion)
			:base(null, null)
		{
			ChildPath = childPath;
			NewModelVersion = newModelVersion;
		}

		protected string ChildPath { get; set; }

		///<summary>
		/// Get the FDO model version number for the change report.
		///</summary>
		public int NewModelVersion { get; private set; }
	}

	///<summary>
	/// FW model change (addition) report
	///</summary>
	internal sealed class FieldWorksModelVersionAdditionChangeReport : FieldWorksModelVersionChangeReport
	{
		///<summary>
		/// Constructor
		///</summary>
		internal FieldWorksModelVersionAdditionChangeReport(FileInRevision fileInRevision, int modelVersion)
			:base(fileInRevision, modelVersion)
		{
		}

		//
		/// <summary>
		/// Constructor
		/// </summary>
		/// <remarks>
		/// When merging, the eventual revision is unknown, so feed that as the child.
		/// </remarks>
		internal FieldWorksModelVersionAdditionChangeReport(string fullPath, int modelVersion)
			: base(null, new FileInUnknownRevision(fullPath, FileInRevision.Action.Modified), modelVersion)
		{
		}

		///<summary>
		/// The report reports creation of the file.
		///</summary>
		public override string ActionLabel
		{
			get { return "Added"; }
		}

		///<summary>
		/// Return the version number that was added.
		///</summary>
		public override string GetFullHumanReadableDescription()
		{
			return string.Format(string.Format("Added a version number file for version: {0}", NewModelVersion));
		}
	}

	///<summary>
	/// Class that knows about model verison number updates.
	///</summary>
	internal sealed class FieldWorksModelVersionUpdatedReport : FieldWorksModelVersionChangeReport
	{
		///<summary>
		/// Constructor.
		///</summary>
		internal FieldWorksModelVersionUpdatedReport(FileInRevision parentFileInRevision, FileInRevision childFileInRevision, int oldModelVersion, int newModelVersion)
			: base(parentFileInRevision, childFileInRevision, newModelVersion)
		{
			OldModelVersion = oldModelVersion;
		}

		internal FieldWorksModelVersionUpdatedReport(string childPath, int oldModelVersion, int newModelVersion)
			: base(childPath, newModelVersion)
		{
			OldModelVersion = oldModelVersion;
		}

		///<summary>
		/// Get the previous model version.
		///</summary>
		public int OldModelVersion { get; private set; }

		///<summary>
		/// The model was updated to a new version.
		///</summary>
		public override string ActionLabel
		{
			get { return "Updated"; }
		}

		///<summary>
		/// Return the old and new version numbers.
		///</summary>
		public override string GetFullHumanReadableDescription()
		{
			return string.Format(string.Format("Updated from {0} to {1}", OldModelVersion, NewModelVersion));
		}
	}
}
