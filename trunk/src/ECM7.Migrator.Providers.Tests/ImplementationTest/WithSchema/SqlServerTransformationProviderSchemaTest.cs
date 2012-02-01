using ECM7.Migrator.Providers.Tests.ImplementationTest.NoSchema;
using NUnit.Framework;

namespace ECM7.Migrator.Providers.Tests.ImplementationTest.WithSchema
{
	[TestFixture, Category("MSSQL")]
	public class SqlServerTransformationProviderSchemaTest : SqlServerTransformationProviderTest
	{
		protected override string DefaultSchema
		{
			get { return "Moo"; }
		}
	}
}
