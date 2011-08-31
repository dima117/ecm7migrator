using System.Text;

namespace ECM7.Migrator.Providers.Tests
{
	using Oracle;

	using NUnit.Framework;

	[TestFixture, Category("Oracle")]
	public class OracleTransformationProviderTest : TransformationProviderBase<OracleTransformationProvider>
	{
		protected override string BatchSql
		{
			get
			{
				StringBuilder sb = new StringBuilder();

				sb.AppendLine("insert into \"TestTwo\" (\"Id\", \"TestId\") values (11, 111)");
				sb.AppendLine("/");
				sb.AppendLine("insert into \"TestTwo\" (\"Id\", \"TestId\") values (22, 222)");
				sb.AppendLine("/");
				sb.AppendLine("insert into \"TestTwo\" (\"Id\", \"TestId\") values (33, 333)");
				sb.AppendLine("/");
				sb.AppendLine("insert into \"TestTwo\" (\"Id\", \"TestId\") values (44, 444)");
				sb.AppendLine("/");
				sb.AppendLine("/");
				sb.AppendLine("insert into \"TestTwo\" (\"Id\", \"TestId\") values (55, 555)");

				return sb.ToString();
			}
		}

		protected override string ResourceSql
		{
			get { return "ECM7.Migrator.TestAssembly.Res.pgsql.ora.fb.test.res.migration.sql"; }
		}

		public override string ConnectionStrinSettingsName
		{
			get { return "OracleConnectionString"; }
		}

		public override bool UseTransaction
		{
			get { return true; }
		}
	}
}