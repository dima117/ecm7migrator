using System.Data;
using ECM7.Migrator.Framework;
using ECM7.Migrator.Providers;
using ECM7.Migrator.Providers.Oracle;
using ECM7.Migrator.Providers.SqlServer;
using NUnit.Framework;

namespace ECM7.Migrator.Tests
{
    [TestFixture]
    public class ColumnPropertyMapperTest
    {

        [Test]
        public void OracleCreatesSql()
        {
            ColumnPropertiesMapper mapper = new ColumnPropertiesMapper(new OracleDialect());
			ColumnSqlMap map = mapper.MapColumnProperties(new Column("foo", DbType.AnsiString.WithSize(30)));
            Assert.AreEqual("foo varchar2(30)", map.ColumnSql.ToLower());
        }

        [Test]
        public void OracleCreatesNotNullSql()
        {
            ColumnPropertiesMapper mapper = new ColumnPropertiesMapper(new OracleDialect());
			ColumnSqlMap map = mapper.MapColumnProperties(new Column("foo", DbType.AnsiString.WithSize(30), ColumnProperty.NotNull));
            Assert.AreEqual("foo varchar2(30) not null", map.ColumnSql.ToLower());
        }

        [Test]
        public void OracleIndexSqlIsNullWhenIndexedFalse()
        {
            ColumnPropertiesMapper mapper = new ColumnPropertiesMapper(new OracleDialect());
			ColumnSqlMap map = mapper.MapColumnProperties(new Column("foo", DbType.AnsiStringFixedLength.WithSize(1)));
            Assert.IsNull(map.IndexSql);
        }

        [Test]
        public void OracleIndexSqlIsNoNullWhenIndexed()
        {
            ColumnPropertiesMapper mapper = new ColumnPropertiesMapper(new OracleDialect());
			ColumnSqlMap map = mapper.MapColumnProperties(new Column("foo", DbType.AnsiStringFixedLength.WithSize(1), ColumnProperty.Indexed));
            Assert.IsNotNull(map.IndexSql);
        }

        [Test]
        public void SqlServerIndexSqlIsNoNullWhenIndexed()
        {
            ColumnPropertiesMapper mapper = new ColumnPropertiesMapper(new SqlServerDialect());
			ColumnSqlMap map = mapper.MapColumnProperties(new Column("foo", DbType.AnsiStringFixedLength.WithSize(1), ColumnProperty.Indexed));
            Assert.IsNull(map.IndexSql);
        }

        [Test]
        public void SqlServerCreatesSql()
        {
            ColumnPropertiesMapper mapper = new ColumnPropertiesMapper(new SqlServerDialect());
			ColumnSqlMap map = mapper.MapColumnProperties(new Column("foo", DbType.AnsiString.WithSize(30), 0));
            Assert.AreEqual("foo varchar(30)", map.ColumnSql.ToLower());
        }

        [Test]
        public void SqlServerCreatesNotNullSql()
        {
            ColumnPropertiesMapper mapper = new ColumnPropertiesMapper(new SqlServerDialect());
			ColumnSqlMap map = mapper.MapColumnProperties(new Column("foo", DbType.AnsiString.WithSize(30), ColumnProperty.NotNull));
            Assert.AreEqual("foo varchar(30) not null", map.ColumnSql.ToLower());
        }

        [Test]
        public void SqlServerCreatesSqWithDefault()
        {
            ColumnPropertiesMapper mapper = new ColumnPropertiesMapper(new SqlServerDialect());
			ColumnSqlMap map = mapper.MapColumnProperties(new Column("foo", DbType.AnsiString.WithSize(30), "'NEW'"));
            Assert.AreEqual("foo varchar(30) default 'new'", map.ColumnSql.ToLower());
        }

        [Test]
        public void SqlServerCreatesSqWithNullDefault()
        {
            ColumnPropertiesMapper mapper = new ColumnPropertiesMapper(new SqlServerDialect());
			ColumnSqlMap map = mapper.MapColumnProperties(new Column("foo", DbType.AnsiString.WithSize(30), "NULL"));
            Assert.AreEqual("foo varchar(30) default null", map.ColumnSql.ToLower());
        }

        [Test]
        public void SqlServerCreatesSqWithBooleanDefault()
        {
            ColumnPropertiesMapper mapper = new ColumnPropertiesMapper(new SqlServerDialect());
			ColumnSqlMap map = mapper.MapColumnProperties(new Column("foo", DbType.Boolean, false));
            Assert.AreEqual("foo bit default 0", map.ColumnSql.ToLower());

			ColumnSqlMap map2 = mapper.MapColumnProperties(new Column("bar", DbType.Boolean, true));
			Assert.AreEqual("bar bit default 1", map2.ColumnSql.ToLower());
        }
    }
}
