// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Chorus.FileTypeHandlers;
using Chorus.merge;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.IO;

namespace LibFLExBridgeChorusPluginTests
{
	internal static class FieldWorksTestServices
	{
		internal const int ExpectedExtensionCount = 28;

		internal static void RemoveTempFiles(ref TempFile ourFile, ref TempFile commonFile, ref TempFile theirFile)
		{
			ourFile.Dispose();
			ourFile = null;

			commonFile.Dispose();
			commonFile = null;

			theirFile.Dispose();
			theirFile = null;
		}

		internal static void RemoveTempFilesAndParentDir(ref TempFile ourFile, ref TempFile commonFile, ref TempFile theirFile)
		{
			var parentDir = Path.GetDirectoryName(ourFile.Path);
			ourFile.Dispose();
			ourFile = null;
			Directory.Delete(parentDir, true);

			parentDir = Path.GetDirectoryName(commonFile.Path);
			commonFile.Dispose();
			commonFile = null;
			Directory.Delete(parentDir, true);

			parentDir = Path.GetDirectoryName(theirFile.Path);
			theirFile.Dispose();
			theirFile = null;
			Directory.Delete(parentDir, true);
		}

		internal static void SetupTempFilesWithName(string filename, out TempFile ourFile, out TempFile commonFile, out TempFile theirFile)
		{
			SetupTempFilesWithName(filename, MetadataCache.MaximumModelVersion, out ourFile, out commonFile, out theirFile);
		}

		internal static void SetupTempFilesWithName(string filename, int modelVersion, out TempFile ourFile, out TempFile commonFile, out TempFile theirFile)
		{
			ourFile = TempFile.TrackExisting(CreateTempFileWithName(filename, modelVersion));
			commonFile = TempFile.TrackExisting(CreateTempFileWithName(filename, modelVersion));
			theirFile = TempFile.TrackExisting(CreateTempFileWithName(filename, modelVersion));
		}

		internal static string CreateTempFileWithName(string filename, int modelVersion)
		{
			var tempFileName = Path.GetTempFileName();
			var tempPath = Path.GetTempPath();
			var newDirName = Path.Combine(tempPath, Path.GetRandomFileName());
			if (Directory.Exists(newDirName))
				Directory.Delete(newDirName, true);
			Directory.CreateDirectory(newDirName);
			var replacement = Path.Combine(newDirName, filename);
			File.Move(tempFileName, replacement);

			if (filename != FlexBridgeConstants.ModelVersionFilename &&
				Path.GetExtension(filename) != "." + FlexBridgeConstants.ModelVersion)
			{
				// Add model version file with given version to 'newDirName'.
				var newModelVersionFileContents = "{\"modelversion\": " + modelVersion + "}";
				File.WriteAllText(Path.Combine(newDirName, FlexBridgeConstants.ModelVersionFilename), newModelVersionFileContents);
			}

			return replacement;
		}

		internal static void SetupTempFilesWithExtension(string extension, out TempFile ourFile, out TempFile commonFile, out TempFile theirFile)
		{
			ourFile = TempFile.WithExtension(extension);
			commonFile = TempFile.WithExtension(extension);
			theirFile = TempFile.WithExtension(extension);
		}

		internal static string DoMerge(
			IChorusFileTypeHandler chorusFileHandler,
			TempFile ourFile, string ourContent,
			TempFile commonFile, string commonAncestor,
			TempFile theirFile, string theirContent,
			IEnumerable<string> matchesExactlyOne, IEnumerable<string> isNull,
			int expectedConflictCount, List<Type> conflictTypes,
			int expectedChangesCount, List<Type> changeTypes)
		{
			List<IConflict> resultingConflicts;
			return DoMerge(chorusFileHandler,
				ourFile, ourContent,
				commonFile, commonAncestor,
				theirFile, theirContent,
				matchesExactlyOne, isNull,
				expectedConflictCount, conflictTypes,
				expectedChangesCount, changeTypes, out resultingConflicts);
		}

