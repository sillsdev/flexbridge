using System.IO;
using System.Linq;
using Chorus.FileTypeHanders;
using Chorus.merge;
using NUnit.Framework;
using Palaso.IO;
using FLEx_ChorusPlugin.Infrastructure;

namespace FLEx_ChorusPluginTests.Integration
{
	/// <summary>
	/// Test class that checks that the MetaDataCache gets updated properly.
	///
	/// This is an integration test, since it works with multiple units (FieldWorksModelVersionFileHandler & MetaDataCache).
	/// </summary>
	[TestFixture]
	public class UpdateMetaDataCacheTests
	{
		/// <summary>
		/// NB: In order to make sure it is all done in the right order,
		/// only one test can be done.
		/// </summary>
		[Test]
		public void MetaDataCacheIsUpdated()
		{
			var mdc = MetadataCache.TestOnlyNewCache; // Ensures it is reset to start with 7000044.
			var fileHandler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
							   where handler.GetType().Name == "FieldWorksModelVersionFileHandler"
							   select handler).First();

			// 7000045: Modified Segment
			//		Add: basic "Reference"	[String]
			CheckUpgrade(mdc, fileHandler, 7000045, "Segment", "Reference", DataType.String);

			// 7000046: Modified RnGenericRec
			//		Add: OA "Text"	[Text]
			CheckUpgrade(mdc, fileHandler, 7000046, "RnGenericRec", "Text", DataType.OwningAtomic);

			// 7000047: No actual model change.
			CheckUpgrade(mdc, fileHandler, 7000047);

			// 7000048: No actual model change.
			CheckUpgrade(mdc, fileHandler, 7000048);

