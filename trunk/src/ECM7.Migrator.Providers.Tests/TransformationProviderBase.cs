namespace ECM7.Migrator.Providers.Tests
{
	using System;
	using System.Configuration;
	using System.Data;
	using System.Reflection;

	using ECM7.Migrator.Framework;

	using log4net.Config;

	using Npgsql;

	using NUnit.Framework;

	/// <summary>
	/// Base class for Provider tests for all non-constraint oriented tests.
	/// </summary>
	//[TestFixture(Ignore = true, IgnoreReason = "")]
	public abstract class TransformationProviderBase<TProvider> where TProvider : ITransformationProvider
	{
		protected ITransformationProvider provider;

		protected static bool isInitialized;

		protected virtual string ResourceSql
		{
			get { return "ECM7.Migrator.TestAssembly.Res.test.res.migration.sql"; }
		}

		protected virtual string BatchSql
		{
			get
			{
				return "переопределите SQL для проверки пакетов запросов";
			}
		}

		public abstract string ConnectionStrinSettingsName { get; }
		public abstract bool UseTransaction { get; }

		[SetUp]
		public virtual void SetUp()
		{
			if (!isInitialized)
			{
				BasicConfigurator.Configure();
				isInitialized = true;
			}

			string constr = ConfigurationManager.AppSettings[ConnectionStrinSettingsName];
			Require.IsNotNullOrEmpty(constr, "Connection string \"{0}\" is not exist", ConnectionStrinSettingsName);

			provider = ProviderFactory.Create<TProvider>(constr);

			if (UseTransaction)
			{
				provider.BeginTransaction();
			}

			AddDefaultTable();
		}

		[TearDown]
		public virtual void TearDown()
		{
			DropTestTables();

			if (UseTransaction)
			{
				provider.Rollback();
			}

			provider.Dispose();
		}

		protected void DropTestTables()
		{
			// Because MySql doesn't support schema transaction
			// we got to remove the tables manually... sad...
			provider.RemoveTable("TestTwo");
			provider.RemoveTable("Test");
			provider.RemoveTable("SchemaInfo");
		}

		public void AddDefaultTable()
		{
			provider.AddTable("TestTwo",
				new Column("Id", DbType.Int32, ColumnProperty.PrimaryKey),
				new Column("TestId", DbType.Int32));
		}

		public void AddTableWithoutPrimaryKey()
		{
			provider.AddTable("Test",
				new Column("Id", DbType.Int32, ColumnProperty.NotNull),
				new Column("Title", DbType.String, 100, ColumnProperty.Null),
				new Column("name", DbType.String, 50, ColumnProperty.Null),
				new Column("blobVal", DbType.Binary, ColumnProperty.Null),
				new Column("boolVal", DbType.Boolean, ColumnProperty.Null),
				new Column("bigstring", DbType.String, 50000, ColumnProperty.Null));
		}

		public void AddTableWithPrimaryKey()
		{
			provider.AddTable("Test",
				new Column("Id", DbType.Int32, ColumnProperty.PrimaryKey),
				new Column("Title", DbType.String, 100, ColumnProperty.Null),
				new Column("Name", DbType.String, 50, ColumnProperty.NotNull, "''"),
				new Column("blobVal", DbType.Binary),
				new Column("boolVal", DbType.Boolean),
				new Column("bigstring", DbType.String, 50000));
		}

		[Test]
		public void CanExecuteBatches()
		{
			provider.ExecuteNonQuery(BatchSql);

			string sql = provider.FormatSql("SELECT {0:NAME} FROM {1:NAME} ORDER BY {2:NAME}", "TestId", "TestTwo", "Id");

			using (var reader = provider.ExecuteReader(sql))
			{
				for (int i = 1; i <= 5; i++)
				{
					Assert.IsTrue(reader.Read());
					Assert.AreEqual(111 * i, Convert.ToInt32(reader[0]));
				}
				Assert.IsFalse(reader.Read());
			}
		}

		[Test]
		public void CanExecuteScriptFromResources()
		{
			Assembly asm = Assembly.Load("ECM7.Migrator.TestAssembly");
			provider.ExecuteFromResource(asm, ResourceSql);

			string sql = provider.FormatSql("SELECT {0:NAME} FROM {1:NAME} WHERE {2:NAME} = {3}",
				"TestId", "TestTwo", "Id", 5555);

			object res = provider.ExecuteScalar(sql);

			Assert.AreEqual(9999, Convert.ToInt32(res));
		}

		[Test]
		public void CanExecuteBadSqlForNonCurrentProvider()
		{
			provider.For<GenericTransformationProvider<NpgsqlConnection>>(
				database => database.ExecuteNonQuery("select foo from bar 123"));
		}

		[Test]
		public void CanExecuteBadSqlInDelegateForNonCurrentProvider()
		{
			provider.For<GenericTransformationProvider<NpgsqlConnection>>(
				database => database.ExecuteNonQuery("select foo from bar 123"));
		}

		[Test]
		public void AppliedMigrations()
		{
			const string KEY = "mi mi mi";
			Assert.IsFalse(provider.TableExists("SchemaInfo"));

			// Check that a "get" call works on the first run.
			Assert.AreEqual(0, provider.GetAppliedMigrations(KEY).Count);
			Assert.IsTrue(provider.TableExists("SchemaInfo"), "No SchemaInfo table created");

			// Check that a "set" called after the first run works.
			provider.MigrationApplied(1, KEY);
			Assert.AreEqual(1, provider.GetAppliedMigrations(KEY)[0]);

			provider.RemoveTable("SchemaInfo");

			// Check that a "set" call works on the first run.
			provider.MigrationApplied(1, KEY);
			Assert.AreEqual(1, provider.GetAppliedMigrations(KEY)[0]);
			Assert.IsTrue(provider.TableExists("SchemaInfo"), "No SchemaInfo table created");
		}

		/// <summary>
		/// Reproduce bug reported by Luke Melia & Daniel Berlinger :
		/// http://macournoyer.wordpress.com/2006/10/15/migrate-nant-task/#comment-113
		/// </summary>
		[Test]
		public void CommitTwice()
		{
			provider.Commit();
			Assert.AreEqual(0, provider.GetAppliedMigrations(string.Empty).Count);
			provider.Commit();
		}

		[Test]
		public void DeleteData()
		{
			//InsertData();
			provider.Delete("TestTwo", provider.QuoteName("TestId") + " = 1");

			string sql = "SELECT {0} FROM {1}".FormatWith(
				provider.QuoteName("TestId"), provider.QuoteName("TestTwo"));

			using (IDataReader reader = provider.ExecuteReader(sql))
			{
				Assert.IsTrue(reader.Read());
				Assert.AreEqual(2, Convert.ToInt32(reader[0]));
				Assert.IsFalse(reader.Read());
			}
		}

		[Test]
		public void DeleteAllData()
		{
			//InsertData();
			provider.Delete("TestTwo");

			string sql = "SELECT {0} FROM {1}".FormatWith(
				provider.QuoteName("TestId"), provider.QuoteName("TestTwo"));

			using (IDataReader reader = provider.ExecuteReader(sql))
			{
				Assert.IsFalse(reader.Read());
			}
		}

		[Test]
		public virtual void UpdateData()
		{
			provider.Insert("TestTwo", new[] { "Id", "TestId" }, new[] { "1", "1" });
			provider.Insert("TestTwo", new[] { "Id", "TestId" }, new[] { "2", "2" });

			provider.Update("TestTwo", new[] { "TestId" }, new[] { "3" });

			string sql = "SELECT {0} FROM {1}".FormatWith(
				provider.QuoteName("TestId"), provider.QuoteName("TestTwo"));

			using (IDataReader reader = provider.ExecuteReader(sql))
			{
				int[] vals = GetVals(reader);

				Assert.IsTrue(Array.Exists(vals, val => val == 3));
				Assert.IsFalse(Array.Exists(vals, val => val == 1));
				Assert.IsFalse(Array.Exists(vals, val => val == 2));
			}
		}

		[Test]
		public virtual void CanUpdateWithNullData()
		{
			AddTableWithPrimaryKey();
			provider.Insert("Test", new[] { "Id", "Title", "Name" }, new[] { "1", "foo", "moo" });
			provider.Insert("Test", new[] { "Id", "Title", "Name" }, new[] { "2", null, "mi mi" });

			provider.Update("Test", new[] { "Title" }, new string[] { null });

			string sql = "SELECT {0} FROM {1}".FormatWith(
				provider.QuoteName("Title"), provider.QuoteName("Test"));

			using (IDataReader reader = provider.ExecuteReader(sql))
			{
				string[] vals = GetStringVals(reader);

				Assert.IsNull(vals[0]);
				Assert.IsNull(vals[1]);
			}
		}

		[Test]
		public void UpdateDataWithWhere()
		{
			provider.Insert("TestTwo", new[] { "Id", "TestId" }, new[] { "1", "1" });
			provider.Insert("TestTwo", new[] { "Id", "TestId" }, new[] { "2", "2" });

			provider.Update("TestTwo", new[] { "TestId" }, new[] { "3" }, provider.QuoteName("TestId") + " = '1'");

			string sql = "SELECT {0} FROM {1}".FormatWith(
				provider.QuoteName("TestId"), provider.QuoteName("TestTwo"));

			using (IDataReader reader = provider.ExecuteReader(sql))
			{
				int[] vals = GetVals(reader);

				Assert.IsTrue(Array.Exists(vals, val => val == 3));
				Assert.IsTrue(Array.Exists(vals, val => val == 2));
				Assert.IsFalse(Array.Exists(vals, val => val == 1));
			}
		}

		protected int[] GetVals(IDataReader reader)
		{
			int[] vals = new int[2];
			Assert.IsTrue(reader.Read());
			vals[0] = Convert.ToInt32(reader[0]);
			Assert.IsTrue(reader.Read());
			vals[1] = Convert.ToInt32(reader[0]);
			return vals;
		}

		protected string[] GetStringVals(IDataReader reader)
		{
			string[] vals = new string[2];
			Assert.IsTrue(reader.Read());
			vals[0] = reader[0] as string;
			Assert.IsTrue(reader.Read());
			vals[1] = reader[0] as string;
			return vals;
		}
	}
}
