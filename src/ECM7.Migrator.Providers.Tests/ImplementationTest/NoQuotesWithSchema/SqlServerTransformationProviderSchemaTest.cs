using NUnit.Framework;

namespace ECM7.Migrator.Providers.Tests.ImplementationTest.NoQuotesWithSchema
{
	[TestFixture, Category("SqlServer")]
	public class SqlServerTransformationProviderSchemaTest : WithSchema.SqlServerTransformationProviderSchemaTest
	{
		protected override bool AddQuotes
		{
			get { return false; }
		}
	}
}
