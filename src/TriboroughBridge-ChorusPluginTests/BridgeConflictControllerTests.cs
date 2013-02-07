using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using TriboroughBridge_ChorusPlugin.Controller;

namespace TriboroughBridge_ChorusPluginTests
{
	/// <summary>
	/// This is only a start on testing BridgeConflictController.
	/// </summary>
	[TestFixture]
	public class BridgeConflictControllerTests
	{
		[Test]
		public void AdjustConflctHtml_ReplacesDatabaseCurrent()
		{
			var input =
				@"silfw://localhost/link?app=flex&amp;database=current&amp;server=&amp;tool=default&amp;guid=7b3a3472-7730-474e-b3d2-06779fd751e8&amp;tag=&amp;label=Uni";
			var controller = new BridgeConflictController();
			controller.InitForAdjustConflict("MyProject", null);
			var result = controller.AdjustConflctHtml(input);
			Assert.That(result, Is.EqualTo(@"silfw://localhost/link?app=flex&amp;database=MyProject&amp;server=&amp;tool=default&amp;guid=7b3a3472-7730-474e-b3d2-06779fd751e8&amp;tag=&amp;label=Uni"));
		}

		/// <summary>
		/// If the html contains something like <span class="ws">en-fonipa</span>,
		/// and we can find a corresponding writing system in the project and extract its user-friendly name,
		/// replace the run content with the user-friendly name.
		/// </summary>
		[Test]
		public void AdjustConflctHtml_ReplacesWsRuns()
		{
			// Input contains:
			// Two strings that will be replaced (verifies we can do more than one substitution correctly)
			// One for which input file is missing.
			// One for which replacement is unchanged (verifies we don't get into infinite replace loop).
			// One where the wsID is further wrapped in conflict-marker span
			var input =
				@"some irrelevant stuff <span class='ws'><span style='background: Yellow'>en-fonipa</span></span> more <span class='ws'>en</span>irrelevant <span class='ws'>en-trash</span> stuff.<span class='ws'>es-fonipa</span>".Replace("'", "\"");

			var ldmlContent1 = @"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
		<palaso:abbreviation
			value='Eng (IPA)' />
	</special>

</ldml>".Replace("'", "\"");
			var ldmlContent2 = @"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
		<palaso:abbreviation
			value='en' />
	</special>

</ldml>".Replace("'", "\"");
			var ldmlContent3 = @"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
		<palaso:abbreviation
			value='Spn (IPA)' />
	</special>

</ldml>".Replace("'", "\"");

			// Create matching dummy writing system files.
			var projFolder = Path.Combine(Path.GetTempPath(), "AdjustConflictTestFolder");
			if (Directory.Exists(projFolder))
				Directory.Delete(projFolder, true);
			var wsFolder = Path.Combine(projFolder, "WritingSystemStore");
			Directory.CreateDirectory(wsFolder);
			File.WriteAllText(Path.Combine(wsFolder, "en-fonipa.ldml"), ldmlContent1);
			File.WriteAllText(Path.Combine(wsFolder, "en.ldml"), ldmlContent2);
			File.WriteAllText(Path.Combine(wsFolder, "es-fonipa.ldml"), ldmlContent3);

			var controller = new BridgeConflictController();
			controller.InitForAdjustConflict(null, projFolder);
			var result = controller.AdjustConflctHtml(input);

			Directory.Delete(projFolder, true);

			Assert.That(result, Is.EqualTo(@"some irrelevant stuff <span class='ws'><span style='background: Yellow'>Eng (IPA)</span></span> more <span class='ws'>en</span>irrelevant <span class='ws'>en-trash</span> stuff.<span class='ws'>Spn (IPA)</span>".Replace("'", "\"")));
		}
	}
}
