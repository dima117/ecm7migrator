using System.Text;
using ECM7.Migrator.Providers.PostgreSQL;
using NUnit.Framework;

namespace ECM7.Migrator.Providers.Tests.ImplementationTest.NoSchema
{
	[TestFixture, Category("PostgreSQL")]
	public class PostgreSQLTransformationProviderTest
		: TransformationProviderTestBase<PostgreSQLTransformationProvider>
	{
		#region Overrides of TransformationProviderTestBase<PostgreSQLTransformationProvider>

		protected override string GetSchemaForCompare()
		{
			return provider.ExecuteScalar("select current_schema()").ToString();
		}

		public override string ConnectionStrinSettingsName
		{
			get { return "NpgsqlConnectionString"; }
		}

		protected override string BatchSql
		{
			get
			{
				StringBuilder sb = new StringBuilder();

				sb.AppendLine("insert into \"BatchSqlTest\" (\"Id\", \"TestId\") values (11, 111);");
				sb.AppendLine("insert into \"BatchSqlTest\" (\"Id\", \"TestId\") values (22, 222);");
				sb.AppendLine("insert into \"BatchSqlTest\" (\"Id\", \"TestId\") values (33, 333);");
				sb.AppendLine("insert into \"BatchSqlTest\" (\"Id\", \"TestId\") values (44, 444);");
				sb.AppendLine("insert into \"BatchSqlTest\" (\"Id\", \"TestId\") values (55, 555);");

				return sb.ToString();
			}
		}

		protected override string ResourceSql
		{
			get { return "ECM7.Migrator.TestAssembly.Res.pgsql.ora.fb.test.res.migration.sql"; }
		}

		#endregion
	}
}
