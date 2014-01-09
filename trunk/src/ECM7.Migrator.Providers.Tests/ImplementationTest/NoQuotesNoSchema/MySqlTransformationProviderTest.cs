using NUnit.Framework;

namespace ECM7.Migrator.Providers.Tests.ImplementationTest.NoQuotesNoSchema
{
	[TestFixture, Category("MySql")]
	public class MySqlTransformationProviderTest
        : NoSchema.MySqlTransformationProviderTest
	{
        protected override bool AddQuotes
        {
            get { return false; }
        }
	}
}
