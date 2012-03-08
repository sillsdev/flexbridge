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
		[Test]
		public void EnsureHandlersAreLoaded()
		{
			var handlerNames = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
							   select handler.GetType().Name).ToList();
			Assert.IsTrue(handlerNames.Contains("FieldWorksCommonFileHandler"));
			var unexpectedBridgeHandlers = new HashSet<string>
											{
												"FieldWorksCustomPropertyFileHandler",
												"FieldWorksModelVersionFileHandler",
												"FieldWorksFileHandler",
												"FieldWorksReversalTypeHandler"
											};
			foreach (var unexpectedBridgeHandler in unexpectedBridgeHandlers)
				Assert.IsFalse(handlerNames.Contains(unexpectedBridgeHandler));
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

			var commonHandler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
						   where handler.GetType().Name == "FieldWorksCommonFileHandler"
						   select handler).First();
			var knownExtensions = new HashSet<string>(commonHandler.GetExtensionsOfKnownTextFileTypes());
			Assert.IsTrue(knownExtensions.SetEquals(supportedExtensions));
		}
	}
}
