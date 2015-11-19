// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using LibFLExBridgeChorusPlugin.Handling;
using NUnit.Framework;

namespace LibFLExBridgeChorusPluginTests.Handling
{
	[TestFixture]
	public class UnknownFileTypeHandlerTests
	{
		private readonly UnknownFileTypeHandlerStrategy _handler = new UnknownFileTypeHandlerStrategy();

		[Test]
		public void CannotValidateFile()
		{
			Assert.IsFalse(_handler.CanValidateFile(null));
		}

		[Test]
		public void ValidateFileThrows()
		{
			Assert.Throws<NotSupportedException>(() => _handler.ValidateFile(null));
		}

		[Test]
		public void GetChangePresenterThrows()
		{
			Assert.Throws<NotSupportedException>(() => _handler.GetChangePresenter(null, null));
		}

		[Test]
		public void Find2WayDifferencesThrows()
		{
			Assert.Throws<NotSupportedException>(() => _handler.Find2WayDifferences(null, null, null));
		}

		[Test]
		public void Do3WayMergeThrows()
		{
			Assert.Throws<NotSupportedException>(() => _handler.Do3WayMerge(null, null));
		}
	}
}
