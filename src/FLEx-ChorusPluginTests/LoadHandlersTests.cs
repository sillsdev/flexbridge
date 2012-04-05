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
				SharedConstants.ModelVersion,		// Better validation done.
				SharedConstants.CustomProperties,	// Better validation done.
				SharedConstants.Style,
				SharedConstants.List,

				// General
				SharedConstants.lint,
				SharedConstants.langproj,
				SharedConstants.Annotation,
				SharedConstants.Filter,

				// Scripture
				SharedConstants.ArchivedDraft,
				SharedConstants.ImportSetting,
				SharedConstants.Srs,
				SharedConstants.Trans,

				// Anthropology
				SharedConstants.Ntbk,

				// Linguistics
				SharedConstants.Reversal,
				SharedConstants.Lexdb, // The lexicon only added one new extension "lexdb", as the lists are already taken care of.
				SharedConstants.TextInCorpus, // Text corpus only added one new extension "textincorpus", as the list is already taken care of.
				SharedConstants.Inventory, // inventory
				SharedConstants.DiscourseExt, // discourse
				SharedConstants.Featsys, // Feature structure systems (Phon and Morph & Syn)
				SharedConstants.Phondata,
				SharedConstants.Morphdata,
				SharedConstants.Agents
			};

			var knownExtensions = new HashSet<string>(_commonHandler.GetExtensionsOfKnownTextFileTypes());
			Assert.IsTrue(knownExtensions.SetEquals(supportedExtensions));
		}
	}
}
