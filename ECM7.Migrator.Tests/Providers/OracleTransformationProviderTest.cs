using System;
using System.Configuration;
using System.Data;
using ECM7.Migrator.Framework;
using ECM7.Migrator.Providers.Oracle;
using NUnit.Framework;

namespace ECM7.Migrator.Tests.Providers
{
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

		[Test]
		public override void ChangeColumn()
		{
			provider.ChangeColumn("TestTwo", new Column("TestId", DbType.String, 50, ColumnProperty.Null));
			Assert.IsTrue(provider.ColumnExists("TestTwo", "TestId"));
			provider.Insert("TestTwo", new string[] { "Id", "TestId" }, new string[] { "0", "Not an Int val." });
		}

		[Test]
		public override void InsertData()
		{
			provider.Insert("TestTwo", new string[] { "Id", "TestId" }, new string[] { "1", "1" });
			provider.Insert("TestTwo", new string[] { "Id", "TestId" }, new string[] { "2", "2" });
			using (IDataReader reader = provider.Select("TestId", "TestTwo"))
			{
				int[] vals = GetVals(reader);

				Assert.IsTrue(Array.Exists(vals, val => val == 1));
				Assert.IsTrue(Array.Exists(vals, val => val == 2));
			}
		}

		[Test]
		public override void UpdateData()
		{
			provider.Insert("TestTwo", new string[] { "Id", "TestId" }, new string[] { "1", "1" });
			provider.Insert("TestTwo", new string[] { "Id", "TestId" }, new string[] { "2", "2" });

			provider.Update("TestTwo", new string[] { "TestId" }, new string[] { "3" });

			using (IDataReader reader = provider.Select("TestId", "TestTwo"))
			{
				int[] vals = GetVals(reader);

				Assert.IsTrue(Array.Exists(vals, delegate(int val) { return val == 3; }));
				Assert.IsFalse(Array.Exists(vals, delegate(int val) { return val == 1; }));
				Assert.IsFalse(Array.Exists(vals, delegate(int val) { return val == 2; }));
			}
		}

		[Test]
		public override void CanUpdateWithNullData()
		{
			AddTable();
			provider.Insert("Test", new string[] { "Id", "Title" }, new string[] { "1", "foo" });
			provider.Insert("Test", new string[] { "Id", "Title" }, new string[] { "2", null });

			provider.Update("Test", new string[] { "Title" }, new string[] { null });

			using (IDataReader reader = provider.Select("Title", "Test"))
			{
				string[] vals = GetStringVals(reader);

				Assert.IsNull(vals[0]);
				Assert.IsNull(vals[1]);
			}

		}
	}
}