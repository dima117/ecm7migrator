namespace ECM7.Migrator.Providers.Tests
{
	using System.Data;

	using ECM7.Migrator.Framework;
	using ECM7.Migrator.Providers.SqlServer;

	using NUnit.Framework;

	[TestFixture, Category("SqlServer")]
	public class SqlServerTransformationProviderTest : TransformationProviderConstraintBase<SqlServerTransformationProvider>
	{
		public override string ConnectionStrinSettingsName
		{
			get { return "SqlServerConnectionString"; }
		}

		public override bool UseTransaction
		{
			get { return true; }
		}

		protected override string BatchSql
		{
			get
			{
				return @"
				insert into [TestTwo] ([Id], [TestId]) values (11, 111)
				GO
				insert into [TestTwo] ([Id], [TestId]) values (22, 222)
				GO
				insert into [TestTwo] ([Id], [TestId]) values (33, 333)
				GO
				insert into [TestTwo] ([Id], [TestId]) values (44, 444)
				GO
				go
				insert into [TestTwo] ([Id], [TestId]) values (55, 555)
				";
			}
		}

		[Test]
		public void QuoteCreatesProperFormat()
		{
			Assert.AreEqual("[foo]", provider.QuoteName("foo"));
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