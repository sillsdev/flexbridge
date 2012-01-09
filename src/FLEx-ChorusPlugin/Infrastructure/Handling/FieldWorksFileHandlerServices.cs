using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHanders;
using Chorus.FileTypeHanders.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers.Mercurial;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using FLEx_ChorusPlugin.Infrastructure.Handling.CustomProperties;
using FLEx_ChorusPlugin.Infrastructure.Handling.ModelVersion;

namespace FLEx_ChorusPlugin.Infrastructure.Handling
{
	internal static class FieldWorksFileHandlerServices
	{
		internal static string ValidateFile(string pathToFile)
		{
			if (String.IsNullOrEmpty(pathToFile))
				return "No Pathname";
			if (!File.Exists(pathToFile))
				return "File does not exist.";

			switch (GetExtensionFromPathname(pathToFile))
			{
				default:
					return "Unrecognized extension";
				case SharedConstants.ClassData:
					try
					{
						var settings = new XmlReaderSettings { ValidationType = ValidationType.None };
						using (var reader = XmlReader.Create(pathToFile, settings))
						{
							reader.MoveToContent();
							if (reader.LocalName == "classdata")
							{
								// It would be nice, if it could really validate it.
								while (reader.Read())
								{
								}
							}
							else
							{
								throw new InvalidOperationException("Not a FieldWorks file.");
							}
						}
					}
					catch (Exception error)
					{
						return error.Message;
					}
					return null;
				case SharedConstants.Ntbk:
					try
					{
						var settings = new XmlReaderSettings { ValidationType = ValidationType.None };
						using (var reader = XmlReader.Create(pathToFile, settings))
						{
							reader.MoveToContent();
							if (reader.LocalName == "Anthropology")
							{
								// It would be nice, if it could really validate it.
								while (reader.Read())
								{
								}
							}
							else
							{
								throw new InvalidOperationException("Not a FieldWorks data notebook file.");
							}
						}
					}
					catch (Exception e)
					{
						return e.Message;
					}
					return null;
				case SharedConstants.Reversal:
					try
					{
						var settings = new XmlReaderSettings { ValidationType = ValidationType.None };
						using (var reader = XmlReader.Create(pathToFile, settings))
						{
							reader.MoveToContent();
							if (reader.LocalName == "Reversal")
							{
								// It would be nice, if it could really validate it.
								while (reader.Read())
								{
								}
							}
							else
							{
								throw new InvalidOperationException("Not a FieldWorks reversal file.");
							}
						}
					}
					catch (Exception e)
					{
						return e.Message;
					}
					return null;
				case SharedConstants.CustomProperties:
					try
					{
						var doc = XDocument.Load(pathToFile);
						var root = doc.Root;
						if (root.Name.LocalName != SharedConstants.OptionalFirstElementTag || root.Elements("CustomField").Count() == 0)
							return "Not valid custom properties file";

						return null;
					}
					catch (Exception e)
					{
						return e.Message;
					}
				case SharedConstants.ModelVersion:
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
		}

		internal static IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			if (report is IXmlChangeReport)
			{
				switch (GetExtensionFromPathname(report.PathToFile))
				{
					default:
						throw new ArgumentException("Extension not recognized.");
					case SharedConstants.Ntbk:
						throw new NotImplementedException();
					case SharedConstants.ClassData:
						// Fall through for now.
						//return new FieldWorksChangePresenter((IXmlChangeReport) report);
					case SharedConstants.Reversal:
					// Fall through for now.
						//return new FieldWorksChangePresenter((IXmlChangeReport)report);
					case SharedConstants.CustomProperties:
						return new FieldWorksChangePresenter((IXmlChangeReport)report);
					case SharedConstants.ModelVersion:
						return new FieldWorksModelVersionChangePresenter((FieldWorksModelVersionChangeReport)report);
				}
			}

			if (report is ErrorDeterminingChangeReport)
				return (IChangePresenter)report;

			return new DefaultChangePresenter(report, repository);
		}

		internal static IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			var optionalElementName = SharedConstants.Header;
			string repeatingElementName;
			var keyAttrName = SharedConstants.GuidStr;

			switch (GetExtensionFromPathname(child.FullPath))
			{
				default:
					throw new ArgumentException("Extension not recognized.");
				case SharedConstants.ClassData:
					optionalElementName = null;
					repeatingElementName = SharedConstants.RtTag;
					break;
				case SharedConstants.Ntbk:
					repeatingElementName = "RnGenericRec";
					break;
				case SharedConstants.Reversal:
					repeatingElementName = "ReversalIndexEntry";
					break;
				case SharedConstants.CustomProperties:
					optionalElementName = null;
					repeatingElementName = "CustomField";
					keyAttrName = "key";
					break;
				case SharedConstants.ModelVersion:
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
			return Xml2WayDiffService.ReportDifferences(repository, parent, child,
				optionalElementName,
				repeatingElementName, keyAttrName);
		}

