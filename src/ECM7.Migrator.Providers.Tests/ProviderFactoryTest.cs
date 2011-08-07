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

		[Test, Category("SqlServer")]
		public void CanLoadSqlServerProvider()
		{
			ITransformationProvider provider = TransformationProviderFactory
				.Create<SqlServerTransformationProvider>(
					ConfigurationManager.AppSettings["SqlServerConnectionString"]);

			Assert.IsNotNull(provider);
			Assert.IsTrue(provider is SqlServerTransformationProvider);
		}

		[Test, Category("SqlServerCe")]
		public void CanLoadSqlServerCeProvider()
		{
			ITransformationProvider provider = TransformationProviderFactory
				.Create<SqlServerCeTransformationProvider>(
					ConfigurationManager.AppSettings["SqlServerCeConnectionString"]);

			Assert.IsNotNull(provider);
			Assert.IsTrue(provider is SqlServerCeTransformationProvider);
		}

		[Test, Category("MySql")]
		public void CanLoadMySqlProvider()
		{
			ITransformationProvider provider = TransformationProviderFactory
				.Create<MySqlTransformationProvider>(
					ConfigurationManager.AppSettings["MySqlConnectionString"]);

			Assert.IsNotNull(provider);
			Assert.IsTrue(provider is MySqlTransformationProvider);
		}

		[Test, Category("PostgreSQL")]
		public void CanLoadPostgreSQLProvider()
		{
			ITransformationProvider provider = TransformationProviderFactory
				.Create<PostgreSQLTransformationProvider>(
					ConfigurationManager.AppSettings["NpgsqlConnectionString"]);

			Assert.IsNotNull(provider);
			Assert.IsTrue(provider is PostgreSQLTransformationProvider);
		}

		[Test, Category("SQLite")]
		public void CanLoadSqLiteProvider()
		{
			ITransformationProvider provider = TransformationProviderFactory
				.Create<SQLiteTransformationProvider>(
					ConfigurationManager.AppSettings["SQLiteConnectionString"]);

			Assert.IsNotNull(provider);
			Assert.IsTrue(provider is SQLiteTransformationProvider);
		}

		[Test, Category("Oracle")]
		public void CanLoadOracleProvider()
		{
			ITransformationProvider provider = TransformationProviderFactory
				.Create<OracleTransformationProvider>(
					ConfigurationManager.AppSettings["OracleConnectionString"]);

			Assert.IsNotNull(provider);
			Assert.IsTrue(provider is OracleTransformationProvider);
		}

		[Test, Category("Firebird")]
		public void CanLoadFirebirdProvider()
		{
			ITransformationProvider provider = TransformationProviderFactory
				.Create<FirebirdTransformationProvider>(
					ConfigurationManager.AppSettings["FirebirdConnectionString"]);

			Assert.IsNotNull(provider);
			Assert.IsTrue(provider is FirebirdTransformationProvider);
		}

		#endregion

		#region Shortcuts tests

		[Test]
		public void SqlServerShortcutTest()
		{
			Assert.AreEqual(
				TransformationProviderFactory.GetProviderType("SqlServer"),
				typeof(SqlServerTransformationProvider));
		}

		[Test]
		public void SqlServerCeShortcutTest()
		{
			Assert.AreEqual(
				TransformationProviderFactory.GetProviderType("SqlServerCe"),
				typeof(SqlServerCeTransformationProvider));
		}

		[Test]
		public void OracleShortcutTest()
		{
			Assert.AreEqual(
				TransformationProviderFactory.GetProviderType("Oracle"),
				typeof(OracleTransformationProvider));
		}

		[Test]
		public void MySqlShortcutTest()
		{
			Assert.AreEqual(
				TransformationProviderFactory.GetProviderType("MySql"),
				typeof(MySqlTransformationProvider));
		}

		[Test]
		public void SQLiteShortcutTest()
		{
			Assert.AreEqual(
				TransformationProviderFactory.GetProviderType("SQLite"),
				typeof(SQLiteTransformationProvider));
		}

		[Test]
		public void PostgreSQLShortcutTest()
		{
			Assert.AreEqual(
				TransformationProviderFactory.GetProviderType("PostgreSQL"),
				typeof(PostgreSQLTransformationProvider));
		}

		[Test]
		public void FirebirdShortcutTest()
		{
			Assert.AreEqual(
				TransformationProviderFactory.GetProviderType("Firebird"),
				typeof(FirebirdTransformationProvider));
		}

		#endregion

		#region TransformationProviderFactoryTest

		[Test]
		public void GetProviderTypeTest()
		{
			Assert.AreEqual(
				TransformationProviderFactory.GetProviderType("ECM7.Migrator.Providers.PostgreSQL.PostgreSQLTransformationProvider, ECM7.Migrator.Providers.PostgreSQL"),
				typeof(PostgreSQLTransformationProvider));
		}

		[Test]
		public void GetInvalidProviderTypeTest()
		{
			Assert.Throws<RequirementNotCompliedException>(() =>
				TransformationProviderFactory.GetProviderType(typeof(DateTime).FullName));
		}

		[Test]
		public void GetInvalidProviderTypeTest2()
		{
			Assert.Throws<RequirementNotCompliedException>(() =>
				TransformationProviderFactory.GetProviderType("moo moo moo"));
		}

		[Test]
		public void CanGetConnectionType()
		{
			Assert.AreEqual(
				typeof(NpgsqlConnection),
				TransformationProviderFactory.GetConnectionType(typeof(PostgreSQLTransformationProvider)));
		}

		[Test]
		public void CanCreateProvider()
		{
			var provider = TransformationProviderFactory.Create(
				typeof(PostgreSQLTransformationProvider), new NpgsqlConnection());

			Assert.IsNotNull(provider);
			Assert.AreEqual(typeof(PostgreSQLTransformationProvider), provider.GetType());
		}

		[Test]
		public void CantCreateProviderWithInvalidConnection()
		{
			Assert.Throws<System.MissingMethodException>(() =>
			TransformationProviderFactory.Create(
				typeof(PostgreSQLTransformationProvider), new SqlConnection()));
		}

		[Test]
		public void CanCreateProviderUsingConnectionString()
		{
			string cstring = ConfigurationManager.AppSettings["NpgsqlConnectionString"];
			ITransformationProvider provider = TransformationProviderFactory.Create(
				typeof(PostgreSQLTransformationProvider), cstring);

			// проверка типа провайдера
			Assert.IsNotNull(provider);
			Assert.AreEqual(typeof(PostgreSQLTransformationProvider), provider.GetType());

			// проверка типа подключения
			Assert.IsNotNull(provider.Connection);
			Assert.AreEqual(typeof(NpgsqlConnection), provider.Connection .GetType());

			// проверка строки подключнеия у созданного провайдера
			NpgsqlConnectionStringBuilder sb1 = new NpgsqlConnectionStringBuilder(cstring);
			NpgsqlConnectionStringBuilder sb2 = new NpgsqlConnectionStringBuilder(provider.Connection.ConnectionString);

			Assert.AreEqual(sb1.Host, sb2.Host);
			Assert.AreEqual(sb1.Database, sb2.Database);
			Assert.AreEqual(sb1.UserName, sb2.UserName);
		}
		
		#endregion
	}
}