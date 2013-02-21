using System.IO;
using System.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using NUnit.Framework;
using Palaso.IO;
using Palaso.Progress;

namespace FLEx_ChorusPluginTests.Infrastructure.Handling.Linguistics.MorphologyAndSyntax
{
	[TestFixture]
	public class FieldWorksAnalyzingAgentTypeHandlerTests : BaseFieldWorksTypeHandlerTests
	{
		private TempFile _ourFile;
		private TempFile _theirFile;
		private TempFile _commonFile;

		[SetUp]
		public void TestSetup()
		{
			FieldWorksTestServices.SetupTempFilesWithName(SharedConstants.AnalyzingAgentsFilename, out _ourFile,
														  out _commonFile, out _theirFile);
			Mdc = MetadataCache.TestOnlyNewCache;
		}

		[TearDown]
		public void TestTearDown()
		{
			FieldWorksTestServices.RemoveTempFilesAndParentDir(ref _ourFile, ref _commonFile, ref _theirFile);
			Mdc = null;
		}

		[Test]
		public void DescribeInitialContentsShouldHaveAddedForLabel()
		{
			var initialContents = FileHandler.DescribeInitialContents(null, null).ToList();
			Assert.AreEqual(1, initialContents.Count);
			var onlyOne = initialContents.First();
			Assert.AreEqual("Added", onlyOne.ActionLabel);
		}

		[Test]
		public void ExtensionOfKnownFileTypesShouldBeAgents()
		{
			var extensions = FileHandler.GetExtensionsOfKnownTextFileTypes().ToArray();
			Assert.AreEqual(FieldWorksTestServices.ExpectedExtensionCount, extensions.Count(), "Wrong number of extensions.");
			Assert.IsTrue(extensions.Contains(SharedConstants.Agents));
		}

		[Test]
		public void ShouldNotBeAbleToValidateIncorrectFormatFile()
		{
			using (var tempModelVersionFile = new TempFile("<classdata />"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, SharedConstants.Agents);
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsFalse(FileHandler.CanValidateFile(newpath));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldBeAbleToValidateInProperlyFormattedFile()
		{
			const string data =
@"<AnalyzingAgents>
<CmAgent guid='fff03918-9674-4401-8bb1-efe6502985a7' />
</AnalyzingAgents>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
		}

		[Test]
		public void ShouldBeAbleToDoAllCanOperations()
		{
			const string data =
@"<AnalyzingAgents>
<CmAgent guid='fff03918-9674-4401-8bb1-efe6502985a7' />
</AnalyzingAgents>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanDiffFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanMergeFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanPresentFile(_ourFile.Path));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFile1()
		{
			const string data = "<classdata />";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFile2()
		{
			const string data =
@"<AnalyzingAgents>
<header>
</header>
</AnalyzingAgents>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldBeAbleToValidateFile1()
		{
			const string data =
@"<AnalyzingAgents>
<CmAgent guid='fff03918-9674-4401-8bb1-efe6502985a7' >
</CmAgent>
</AnalyzingAgents>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldBeAbleToValidateFile2()
		{
			const string data =
@"<AnalyzingAgents>
<CmAgent guid='fff03918-9674-4401-8bb1-efe6502985a7' >
</CmAgent>
<CmAgent guid='fff03918-9674-4401-8bb1-efe6502985a8' >
</CmAgent>
</AnalyzingAgents>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}
	}
}