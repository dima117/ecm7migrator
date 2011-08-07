using System.Data.SqlClient;
using ECM7.Migrator.Providers;
using Oracle.DataAccess.Client;

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
		private readonly OracleTransformationProvider oracleProvider = TransformationProviderFactory
			.Create<OracleTransformationProvider>(new OracleConnection()) as OracleTransformationProvider;

		private readonly SqlServerTransformationProvider sqlServerProvider = TransformationProviderFactory
			.Create<SqlServerTransformationProvider>(new SqlConnection()) as SqlServerTransformationProvider;

		[Test]
		public void OracleCreatesSql()
		{
			string columnSql = this.oracleProvider.GetColumnSql(new Column("foo", DbType.AnsiString.WithSize(30)), false);
			Assert.AreEqual("\"foo\" varchar2(30)", columnSql.ToLower());
		}

		[Test]
		public void OracleCreatesNotNullSql()
		{
			string columnSql = this.oracleProvider.GetColumnSql(new Column("foo", DbType.AnsiString.WithSize(30), ColumnProperty.NotNull), false);
			Assert.AreEqual("\"foo\" varchar2(30) not null", columnSql.ToLower());
		}

		[Test]
		public void OracleCreatesNotNullSqlWithDefault()
		{
			string columnSql = this.oracleProvider.GetColumnSql(new Column("foo", DbType.AnsiString.WithSize(30), ColumnProperty.NotNull, "'test'"), false);
			Assert.AreEqual("\"foo\" varchar2(30) default 'test' not null", columnSql.ToLower());
		}

		[Test]
		public void SqlServerCreatesSql()
		{
			string columnSql = this.sqlServerProvider.GetColumnSql(new Column("foo", DbType.AnsiString.WithSize(30), 0), false);
			Assert.AreEqual("[foo] varchar(30)", columnSql.ToLower());
		}

		[Test]
		public void SqlServerCreatesNotNullSql()
		{
			string columnSql = this.sqlServerProvider.GetColumnSql(new Column("foo", DbType.AnsiString.WithSize(30), ColumnProperty.NotNull), false);
			Assert.AreEqual("[foo] varchar(30) not null", columnSql.ToLower());
		}

		[Test]
		public void SqlServerCreatesSqWithDefault()
		{
			string columnSql = this.sqlServerProvider.GetColumnSql(new Column("foo", DbType.AnsiString.WithSize(30), "'NEW'"), false);
			Assert.AreEqual("[foo] varchar(30) default 'new'", columnSql.ToLower());
		}

		[Test]
		public void SqlServerCreatesSqWithNullDefault()
		{
			string columnSql = this.sqlServerProvider.GetColumnSql(new Column("foo", DbType.AnsiString.WithSize(30), "NULL"), false);
			Assert.AreEqual("[foo] varchar(30) default null", columnSql.ToLower());
		}

		[Test]
		public void SqlServerCreatesSqWithBooleanDefault()
		{
			string columnSql1 = this.sqlServerProvider.GetColumnSql(new Column("foo", DbType.Boolean, false), false);
			Assert.AreEqual("[foo] bit default 0", columnSql1.ToLower());

			string columnSql2 = this.sqlServerProvider.GetColumnSql(new Column("bar", DbType.Boolean, true), false);
			Assert.AreEqual("[bar] bit default 1", columnSql2.ToLower());
		}
	}
}