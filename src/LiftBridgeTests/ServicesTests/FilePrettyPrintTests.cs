using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.IO;
using SIL.LiftBridge.Services;

namespace LiftBridgeTests.ServicesTests
{
	[TestFixture]
	public class FilePrettyPrintTests
	{
		[Test]
		public void ShouldMaintainCorrectOrderOfEntries()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<lift
	version='0.13'
	producer='SIL.FLEx 7.0.0.40590'>
	<entry
		id='pos1' />
	<entry
		id='pos2' />
	<entry
		id='pos3' />
</lift>";
			const string child =
@"<?xml version='1.0' encoding='utf-8'?><lift version='0.13' producer='SIL.FLEx 7.0.0.40590'><entry id='pos3'/><entry id='pos1'/><entry id='pos2'/></lift>";

			using (var tempParent = new TempFile(parent))
			using (var tempChild = new TempFile(child))
			{
				LiftFileServices.PrettyPrintFile(tempParent.Path, tempChild.Path);
				var newOutputContents = File.ReadAllText(tempParent.Path);
				Assert.AreEqual(parent.Replace("'", "\""), newOutputContents); // Correct order and format.
			}
		}

		[Test]
		public void NewHeaderShouldBeAdded()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<lift
	version='0.13'
	producer='SIL.FLEx 7.0.0.40590'>
	<entry
		id='pos1' />
	<entry
		id='pos2' />
	<entry
		id='pos3' />
</lift>";
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<lift
	version='0.13'
	producer='SIL.FLEx 7.0.0.40590'><header><stuff></stuff></header>
	<entry
		id='pos1' />
	<entry
		id='pos2' />
	<entry
		id='pos3' />
</lift>";

			const string wellformedHeader =
@"	<header>
		<stuff></stuff>
	</header>";

			using (var tempParent = new TempFile(parent))
			using (var tempChild = new TempFile(child))
			{
				LiftFileServices.PrettyPrintFile(tempParent.Path, tempChild.Path);
				var newOutputContents = File.ReadAllText(tempParent.Path);
				Assert.IsTrue(newOutputContents.Contains(wellformedHeader)); // Header added with good format.
			}
		}

		[Test]
		public void ChangedHeaderShouldBeAdded()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<lift
	version='0.13'
	producer='SIL.FLEx 7.0.0.40590'>
	<header>
		<stuff></stuff>
	</header>
	<entry
		id='pos1' />
	<entry
		id='pos2' />
	<entry
		id='pos3' />
</lift>";
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<lift
	version='0.13'
	producer='SIL.FLEx 7.0.0.40590'><header><newstuff></newstuff></header>
	<entry
		id='pos1' />
	<entry
		id='pos2' />
	<entry
		id='pos3' />
</lift>";

			const string replacedHeader =
@"	<header>
		<newstuff></newstuff>
	</header>";

			using (var tempParent = new TempFile(parent))
			using (var tempChild = new TempFile(child))
			{
				LiftFileServices.PrettyPrintFile(tempParent.Path, tempChild.Path);
				var newOutputContents = File.ReadAllText(tempParent.Path);
				Assert.IsTrue(newOutputContents.Contains(replacedHeader)); // Header changed with good format.
			}
		}

		[Test]
		public void RemovedHeaderShouldNotBePresent()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<lift
	version='0.13'
	producer='SIL.FLEx 7.0.0.40590'>
	<header>
		<stuff></stuff>
	</header>
	<entry
		id='pos1' />
	<entry
		id='pos2' />
	<entry
		id='pos3' />
</lift>";
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<lift
	version='0.13'
	producer='SIL.FLEx 7.0.0.40590'>
	<entry
		id='pos1' />
	<entry
		id='pos2' />
	<entry
		id='pos3' />
</lift>";

			const string removedHeader =
@"	<header>
		<stuff></stuff>
	</header>";

			using (var tempParent = new TempFile(parent))
			using (var tempChild = new TempFile(child))
			{
				LiftFileServices.PrettyPrintFile(tempParent.Path, tempChild.Path);
				var newOutputContents = File.ReadAllText(tempParent.Path);
				Assert.IsFalse(newOutputContents.Contains(removedHeader)); // Header removed.
			}
		}
	}
}
