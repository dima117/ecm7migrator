namespace ECM7.Migrator.Tests2
{
	using System.Configuration;

	using ECM7.Common.Utils.Exceptions;
	using ECM7.Migrator.Providers;

	using NUnit.Framework;

	using ECM7.Migrator.Framework;

	[TestFixture]
	public class ProviderFactoryTest
	{
		public static class TestProviders
		{
			#region const

			private const string MYSQL_DIALECT = "ECM7.Migrator.Providers.MySql.MySqlDialect, ECM7.Migrator.Providers.MySql";
			private const string ORACLE_DIALECT = "ECM7.Migrator.Providers.Oracle.OracleDialect, ECM7.Migrator.Providers.Oracle";
			private const string SQLITE_DIALECT = "ECM7.Migrator.Providers.SQLite.SQLiteDialect, ECM7.Migrator.Providers.SQLite";
			private const string POSTGRE_SQL_DIALECT = "ECM7.Migrator.Providers.PostgreSQL.PostgreSQLDialect, ECM7.Migrator.Providers.PostgreSQL";
			private const string FIREBIRD_DIALECT = "ECM7.Migrator.Providers.Firebird.FirebirdDialect, ECM7.Migrator.Providers.Firebird";

			private const string SQL_SERVER_DIALECT = "ECM7.Migrator.Providers.SqlServer.SqlServerDialect, ECM7.Migrator.Providers.SqlServer";
			private const string SQL_SERVER_CE_DIALECT = "ECM7.Migrator.Providers.SqlServer.SqlServerCeDialect, ECM7.Migrator.Providers.SqlServer";

			#endregion

			public static ITransformationProvider SqlServer
			{
				get
				{
					return ProviderFactory.Create(
						SQL_SERVER_DIALECT, ConfigurationManager.AppSettings["SqlServerConnectionString"]);
				}
			}

			public static ITransformationProvider SqlServerCe
			{
				get
				{
					return ProviderFactory.Create(
						SQL_SERVER_CE_DIALECT, ConfigurationManager.AppSettings["SqlServerCeConnectionString"]);
				}
			}

			public static ITransformationProvider MySql
			{
				get
				{
					return ProviderFactory.Create(
						MYSQL_DIALECT, ConfigurationManager.AppSettings["MySqlConnectionString"]);
				}
			}

			public static ITransformationProvider PostgreSQL
			{
				get
				{
					return ProviderFactory.Create(
						POSTGRE_SQL_DIALECT, ConfigurationManager.AppSettings["NpgsqlConnectionString"]);
				}
			}

			public static ITransformationProvider SQLite
			{
				get
				{
					return ProviderFactory.Create(
						SQLITE_DIALECT, ConfigurationManager.AppSettings["SQLiteConnectionString"]);
				}
			}

			public static ITransformationProvider Oracle
			{
				get
				{
					return ProviderFactory.Create(
						ORACLE_DIALECT, ConfigurationManager.AppSettings["OracleConnectionString"]);
				}
			}

			public static ITransformationProvider Firebird
			{
				get
				{
					return ProviderFactory.Create(
						FIREBIRD_DIALECT, ConfigurationManager.AppSettings["FirebirdConnectionString"]);
				}
			}
		}

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

        [Test, Category("Firebird")]
        public void CanLoadFirebirdProvider()
        {
            Assert.IsNotNull(TestProviders.Firebird);
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