using ECM7.Migrator.Providers;

namespace ECM7.Migrator.Tests2
{
	using System.Data;

	using NUnit.Framework;

	[TestFixture]
	public abstract class ColumnPropertyMappingTest<TProvider, TConnection>
		where TProvider : TransformationProvider<TConnection> where TConnection : IDbConnection
	{
		public abstract string CString { get; }

		#region Helpers

		private TProvider CreateProvider()
		{
			return ProviderFactory.Create<TProvider>(CString) as TProvider;
		}

		#endregion


		// todo: напистаь тесты на формирование SQL для колонок
		//#region Oracle

		//[Test]
		//public void OracleCreatesSql()
		//{
		//    using (var provider = CreateOracleProvider())
		//    {
		//        string columnSql = provider.GetSqlColumnDef(new Column("foo", DbType.AnsiString.WithSize(30)), false);
		//        Assert.AreEqual("\"foo\" varchar2(30)", columnSql.ToLower());
		//    }
		//}

		//[Test]
		//public void OracleCreatesNotNullSql()
		//{
		//    using (var provider = CreateOracleProvider())
		//    {
		//        string columnSql = provider.GetSqlColumnDef(new Column("foo", DbType.AnsiString.WithSize(30), ColumnProperty.NotNull), false);
		//        Assert.AreEqual("\"foo\" varchar2(30) not null", columnSql.ToLower());
		//    }
		//}

		//[Test]
		//public void OracleCreatesNotNullSqlWithDefault()
		//{
		//    using (var provider = CreateOracleProvider())
		//    {
		//        string columnSql = provider.GetSqlColumnDef(
		//                new Column("foo", DbType.AnsiString.WithSize(30), ColumnProperty.NotNull, "'test'"), false);
		//        Assert.AreEqual("\"foo\" varchar2(30) default 'test' not null", columnSql.ToLower());
		//    }
		//}

		//#endregion

		//[Test]
		//public void SqlServerCreatesSql()
		//{
		//    using (var provider = CreateSqlServerProvider())
		//    {
		//        string columnSql = provider.GetSqlColumnDef(
		//            new Column("foo", DbType.AnsiString.WithSize(30), 0), false);
		//        Assert.AreEqual("[foo] varchar(30)", columnSql.ToLower());
		//    }
		//}

		//[Test]
		//public void SqlServerCreatesNotNullSql()
		//{
		//    using (var provider = CreateSqlServerProvider())
		//    {
		//        string columnSql = provider.GetSqlColumnDef(
		//                new Column("foo", DbType.AnsiString.WithSize(30), ColumnProperty.NotNull), false);
		//        Assert.AreEqual("[foo] varchar(30) not null", columnSql.ToLower());
		//    }
		//}

		//[Test]
		//public void SqlServerCreatesSqWithDefault()
		//{
		//    using (var provider = CreateSqlServerProvider())
		//    {
		//        string columnSql = provider.GetSqlColumnDef(
		//            new Column("foo", DbType.AnsiString.WithSize(30), "'NEW'"), false);
		//        Assert.AreEqual("[foo] varchar(30) default 'new'", columnSql.ToLower());
		//    }
		//}

		//[Test]
		//public void SqlServerCreatesSqWithNullDefault()
		//{
		//    using (var provider = CreateSqlServerProvider())
		//    {
		//        string columnSql = provider.GetSqlColumnDef(
		//            new Column("foo", DbType.AnsiString.WithSize(30), "NULL"), false);
		//        Assert.AreEqual("[foo] varchar(30) default null", columnSql.ToLower());
		//    }
		//}

		//[Test]
		//public void SqlServerCreatesSqWithBooleanDefault()
		//{
		//    using (var provider = CreateSqlServerProvider())
		//    {
		//        string columnSql1 = provider.GetSqlColumnDef(new Column("foo", DbType.Boolean, false), false);
		//        Assert.AreEqual("[foo] bit default 0", columnSql1.ToLower());

		//        string columnSql2 = provider.GetSqlColumnDef(new Column("bar", DbType.Boolean, true), false);
		//        Assert.AreEqual("[bar] bit default 1", columnSql2.ToLower());
		//    }
		//}
	}
}