			// 7000049:
			//		1. Add CmObject::CmMediaContainer (concrete)
			//			Add: basic "OffsetType"							[Unicode]
			//			Add: OC "MediaURIs"								[CmMediaURI]
			//		1A. Add: CmObject::CmMediaURI classes (concrete)
			//			Add: basic "MediaURI"							[Unicode]
			//		2. Modified Segment
			//			Add: RA "MediaURI"								[CmMediaURI]
			//			Add: basic "BeginTimeOffset"					[Unicode]
			//			Add: basic "EndTimeOffset"						[Unicode]
			//		3. Modified Text:
			//			Remove: SoundFilePath							[Unicode]
			//			Add: OA "MediaFiles"							[CmMediaContainer]
			CheckClassDoesNotExistBeforeUpGrade(mdc, "CmMediaContainer");
			CheckClassDoesNotExistBeforeUpGrade(mdc, "CmMediaURI");
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "Segment", "MediaURI");
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "Segment", "BeginTimeOffset");
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "Segment", "EndTimeOffset");
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "Text", "MediaFiles");
			DoMerge(fileHandler, 7000049);
			// 1.
			var classInfo = CheckClassDoesExistAfterUpGrade(mdc, mdc.GetClassInfo("CmObject"), "CmMediaContainer");
			CheckNewPropertyAfterUpgrade(classInfo, "OffsetType", DataType.Unicode);
			CheckNewPropertyAfterUpgrade(classInfo, "MediaURIs", DataType.OwningCollection);
			// 1A.
			classInfo = CheckClassDoesExistAfterUpGrade(mdc, mdc.GetClassInfo("CmObject"), "CmMediaURI");
			CheckNewPropertyAfterUpgrade(classInfo, "MediaURI", DataType.Unicode);
			// 2.
			classInfo = mdc.GetClassInfo("Segment");
			CheckNewPropertyAfterUpgrade(classInfo, "MediaURI", DataType.ReferenceAtomic);
			CheckNewPropertyAfterUpgrade(classInfo, "BeginTimeOffset", DataType.Unicode);
			CheckNewPropertyAfterUpgrade(classInfo, "EndTimeOffset", DataType.Unicode);
			// 3.
			classInfo = mdc.GetClassInfo("Text");
			CheckNewPropertyAfterUpgrade(classInfo, "SoundFilePath", DataType.Unicode); // Do NOT remove it, yet.
			CheckNewPropertyAfterUpgrade(classInfo, "MediaFiles", DataType.OwningAtomic);

			// 7000050: Modified Segment
			//		Add: RA "Speaker"									[CmPerson]
			CheckUpgrade(mdc, fileHandler, 7000050);
		}

		private static FdoClassInfo CheckClassDoesExistAfterUpGrade(MetadataCache mdc, FdoClassInfo superclass, string newClassname)
		{
			var result = mdc.GetClassInfo(newClassname);
			Assert.IsNotNull(result);
			Assert.AreSame(superclass, result.Superclass);
			return result;
		}

		private static void CheckUpgrade(MetadataCache mdc, IChorusFileTypeHandler fileHandler, int ours)
		{
			var startingClassCount = mdc.AllClasses.Count();
			var startingPropertyCount = mdc.AllClasses.Sum(classInfo => classInfo.AllProperties.Count());
			DoMerge(fileHandler, ours);
			Assert.AreEqual(startingClassCount, mdc.AllClasses.Count(), "Different number of classes.");
			Assert.AreEqual(startingPropertyCount, mdc.AllClasses.Sum(classInfo => classInfo.AllProperties.Count()), "Different number of properties.");
		}

		private static void CheckUpgrade(MetadataCache mdc, IChorusFileTypeHandler fileHandler, int ours, string className, string newPropName, DataType dataType)
		{
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, className, newPropName);

			DoMerge(fileHandler, ours);

			CheckNewPropertyAfterUpgrade(mdc.GetClassInfo(className), newPropName, dataType);
		}

		private static void CheckClassDoesNotExistBeforeUpGrade(MetadataCache mdc, string className)
		{
			Assert.IsNull(mdc.GetClassInfo(className));
		}

		private static void CheckPropertyDoesNotExistBeforeUpGrade(MetadataCache mdc, string className, string newPropName)
		{
			var classInfo = mdc.GetClassInfo(className);
			var newProperty = (from propInfo in classInfo.AllProperties
							   where propInfo.PropertyName == newPropName
							   select propInfo).FirstOrDefault();
			Assert.IsNull(newProperty, string.Format("{0} {1} should not exist yet.", className, newPropName));
		}

		private static void CheckNewPropertyAfterUpgrade(FdoClassInfo classInfo, string newPropName, DataType dataType)
		{
			var newProperty = (from propInfo in classInfo.AllProperties
						   where propInfo.PropertyName == newPropName
						   select propInfo).FirstOrDefault();
			Assert.IsNotNull(newProperty, string.Format("{0} {1} should exist now.", classInfo.ClassName, newPropName));
			Assert.AreEqual(dataType, newProperty.DataType, string.Format("{0} {1} data type should be {2}.", classInfo.ClassName, newPropName, dataType));
		}

		private static void DoMerge(IChorusFileTypeHandler fileHandler, int ours)
		{
			using (var commonTempFile = new TempFile("Common.ModelVersion"))
			using (var ourTempFile = new TempFile("Our.ModelVersion"))
			using (var theirTempFile = new TempFile("Their.ModelVersion"))
			{
				var baseModelVersion = ours - 1;
				File.WriteAllText(commonTempFile.Path, FormatModelVersionData(baseModelVersion));
				File.WriteAllText(ourTempFile.Path, FormatModelVersionData(ours));
				File.WriteAllText(theirTempFile.Path, FormatModelVersionData(baseModelVersion));

				var listener = new ListenerForUnitTests();
				var mergeOrder = new MergeOrder(ourTempFile.Path, commonTempFile.Path, theirTempFile.Path, new NullMergeSituation())
									{
										EventListener = listener
									};
				fileHandler.Do3WayMerge(mergeOrder);
			}
		}

		private static string FormatModelVersionData(int modelVersion)
		{
			return "{\"modelversion\": " + modelVersion + "}";
		}
	}
}