		internal static string DoMerge(
			IChorusFileTypeHandler chorusFileHandler,
			TempFile ourFile, string ourContent,
			TempFile commonFile, string commonAncestor,
			TempFile theirFile, string theirContent,
			IEnumerable<string> matchesExactlyOne, IEnumerable<string> isNull,
			int expectedConflictCount, List<Type> conflictTypes,
			int expectedChangesCount, List<Type> changeTypes, out List<IConflict> resultingConflicts)
		{
			File.WriteAllText(ourFile.Path, ourContent);
			if (commonFile != null)
				File.WriteAllText(commonFile.Path, commonAncestor);
			File.WriteAllText(theirFile.Path, theirContent);

			var situation = new NullMergeSituation();
			var mergeOrder = new MergeOrder(ourFile.Path, (commonFile == null ? null : commonFile.Path), theirFile.Path, situation);
			var eventListener = new ListenerForUnitTests();
			mergeOrder.EventListener = eventListener;

			chorusFileHandler.Do3WayMerge(mergeOrder);
			var result = File.ReadAllText(ourFile.Path);
			if (matchesExactlyOne != null)
			{
				foreach (var query in matchesExactlyOne)
					XmlTestHelper.AssertXPathMatchesExactlyOne(result, query);
			}
			if (isNull != null)
			{
				foreach (var query in isNull)
					XmlTestHelper.AssertXPathIsNull(result, query);
			}
			eventListener.AssertExpectedConflictCount(expectedConflictCount);
			Assert.That(eventListener.Conflicts.Select(x => x.GetType()), Is.EqualTo(conflictTypes));

			eventListener.AssertExpectedChangesCount(expectedChangesCount);
			Assert.That(expectedChangesCount, Is.EqualTo(changeTypes.Count),
				"Test setup error: expected changes and the count of expected change types don't match");
			Assert.That(eventListener.Changes.Select(x => x.GetType()), Is.EqualTo(changeTypes));

			resultingConflicts = eventListener.Conflicts;
			return result;
		}

		internal static string DoMerge(
			IChorusFileTypeHandler chorusFileHandler,
			string extension,
			string commonAncestor, string ourContent, string theirContent,
			IEnumerable<string> matchesExactlyOne, IEnumerable<string> isNull,
			int expectedConflictCount, List<Type> conflictTypes,
			int expectedChangesCount, List<Type> changeTypes)
		{
			string result;
			using (var ours = TempFile.WithFilename("ours." + extension))
			using (var theirs = new TempFile("theirs." + extension))
			using (var ancestor = new TempFile("common." + extension))
			{
				result = DoMerge(chorusFileHandler,
								 ours, ourContent,
								 ancestor, commonAncestor,
								 theirs, theirContent,
								 matchesExactlyOne, isNull,
								 expectedConflictCount, conflictTypes,
								 expectedChangesCount, changeTypes);
			}
			return result;
		}

		internal static XmlNode CreateNodes(string commonAncestor, string ourContent, string theirContent, out XmlNode theirNode, out XmlNode ancestorNode)
		{
			var ancestorDoc = new XmlDocument();
			ancestorDoc.LoadXml(commonAncestor);
			ancestorNode = ancestorDoc.DocumentElement.FirstChild;

			var ourDoc = new XmlDocument();
			ourDoc.LoadXml(ourContent);
			var ourNode = ourDoc.DocumentElement.FirstChild;

			var theirDoc = new XmlDocument();
			theirDoc.LoadXml(theirContent);
			theirNode = theirDoc.DocumentElement.FirstChild;
			return ourNode;
		}

		internal static XmlNode GetNode(string input)
		{
			var doc = new XmlDocument();
			doc.LoadXml(input);
			return doc.DocumentElement;
		}

		internal static IChorusFileTypeHandler CreateChorusFileHandlers()
		{
			return (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
			 where handler.GetType().Name == "FieldWorksCommonFileHandler"
			 select handler).First();
		}
	}
}