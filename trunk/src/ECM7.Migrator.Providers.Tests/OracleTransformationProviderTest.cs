namespace ECM7.Migrator.Providers.Tests
{
	using System;
	using System.Configuration;
	using System.Data;

	using ECM7.Migrator.Framework;
	using ECM7.Migrator.Providers.Oracle;

	using NUnit.Framework;

	[TestFixture, Category("Oracle")]
	public class OracleTransformationProviderTest : TransformationProviderConstraintBase
	{
		[SetUp]
		public void SetUp()
		{
			string constr = ConfigurationManager.AppSettings["OracleConnectionString"];
			if (constr == null)
				throw new ArgumentNullException("OracleConnectionString", "No config file");
			provider = new OracleTransformationProvider(new OracleDialect(), constr);
			provider.BeginTransaction();

			AddDefaultTable();
		}

		[Test, ExpectedException(typeof(NotSupportedException))]
		public override void CanAddForeignKeyWithDifferentActions()
		{
			base.CanAddForeignKeyWithDifferentActions();
		}

		[Test]
		public override void ChangeColumn()
		{
			provider.ChangeColumn("TestTwo", new Column("TestId", DbType.String, 50, ColumnProperty.Null));
			Assert.IsTrue(provider.ColumnExists("TestTwo", "TestId"));
			provider.Insert("TestTwo", new[] { "Id", "TestId" }, new[] { "0", "Not an Int val." });
		}

		[Test]
		public override void InsertData()
		{
			provider.Insert("TestTwo", new[] { "Id", "TestId" }, new[] { "1", "1" });
			provider.Insert("TestTwo", new[] { "Id", "TestId" }, new[] { "2", "2" });

			string sql = "SELECT {0} FROM {1}".FormatWith(
				provider.QuoteName("TestId"), provider.QuoteName("TestTwo"));

			using (IDataReader reader = provider.ExecuteQuery(sql))
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


			using (IDataReader reader = provider.ExecuteQuery(sql))
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
			AddTable();
			provider.Insert("Test", new[] { "Id", "Title" }, new[] { "1", "foo" });
			provider.Insert("Test", new[] { "Id", "Title" }, new[] { "2", null });

			provider.Update("Test", new[] { "Title" }, new string[] { null });

			string sql = "SELECT {0} FROM {1}".FormatWith(
				provider.QuoteName("Title"), provider.QuoteName("Test"));


			using (IDataReader reader = provider.ExecuteQuery(sql))
			{
				string[] vals = GetStringVals(reader);

				Assert.IsNull(vals[0]);
				Assert.IsNull(vals[1]);
			}

		}
	}
}