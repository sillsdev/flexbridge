// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
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

		public static string GetXPathNodeFrom(string xml, string xpath)
		{
			var doc = new XmlDocument();
			doc.LoadXml(xml);
			var nodes = doc.SelectNodes(xpath);
			if (nodes != null && nodes.Count == 1)
				return nodes[0].InnerXml;
			return string.Empty;
		}

		public static string DateTimeNowString
		{
			get
			{
				var dateTimeNow = DateTime.UtcNow.ToString("yyyy-M-d H:m:s.fff");
				return dateTimeNow;
			}
		}
	}
}
