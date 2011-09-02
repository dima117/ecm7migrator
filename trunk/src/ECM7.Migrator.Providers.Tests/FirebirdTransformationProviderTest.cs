using System.Text;

namespace ECM7.Migrator.Providers.Tests
{
	using ECM7.Migrator.Providers.Firebird;

    using NUnit.Framework;

    [TestFixture, Category("Firebird")]
    public class FirebirdTransformationProviderTest
	{
    	public bool UseTransaction
    	{
			get { return false; }
    	}

		protected string BatchSql
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

		protected string ResourceSql
		{
			get
			{
				return "ECM7.Migrator.TestAssembly.Res.pgsql.ora.fb.test.res.migration.sql";
			}
		}
	}
}