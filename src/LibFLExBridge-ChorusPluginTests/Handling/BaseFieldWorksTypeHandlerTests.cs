// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using Chorus.FileTypeHandlers;
using LibFLExBridgeChorusPlugin.Infrastructure;
using NUnit.Framework;

namespace LibFLExBridgeChorusPluginTests.Handling
{
	public abstract class BaseFieldWorksTypeHandlerTests
	{
		protected IChorusFileTypeHandler FileHandler;
		internal MetadataCache Mdc;

		[TestFixtureSetUp]
		public virtual void FixtureSetup()
		{
			FileHandler = FieldWorksTestServices.CreateChorusFileHandlers();
		}

		[TestFixtureTearDown]
		public virtual void FixtureTearDown()
		{
			FileHandler = null;
		}

		[SetUp]
		public virtual void TestSetup()
		{
			Mdc = MetadataCache.TestOnlyNewCache;
		}

		[TearDown]
		public virtual void TestTearDown()
		{
			Mdc = null;
		}
	}
}