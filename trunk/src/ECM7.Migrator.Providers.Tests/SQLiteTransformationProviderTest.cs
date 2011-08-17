namespace ECM7.Migrator.Providers.Tests
{
	using ECM7.Migrator.Providers.SQLite;

	using NUnit.Framework;

	[TestFixture, Category("SQLite")]
	public class SQLiteTransformationProviderTest : TransformationProviderBase<SQLiteTransformationProvider>
	{
		public override string ConnectionStrinSettingsName
		{
			get { return "SQLiteConnectionString"; }
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
		public void CanParseSqlDefinitions() 
		{
			const string TEST_SQL = "CREATE TABLE bar ( id INTEGER PRIMARY KEY AUTOINCREMENT, bar TEXT, baz INTEGER NOT NULL )";
			string[] columns = ((SQLiteTransformationProvider) provider).ParseSqlColumnDefs(TEST_SQL);
			Assert.IsNotNull(columns);
			Assert.AreEqual(3, columns.Length);
			Assert.AreEqual("id INTEGER PRIMARY KEY AUTOINCREMENT", columns[0]);
			Assert.AreEqual("bar TEXT", columns[1]);
			Assert.AreEqual("baz INTEGER NOT NULL", columns[2]);
		}
         
		[Test]
		public void CanParseSqlDefinitionsForColumnNames() 
		{
			const string TEST_SQL = "CREATE TABLE bar ( id INTEGER PRIMARY KEY AUTOINCREMENT, bar TEXT, baz INTEGER NOT NULL )";
			string[] columns = ((SQLiteTransformationProvider) provider).ParseSqlForColumnNames(TEST_SQL);
			Assert.IsNotNull(columns);
			Assert.AreEqual(3, columns.Length);
			Assert.AreEqual("id", columns[0]);
			Assert.AreEqual("bar", columns[1]);
			Assert.AreEqual("baz", columns[2]);
		}

		[Test]
		public void CanParseColumnDefForNotNull()
		{
			const string NULL_STRING = "bar TEXT";
			const string NOT_NULL_STRING = "baz INTEGER NOT NULL";
			Assert.IsTrue(((SQLiteTransformationProvider)provider).IsNullable(NULL_STRING));
			Assert.IsFalse(((SQLiteTransformationProvider)provider).IsNullable(NOT_NULL_STRING));
		}

		[Test]
		public void CanParseColumnDefForName()
		{
			const string NULL_STRING = "bar TEXT";
			const string NOT_NULL_STRING = "baz INTEGER NOT NULL";
			Assert.AreEqual("bar", ((SQLiteTransformationProvider)provider).ExtractNameFromColumnDef(NULL_STRING));
			Assert.AreEqual("baz", ((SQLiteTransformationProvider)provider).ExtractNameFromColumnDef(NOT_NULL_STRING));
		}
	}
}