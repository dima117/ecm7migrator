using System.Configuration;
using ECM7.Migrator.Framework;
using NUnit.Framework;

namespace ECM7.Migrator.Tests
{
	[TestFixture]
	public class ProviderFactoryTest
	{
		private const string MYSQL_DIALECT = "ECM7.Migrator.Providers.MySql.MySqlDialect, ECM7.Migrator.Providers.MySql";
		private const string ORACLE_DIALECT = "ECM7.Migrator.Providers.Oracle.OracleDialect, ECM7.Migrator.Providers.Oracle";
		private const string SQLITE_DIALECT = "ECM7.Migrator.Providers.SQLite.SQLiteDialect, ECM7.Migrator.Providers.SQLite";

		private const string SQL_SERVER_DIALECT = "ECM7.Migrator.Providers.SqlServer.SqlServerDialect, ECM7.Migrator.Providers.SqlServer";
		private const string SQL_SERVER_2005_DIALECT = "ECM7.Migrator.Providers.SqlServer.SqlServer2005Dialect, ECM7.Migrator.Providers.SqlServer";
		private const string SQL_SERVER_CE_DIALECT = "ECM7.Migrator.Providers.SqlServer.SqlServerCeDialect, ECM7.Migrator.Providers.SqlServer";


		// todo: добавить тест на некорректные диалекты	
		// todo: разнести диалекты по отдельным проектам
		[Test, Category("SqlServer")]
		public void CanLoadSqlServerProvider()
		{
			ITransformationProvider provider = ProviderFactory.Create(
				SQL_SERVER_DIALECT, ConfigurationManager.AppSettings["SqlServerConnectionString"]);
			Assert.IsNotNull(provider);
		}


		[Test, Category("SqlServerCe")]
		public void CanLoadSqlServerCeProvider()
		{
			ITransformationProvider provider = ProviderFactory.Create(
				SQL_SERVER_CE_DIALECT, ConfigurationManager.AppSettings["SqlServerCeConnectionString"]);
			Assert.IsNotNull(provider);
		}


		[Test, Category("SqlServer2005")]
		public void CanLoadSqlServer2005Provider()
		{
			ITransformationProvider provider = ProviderFactory.Create(
				SQL_SERVER_2005_DIALECT, ConfigurationManager.AppSettings["SqlServer2005ConnectionString"]);
			Assert.IsNotNull(provider);
		}

		[Test, Category("MySql")]
		public void CanLoadMySqlProvider()
		{
			ITransformationProvider provider = ProviderFactory.Create(
				MYSQL_DIALECT, ConfigurationManager.AppSettings["MySqlConnectionString"]);
			Assert.IsNotNull(provider);
		}

		[Test, Category("SQLite")]
		public void CanLoadSqLiteProvider()
		{
			ITransformationProvider provider = ProviderFactory.Create(
				SQLITE_DIALECT, ConfigurationManager.AppSettings["SQLiteConnectionString"]);
			Assert.IsNotNull(provider);
		}

		[Test, Category("Oracle")]
		public void CanLoadOracleProvider()
		{
			ITransformationProvider provider = ProviderFactory.Create(
				ORACLE_DIALECT, ConfigurationManager.AppSettings["OracleConnectionString"]);
			Assert.IsNotNull(provider);
		}
	}
}
