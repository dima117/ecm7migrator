using System.Configuration;
using ECM7.Common.Utils.Exceptions;
using ECM7.Migrator.Providers;
using ECM7.Migrator.Tests.Helpers;
using NUnit.Framework;

namespace ECM7.Migrator.Tests.TestClasses.Common
{
	[TestFixture]
	public class ProviderFactoryTest
	{
		#region Provider loading tests

		[Test, Category("SqlServer")]
		public void CanLoadSqlServerProvider()
		{
			Assert.IsNotNull(TestProviders.SqlServer);
		}

		[Test, Category("SqlServerCe")]
		public void CanLoadSqlServerCeProvider()
		{
			Assert.IsNotNull(TestProviders.SqlServerCe);
		}

		[Test, Category("SqlServer2005")]
		public void CanLoadSqlServer2005Provider()
		{
			Assert.IsNotNull(TestProviders.SqlServer2005);
		}

		[Test, Category("MySql")]
		public void CanLoadMySqlProvider()
		{
			Assert.IsNotNull(TestProviders.MySql);
		}

		[Test, Category("PostgreSQL")]
		public void CanLoadPostgreSQLProvider()
		{
			Assert.IsNotNull(TestProviders.PostgreSQL);
		}

		[Test, Category("SQLite")]
		public void CanLoadSqLiteProvider()
		{
			Assert.IsNotNull(TestProviders.SQLite);
		}

		[Test, Category("Oracle")]
		public void CanLoadOracleProvider()
		{
			Assert.IsNotNull(TestProviders.Oracle);
		}

		[Test, Category("Некорректные диалекты"), ExpectedException(typeof(RequirementNotCompliedException))]
		public void CantLoadNotExistsDialect()
		{
			ProviderFactory.Create("NotExistsDialect", string.Empty, null);
		}

		[Test, Category("Некорректные диалекты"), ExpectedException(typeof(RequirementNotCompliedException))]
		public void CantLoadNotDialectClass()
		{
			ProviderFactory.Create("System.Int32", string.Empty, null);
		}

		#endregion

		#region Shortcuts tests

		[Test]
		public void SqlServerShortcutTest()
		{
			TransformationProvider tp = ProviderFactory.Create(
				"SqlServer", ConfigurationManager.AppSettings["SqlServerConnectionString"], null);
			Assert.That(tp is ECM7.Migrator.Providers.SqlServer.SqlServerTransformationProvider);
			Assert.That(tp.Dialect is ECM7.Migrator.Providers.SqlServer.SqlServerDialect);
		}

		[Test]
		public void SqlServer2005ShortcutTest()
		{
			TransformationProvider tp = ProviderFactory.Create(
				"SqlServer2005", ConfigurationManager.AppSettings["SqlServerConnectionString"], null);
			Assert.That(tp is ECM7.Migrator.Providers.SqlServer.SqlServerTransformationProvider);
			Assert.That(tp.Dialect is ECM7.Migrator.Providers.SqlServer.SqlServer2005Dialect);
		}

		[Test]
		public void SqlServerCeShortcutTest()
		{
			TransformationProvider tp = ProviderFactory.Create(
				"SqlServerCe", ConfigurationManager.AppSettings["SqlServerCeConnectionString"], null);
			Assert.That(tp is ECM7.Migrator.Providers.SqlServer.SqlServerCeTransformationProvider);
			Assert.That(tp.Dialect is ECM7.Migrator.Providers.SqlServer.SqlServerCeDialect);
		}

		[Test]
		public void OracleShortcutTest()
		{
			TransformationProvider tp = ProviderFactory.Create(
				"Oracle", ConfigurationManager.AppSettings["OracleConnectionString"], null);
			Assert.That(tp is ECM7.Migrator.Providers.Oracle.OracleTransformationProvider);
			Assert.That(tp.Dialect is ECM7.Migrator.Providers.Oracle.OracleDialect);
		}

		[Test]
		public void MySqlShortcutTest()
		{
			TransformationProvider tp = ProviderFactory.Create(
				"MySql", ConfigurationManager.AppSettings["MySqlConnectionString"], null);
			Assert.That(tp is ECM7.Migrator.Providers.MySql.MySqlTransformationProvider);
			Assert.That(tp.Dialect is ECM7.Migrator.Providers.MySql.MySqlDialect);
		}

		[Test]
		public void SQLiteShortcutTest()
		{
			TransformationProvider tp = ProviderFactory.Create(
				"SQLite", ConfigurationManager.AppSettings["SQLiteConnectionString"], null);
			Assert.That(tp is ECM7.Migrator.Providers.SQLite.SQLiteTransformationProvider);
			Assert.That(tp.Dialect is ECM7.Migrator.Providers.SQLite.SQLiteDialect);
		}

		[Test]
		public void PostgreSQLShortcutTest()
		{
			TransformationProvider tp = ProviderFactory.Create(
				"PostgreSQL", ConfigurationManager.AppSettings["NpgsqlConnectionString"], null);
			Assert.That(tp is ECM7.Migrator.Providers.PostgreSQL.PostgreSQLTransformationProvider);
			Assert.That(tp.Dialect is ECM7.Migrator.Providers.PostgreSQL.PostgreSQLDialect);
		}

		#endregion
	}
}