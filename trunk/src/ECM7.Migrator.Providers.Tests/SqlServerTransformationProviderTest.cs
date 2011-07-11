namespace ECM7.Migrator.Providers.Tests
{
	using System;
	using System.Configuration;
	using System.Data;

	using ECM7.Migrator.Framework;
	using ECM7.Migrator.Providers;
	using ECM7.Migrator.Providers.SqlServer;

	using NUnit.Framework;

	[TestFixture, Category("SqlServer")]
	public class SqlServerTransformationProviderTest : TransformationProviderConstraintBase
	{
		[SetUp]
		public void SetUp()
		{
			string constr = ConfigurationManager.AppSettings["SqlServerConnectionString"];
			if (constr == null)
				throw new ArgumentNullException("SqlServerConnectionString", "No config file");

			provider = new SqlServerTransformationProvider(new SqlServerDialect(), constr);
			provider.BeginTransaction();

			AddDefaultTable();
		}

		[Test]
		public void QuoteCreatesProperFormat()
		{
			Dialect dialect = new SqlServerDialect();
			Assert.AreEqual("[foo]", dialect.Quote("foo"));
		}
        
		[Test]
		public void ByteColumnWillBeCreatedAsBlob()
		{
			provider.AddColumn("TestTwo", "BlobColumn", DbType.Byte);
			Assert.IsTrue(provider.ColumnExists("TestTwo", "BlobColumn"));
		}

		[Test]
		public void CanAddTableWithPrimaryKeyAndIdentity()
		{
			provider.AddTable("Test",
				new Column("Id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
				new Column("Name", DbType.String, 100, ColumnProperty.Null)
				);
			Assert.IsTrue(provider.TableExists("Test"));
		}
	}
}