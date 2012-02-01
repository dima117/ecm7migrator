namespace ECM7.Migrator.Providers.Tests
{
	using System;
	using System.Configuration;
	using System.Data.SqlClient;

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
		#region Provider loading tests

		private static void CanLoadProviderInternalTest<TProvider>(string cstringName)
			where TProvider : ITransformationProvider
		{
			string cstring = ConfigurationManager.AppSettings[cstringName];

			using (ITransformationProvider provider = ProviderFactory.Create<TProvider>(cstring))
			{
				Assert.IsNotNull(provider);
				Assert.IsTrue(provider is TProvider);
			}
		}

		[Test, Category("SqlServer")]
		public void CanLoadSqlServerProvider()
		{
			CanLoadProviderInternalTest<SqlServerTransformationProvider>("SqlServerConnectionString");
		}

		[Test, Category("SqlServerCe")]
		public void CanLoadSqlServerCeProvider()
		{
			CanLoadProviderInternalTest<SqlServerCeTransformationProvider>("SqlServerCeConnectionString");
		}

		[Test, Category("MySql")]
		public void CanLoadMySqlProvider()
		{
			CanLoadProviderInternalTest<MySqlTransformationProvider>("MySqlConnectionString");
		}

		[Test, Category("PostgreSQL")]
		public void CanLoadPostgreSQLProvider()
		{
			CanLoadProviderInternalTest<PostgreSQLTransformationProvider>("NpgsqlConnectionString");
		}

		[Test, Category("SQLite")]
		public void CanLoadSqLiteProvider()
		{
			CanLoadProviderInternalTest<SQLiteTransformationProvider>("SQLiteConnectionString");
		}

		[Test, Category("Oracle")]
		public void CanLoadOracleProvider()
		{
			CanLoadProviderInternalTest<OracleTransformationProvider>("OracleConnectionString");
		}

		[Test, Category("Firebird")]
		public void CanLoadFirebirdProvider()
		{
			CanLoadProviderInternalTest<FirebirdTransformationProvider>("FirebirdConnectionString");
		}

		#endregion

		#region Shortcuts tests

		[Test, Category("SqlServer")]
		public void SqlServerShortcutTest()
		{
			Assert.AreEqual(
				ProviderFactory.GetProviderType("SqlServer"),
				typeof(SqlServerTransformationProvider));
		}

		[Test, Category("SqlServerCe")]
		public void SqlServerCeShortcutTest()
		{
			Assert.AreEqual(
				ProviderFactory.GetProviderType("SqlServerCe"),
				typeof(SqlServerCeTransformationProvider));
		}

		[Test, Category("Oracle")]
		public void OracleShortcutTest()
		{
			Assert.AreEqual(
				ProviderFactory.GetProviderType("Oracle"),
				typeof(OracleTransformationProvider));
		}

		[Test, Category("MySql")]
		public void MySqlShortcutTest()
		{
			Assert.AreEqual(
				ProviderFactory.GetProviderType("MySql"),
				typeof(MySqlTransformationProvider));
		}

		[Test, Category("SQLite")]
		public void SQLiteShortcutTest()
		{
			Assert.AreEqual(
				ProviderFactory.GetProviderType("SQLite"),
				typeof(SQLiteTransformationProvider));
		}

		[Test, Category("PostgreSQL")]
		public void PostgreSQLShortcutTest()
		{
			Assert.AreEqual(
				ProviderFactory.GetProviderType("PostgreSQL"),
				typeof(PostgreSQLTransformationProvider));
		}

		[Test, Category("Firebird")]
		public void FirebirdShortcutTest()
		{
			Assert.AreEqual(
				ProviderFactory.GetProviderType("Firebird"),
				typeof(FirebirdTransformationProvider));
		}

		#endregion

		#region TransformationProviderFactoryTest

		[Test]
		public void GetProviderTypeTest()
		{
			Assert.AreEqual(
				ProviderFactory.GetProviderType("ECM7.Migrator.Providers.PostgreSQL.PostgreSQLTransformationProvider, ECM7.Migrator.Providers.PostgreSQL"),
				typeof(PostgreSQLTransformationProvider));
		}

		[Test]
		public void GetInvalidProviderTypeTest()
		{
			Assert.Throws<RequirementNotCompliedException>(() =>
				ProviderFactory.GetProviderType(typeof(DateTime).FullName));
		}

		[Test]
		public void GetInvalidProviderTypeTest2()
		{
			Assert.Throws<RequirementNotCompliedException>(() =>
				ProviderFactory.GetProviderType("moo moo moo"));
		}

		[Test]
		public void CanGetConnectionType()
		{
			Assert.AreEqual(
				typeof(NpgsqlConnection),
				ProviderFactory.GetConnectionType(typeof(PostgreSQLTransformationProvider)));
		}

		[Test]
		public void CanCreateProvider()
		{
			using (var provider = ProviderFactory
				.Create(typeof(PostgreSQLTransformationProvider), new NpgsqlConnection()))
			{
				Assert.IsNotNull(provider);
				Assert.AreEqual(typeof(PostgreSQLTransformationProvider), provider.GetType());
			}
		}

		[Test]
		public void CantCreateProviderWithInvalidConnection()
		{
			Assert.Throws<System.MissingMethodException>(() =>
			ProviderFactory.Create(
				typeof(PostgreSQLTransformationProvider), new SqlConnection()));
		}

		[Test]
		public void CanCreateProviderUsingConnectionString()
		{
			string cstring = ConfigurationManager.AppSettings["NpgsqlConnectionString"];
			using (var provider = ProviderFactory.Create(typeof(PostgreSQLTransformationProvider), cstring))
			{
				// проверка типа провайдера
				Assert.IsNotNull(provider);
				Assert.AreEqual(typeof(PostgreSQLTransformationProvider), provider.GetType());

				// проверка типа подключения
				Assert.IsNotNull(provider.Connection);
				Assert.AreEqual(typeof(NpgsqlConnection), provider.Connection.GetType());

				// проверка строки подключнеия у созданного провайдера
				NpgsqlConnectionStringBuilder sb1 = new NpgsqlConnectionStringBuilder(cstring);
				NpgsqlConnectionStringBuilder sb2 = new NpgsqlConnectionStringBuilder(provider.Connection.ConnectionString);

				Assert.AreEqual(sb1.Host, sb2.Host);
				Assert.AreEqual(sb1.Database, sb2.Database);
				Assert.AreEqual(sb1.UserName, sb2.UserName);
			}
		}

		#endregion
	}
}