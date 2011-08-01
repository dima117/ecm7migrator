namespace ECM7.Migrator.Tests2
{
	using System.Data;

	using ECM7.Migrator.Framework;
	using ECM7.Migrator.Providers.Oracle;
	using ECM7.Migrator.Providers.SqlServer;

	using NUnit.Framework;

	[TestFixture]
	public class ColumnPropertyMappingTest
	{
		private readonly OracleDialect oracleDialect = new OracleDialect();
		private readonly SqlServerDialect sqlServerDialect = new SqlServerDialect();

		[Test]
		public void OracleCreatesSql()
		{
			string columnSql = oracleDialect.GetColumnSql(new Column("foo", DbType.AnsiString.WithSize(30)), false);
			Assert.AreEqual("\"foo\" varchar2(30)", columnSql.ToLower());
		}

		[Test]
		public void OracleCreatesNotNullSql()
		{
			string columnSql = oracleDialect.GetColumnSql(new Column("foo", DbType.AnsiString.WithSize(30), ColumnProperty.NotNull), false);
			Assert.AreEqual("\"foo\" varchar2(30) not null", columnSql.ToLower());
		}

		[Test]
		public void OracleCreatesNotNullSqlWithDefault()
		{
			string columnSql = oracleDialect.GetColumnSql(new Column("foo", DbType.AnsiString.WithSize(30), ColumnProperty.NotNull, "'test'"), false);
			Assert.AreEqual("\"foo\" varchar2(30) default 'test' not null", columnSql.ToLower());
		}

		[Test]
		public void SqlServerCreatesSql()
		{
			string columnSql = sqlServerDialect.GetColumnSql(new Column("foo", DbType.AnsiString.WithSize(30), 0), false);
			Assert.AreEqual("[foo] varchar(30)", columnSql.ToLower());
		}

		[Test]
		public void SqlServerCreatesNotNullSql()
		{
			string columnSql = sqlServerDialect.GetColumnSql(new Column("foo", DbType.AnsiString.WithSize(30), ColumnProperty.NotNull), false);
			Assert.AreEqual("[foo] varchar(30) not null", columnSql.ToLower());
		}

		[Test]
		public void SqlServerCreatesSqWithDefault()
		{
			string columnSql = sqlServerDialect.GetColumnSql(new Column("foo", DbType.AnsiString.WithSize(30), "'NEW'"), false);
			Assert.AreEqual("[foo] varchar(30) default 'new'", columnSql.ToLower());
		}

		[Test]
		public void SqlServerCreatesSqWithNullDefault()
		{
			string columnSql = sqlServerDialect.GetColumnSql(new Column("foo", DbType.AnsiString.WithSize(30), "NULL"), false);
			Assert.AreEqual("[foo] varchar(30) default null", columnSql.ToLower());
		}

		[Test]
		public void SqlServerCreatesSqWithBooleanDefault()
		{
			string columnSql1 = sqlServerDialect.GetColumnSql(new Column("foo", DbType.Boolean, false), false);
			Assert.AreEqual("[foo] bit default 0", columnSql1.ToLower());

			string columnSql2 = sqlServerDialect.GetColumnSql(new Column("bar", DbType.Boolean, true), false);
			Assert.AreEqual("[bar] bit default 1", columnSql2.ToLower());
		}
	}
}