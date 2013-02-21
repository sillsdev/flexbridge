using System.Linq;
using Chorus.FileTypeHanders;
using FLEx_ChorusPlugin.Infrastructure;
using NUnit.Framework;

namespace FLEx_ChorusPluginTests.Infrastructure.Handling
{
	public abstract class BaseFieldWorksTypeHandlerTests
	{
		protected IChorusFileTypeHandler FileHandler;
		internal MetadataCache Mdc;

		[TestFixtureSetUp]
		public virtual void FixtureSetup()
		{
			FileHandler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
						   where handler.GetType().Name == "FieldWorksCommonFileHandler"
						   select handler).First();
		}

		[TestFixtureTearDown]
		public virtual void FixtureTearDown()
		{
			FileHandler = null;
		}
	}
}