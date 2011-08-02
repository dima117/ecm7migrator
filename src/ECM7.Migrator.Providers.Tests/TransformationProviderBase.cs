namespace ECM7.Migrator.Providers.Tests
{
	using System;
	using System.Data;
	using System.Reflection;

	using ECM7.Migrator.Framework;
	using ECM7.Migrator.Framework.Logging;

	using log4net.Config;

	using Npgsql;

	using NUnit.Framework;

	/// <summary>
	/// Base class for Provider tests for all non-constraint oriented tests.
	/// </summary>
	//[TestFixture(Ignore = true, IgnoreReason = "")]
	public abstract class TransformationProviderBase
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

		[SetUp]
		public void SetUp()
		{
			if (!isInitialized)
			{
				BasicConfigurator.Configure();
				isInitialized = true;
			}
		}

		[TearDown]
		public virtual void TearDown()
		{
			DropTestTables();

			provider.Rollback();
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
			new Column("TestId", DbType.Int32, ColumnProperty.ForeignKey)
			);
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

			using (var reader = provider.ExecuteQuery(sql))
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
		public void TableExistsWorks()
		{
			Assert.IsFalse(provider.TableExists("gadadadadseeqwe"));
			Assert.IsTrue(provider.TableExists("TestTwo"));
		}

		[Test]
		public void ColumnExistsWorks()
		{
			Assert.IsFalse(provider.ColumnExists("gadadadadseeqwe", "eqweqeq"));
			Assert.IsFalse(provider.ColumnExists("TestTwo", "eqweqeq"));
			Assert.IsTrue(provider.ColumnExists("TestTwo", "Id"));
		}

		[Test]
		public void CanExecuteBadSqlForNonCurrentProvider()
		{
			provider.For<GenericTransformationProvider<NpgsqlConnection>>()
				.ExecuteNonQuery("select foo from bar 123");
		}

		[Test]
		public void CanExecuteBadSqlInDelegateForNonCurrentProvider()
		{
			provider.For<GenericTransformationProvider<NpgsqlConnection>>(
				database => database.ExecuteNonQuery("select foo from bar 123"));
		}

		[Test]
		public void TableCanBeAdded()
		{
			AddTableWithPrimaryKey();
			Assert.IsTrue(provider.TableExists("Test"));
		}

		[Test]
		public void GetTablesWorks()
		{
			foreach (string name in provider.GetTables())
			{
				MigratorLogManager.Log.InfoFormat("Table: {0}", name);
			}
			Assert.AreEqual(1, provider.GetTables().Length);
			AddTableWithPrimaryKey();
			Assert.AreEqual(2, provider.GetTables().Length);
		}

		[Test]
		public void GetColumnsReturnsProperCount()
		{
			AddTableWithPrimaryKey();
			Column[] cols = provider.GetColumns("Test");
			Assert.IsNotNull(cols);
			Assert.AreEqual(6, cols.Length);
		}

		[Test]
		public void GetColumnsContainsProperNullInformation()
		{
			AddTableWithPrimaryKey();
			Column[] cols = provider.GetColumns("Test");
			Assert.IsNotNull(cols);
			foreach (Column column in cols)
			{
				if (column.Name == "name")
					Assert.IsTrue((column.ColumnProperty & ColumnProperty.NotNull) == ColumnProperty.NotNull);
				else if (column.Name == "Title")
					Assert.IsTrue((column.ColumnProperty & ColumnProperty.Null) == ColumnProperty.Null);
			}
		}

		[Test]
		public void CanAddTableWithPrimaryKey()
		{
			AddTableWithPrimaryKey();
			Assert.IsTrue(provider.TableExists("Test"));
		}

		[Test]
		public void RemoveTable()
		{
			AddTableWithPrimaryKey();
			provider.RemoveTable("Test");
			Assert.IsFalse(provider.TableExists("Test"));
		}

		[Test]
		public virtual void RenameTableThatExists()
		{
			AddTableWithPrimaryKey();
			provider.RenameTable("Test", "Test_Rename");

			Assert.IsTrue(provider.TableExists("Test_Rename"));
			Assert.IsFalse(provider.TableExists("Test"));
			provider.RemoveTable("Test_Rename");
		}

		[Test, ExpectedException(typeof(MigrationException))]
		public void RenameTableToExistingTable()
		{
			AddTableWithPrimaryKey();
			provider.RenameTable("Test", "TestTwo");

		}

		[Test]
		public virtual void RenameColumnThatExists()
		{
			AddTableWithPrimaryKey();
			provider.RenameColumn("Test", "Name", "name_rename");

			Assert.IsTrue(provider.ColumnExists("Test", "name_rename"));
			Assert.IsFalse(provider.ColumnExists("Test", "Name"));
		}

		[Test, ExpectedException(typeof(MigrationException))]
		public void RenameColumnToExistingColumn()
		{
			AddTableWithPrimaryKey();
			provider.RenameColumn("Test", "Title", "Name");
		}

		[Test]
		public void RemoveUnexistingTable()
		{
			provider.RemoveTable("abc");
		}

		[Test]
		public void AddColumn()
		{
			provider.AddColumn("TestTwo", "Test", DbType.String, 50);
			Assert.IsTrue(provider.ColumnExists("TestTwo", "Test"));
		}

		[Test]
		public virtual void ChangeColumn()
		{
			provider.ChangeColumn("TestTwo", new Column("TestId", DbType.String, 50, ColumnProperty.Null));
			Assert.IsTrue(provider.ColumnExists("TestTwo", "TestId"));
			provider.Insert("TestTwo", new[] { "Id", "TestId" }, new[] { "1", "Not an Int val." });
		}

		[Test]
		public void AddDecimalColumn()
		{
			provider.AddColumn("TestTwo", "TestDecimal", DbType.Decimal, 38);
			Assert.IsTrue(provider.ColumnExists("TestTwo", "TestDecimal"));
		}

		[Test]
		public void AddColumnWithDefault()
		{
			provider.AddColumn("TestTwo", "TestWithDefault", DbType.Int32, 50, 0, 10);
			Assert.IsTrue(provider.ColumnExists("TestTwo", "TestWithDefault"));
		}

		[Test]
		public void AddColumnWithDefaultButNoSize()
		{
			provider.AddColumn("TestTwo", "TestWithDefault", DbType.Int32, 10);
			Assert.IsTrue(provider.ColumnExists("TestTwo", "TestWithDefault"));


			provider.AddColumn("TestTwo", "TestWithDefaultString", DbType.String, "'foo'");
			Assert.IsTrue(provider.ColumnExists("TestTwo", "TestWithDefaultString"));
		}

		[Test]
		public void AddBooleanColumnWithDefault()
		{
			provider.AddColumn("TestTwo", "TestBoolean", DbType.Boolean, 0, 0, false);
			Assert.IsTrue(provider.ColumnExists("TestTwo", "TestBoolean"));
		}

		[Test]
		public void CanGetNullableFromProvider()
		{
			provider.AddColumn("TestTwo", "NullableColumn", DbType.String, 30, ColumnProperty.Null);
			Column[] columns = provider.GetColumns("TestTwo");
			foreach (Column column in columns)
			{
				if (column.Name == "NullableColumn")
				{
					Assert.IsTrue((column.ColumnProperty & ColumnProperty.Null) == ColumnProperty.Null);
				}
			}
		}

		[Test]
		public void RemoveColumn()
		{
			AddColumn();
			provider.RemoveColumn("TestTwo", "Test");
			Assert.IsFalse(provider.ColumnExists("TestTwo", "Test"));
		}

		[Test]
		public void RemoveColumnWithDefault()
		{
			AddColumnWithDefault();
			provider.RemoveColumn("TestTwo", "TestWithDefault");
			Assert.IsFalse(provider.ColumnExists("TestTwo", "TestWithDefault"));
		}

		[Test]
		public void RemoveUnexistingColumn()
		{
			provider.RemoveColumn("TestTwo", "abc");
			provider.RemoveColumn("abc", "abc");
		}

		/// <summary>
		/// Supprimer une colonne bit causait une erreur à cause
		/// de la valeur par défaut.
		/// </summary>
		[Test]
		public void RemoveBoolColumn()
		{
			AddTableWithPrimaryKey();
			provider.AddColumn("Test", "Inactif", DbType.Boolean);
			Assert.IsTrue(provider.ColumnExists("Test", "Inactif"));

			provider.RemoveColumn("Test", "Inactif");
			Assert.IsFalse(provider.ColumnExists("Test", "Inactif"));
		}

		[Test]
		public void HasColumn()
		{
			AddColumn();
			Assert.IsTrue(provider.ColumnExists("TestTwo", "Test"));
			Assert.IsFalse(provider.ColumnExists("TestTwo", "TestPasLa"));
		}

		[Test]
		public void HasTable()
		{
			Assert.IsTrue(provider.TableExists("TestTwo"));
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
		public virtual void InsertData()
		{
			provider.Insert("TestTwo", new[] { "Id", "TestId" }, new[] { "1", "1" });
			provider.Insert("TestTwo", new[] { "Id", "TestId" }, new[] { "2", "2" });

			string sql = "SELECT {0} FROM {1}".FormatWith(
				provider.QuoteName("TestId"), provider.QuoteName("TestTwo"));

			using (IDataReader reader = provider.ExecuteQuery(sql))
			{
				int[] vals = GetVals(reader);

				Assert.IsTrue(Array.Exists(vals, val => val == 1));
				Assert.IsTrue(Array.Exists(vals, val => val == 2));
			}
		}

		[Test]
		public void CanInsertNullData()
		{
			AddTableWithPrimaryKey();
			provider.Insert("Test", new[] { "Id", "Title", "Name" }, new[] { "1", "foo", "moo" });
			provider.Insert("Test", new[] { "Id", "Title", "Name" }, new[] { "2", null, "mi mi" });

			string sql = "SELECT {0} FROM {1}".FormatWith(
				provider.QuoteName("Title"), provider.QuoteName("Test"));

			using (IDataReader reader = provider.ExecuteQuery(sql))
			{
				string[] vals = GetStringVals(reader);

				Assert.IsTrue(Array.Exists(vals, val => val == "foo"));
				Assert.IsTrue(Array.Exists(vals, val => val == null));
			}
		}

		[Test]
		public void CanInsertDataWithSingleQuotes()
		{
			AddTableWithPrimaryKey();
			provider.Insert("Test", new[] { "Id", "Name" }, new[] { "1", "Muad'Dib" });

			string sql = "SELECT {0} FROM {1}".FormatWith(
				provider.QuoteName("Name"), provider.QuoteName("Test"));

			using (IDataReader reader = provider.ExecuteQuery(sql))
			{
				Assert.IsTrue(reader.Read());
				Assert.AreEqual("Muad'Dib", reader.GetString(0));
				Assert.IsFalse(reader.Read());
			}
		}

		[Test]
		public void DeleteData()
		{
			InsertData();
			provider.Delete("TestTwo", provider.QuoteName("TestId") + " = 1");

			string sql = "SELECT {0} FROM {1}".FormatWith(
				provider.QuoteName("TestId"), provider.QuoteName("TestTwo"));

			using (IDataReader reader = provider.ExecuteQuery(sql))
			{
				Assert.IsTrue(reader.Read());
				Assert.AreEqual(2, Convert.ToInt32(reader[0]));
				Assert.IsFalse(reader.Read());
			}
		}

		[Test]
		public void DeleteAllData()
		{
			InsertData();
			provider.Delete("TestTwo");

			string sql = "SELECT {0} FROM {1}".FormatWith(
				provider.QuoteName("TestId"), provider.QuoteName("TestTwo"));

			using (IDataReader reader = provider.ExecuteQuery(sql))
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

			using (IDataReader reader = provider.ExecuteQuery(sql))
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

			using (IDataReader reader = provider.ExecuteQuery(sql))
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

			using (IDataReader reader = provider.ExecuteQuery(sql))
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
