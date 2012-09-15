using System.Collections.Generic;
using System.Linq;
using Chorus.FileTypeHanders;
using FLEx_ChorusPlugin.Infrastructure;
using NUnit.Framework;

namespace FLEx_ChorusPluginTests
{
	[TestFixture]
	public class LoadHandlersTests
	{
		private IChorusFileTypeHandler _commonHandler;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_commonHandler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
							  where handler.GetType().Name == "FieldWorksCommonFileHandler"
							  select handler).FirstOrDefault();
		}

		[TestFixtureTearDown]
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
				SharedConstants.ModelVersion,		// 'ModelVersion' Better validation done.
				SharedConstants.CustomProperties,	// 'CustomProperties' Better validation done.
				SharedConstants.Style,				// 'style'
				SharedConstants.List,				// 'list'

				// General
				SharedConstants.langproj,			// 'langproj'
				SharedConstants.Annotation,			// 'annotation'
				SharedConstants.Filter,				// 'filter'

				// Scripture
				SharedConstants.ArchivedDraft,		// 'ArchivedDraft'
				SharedConstants.ImportSetting,		// 'ImportSetting'
				SharedConstants.Srs,				// 'srs'
				SharedConstants.Trans,				// 'trans'
				SharedConstants.bookannotations,	// 'bookannotations'
				SharedConstants.book,				// 'book'

				// Anthropology
				SharedConstants.Ntbk,				// 'ntbk'

				// Linguistics
				SharedConstants.Reversal,			// 'reversal'
				SharedConstants.Lexdb,				// 'lexdb' The lexicon only added one new extension "lexdb", as the lists are already taken care of.
				SharedConstants.TextInCorpus,		// 'textincorpus' Text corpus only added one new extension "textincorpus", as the list is already taken care of.
				SharedConstants.Inventory,			// 'inventory' inventory
				SharedConstants.DiscourseExt,		// 'discourse' discourse
				SharedConstants.Featsys,			// 'featsys' Feature structure systems (Phon and Morph & Syn)
				SharedConstants.Phondata,			// 'phondata'
				SharedConstants.Morphdata,			// 'morphdata'
				SharedConstants.Agents,				// 'agents'

				// FW layouts
				SharedConstants.fwlayout			// 'fwlayout'
			};

			var knownExtensions = new HashSet<string>(_commonHandler.GetExtensionsOfKnownTextFileTypes());
			Assert.IsTrue(knownExtensions.SetEquals(supportedExtensions));
		}
	}
}
