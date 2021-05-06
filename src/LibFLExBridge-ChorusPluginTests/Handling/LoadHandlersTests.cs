// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using System.Linq;
using Chorus.FileTypeHandlers;
using LibFLExBridgeChorusPlugin.Infrastructure;
using NUnit.Framework;

namespace LibFLExBridgeChorusPluginTests.Handling
{
	[TestFixture]
	public class LoadHandlersTests
	{
		private IChorusFileTypeHandler _commonHandler;

		[OneTimeSetUp]
		public void FixtureSetup()
		{
			_commonHandler = ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
				.FirstOrDefault(h => h.GetType().Name == "FieldWorksCommonFileHandler");
		}

		[OneTimeTearDown]
		public void FixtureTearDown()
		{
			_commonHandler = null;
		}

		[Test]
		public void EnsureHandlerIsLoaded()
		{
			Assert.IsNotNull(_commonHandler);
		}

		[Test]
		public void EnsureAllSupportedExtensionsAreReturned()
		{
			var supportedExtensions = new HashSet<string>
			{
				// Common
				FlexBridgeConstants.ModelVersion,		// 'ModelVersion' Better validation done.
				FlexBridgeConstants.CustomProperties,	// 'CustomProperties' Better validation done.
				FlexBridgeConstants.Style,				// 'style'
				FlexBridgeConstants.List,				// 'list'

				// General
				FlexBridgeConstants.langproj,			// 'langproj'
				FlexBridgeConstants.Annotation,			// 'annotation'
				FlexBridgeConstants.Filter,				// 'filter'
				FlexBridgeConstants.orderings,			// 'orderings'
				FlexBridgeConstants.pictures,			// 'pictures'

				// Scripture
				FlexBridgeConstants.ArchivedDraft,		// 'ArchivedDraft'
				FlexBridgeConstants.ImportSetting,		// 'ImportSetting'
				FlexBridgeConstants.Srs,				// 'srs'
				FlexBridgeConstants.Trans,				// 'trans'
				FlexBridgeConstants.bookannotations,	// 'bookannotations'
				FlexBridgeConstants.book,				// 'book'

				// Anthropology
				FlexBridgeConstants.Ntbk,				// 'ntbk'

				// Linguistics
				FlexBridgeConstants.Reversal,			// 'reversal'
				FlexBridgeConstants.Lexdb,				// 'lexdb' The lexicon only added one new extension "lexdb", as the lists are already taken care of.
				FlexBridgeConstants.TextInCorpus,		// 'textincorpus' Text corpus only added one new extension "textincorpus", as the list is already taken care of.
				FlexBridgeConstants.Inventory,			// 'inventory' inventory
				FlexBridgeConstants.DiscourseExt,		// 'discourse' discourse
				FlexBridgeConstants.Featsys,			// 'featsys' Feature structure systems (Phon and Morph & Syn)
				FlexBridgeConstants.Phondata,			// 'phondata'
				FlexBridgeConstants.Morphdata,			// 'morphdata'
				FlexBridgeConstants.Agents,				// 'agents'

				// FW layouts
				FlexBridgeConstants.fwlayout,			// 'fwlayout'
				FlexBridgeConstants.fwdictconfig,		// 'fwdictconfig'

				// Lexicon settings
				FlexBridgeConstants.ProjectLexiconSettingsExtension
			};

			var knownExtensions = new HashSet<string>(_commonHandler.GetExtensionsOfKnownTextFileTypes());
			Assert.That(knownExtensions, Is.EquivalentTo(supportedExtensions));
		}
	}
}
