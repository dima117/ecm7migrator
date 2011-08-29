using System.Text;

namespace ECM7.Migrator.Providers.Tests
{
	using System;
	using System.Data;

	using Framework;
	using Oracle;

	using NUnit.Framework;

	[TestFixture, Category("Oracle")]
	public class OracleTransformationProviderTest : TransformationProviderConstraintBase<OracleTransformationProvider>
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

		[Test, ExpectedException(typeof(NotSupportedException))]
		public override void CanAddForeignKeyWithDifferentActions()
		{
			base.CanAddForeignKeyWithDifferentActions();
		}

		[Test]
		public override void InsertData()
		{
			provider.Insert("TestTwo", new[] { "Id", "TestId" }, new[] { "1", "1" });
			provider.Insert("TestTwo", new[] { "Id", "TestId" }, new[] { "2", "2" });

			string sql = "SELECT {0} FROM {1}".FormatWith(
				provider.QuoteName("TestId"), provider.QuoteName("TestTwo"));

			using (IDataReader reader = provider.ExecuteReader(sql))
			{
				int[] vals = GetVals(reader);

				Assert.IsTrue(Array.Exists(vals, val => val == 1));
				Assert.IsTrue(Array.Exists(vals, val => val == 2));
			}
		}

		[Test]
		public override void UpdateData()
		{
			provider.Insert("TestTwo", new[] { "Id", "TestId" }, new[] { "1", "1" });
			provider.Insert("TestTwo", new[] { "Id", "TestId" }, new[] { "2", "2" });

			provider.Update("TestTwo", new[] { "TestId" }, new[] { "3" });

			string sql = "SELECT {0} FROM {1}".FormatWith(
				provider.QuoteName("TestId"), provider.QuoteName("TestTwo"));


			using (IDataReader reader = provider.ExecuteReader(sql))
			{
				int[] vals = GetVals(reader);

				Assert.IsTrue(Array.Exists(vals, val => val == 3));
				Assert.IsFalse(Array.Exists(vals, val => val == 1));
				Assert.IsFalse(Array.Exists(vals, val => val == 2));
			}
		}

		[Test]
		public override void CanUpdateWithNullData()
		{
			AddTableWithPrimaryKey();
			provider.Insert("Test", new[] { "Id", "Title", "Name" }, new[] { "1", "foo", "moo" });
			provider.Insert("Test", new[] { "Id", "Title", "Name" }, new[] { "2", null, "mi mi" });

			provider.Update("Test", new[] { "Title" }, new string[] { null });

			string sql = provider.FormatSql("SELECT {0:NAME} FROM {1:NAME}", "Title", "Test");

			using (IDataReader reader = provider.ExecuteReader(sql))
			{
				string[] vals = GetStringVals(reader);

				Assert.IsNull(vals[0]);
				Assert.IsNull(vals[1]);
			}
		}
	}
}