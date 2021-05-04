// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using Chorus.FileTypeHandlers;
using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;
using Chorus.merge.xml.generic;
using LibFLExBridgeChorusPlugin.DomainServices;
using LibFLExBridgeChorusPlugin.Handling.ModelVersion;
using LibFLExBridgeChorusPlugin.Infrastructure;
using SIL.IO;
using SIL.Progress;

namespace LibFLExBridgeChorusPlugin.Handling
{
	[Export(typeof(IChorusFileTypeHandler))]
	internal sealed class FieldWorksCommonFileHandler : IChorusFileTypeHandler
	{
#pragma warning disable CS0649
		[Import(typeof(UnknownFileTypeHandlerStrategy))]
		private IFieldWorksFileHandler _unknownFileTypeHandler;

		[ImportMany]
		private IEnumerable<IFieldWorksFileHandler> _handlers;
#pragma warning restore CS0649

		internal FieldWorksCommonFileHandler()
		{
			using (var catalog = new AssemblyCatalog(Assembly.GetExecutingAssembly()))
			{
				using (var container = new CompositionContainer(catalog))
				{
					container.ComposeParts(this);
				}
			}
		}

		private IChorusFileTypeHandler AsIChorusFileTypeHandler => this;

		private IFieldWorksFileHandler GetHandlerFromExtension(string extension)
		{
			return _handlers.FirstOrDefault(handlerCandidate => handlerCandidate.Extension == extension) ?? _unknownFileTypeHandler;
		}

		/// <summary>
		/// All callers merging FieldWorks data need to pass 'true', so the MDC will know about  any custom properties for their classes.
		///
		/// Non-object callers (currently only the merge of the custom property definitions themselves) should pass 'false'.
		/// </summary>
		internal static void Do3WayMerge(MergeOrder mergeOrder, MetadataCache mdc, bool addCustomPropertyInformation)
		{
			// Skip doing this for the Custom property definition file, since it has no real need for the custom prop definitions,
			// which are being merged (when 'false' is provided).
			if (addCustomPropertyInformation)
				mdc.AddCustomPropInfo(mergeOrder); // NB: Must be done before FieldWorksCommonMergeStrategy is created. since it used the MDC.

			var merger = FieldWorksMergeServices.CreateXmlMergerForFieldWorksData(mergeOrder, mdc);
			merger.EventListener = mergeOrder.EventListener;
			var mergeResults = merger.MergeFiles(mergeOrder.pathToOurs, mergeOrder.pathToTheirs, mergeOrder.pathToCommonAncestor);
			// Write out merged data.
			FileWriterService.WriteNestedFile(mergeOrder.pathToOurs, mergeResults.MergedNode);
		}

		#region Implementation of IChorusFileTypeHandler

		bool IChorusFileTypeHandler.CanDiffFile(string pathToFile)
		{
			return AsIChorusFileTypeHandler.CanValidateFile(pathToFile);
		}

		bool IChorusFileTypeHandler.CanMergeFile(string pathToFile)
		{
			return AsIChorusFileTypeHandler.CanValidateFile(pathToFile);
		}

		bool IChorusFileTypeHandler.CanPresentFile(string pathToFile)
		{
			return AsIChorusFileTypeHandler.CanValidateFile(pathToFile);
		}

		bool IChorusFileTypeHandler.CanValidateFile(string pathToFile)
		{
			if (string.IsNullOrEmpty(pathToFile))
				return false;
			if (!File.Exists(pathToFile))
				return false;
			var extension = Path.GetExtension(pathToFile);
			if (string.IsNullOrEmpty(extension))
				return false;
			if (extension[0] != '.')
				return false;

			var handler = GetHandlerFromExtension(extension.Substring(1));
			return handler.CanValidateFile(pathToFile);
		}

