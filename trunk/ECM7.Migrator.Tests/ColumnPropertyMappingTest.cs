using System.Data;
using ECM7.Migrator.Framework;
using ECM7.Migrator.Providers;
using ECM7.Migrator.Providers.Oracle;
using ECM7.Migrator.Providers.SqlServer;
using NUnit.Framework;

namespace ECM7.Migrator.Tests
{
	[TestFixture]
	public class ColumnPropertyMappingTest
	{
		private readonly OracleDialect oracleDialect = new OracleDialect();
		private readonly SqlServerDialect sqlServerDialect = new SqlServerDialect();

		[Test]
		public void OracleCreatesSql()
		{
			ColumnSqlMap map = oracleDialect.MapColumnProperties(new Column("foo", DbType.AnsiString.WithSize(30)));
			Assert.AreEqual("foo varchar2(30)", map.ColumnSql.ToLower());
		}

		[Test]
		public void OracleCreatesNotNullSql()
		{
			ColumnSqlMap map = oracleDialect.MapColumnProperties(new Column("foo", DbType.AnsiString.WithSize(30), ColumnProperty.NotNull));
			Assert.AreEqual("foo varchar2(30) not null", map.ColumnSql.ToLower());
		}

		[Test]
		public void OracleIndexSqlIsNullWhenIndexedFalse()
		{
			ColumnSqlMap map = oracleDialect.MapColumnProperties(new Column("foo", DbType.AnsiStringFixedLength.WithSize(1)));
			Assert.IsNull(map.IndexSql);
		}

		[Test]
		public void OracleIndexSqlIsNotNullWhenIndexed()
		{
			ColumnSqlMap map = oracleDialect.MapColumnProperties(new Column("foo", DbType.AnsiStringFixedLength.WithSize(1), ColumnProperty.Indexed));
			Assert.IsNotNull(map.IndexSql);
		}

		[Test]
		public void OracleCreatesNotNullSqlWithDefault()
		{
			ColumnSqlMap map = oracleDialect.MapColumnProperties(new Column("foo", DbType.AnsiString.WithSize(30), ColumnProperty.NotNull, "'test'"));
			Assert.AreEqual("foo varchar2(30) default 'test' not null", map.ColumnSql.ToLower());
		}

		[Test]
		public void SqlServerIndexSqlIsNoNullWhenIndexed()
		{
			ColumnSqlMap map = sqlServerDialect.MapColumnProperties(new Column("foo", DbType.AnsiStringFixedLength.WithSize(1), ColumnProperty.Indexed));
			Assert.IsNull(map.IndexSql);
		}

		[Test]
		public void SqlServerCreatesSql()
		{
			ColumnSqlMap map = sqlServerDialect.MapColumnProperties(new Column("foo", DbType.AnsiString.WithSize(30), 0));
			Assert.AreEqual("foo varchar(30)", map.ColumnSql.ToLower());
		}

		[Test]
		public void SqlServerCreatesNotNullSql()
		{
			ColumnSqlMap map = sqlServerDialect.MapColumnProperties(new Column("foo", DbType.AnsiString.WithSize(30), ColumnProperty.NotNull));
			Assert.AreEqual("foo varchar(30) not null", map.ColumnSql.ToLower());
		}

		[Test]
		public void SqlServerCreatesSqWithDefault()
		{
			ColumnSqlMap map = sqlServerDialect.MapColumnProperties(new Column("foo", DbType.AnsiString.WithSize(30), "'NEW'"));
			Assert.AreEqual("foo varchar(30) default 'new'", map.ColumnSql.ToLower());
		}

		[Test]
		public void SqlServerCreatesSqWithNullDefault()
		{
			ColumnSqlMap map = sqlServerDialect.MapColumnProperties(new Column("foo", DbType.AnsiString.WithSize(30), "NULL"));
			Assert.AreEqual("foo varchar(30) default null", map.ColumnSql.ToLower());
		}

		[Test]
		public void SqlServerCreatesSqWithBooleanDefault()
		{
			ColumnSqlMap map = sqlServerDialect.MapColumnProperties(new Column("foo", DbType.Boolean, false));
			Assert.AreEqual("foo bit default 0", map.ColumnSql.ToLower());

			ColumnSqlMap map2 = sqlServerDialect.MapColumnProperties(new Column("bar", DbType.Boolean, true));
			Assert.AreEqual("bar bit default 1", map2.ColumnSql.ToLower());
		}
	}
}
