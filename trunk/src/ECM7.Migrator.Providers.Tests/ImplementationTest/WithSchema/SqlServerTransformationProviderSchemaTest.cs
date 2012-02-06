using ECM7.Migrator.Providers.Tests.ImplementationTest.NoSchema;
using NUnit.Framework;

namespace ECM7.Migrator.Providers.Tests.ImplementationTest.WithSchema
{
	[TestFixture, Category("SqlServer")]
	public class SqlServerTransformationProviderSchemaTest : SqlServerTransformationProviderTest
	{
		protected override string GetSchemaForCreateTables()
		{
			return "Moo";
		}

		protected override string GetSchemaForCompare()
		{
			return "Moo";
		}
	}
}
