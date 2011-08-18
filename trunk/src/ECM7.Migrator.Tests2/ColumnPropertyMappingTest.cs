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
		#region Helpers

		private static OracleTransformationProvider CreateOracleProvider()
		{
			return ProviderFactory
				.Create<OracleTransformationProvider>(new OracleConnection())
					as OracleTransformationProvider;
		}

		private static SqlServerTransformationProvider CreateSqlServerProvider()
		{
			return ProviderFactory
				.Create<SqlServerTransformationProvider>(new SqlConnection())
					as SqlServerTransformationProvider;
		}

		#endregion

		#region Oracle

		[Test]
		public void OracleCreatesSql()
		{
			using (var provider = CreateOracleProvider())
			{
				string columnSql = provider.GetColumnSql(new Column("foo", DbType.AnsiString.WithSize(30)), false);
				Assert.AreEqual("\"foo\" varchar2(30)", columnSql.ToLower());
			}
		}

		[Test]
		public void OracleCreatesNotNullSql()
		{
			using (var provider = CreateOracleProvider())
			{
				string columnSql = provider.GetColumnSql(new Column("foo", DbType.AnsiString.WithSize(30), ColumnProperty.NotNull), false);
				Assert.AreEqual("\"foo\" varchar2(30) not null", columnSql.ToLower());
			}
		}

		[Test]
		public void OracleCreatesNotNullSqlWithDefault()
		{
			using (var provider = CreateOracleProvider())
			{
				string columnSql = provider.GetColumnSql(
						new Column("foo", DbType.AnsiString.WithSize(30), ColumnProperty.NotNull, "'test'"), false);
				Assert.AreEqual("\"foo\" varchar2(30) default 'test' not null", columnSql.ToLower());
			}
		}

		#endregion

		[Test]
		public void SqlServerCreatesSql()
		{
			using (var provider = CreateSqlServerProvider())
			{
				string columnSql = provider.GetColumnSql(
					new Column("foo", DbType.AnsiString.WithSize(30), 0), false);
				Assert.AreEqual("[foo] varchar(30)", columnSql.ToLower());
			}
		}

		[Test]
		public void SqlServerCreatesNotNullSql()
		{
			using (var provider = CreateSqlServerProvider())
			{
				string columnSql = provider.GetColumnSql(
						new Column("foo", DbType.AnsiString.WithSize(30), ColumnProperty.NotNull), false);
				Assert.AreEqual("[foo] varchar(30) not null", columnSql.ToLower());
			}
		}

		[Test]
		public void SqlServerCreatesSqWithDefault()
		{
			using (var provider = CreateSqlServerProvider())
			{
				string columnSql = provider.GetColumnSql(
					new Column("foo", DbType.AnsiString.WithSize(30), "'NEW'"), false);
				Assert.AreEqual("[foo] varchar(30) default 'new'", columnSql.ToLower());
			}
		}

		[Test]
		public void SqlServerCreatesSqWithNullDefault()
		{
			using (var provider = CreateSqlServerProvider())
			{
				string columnSql = provider.GetColumnSql(
					new Column("foo", DbType.AnsiString.WithSize(30), "NULL"), false);
				Assert.AreEqual("[foo] varchar(30) default null", columnSql.ToLower());
			}
		}

		[Test]
		public void SqlServerCreatesSqWithBooleanDefault()
		{
			using (var provider = CreateSqlServerProvider())
			{
				string columnSql1 = provider.GetColumnSql(new Column("foo", DbType.Boolean, false), false);
				Assert.AreEqual("[foo] bit default 0", columnSql1.ToLower());

				string columnSql2 = provider.GetColumnSql(new Column("bar", DbType.Boolean, true), false);
				Assert.AreEqual("[bar] bit default 1", columnSql2.ToLower());
			}
		}
	}
}