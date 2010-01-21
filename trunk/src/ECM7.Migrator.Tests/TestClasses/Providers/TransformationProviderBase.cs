using System;
using System.Data;
using ECM7.Migrator.Framework;
using ECM7.Migrator.Tests.TestClasses.Providers;
using NUnit.Framework;

namespace ECM7.Migrator.Tests.Providers
{
	/// <summary>
	/// Base class for Provider tests for all non-constraint oriented tests.
	/// </summary>
	public class TransformationProviderBase
	{
		protected ITransformationProvider provider;

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

		public void AddTable()
		{
			provider.AddTable("Test",
			new Column("Id", DbType.Int32, ColumnProperty.NotNull),
			new Column("Title", DbType.String, 100, ColumnProperty.Null),
			new Column("name", DbType.String, 50, ColumnProperty.Null),
			new Column("blobVal", DbType.Binary, ColumnProperty.Null),
			new Column("boolVal", DbType.Boolean, ColumnProperty.Null),
			new Column("bigstring", DbType.String, 50000, ColumnProperty.Null)
			);
		}

		public void AddTableWithPrimaryKey()
		{
			provider.AddTable("Test",
			new Column("Id", DbType.Int32, ColumnProperty.PrimaryKey),
			new Column("Title", DbType.String, 100, ColumnProperty.Null),
			new Column("name", DbType.String, 50, ColumnProperty.NotNull),
			new Column("blobVal", DbType.Binary),
			new Column("boolVal", DbType.Boolean),
			new Column("bigstring", DbType.String, 50000)
			);
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
			provider.For<GenericDialect>()
				.ExecuteNonQuery("select foo from bar 123");
		}

		[Test]
		public void CanExecuteBadSqlInDelegateForNonCurrentProvider()
		{
			provider.For<GenericDialect>(database => database.ExecuteNonQuery("select foo from bar 123"));
		}

		[Test]
		public void TableCanBeAdded()
		{
			AddTable();
			Assert.IsTrue(provider.TableExists("Test"));
		}

		[Test]
		public void GetTablesWorks()
		{
			foreach (string name in provider.GetTables())
			{
				provider.Logger.Log("Table: {0}", name);
			}
			Assert.AreEqual(1, provider.GetTables().Length);
			AddTable();
			Assert.AreEqual(2, provider.GetTables().Length);
		}

		[Test]
		public void GetColumnsReturnsProperCount()
		{
			AddTable();
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
			AddTable();
			provider.RemoveTable("Test");
			Assert.IsFalse(provider.TableExists("Test"));
		}

		[Test]
		public virtual void RenameTableThatExists()
		{
			AddTable();
			provider.RenameTable("Test", "Test_Rename");

			Assert.IsTrue(provider.TableExists("Test_Rename"));
			Assert.IsFalse(provider.TableExists("Test"));
			provider.RemoveTable("Test_Rename");
		}

		[Test, ExpectedException(typeof(MigrationException))]
		public void RenameTableToExistingTable()
		{
			AddTable();
			provider.RenameTable("Test", "TestTwo");

		}

		[Test]
		public void RenameColumnThatExists()
		{
			AddTable();
			provider.RenameColumn("Test", "name", "name_rename");

			Assert.IsTrue(provider.ColumnExists("Test", "name_rename"));
			Assert.IsFalse(provider.ColumnExists("Test", "name"));
		}

		[Test, ExpectedException(typeof(MigrationException))]
		public void RenameColumnToExistingColumn()
		{
			AddTable();
			provider.RenameColumn("Test", "Title", "name");
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
			AddTable();
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
			Assert.IsFalse(provider.TableExists("SchemaInfo"));

			// Check that a "get" call works on the first run.
			Assert.AreEqual(0, provider.AppliedMigrations.Count);
			Assert.IsTrue(provider.TableExists("SchemaInfo"), "No SchemaInfo table created");

			// Check that a "set" called after the first run works.
			provider.MigrationApplied(1);
			Assert.AreEqual(1, provider.AppliedMigrations[0]);

			provider.RemoveTable("SchemaInfo");
			// Check that a "set" call works on the first run.
			provider.MigrationApplied(1);
			Assert.AreEqual(1, provider.AppliedMigrations[0]);
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
			Assert.AreEqual(0, provider.AppliedMigrations.Count);
			provider.Commit();
		}

		[Test]
		public virtual void InsertData()
		{
			provider.Insert("TestTwo", new[] { "Id", "TestId" }, new[] { "1", "1" });
			provider.Insert("TestTwo", new[] { "Id", "TestId" }, new[] { "2", "2" });
			using (IDataReader reader = provider.Select("TestId", "TestTwo"))
			{
				int[] vals = GetVals(reader);

				Assert.IsTrue(Array.Exists(vals, val => val == 1));
				Assert.IsTrue(Array.Exists(vals, val => val == 2));
			}
		}

		[Test]
		public void CanInsertNullData()
		{
			AddTable();
			provider.Insert("Test", new[] { "Id", "Title" }, new[] { "1", "foo" });
			provider.Insert("Test", new[] { "Id", "Title" }, new[] { "2", null });
			using (IDataReader reader = provider.Select("Title", "Test"))
			{
				string[] vals = GetStringVals(reader);

				Assert.IsTrue(Array.Exists(vals, val => val == "foo"));
				Assert.IsTrue(Array.Exists(vals, val => val == null));
			}
		}

		[Test]
		public void CanInsertDataWithSingleQuotes()
		{
			AddTable();
			provider.Insert("Test", new[] { "Id", "Title" }, new[] { "1", "Muad'Dib" });
			using (IDataReader reader = provider.Select("Title", "Test"))
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
			provider.Delete("TestTwo", "TestId", "1");

			using (IDataReader reader = provider.Select("TestId", "TestTwo"))
			{
				Assert.IsTrue(reader.Read());
				Assert.AreEqual(2, Convert.ToInt32(reader[0]));
				Assert.IsFalse(reader.Read());
			}
		}

		[Test]
		public void DeleteDataWithArrays()
		{
			InsertData();
			provider.Delete("TestTwo", new[] { "Id", "TestId" }, new[] { "1", "1" });

			using (IDataReader reader = provider.Select("TestId", "TestTwo"))
			{
				Assert.IsTrue(reader.Read());
				Assert.AreEqual(2, Convert.ToInt32(reader[0]));
				Assert.IsFalse(reader.Read());
			}
		}

		[Test]
		public virtual void UpdateData()
		{
			provider.Insert("TestTwo", new[] { "Id", "TestId" }, new[] { "1", "1" });
			provider.Insert("TestTwo", new[] { "Id", "TestId" }, new[] { "2", "2" });

			provider.Update("TestTwo", new[] { "TestId" }, new[] { "3" });

			using (IDataReader reader = provider.Select("TestId", "TestTwo"))
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
			AddTable();
			provider.Insert("Test", new[] { "Id", "Title" }, new[] { "1", "foo" });
			provider.Insert("Test", new[] { "Id", "Title" }, new[] { "2", null });

			provider.Update("Test", new[] { "Title" }, new string[] { null });

			using (IDataReader reader = provider.Select("Title", "Test"))
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

			provider.Update("TestTwo", new[] { "TestId" }, new[] { "3" }, "TestId='1'");

			using (IDataReader reader = provider.Select("TestId", "TestTwo"))
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