		internal static void Do3WayMerge(MergeOrder mergeOrder)
		{
			if (mergeOrder == null)
				throw new ArgumentNullException("mergeOrder");

			var optionalElementName = SharedConstants.Header;
			string repeatingElementName;
			var keyAttrName = SharedConstants.GuidStr;

			IMergeStrategy mergeStrategy;
			Action<XmlReader, XmlWriter> writePreliminaryInformationDelegate;

			var extension = GetExtensionFromPathname(mergeOrder.pathToOurs);
			var mdc = MetadataCache.MdCache;
			switch (extension)
			{
				default:
					throw new ArgumentException("Extension not recognized.");
				case SharedConstants.ClassData:
					optionalElementName = null;
					repeatingElementName = SharedConstants.RtTag;
					FieldWorksMergeStrategyServices.AddCustomPropInfo(mdc, mergeOrder, "DataFiles", 1); // NB: Must be done before FieldWorksMergingStrategy is created.
					mergeStrategy = new FieldWorksMergingStrategy(mergeOrder.MergeSituation, mdc);
					writePreliminaryInformationDelegate = WritePreliminaryClassDataInformation;
					break;
				case SharedConstants.Ntbk:
					repeatingElementName = "RnGenericRec";
					FieldWorksMergeStrategyServices.AddCustomPropInfo(mdc, mergeOrder, "Anthropology", 1); // NB: Must be done before FieldWorksAnthropologyMergeStrategy is created.
					mergeStrategy = new FieldWorksCommonMergeStrategy(mergeOrder.MergeSituation, mdc);
					writePreliminaryInformationDelegate = WritePreliminaryAnthropologyInformation;
					break;
				case SharedConstants.Reversal:
					repeatingElementName = "ReversalIndexEntry";
					FieldWorksMergeStrategyServices.AddCustomPropInfo(mdc, mergeOrder, "Linguistics", 1); // NB: Must be done before FieldWorksReversalMergeStrategy is created.
					mergeStrategy = new FieldWorksCommonMergeStrategy(mergeOrder.MergeSituation, mdc);
					writePreliminaryInformationDelegate = WritePreliminaryReversalInformation;
					break;
				case SharedConstants.CustomProperties:
					optionalElementName = null;
					repeatingElementName = "CustomField";
					keyAttrName = "key";
					// NB: Doesn't need the mdc with custom props.
					mergeStrategy = new FieldWorksCustomPropertyMergingStrategy(mergeOrder.MergeSituation);
					writePreliminaryInformationDelegate = WritePreliminaryCustomPropertyInformation;
					break;
				case SharedConstants.ModelVersion:
					// NB: Doesn't need the mdc with custom props.
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
					return;
			}

			XmlMergeService.Do3WayMerge(mergeOrder,
				mergeStrategy,
				optionalElementName,
				repeatingElementName, keyAttrName, writePreliminaryInformationDelegate);
		}

		internal static bool CanValidateFile(string pathToFile)
		{
			if (string.IsNullOrEmpty(pathToFile))
				return false;
			if (!File.Exists(pathToFile))
				return false;

			switch (GetExtensionFromPathname(pathToFile))
			{
				default:
					return false;
				case SharedConstants.ClassData:
					return FieldWorksOldStyleValidationServices.CanValidateFile(pathToFile);
				case SharedConstants.Ntbk:
					return FieldWorksAnthropologyValidationServices.CanValidateFile(pathToFile);
				case SharedConstants.Reversal:
					return FieldWorksReversalValidationServices.CanValidateFile(pathToFile);
				case SharedConstants.CustomProperties:
					return FieldWorksCustomPropertyValidationServices.CanValidateFile(pathToFile);
				case SharedConstants.ModelVersion:
					return FieldWorksModelVersionValidationServices.CanValidateFile(pathToFile);
			}
		}

		internal static string GetExtensionFromPathname(string pathname)
		{
			if (String.IsNullOrEmpty(pathname))
				throw new ArgumentNullException("pathname");

			return Path.GetExtension(pathname).Substring(1);
		}

		internal static string[] SplitData(string data)
		{
			return data.Split(new[] { "{", ":", "}" }, StringSplitOptions.RemoveEmptyEntries);
		}

		private static void WritePreliminaryClassDataInformation(XmlReader reader, XmlWriter writer)
		{
			reader.MoveToContent();
			writer.WriteStartElement("classdata");
			reader.Read();
		}

		private static void WritePreliminaryAnthropologyInformation(XmlReader reader, XmlWriter writer)
		{
			reader.MoveToContent();
			writer.WriteStartElement("Anthropology");
			reader.Read();
		}

		private static void WritePreliminaryReversalInformation(XmlReader reader, XmlWriter writer)
		{
			reader.MoveToContent();
			writer.WriteStartElement("Reversal");
			reader.Read();
		}

		private static void WritePreliminaryCustomPropertyInformation(XmlReader reader, XmlWriter writer)
		{
			reader.MoveToContent();
			writer.WriteStartElement(SharedConstants.OptionalFirstElementTag);
			reader.Read();
		}
	}
}