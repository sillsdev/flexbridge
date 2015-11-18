﻿// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml.Linq;
using Chorus.FileTypeHandlers;
using Chorus.VcsDrivers.Mercurial;
using Chorus.merge;
using LibFLExBridgeChorusPlugin.DomainServices;
using LibFLExBridgeChorusPlugin.Infrastructure;
using SIL.IO;

namespace LibFLExBridgeChorusPlugin.Handling.Common
{
	/// <summary>
	/// This class deals with files with extension of "style".
	/// The files may be located in various contexts.
	/// </summary>
	[Export(typeof(IFieldWorksFileHandler))]
	internal sealed class StyleFileTypeHandlerStrategy : IFieldWorksFileHandler
	{
		#region Implementation of IFieldWorksFileHandler

		public bool CanValidateFile(string pathToFile)
		{
			return FileUtils.CheckValidPathname(pathToFile, FlexBridgeConstants.Style);
		}

		public string ValidateFile(string pathToFile)
		{
			try
			{
				var doc = XDocument.Load(pathToFile);
				var root = doc.Root;
				if (root.Name.LocalName != FlexBridgeConstants.Styles || !root.Elements(FlexBridgeConstants.StStyle).Any())
					return "Not valid styles file";

				var styles = root.Elements(FlexBridgeConstants.StStyle).ToList();
				var duplicateStyleElts = new List<XElement[]>();
				var dupName = new HashSet<string>();
				foreach (var style in styles)
				{
					// NOTE: there is a SharedConstants.Name, but it is not initial cap, it's all lower case.
					// Right. That is because it is for an attribute 'name', not an element name.
					var currentName = style.Element("Name").Element(FlexBridgeConstants.Uni).Value;
					if (dupName.Contains(currentName))
						continue; // Don't fret over a triplicate style.

					var styleWithSameName = (styles.Where(otherStyle => style != otherStyle
						&& otherStyle.Element("Name").Element(FlexBridgeConstants.Uni).Value == currentName)).FirstOrDefault();
					if (styleWithSameName == null)
						continue;
					duplicateStyleElts.Add(new[] { style, styleWithSameName });
					dupName.Add(currentName);
				}
				if (duplicateStyleElts.Any())
				{
					// There were duplicate style names. Return a message to caller.
					var message = "Duplicate Styles: ";
					var comma = false;
					foreach (var duplicates in duplicateStyleElts)
					{
						if (comma)
							message += ", ";
						comma = true;
						message += "{" + duplicates[0].Element("Name").Element(FlexBridgeConstants.Uni).Value + "}";
						message += " with guids " + duplicates[0].Attribute(FlexBridgeConstants.GuidStr).Value + " and " +
							duplicates[1].Attribute(FlexBridgeConstants.GuidStr).Value;
					}
					return message + ".";
				}
				return root.Elements(FlexBridgeConstants.StStyle)
					.Select(style => CmObjectValidator.ValidateObject(MetadataCache.MdCache, style)).FirstOrDefault(result => result != null);
			}
			catch (Exception e)
			{
				return e.Message;
			}
		}

		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			return FieldWorksChangePresenter.GetCommonChangePresenter(report, repository);
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			return Xml2WayDiffService.ReportDifferences(repository, parent, child,
				null,
				FlexBridgeConstants.StStyle, FlexBridgeConstants.GuidStr);
		}

		public void Do3WayMerge(MetadataCache mdc, MergeOrder mergeOrder)
		{
			FieldWorksCommonFileHandler.Do3WayMerge(mergeOrder, mdc, true);
		}

		public string Extension
		{
			get { return FlexBridgeConstants.Style; }
		}

		#endregion
	}
}
