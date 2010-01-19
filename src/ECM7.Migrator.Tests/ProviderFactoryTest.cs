using System.Configuration;
using ECM7.Common.Utils.Exceptions;
using ECM7.Migrator.Framework;
using ECM7.Migrator.Providers;
using NUnit.Framework;

namespace ECM7.Migrator.Tests
{
	[TestFixture]
	public class ProviderFactoryTest
	{
		#region const

		private const string MYSQL_DIALECT = "ECM7.Migrator.Providers.MySql.MySqlDialect, ECM7.Migrator.Providers.MySql";
		private const string ORACLE_DIALECT = "ECM7.Migrator.Providers.Oracle.OracleDialect, ECM7.Migrator.Providers.Oracle";
		private const string SQLITE_DIALECT = "ECM7.Migrator.Providers.SQLite.SQLiteDialect, ECM7.Migrator.Providers.SQLite";
		private const string POSTGRE_SQL_DIALECT = "ECM7.Migrator.Providers.PostgreSQL.PostgreSQLDialect, ECM7.Migrator.Providers.PostgreSQL";

		private const string SQL_SERVER_DIALECT = "ECM7.Migrator.Providers.SqlServer.SqlServerDialect, ECM7.Migrator.Providers.SqlServer";
		private const string SQL_SERVER_2005_DIALECT = "ECM7.Migrator.Providers.SqlServer.SqlServer2005Dialect, ECM7.Migrator.Providers.SqlServer";
		private const string SQL_SERVER_CE_DIALECT = "ECM7.Migrator.Providers.SqlServer.SqlServerCeDialect, ECM7.Migrator.Providers.SqlServer";

		#endregion

		#region Provider loading tests

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

		[Test, Category("PostgreSQL")]
		public void CanLoadPostgreSQLProvider()
		{
			ITransformationProvider provider = ProviderFactory.Create(
				POSTGRE_SQL_DIALECT, ConfigurationManager.AppSettings["NpgsqlConnectionString"]);
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

		[Test, Category("Некорректные диалекты"), ExpectedException(typeof(RequirementNotCompliedException))]
		public void CantLoadNotExistsDialect()
		{
			ProviderFactory.Create("NotExistsDialect", string.Empty);
		}

		[Test, Category("Некорректные диалекты"), ExpectedException(typeof(RequirementNotCompliedException))]
		public void CantLoadNotDialectClass()
		{
			ProviderFactory.Create("System.Int32", string.Empty);
		}

		#endregion

		#region Shortcuts tests

		[Test]
		public void SqlServerShortcutTest()
		{
			TransformationProvider tp = ProviderFactory.Create(
				"SqlServer", ConfigurationManager.AppSettings["SqlServerConnectionString"]);
			Assert.That(tp is ECM7.Migrator.Providers.SqlServer.SqlServerTransformationProvider);
			Assert.That(tp.Dialect is ECM7.Migrator.Providers.SqlServer.SqlServerDialect);
		}

		[Test]
		public void SqlServer2005ShortcutTest()
		{
			TransformationProvider tp = ProviderFactory.Create(
				"SqlServer2005", ConfigurationManager.AppSettings["SqlServerConnectionString"]);
			Assert.That(tp is ECM7.Migrator.Providers.SqlServer.SqlServerTransformationProvider);
			Assert.That(tp.Dialect is ECM7.Migrator.Providers.SqlServer.SqlServer2005Dialect);
		}

		[Test]
		public void SqlServerCeShortcutTest()
		{
			TransformationProvider tp = ProviderFactory.Create(
				"SqlServerCe", ConfigurationManager.AppSettings["SqlServerCeConnectionString"]);
			Assert.That(tp is ECM7.Migrator.Providers.SqlServer.SqlServerCeTransformationProvider);
			Assert.That(tp.Dialect is ECM7.Migrator.Providers.SqlServer.SqlServerCeDialect);
		}

		[Test]
		public void OracleShortcutTest()
		{
			TransformationProvider tp = ProviderFactory.Create(
				"Oracle", ConfigurationManager.AppSettings["OracleConnectionString"]);
			Assert.That(tp is ECM7.Migrator.Providers.Oracle.OracleTransformationProvider);
			Assert.That(tp.Dialect is ECM7.Migrator.Providers.Oracle.OracleDialect);
		}

		[Test]
		public void MySqlShortcutTest()
		{
			TransformationProvider tp = ProviderFactory.Create(
				"MySql", ConfigurationManager.AppSettings["MySqlConnectionString"]);
			Assert.That(tp is ECM7.Migrator.Providers.MySql.MySqlTransformationProvider);
			Assert.That(tp.Dialect is ECM7.Migrator.Providers.MySql.MySqlDialect);
		}

		[Test]
		public void SQLiteShortcutTest()
		{
			TransformationProvider tp = ProviderFactory.Create(
				"SQLite", ConfigurationManager.AppSettings["SQLiteConnectionString"]);
			Assert.That(tp is ECM7.Migrator.Providers.SQLite.SQLiteTransformationProvider);
			Assert.That(tp.Dialect is ECM7.Migrator.Providers.SQLite.SQLiteDialect);
		}

		[Test]
		public void PostgreSQLShortcutTest()
		{
			TransformationProvider tp = ProviderFactory.Create(
				"PostgreSQL", ConfigurationManager.AppSettings["NpgsqlConnectionString"]);
			Assert.That(tp is ECM7.Migrator.Providers.PostgreSQL.PostgreSQLTransformationProvider);
			Assert.That(tp.Dialect is ECM7.Migrator.Providers.PostgreSQL.PostgreSQLDialect);
		}

		#endregion
	}
}
