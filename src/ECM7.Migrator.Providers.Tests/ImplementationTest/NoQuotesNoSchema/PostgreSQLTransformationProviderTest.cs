using NUnit.Framework;

namespace ECM7.Migrator.Providers.Tests.ImplementationTest.NoQuotesNoSchema
{
	[TestFixture, Category("PostgreSQL")]
	public class PostgreSQLTransformationProviderTest
		: NoSchema.PostgreSQLTransformationProviderTest
	{
		protected override bool IgnoreCase
		{
			get { return true; }
		}

		protected override bool AddQuotes
		{
			get { return false; }
		}
	}
}
