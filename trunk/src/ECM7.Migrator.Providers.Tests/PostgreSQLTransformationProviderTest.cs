using System.Text;

namespace ECM7.Migrator.Providers.Tests
{
	using PostgreSQL;

	using NUnit.Framework;

	[TestFixture, Category("Postgre")]
	public class PostgreSQLTransformationProviderTest 
		: TransformationProviderConstraintBase<PostgreSQLTransformationProvider>
	{
		public override string ConnectionStrinSettingsName
		{
			get { return "NpgsqlConnectionString"; }
		}

		public override bool UseTransaction
		{
			get { return true; }
		}

		protected override string BatchSql
		{
			get
			{
				StringBuilder sb = new StringBuilder();

				sb.AppendLine("insert into \"TestTwo\" (\"Id\", \"TestId\") values (11, 111);");
				sb.AppendLine("insert into \"TestTwo\" (\"Id\", \"TestId\") values (22, 222);");
				sb.AppendLine("insert into \"TestTwo\" (\"Id\", \"TestId\") values (33, 333);");
				sb.AppendLine("insert into \"TestTwo\" (\"Id\", \"TestId\") values (44, 444);");
				sb.AppendLine("insert into \"TestTwo\" (\"Id\", \"TestId\") values (55, 555);");

				return sb.ToString();
			}
		}

		protected override string ResourceSql
		{
			get { return "ECM7.Migrator.TestAssembly.Res.pgsql.ora.test.res.migration.sql"; }
		}
	}
}