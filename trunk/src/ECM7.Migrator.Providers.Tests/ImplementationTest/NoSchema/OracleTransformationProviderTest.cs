using System;
using System.Text;
using ECM7.Migrator.Providers.Oracle;
using NUnit.Framework;

namespace ECM7.Migrator.Providers.Tests.ImplementationTest.NoSchema
{
	[TestFixture, Category("Oracle")]
	public class OracleTransformationProviderTest
		: TransformationProviderTestBase<OracleTransformationProvider>
	{
		#region Overrides of TransformationProviderTestBase<OracleTransformationProvider>

		protected override string GetSchemaForCompare()
		{
			return provider.ExecuteScalar("select user from dual").ToString();
		}

		public override string ConnectionStrinSettingsName
		{
			get { return "OracleConnectionString"; }
		}

		protected override string BatchSql
		{
			get
			{
				StringBuilder sb = new StringBuilder();

				sb.AppendLine("insert into \"BatchSqlTest\" (\"Id\", \"TestId\") values (11, 111)");
				sb.AppendLine("/");
				sb.AppendLine("insert into \"BatchSqlTest\" (\"Id\", \"TestId\") values (22, 222)");
				sb.AppendLine("/");
				sb.AppendLine("insert into \"BatchSqlTest\" (\"Id\", \"TestId\") values (33, 333)");
				sb.AppendLine("/");
				sb.AppendLine("insert into \"BatchSqlTest\" (\"Id\", \"TestId\") values (44, 444)");
				sb.AppendLine("/");
				sb.AppendLine("/");
				sb.AppendLine("insert into \"BatchSqlTest\" (\"Id\", \"TestId\") values (55, 555)");

				return sb.ToString();
			}
		}

		protected override string ResourceSql
		{
			get { return "ECM7.Migrator.TestAssembly.Res.pgsql.ora.fb.test.res.migration.sql"; }
		}

		#endregion

		[Test]
		public override void CanAddForeignKeyWithUpdateCascade()
		{
			Assert.Throws<NotSupportedException>(base.CanAddForeignKeyWithUpdateCascade);
		}

		[Test]
		public override void CanAddForeignKeyWithUpdateSetDefault()
		{
			Assert.Throws<NotSupportedException>(base.CanAddForeignKeyWithUpdateSetDefault);
		}

		[Test]
		public override void CanAddForeignKeyWithUpdateSetNull()
		{
			Assert.Throws<NotSupportedException>(base.CanAddForeignKeyWithUpdateSetNull);
		}

		[Test]
		public override void CanAddForeignKeyWithDeleteSetDefault()
		{
			Assert.Throws<NotSupportedException>(base.CanAddForeignKeyWithDeleteSetDefault);
		}

		protected override string GetRandomName(string baseName = "")
		{
			return base.GetRandomName(baseName).Substring(0, 27);
		}
	}
}
