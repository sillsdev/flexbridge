using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Chorus.FileTypeHanders;
using Chorus.merge;
using FLEx_ChorusPluginTests.BorrowedCode;
using NUnit.Framework;
using Palaso.IO;

namespace FLEx_ChorusPluginTests
{
	internal static class FieldWorksTestServices
	{
		internal const int ExpectedExtensionCount = 13;

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
			Directory.Delete(parentDir);

			parentDir = Path.GetDirectoryName(commonFile.Path);
			commonFile.Dispose();
			commonFile = null;
			Directory.Delete(parentDir);

			parentDir = Path.GetDirectoryName(theirFile.Path);
			theirFile.Dispose();
			theirFile = null;
			Directory.Delete(parentDir);
		}

		internal static void SetupTempFilesWithName(string filename, out TempFile ourFile, out TempFile commonFile, out TempFile theirFile)
		{
			var counter = 1;
			ourFile = TempFile.TrackExisting(CreateTempFileWithName(filename, counter++));
			commonFile = TempFile.TrackExisting(CreateTempFileWithName(filename, counter++));
			theirFile = TempFile.TrackExisting(CreateTempFileWithName(filename, counter));
		}

		internal static string CreateTempFileWithName(string filename, int counter)
		{
			var tempPath = Path.GetTempFileName();
			var dirName = Path.GetDirectoryName(tempPath);
			var newDirName = dirName + counter;
			if (Directory.Exists(newDirName))
				Directory.Delete(newDirName, true);
			Directory.CreateDirectory(dirName + counter);
			var replacement = Path.Combine(dirName + counter, filename);
			File.Move(tempPath, replacement);
			return replacement;
		}

		internal static void SetupTempFilesWithExtension(string extension, out TempFile ourFile, out TempFile commonFile, out TempFile theirFile)
		{
			ourFile = TempFile.TrackExisting(CreateTempFileWithExtension(extension));
			commonFile = TempFile.TrackExisting(CreateTempFileWithExtension(extension));
			theirFile = TempFile.TrackExisting(CreateTempFileWithExtension(extension));
		}

		internal static string CreateTempFileWithExtension(string extension)
		{
			var tempPath = Path.GetTempFileName();
			var replacement = tempPath.Replace(Path.GetExtension(tempPath), extension);
			if (File.Exists(replacement))
				File.Delete(replacement);
			File.Move(tempPath, replacement);
			return replacement;
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
			File.WriteAllText(ourFile.Path, ourContent);
			File.WriteAllText(commonFile.Path, commonAncestor);
			File.WriteAllText(theirFile.Path, theirContent);

			var situation = new NullMergeSituation();
			var mergeOrder = new MergeOrder(ourFile.Path, commonFile.Path, theirFile.Path, situation);
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
			Assert.AreEqual(conflictTypes.Count, eventListener.Conflicts.Count);
			for (var idx = 0; idx < conflictTypes.Count; ++idx)
				Assert.AreSame(conflictTypes[idx], eventListener.Conflicts[idx].GetType());
			eventListener.AssertExpectedChangesCount(expectedChangesCount);
			Assert.AreEqual(changeTypes.Count, eventListener.Changes.Count);
			for (var idx = 0; idx < changeTypes.Count; ++idx)
				Assert.AreSame(changeTypes[idx], eventListener.Changes[idx].GetType());
			return result;
		}

		internal static string DoMerge(
			IChorusFileTypeHandler chorusFileHandler,
			string commonAncestor, string ourContent, string theirContent,
			IEnumerable<string> matchesExactlyOne, IEnumerable<string> isNull,
			int expectedConflictCount, List<Type> conflictTypes,
			int expectedChangesCount, List<Type> changeTypes)
		{
			string result;
			using (var ours = new TempFile(ourContent))
			using (var theirs = new TempFile(theirContent))
			using (var ancestor = new TempFile(commonAncestor))
			{
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(ours.Path, ancestor.Path, theirs.Path, situation);
				var eventListener = new ListenerForUnitTests();
				mergeOrder.EventListener = eventListener;

				chorusFileHandler.Do3WayMerge(mergeOrder);
				result = File.ReadAllText(ours.Path);
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
				Assert.AreEqual(conflictTypes.Count, eventListener.Conflicts.Count);
				for (var idx = 0; idx < conflictTypes.Count; ++idx)
					Assert.AreSame(conflictTypes[idx], eventListener.Conflicts[idx].GetType());
				eventListener.AssertExpectedChangesCount(expectedChangesCount);
				Assert.AreEqual(changeTypes.Count, eventListener.Changes.Count);
				for (var idx = 0; idx < changeTypes.Count; ++idx)
					Assert.AreSame(changeTypes[idx], eventListener.Changes[idx].GetType());
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
	}
}