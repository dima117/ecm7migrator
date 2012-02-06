using ECM7.Migrator.Providers.Tests.ImplementationTest.NoSchema;
using NUnit.Framework;

namespace ECM7.Migrator.Providers.Tests.ImplementationTest.WithSchema
{
	[TestFixture, Category("Oracle")]
	public class OracleTransformationProviderSchemaTest : OracleTransformationProviderTest
	{
		protected override string GetSchemaForCreateTables()
		{
			return "MOO";
		}

		protected override string GetSchemaForCompare()
		{
			return "MOO";
		}
	}
}
