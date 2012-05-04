using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Contexts.General.UserDefinedLists
{
	internal class UserDefinedListsBoundedContextService
	{
		internal static void NestContext(string generalBaseDir,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			// Write out each user-defined list (unowned CmPossibilityList) in a separate file.
			var userDefinedLists = classData[SharedConstants.CmPossibilityList].Values.Where(listElement => listElement.Attribute(SharedConstants.OwnerGuid) == null).ToList();
			if (!userDefinedLists.Any())
				return; // Nothing to do.

			var userDefinedDir = Path.Combine(generalBaseDir, "UserDefinedLists");
			if (!Directory.Exists(userDefinedDir))
				Directory.CreateDirectory(userDefinedDir);

			foreach (var userDefinedListElement in userDefinedLists)
			{
				CmObjectNestingService.NestObject(
					false,
					userDefinedListElement,
					classData,
					guidToClassMapping);
				FileWriterService.WriteNestedFile(
					Path.Combine(userDefinedDir, "UserList-" + userDefinedListElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant() + "." + SharedConstants.List),
					new XElement("UserDefinedList", userDefinedListElement));
			}
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string generalBaseDir)
		{
			var userDefinedDir = Path.Combine(generalBaseDir, "UserDefinedLists");
			if (!Directory.Exists(userDefinedDir))
				return;

			foreach (var userDefinedListPathname in Directory.GetFiles(userDefinedDir, "*." + SharedConstants.List, SearchOption.TopDirectoryOnly))
			{
				// These are un-owned lists.
				var userDefinedListDoc = XDocument.Load(userDefinedListPathname);
				CmObjectFlatteningService.FlattenObject(userDefinedListPathname,
					sortedData,
					userDefinedListDoc.Root.Element(SharedConstants.CmPossibilityList),
					null); // No owner.
			}
		}
	}
}