		void IChorusFileTypeHandler.Do3WayMerge(MergeOrder mergeOrder)
		{
			if (mergeOrder == null)
				throw new ArgumentNullException(nameof(mergeOrder));

			// Make sure MDC is updated.
			// Since this method is called in another process by ChorusMerge,
			// the MDC that was set up for splitting the file is not available.
			var extension = FileWriterService.GetExtensionFromPathname(mergeOrder.pathToOurs);
			if (extension != FlexBridgeConstants.ModelVersion)
			{
				var pathToOurs = mergeOrder.pathToOurs;
				var folder = Path.GetDirectoryName(pathToOurs);
				while (folder != null && !File.Exists(Path.Combine(folder, FlexBridgeConstants.ModelVersionFilename)))
				{
					var parent = Directory.GetParent(folder);
					folder = parent?.ToString();
				}
				// 'folder' should now have the required model version file in it, or null for some tests.
				var desiredModelNumber = MetadataCache.MaximumModelVersion;
				if (folder != null)
				{
					var ourModelFileData = File.ReadAllText(Path.Combine(folder, FlexBridgeConstants.ModelVersionFilename));
					desiredModelNumber = int.Parse(ModelVersionFileTypeHandlerStrategy.SplitData(ourModelFileData)[1]);
				}
				MetadataCache.MdCache.UpgradeToVersion(desiredModelNumber);
			}

			XmlMergeService.RemoveAmbiguousChildNodes = false; // Live on the edge. Opt out of that expensive code.

			GetHandlerFromExtension(extension).Do3WayMerge(MetadataCache.MdCache, mergeOrder);
		}

		IEnumerable<IChangeReport> IChorusFileTypeHandler.Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			if (parent == null)
				throw new ArgumentNullException(nameof(parent)); // Parent seems not be optional in Chorus usage.
			if (child == null)
				throw new ArgumentNullException(nameof(child));
			if (repository == null)
				throw new ArgumentNullException(nameof(repository));

			var extension = FileWriterService.GetExtensionFromPathname(child.FullPath);
			return GetHandlerFromExtension(extension).Find2WayDifferences(parent, child, repository);
		}

		IChangePresenter IChorusFileTypeHandler.GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			if (report == null)
				throw new ArgumentNullException(nameof(report));
			if (repository == null)
				throw new ArgumentNullException(nameof(repository));

			var extension = FileWriterService.GetExtensionFromPathname(report.PathToFile);
			return GetHandlerFromExtension(extension).GetChangePresenter(report, repository);
		}

		string IChorusFileTypeHandler.ValidateFile(string pathToFile, IProgress progress)
		{
			if (progress == null)
				throw new ArgumentNullException(nameof(progress));

			if (string.IsNullOrEmpty(pathToFile))
				return "No file to work with.";
			if (!File.Exists(pathToFile))
				return "File does not exist.";
			var extension = Path.GetExtension(pathToFile);
			if (string.IsNullOrEmpty(extension))
				return "File has no extension.";
			if (extension[0] != '.')
				return "File has no extension.";

			var handler = GetHandlerFromExtension(extension.Substring(1));
			var results = handler.ValidateFile(pathToFile);
			if (results != null)
			{
				progress.WriteError("File '{0}' is not valid with message:{1}\t{2}", pathToFile, Environment.NewLine, results);
				progress.WriteWarning("It may also have other problems in addition to the one that was reported.");
			}
			return results;
		}

		IEnumerable<IChangeReport> IChorusFileTypeHandler.DescribeInitialContents(FileInRevision fileInRevision, TempFile file)
		{
			// Skip check, since DefaultChangeReport doesn't require it.
			//if (fileInRevision == null)
			//    throw new ArgumentNullException("fileInRevision");

			// Not used here, so don't fret if it is null.
			//if (file == null)
			//    throw new ArgumentNullException("file");

			return new IChangeReport[] { new DefaultChangeReport(fileInRevision, "Added") };
		}

		IEnumerable<string> IChorusFileTypeHandler.GetExtensionsOfKnownTextFileTypes()
		{
			return _handlers.Select(handlerStrategy => handlerStrategy.Extension);
		}

		uint IChorusFileTypeHandler.MaximumFileSize => int.MaxValue;

		#endregion
	}
}
