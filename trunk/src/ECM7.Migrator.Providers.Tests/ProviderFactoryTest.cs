namespace ECM7.Migrator.Providers.Tests
{
	using System.Configuration;

	using ECM7.Common.Utils.Exceptions;
	using ECM7.Migrator.Providers;
	using ECM7.Migrator.Providers.Firebird;
	using ECM7.Migrator.Providers.MySql;
	using ECM7.Migrator.Providers.Oracle;
	using ECM7.Migrator.Providers.PostgreSQL;
	using ECM7.Migrator.Providers.SQLite;
	using ECM7.Migrator.Providers.SqlServer;

	using Npgsql;

	using NUnit.Framework;

	using ECM7.Migrator.Framework;

	[TestFixture]
	public class ProviderFactoryTest
	{
		public static class TestProviders
		{
			public static ITransformationProvider SqlServer
			{
				get
				{
					return ProviderFactoryBuilder
						.CreateProviderFactory<SqlServerTransformationProviderFactory>()
						.CreateProvider(ConfigurationManager.AppSettings["SqlServerConnectionString"]);
				}
			}

			public static ITransformationProvider SqlServerCe
			{
				get
				{
					return ProviderFactoryBuilder
						.CreateProviderFactory<SqlServerCeTransformationProviderFactory>()
						.CreateProvider(ConfigurationManager.AppSettings["SqlServerCeConnectionString"]);
				}
			}

			public static ITransformationProvider MySql
			{
				get
				{
					return ProviderFactoryBuilder
						.CreateProviderFactory<MySqlTransformationProviderFactory>()
						.CreateProvider(ConfigurationManager.AppSettings["MySqlConnectionString"]);
				}
			}

			public static ITransformationProvider PostgreSQL
			{
				get
				{
					return ProviderFactoryBuilder
						.CreateProviderFactory<PostgreSQLTransformationProviderFactory>()
						.CreateProvider(ConfigurationManager.AppSettings["NpgsqlConnectionString"]);
				}
			}

			public static ITransformationProvider SQLite
			{
				get
				{
					return ProviderFactoryBuilder
						.CreateProviderFactory<SQLiteTransformationProviderFactory>()
						.CreateProvider(ConfigurationManager.AppSettings["SQLiteConnectionString"]);
				}
			}

			public static ITransformationProvider Oracle
			{
				get
				{
					return ProviderFactoryBuilder
						.CreateProviderFactory<OracleTransformationProviderFactory>()
						.CreateProvider(ConfigurationManager.AppSettings["OracleConnectionString"]);
				}
			}

			public static ITransformationProvider Firebird
			{
				get
				{
					return ProviderFactoryBuilder
						.CreateProviderFactory<FirebirdTransformationProviderFactory>()
						.CreateProvider(ConfigurationManager.AppSettings["FirebirdConnectionString"]);
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
			ProviderFactoryBuilder
				.CreateProviderFactory("NotExistsFactory")
				.CreateProvider(ConfigurationManager.AppSettings["OracleConnectionString"]);
		}

		[Test, Category("Некорректные диалекты"), ExpectedException(typeof(RequirementNotCompliedException))]
		public void CantLoadNotDialectClass()
		{
			ProviderFactoryBuilder
				.CreateProviderFactory("System.Int32")
				.CreateProvider(ConfigurationManager.AppSettings["OracleConnectionString"]);
		}

		#endregion

		#region Shortcuts tests

		[Test]
		public void SqlServerShortcutTest()
		{
			ITransformationProvider tp = ProviderFactoryBuilder
				.CreateProviderFactory("SqlServer")
				.CreateProvider(ConfigurationManager.AppSettings["SqlServerConnectionString"]);

			Assert.That(tp is ECM7.Migrator.Providers.SqlServer.SqlServerTransformationProvider);
		}

		[Test]
		public void SqlServerCeShortcutTest()
		{
			ITransformationProvider tp = ProviderFactoryBuilder
				.CreateProviderFactory("SqlServerCe")
				.CreateProvider(ConfigurationManager.AppSettings["SqlServerCeConnectionString"]);

			Assert.That(tp is ECM7.Migrator.Providers.SqlServer.SqlServerCeTransformationProvider);
		}

		[Test]
		public void OracleShortcutTest()
		{
			ITransformationProvider tp = ProviderFactoryBuilder
				.CreateProviderFactory("Oracle")
				.CreateProvider(ConfigurationManager.AppSettings["OracleConnectionString"]);

			Assert.That(tp is ECM7.Migrator.Providers.Oracle.OracleTransformationProvider);
		}

		[Test]
		public void MySqlShortcutTest()
		{
			ITransformationProvider tp = ProviderFactoryBuilder
				.CreateProviderFactory("MySql")
				.CreateProvider(ConfigurationManager.AppSettings["MySqlConnectionString"]);

			Assert.That(tp is ECM7.Migrator.Providers.MySql.MySqlTransformationProvider);
		}

		[Test]
		public void SQLiteShortcutTest()
		{
			ITransformationProvider tp = ProviderFactoryBuilder
				.CreateProviderFactory("SQLite")
				.CreateProvider(ConfigurationManager.AppSettings["SQLiteConnectionString"]);

			Assert.That(tp is ECM7.Migrator.Providers.SQLite.SQLiteTransformationProvider);
		}

		[Test]
		public void PostgreSQLShortcutTest()
		{
			ITransformationProvider tp = ProviderFactoryBuilder
				.CreateProviderFactory("PostgreSQL")
				.CreateProvider(ConfigurationManager.AppSettings["NpgsqlConnectionString"]);

			Assert.That(tp is ECM7.Migrator.Providers.PostgreSQL.PostgreSQLTransformationProvider);
		}

		[Test]
		public void FirebirdShortcutTest()
		{
			ITransformationProvider tp = ProviderFactoryBuilder
				.CreateProviderFactory("Firebird")
				.CreateProvider(ConfigurationManager.AppSettings["FirebirdConnectionString"]);

			Assert.That(tp is ECM7.Migrator.Providers.Firebird.FirebirdTransformationProvider);
		}

		#endregion

		#region TransformationProviderFactoryTest

		[Test]
		public void CanGetConnectionType()
		{
			Assert.AreEqual(
				typeof(NpgsqlConnection),
				TransformationProviderFactory.GetConnectionType(typeof(PostgreSQLTransformationProvider)));
		}

		#endregion
	}
}