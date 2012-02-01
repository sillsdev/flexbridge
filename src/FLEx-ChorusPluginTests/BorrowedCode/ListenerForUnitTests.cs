using System.Collections.Generic;
#if DEBUG
using System.Diagnostics;
#endif
using Chorus.merge;
using Chorus.merge.xml.generic;
using NUnit.Framework;

namespace FLEx_ChorusPluginTests.BorrowedCode
{
	internal sealed class ListenerForUnitTests : IMergeEventListener
	{
		internal List<IConflict> Conflicts = new List<IConflict>();
		internal List<IConflict> Warnings = new List<IConflict>();
		internal List<IChangeReport> Changes = new List<IChangeReport>();
		internal List<ContextDescriptor> Contexts = new List<ContextDescriptor>();

		#region Implementation of IMergeEventListener

		void IMergeEventListener.ConflictOccurred(IConflict conflict)
		{
			Conflicts.Add(conflict);
		}

		void IMergeEventListener.WarningOccurred(IConflict warning)
		{
			Warnings.Add(warning);
		}

		void IMergeEventListener.ChangeOccurred(IChangeReport change)
		{
			Changes.Add(change);
		}

		void IMergeEventListener.EnteringContext(ContextDescriptor context)
		{
			Contexts.Add(context);
		}

		#endregion Implementation of IMergeEventListener

		internal void AssertExpectedConflictCount(int count)
		{
			if (count != Conflicts.Count)
			{
#if DEBUG
				Debug.WriteLine("***Got these conflicts:");
				foreach (var conflict in Conflicts)
				{
					Debug.WriteLine("    " + conflict);
				}
#endif
				Assert.AreEqual(count, Conflicts.Count, "Unexpected Conflict Count");
			}
		}

		internal void AssertExpectedChangesCount(int count)
		{
			if (count != Changes.Count)
			{
#if DEBUG
				Debug.WriteLine("***Got these changes:");
				foreach (var change in Changes)
				{
					Debug.WriteLine("    " + change.GetType().Name);
				}
#endif
				Assert.AreEqual(count, Changes.Count, "Unexpected Change Count");
			}
		}

		internal void AssertFirstChangeType<TExpected>()
		{
			Assert.AreEqual(typeof(TExpected), Changes[0].GetType());
		}

		internal void AssertFirstConflictType<TExpected>()
		{
			Assert.AreEqual(typeof(TExpected), Conflicts[0].GetType());
		}
	}
}