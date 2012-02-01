using ECM7.Migrator.Providers.Tests.ImplementationTest.NoSchema;
using NUnit.Framework;

namespace ECM7.Migrator.Providers.Tests.ImplementationTest.WithSchema
{
	[TestFixture, Category("PostgreSQL")]
	public class PostgreSQLTransformationProvidersSchemaTest : PostgreSQLTransformationProviderTest
	{
		protected override string DefaultSchema
		{
			get { return "Moo"; }
		}
	}
}
