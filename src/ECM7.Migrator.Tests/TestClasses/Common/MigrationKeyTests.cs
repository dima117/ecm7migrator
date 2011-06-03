using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ECM7.Migrator.Loader;
using NUnit.Mocks;
using ECM7.Migrator.Framework;
using ECM7.Migrator.Framework.Loggers;
using ECM7.Migrator.TestAssembly;
using ECM7.Migrator.Providers.SqlServer;
using System.Configuration;
using System.Data;

namespace ECM7.Migrator.Tests.TestClasses.Common
{
	[TestFixture, Category("SqlServer2005")]
	public class MigrationKeyTests
	{
		[Test]
		public void LoadKeyMigrations()
		{
			MigrationLoader loader = new MigrationLoader(
				"key",
				null,
				this.GetType().Assembly,
				typeof(ECM7.Migrator.TestAssembly.FirstTestMigration).Assembly);


			var migrations = loader.MigrationsTypes.Select(x => x.Version).ToList();
			Assert.AreEqual(2, migrations.Count);
			Assert.AreEqual(1, migrations[0]);
			Assert.AreEqual(2, migrations[1]);
		}

		[Test]
		public void AppliedMigrationsWithKey()
		{
			var provider = CreateSqlServer2005Provider("key");
			if (provider.TableExists("SchemaInfo"))
			{
				provider.RemoveTable("SchemaInfo");
			}

			// Check that a "get" call works on the first run.
			Assert.AreEqual(0, provider.GetAppliedMigrations("key").Count);
			Assert.IsTrue(provider.TableExists("SchemaInfo"), "No SchemaInfo table created");
			Assert.IsTrue(provider.ColumnExists("SchemaInfo", "Key"), "No Key column in SchemaInfo table created");

			// Check that a "set" called after the first run works.
			provider.MigrationApplied(1, "key");
			Assert.AreEqual(1, provider.GetAppliedMigrations("key")[0]);
			using (IDataReader reader = provider.ExecuteQuery("SELECT [Key] FROM SchemaInfo"))
			{
				reader.Read();
				Assert.AreEqual("key", ((string)reader[0]));
				Assert.IsFalse(reader.Read());
			}

			provider.RemoveTable("SchemaInfo");

			// Check that a "set" call works on the first run.
			provider.MigrationApplied(1, "key");
			Assert.AreEqual(1, provider.GetAppliedMigrations("key")[0]);
			Assert.IsTrue(provider.TableExists("SchemaInfo"), "No SchemaInfo table created");

			provider.RemoveTable("SchemaInfo");
		}

		[Test]
		public void AppliedMigrationsWithTwoKeys()
		{
			var provider = CreateSqlServer2005Provider(string.Empty);

			Migrator migrator = new Migrator(
				provider,
				string.Empty,
				null,
				this.GetType().Assembly,
				typeof(ECM7.Migrator.TestAssembly.FirstTestMigration).Assembly);

			Migrator migratorKey = new Migrator(
				"SqlServer2005",
				ConnectionString,
				"key",
				null,
				this.GetType().Assembly,
				typeof(ECM7.Migrator.TestAssembly.FirstTestMigration).Assembly);

			Assert.AreEqual(0, migrator.GetAppliedMigrations().Count);
			Assert.AreEqual(0, migratorKey.GetAppliedMigrations().Count);

			migrator.Migrate(2);
			migratorKey.Migrate(2);

			Assert.AreEqual(2, migrator.GetAppliedMigrations().Count);
			Assert.AreEqual(2, migratorKey.GetAppliedMigrations().Count);

			migrator.Migrate(0);
			migratorKey.Migrate(0);

			Assert.AreEqual(0, migrator.GetAppliedMigrations().Count);
			Assert.AreEqual(0, migratorKey.GetAppliedMigrations().Count);
		}

		[Test]
		public void UpdateOldStyleSchemaInfo()
		{
			var provider = CreateSqlServer2005Provider("some key");
			if (provider.TableExists("SchemaInfo"))
			{
				provider.RemoveTable("SchemaInfo");
			}

			provider.AddTable(
				"SchemaInfo",
				new Column("Version", DbType.Int64, ColumnProperty.PrimaryKey));
			provider.Insert("SchemaInfo", new[] { "Version" }, new[] { "1" });

			Migrator migrator = new Migrator(provider, string.Empty, null);
			Assert.AreEqual(0, migrator.GetAppliedMigrations().Count);

			using (IDataReader reader = provider.ExecuteQuery("SELECT [Key], Version FROM SchemaInfo"))
			{
				reader.Read();
				Assert.AreEqual(string.Empty, reader[0]);
				Assert.AreEqual(1, ((Int64)reader[1]));
				Assert.IsFalse(reader.Read());
			}

			provider.RemoveTable("SchemaInfo");
		}

		#region Helpers
		private string ConnectionString
		{
			get
			{
				string constr = ConfigurationManager.AppSettings["SqlServer2005ConnectionString"];
				if (constr == null)
				{
					throw new ArgumentNullException("SqlServer2005ConnectionString", "No config file");
				}

				return constr;
			}
		}

		public ITransformationProvider CreateSqlServer2005Provider(string key)
		{
			var provider = new SqlServerTransformationProvider(new SqlServer2005Dialect(), ConnectionString);

			return provider;
		}

		#endregion
	}
}
