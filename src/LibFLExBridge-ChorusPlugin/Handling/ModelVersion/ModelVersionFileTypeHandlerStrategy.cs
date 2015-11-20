// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using Chorus.FileTypeHandlers;
using Chorus.FileTypeHandlers.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers.Mercurial;
using SIL.IO;
using LibFLExBridgeChorusPlugin.Infrastructure;
using System.ComponentModel.Composition;

namespace LibFLExBridgeChorusPlugin.Handling.ModelVersion
{
	[Export(typeof(IFieldWorksFileHandler))]
	internal sealed class ModelVersionFileTypeHandlerStrategy : IFieldWorksFileHandler
	{
		#region Implementation of IFieldWorksFileHandler

		public bool CanValidateFile(string pathToFile)
		{
			return FileUtils.CheckValidPathname(pathToFile, FlexBridgeConstants.ModelVersion) &&
				   Path.GetFileName(pathToFile) == FlexBridgeConstants.ModelVersionFilename;
		}

		public string ValidateFile(string pathToFile)
		{
			try
			{
				// Uses JSON: {"modelversion": #####}
				var data = File.ReadAllText(pathToFile);
				var splitData = SplitData(data);
				if (splitData.Length == 2 && splitData[0] == "\"modelversion\"" && Int32.Parse(splitData[1].Trim()) >= 7000000)
					return null;
				return "Not a valid JSON model version file.";
			}
			catch (Exception e)
			{
				return e.Message;
			}
		}

		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			var fieldWorksModelVersionChangeReport = report as FieldWorksModelVersionChangeReport;
			if (fieldWorksModelVersionChangeReport != null)
				return new FieldWorksModelVersionChangePresenter(fieldWorksModelVersionChangeReport);

			if (report is ErrorDeterminingChangeReport)
				return (IChangePresenter)report;

			return new DefaultChangePresenter(report, repository);
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			var diffReports = new List<IChangeReport>(1);

			// The only relevant change to report is the version number.
			var childData = child.GetFileContents(repository);
			var splitData = SplitData(childData);
			var childModelNumber = Int32.Parse(splitData[1]);
			if (parent == null)
			{
				diffReports.Add(new FieldWorksModelVersionAdditionChangeReport(child, childModelNumber));
			}
			else
			{
				var parentData = parent.GetFileContents(repository);
				splitData = SplitData(parentData);
				var parentModelNumber = Int32.Parse(splitData[1]);
				if (parentModelNumber != childModelNumber)
					diffReports.Add(new FieldWorksModelVersionUpdatedReport(parent, child, parentModelNumber, childModelNumber));
				else
					throw new InvalidOperationException("The version number has downgraded");
			}

			return diffReports;
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			// NB: Doesn't need the mdc updated with custom props.
			if (mergeOrder.EventListener is NullMergeEventListener)
				mergeOrder.EventListener = new ChangeAndConflictAccumulator();

			// The bigger model number wins, no matter if it came in ours or theirs.
			var commonData = File.ReadAllText(mergeOrder.pathToCommonAncestor);
			var commonNumber = Int32.Parse(SplitData(commonData)[1]);

			var ourData = File.ReadAllText(mergeOrder.pathToOurs);
			var ourNumber = Int32.Parse(SplitData(ourData)[1]);

			var theirData = File.ReadAllText(mergeOrder.pathToTheirs);
			var theirNumber = Int32.Parse(SplitData(theirData)[1]);

			if (commonNumber == ourNumber && commonNumber == theirNumber)
				return; // No changes.

			if (ourNumber < commonNumber || theirNumber < commonNumber)
				throw new InvalidOperationException("Downgrade in model version number.");

			var mergedNumber = ourNumber;
			var listener = mergeOrder.EventListener;
			if (ourNumber > theirNumber)
			{
				listener.ChangeOccurred(new FieldWorksModelVersionUpdatedReport(mergeOrder.pathToOurs, commonNumber, ourNumber));
			}
			else
			{
				mergedNumber = theirNumber;
				// Put their number in our file. {"modelversion": #####}
				var newFileContents = "{\"modelversion\": " + theirNumber + "}";
				File.WriteAllText(mergeOrder.pathToOurs, newFileContents);
				listener.ChangeOccurred(new FieldWorksModelVersionUpdatedReport(mergeOrder.pathToTheirs, commonNumber, theirNumber));
			}

			mdc.UpgradeToVersion(mergedNumber);
		}

		public string Extension
		{
			get { return FlexBridgeConstants.ModelVersion; }
		}

		#endregion

		internal static string[] SplitData(string data)
		{
			return data.Split(new[] { "{", ":", "}" }, StringSplitOptions.RemoveEmptyEntries);
		}
	}
}