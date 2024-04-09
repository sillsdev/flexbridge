// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Globalization;
using System.Xml;
using Chorus.FileTypeHandlers;
using LibFLExBridgeChorusPlugin.Infrastructure;
using NUnit.Framework;

namespace LibFLExBridgeChorusPluginTests.Handling
{
	public abstract class BaseFieldWorksTypeHandlerTests
	{
		protected IChorusFileTypeHandler FileHandler;
		internal MetadataCache Mdc;

		[OneTimeSetUp]
		public virtual void FixtureSetup()
		{
			FileHandler = FieldWorksTestServices.CreateChorusFileHandlers();
		}

		[OneTimeTearDown]
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

		public static string GetXPathNodeFrom(string xml, string xpath)
		{
			var doc = new XmlDocument();
			doc.LoadXml(xml);
			var nodes = doc.SelectNodes(xpath);
			if (nodes != null && nodes.Count == 1)
				return nodes[0].InnerXml;
			return string.Empty;
		}

		public static DateTime _expectedUtcDateTime;

		public static string ExpectedUtcDateTimeString => ExpectedDateTime.ToString(SIL.LCModel.Utils.SilUtilsExtensions.LCMTimeFormatWithMillis, CultureInfo.InvariantCulture);

		public static DateTime ExpectedDateTime
		{
			get
			{
				if (_expectedUtcDateTime == null)
				{
					throw new NullReferenceException(
						"You need to set _expectedUtcDateTime prior to calling ExpectedDateTime/ExpectedUtcDateTimeString");
				}

				// Return a DateTime object in the same resolution we get from Chorus
				// (i.e. no timezone set, trimmed ticks)
				return new DateTime(_expectedUtcDateTime.Year, _expectedUtcDateTime.Month,
					_expectedUtcDateTime.Day, _expectedUtcDateTime.Hour,
					_expectedUtcDateTime.Minute, _expectedUtcDateTime.Second,
					_expectedUtcDateTime.Millisecond);
			}
		}
	}
}
