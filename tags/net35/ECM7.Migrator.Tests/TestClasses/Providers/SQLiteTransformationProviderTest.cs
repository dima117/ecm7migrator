using System;
using System.Configuration;
using ECM7.Migrator.Providers.SQLite;
using ECM7.Migrator.Tests.Providers;
using NUnit.Framework;

namespace ECM7.Migrator.Tests.TestClasses.Providers
{
	[TestFixture, Category("SQLite")]
	public class SQLiteTransformationProviderTest : TransformationProviderBase
	{
		[SetUp]
		public void SetUp()
		{
			string constr = ConfigurationManager.AppSettings["SQLiteConnectionString"];
			if (constr == null)
				throw new ArgumentNullException("SQLiteConnectionString", "No config file");

			provider = new SQLiteTransformationProvider(new SQLiteDialect(), constr);
			provider.BeginTransaction();
            
			AddDefaultTable();
		}

		[Test]
		public void CanParseSqlDefinitions() 
		{
			const string testSql = "CREATE TABLE bar ( id INTEGER PRIMARY KEY AUTOINCREMENT, bar TEXT, baz INTEGER NOT NULL )";
			string[] columns = ((SQLiteTransformationProvider) provider).ParseSqlColumnDefs(testSql);
			Assert.IsNotNull(columns);
			Assert.AreEqual(3, columns.Length);
			Assert.AreEqual("id INTEGER PRIMARY KEY AUTOINCREMENT", columns[0]);
			Assert.AreEqual("bar TEXT", columns[1]);
			Assert.AreEqual("baz INTEGER NOT NULL", columns[2]);
		}
         
		[Test]
		public void CanParseSqlDefinitionsForColumnNames() 
		{
			const string testSql = "CREATE TABLE bar ( id INTEGER PRIMARY KEY AUTOINCREMENT, bar TEXT, baz INTEGER NOT NULL )";
			string[] columns = ((SQLiteTransformationProvider) provider).ParseSqlForColumnNames(testSql);
			Assert.IsNotNull(columns);
			Assert.AreEqual(3, columns.Length);
			Assert.AreEqual("id", columns[0]);
			Assert.AreEqual("bar", columns[1]);
			Assert.AreEqual("baz", columns[2]);
		}

		[Test]
		public void CanParseColumnDefForNotNull()
		{
			const string nullString = "bar TEXT";
			const string notNullString = "baz INTEGER NOT NULL";
			Assert.IsTrue(((SQLiteTransformationProvider)provider).IsNullable(nullString));
			Assert.IsFalse(((SQLiteTransformationProvider)provider).IsNullable(notNullString));
		}

		[Test]
		public void CanParseColumnDefForName()
		{
			const string nullString = "bar TEXT";
			const string notNullString = "baz INTEGER NOT NULL";
			Assert.AreEqual("bar", ((SQLiteTransformationProvider)provider).ExtractNameFromColumnDef(nullString));
			Assert.AreEqual("baz", ((SQLiteTransformationProvider)provider).ExtractNameFromColumnDef(notNullString));
		}
	}
}