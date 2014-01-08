using NUnit.Framework;

namespace ECM7.Migrator.Providers.Tests.ImplementationTest.NoQuotesNoSchema
{
	[TestFixture, Category("SqlServerCe")]
	public class SqlServerCeTransformationProviderTest
        : NoSchema.SqlServerCeTransformationProviderTest
	{
        protected override bool AddQuotes
        {
            get { return false; }
        }
	}
}
