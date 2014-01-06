using NUnit.Framework;

namespace ECM7.Migrator.Providers.Tests.ImplementationTest.NoQuotesNoSchema
{
	[TestFixture, Category("SqlServer")]
	public class SqlServerTransformationProviderTest : NoSchema.SqlServerTransformationProviderTest
	{
		protected override bool AddQuotes
		{
			get { return false; }
		}
	}
}
