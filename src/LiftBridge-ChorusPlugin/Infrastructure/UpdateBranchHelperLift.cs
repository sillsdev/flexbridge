﻿// Copyright (c) 2015-2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Globalization;
using System.Xml;
using LibTriboroughBridgeChorusPlugin.Infrastructure;
using SIL.Xml;
using SIL.LiftBridge.Services;

namespace SIL.LiftBridge.Infrastructure
{
	internal sealed class UpdateBranchHelperLift : IUpdateBranchHelperStrategy
	{
		internal static float GetLiftVersionNumber(string repoLocation)
		{
			var firstLiftFile = FileAndDirectoryServices.GetPathToFirstLiftFile(repoLocation);
			if (string.IsNullOrEmpty(firstLiftFile))
				return float.MaxValue;

			using (var reader = XmlReader.Create(firstLiftFile, CanonicalXmlSettings.CreateXmlReaderSettings()))
			{
				reader.MoveToContent();
				reader.MoveToAttribute("version");
				return float.Parse(reader.Value, NumberFormatInfo.InvariantInfo);
			}
		}

		public float GetModelVersionFromBranchName(string branchName)
		{
			return float.Parse(branchName.Replace("LIFT", null).Split('_')[0], NumberFormatInfo.InvariantInfo);
		}

		public float GetModelVersionFromClone(string cloneLocation)
		{
			return GetLiftVersionNumber(cloneLocation);
		}

		public string GetFullModelVersion(string cloneLocation)
		{
			var modelVersion = GetModelVersionFromClone(cloneLocation);
			return "LIFT" + modelVersion + "_ldml3";
		}
	}
}

