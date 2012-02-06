using ECM7.Migrator.Providers.SqlServer;
using NUnit.Framework;

namespace ECM7.Migrator.Providers.Tests.ImplementationTest.NoSchema
{
	[TestFixture, Category("SqlServer")]
	public class SqlServerTransformationProviderTest
		: TransformationProviderTestBase<SqlServerTransformationProvider>
	{
		#region Overrides of TransformationProviderTestBase<SqlServerTransformationProvider>

		protected override string GetSchemaForCompare()
		{
			return provider.ExecuteScalar("select SCHEMA_NAME()").ToString();
		}

		public override string ConnectionStrinSettingsName
		{
			get { return "SqlServerConnectionString"; }
		}

		protected override string BatchSql
		{
			get
			{
				return @"
					insert into [BatchSqlTest] ([Id], [TestId]) values (11, 111)
					GO
					insert into [BatchSqlTest] ([Id], [TestId]) values (22, 222)
					GO
					insert into [BatchSqlTest] ([Id], [TestId]) values (33, 333)
					GO
					insert into [BatchSqlTest] ([Id], [TestId]) values (44, 444)
					GO
					go
					insert into [BatchSqlTest] ([Id], [TestId]) values (55, 555)";
			}
		}

		#endregion
	}
}
