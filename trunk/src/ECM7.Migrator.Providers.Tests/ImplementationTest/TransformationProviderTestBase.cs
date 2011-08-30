namespace ECM7.Migrator.Providers.Tests.ImplementationTest
{
	using System;
	using System.Configuration;
	using System.Data;
	using System.Linq;

	using ECM7.Migrator.Exceptions;
	using ECM7.Migrator.Framework;

	using log4net.Config;

	using NUnit.Framework;

	using ForeignKeyConstraint = ECM7.Migrator.Framework.ForeignKeyConstraint;

	public abstract class TransformationProviderTestBase<TProvider> where TProvider : ITransformationProvider
	{
		#region common

		protected ITransformationProvider provider;

		protected static bool isInitialized;

		public abstract string ConnectionStrinSettingsName { get; }

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
		}

		[TearDown]
		public virtual void TearDown()
		{
			provider.Dispose();
		}


		#endregion

		#region tests

		#region table

		[Test]
		public void CanAddAndDropTable()
		{
			string tableName = this.GetRandomName("MooTable");

			Assert.IsFalse(provider.TableExists(tableName));

			provider.AddTable(tableName, new Column("ID", DbType.Int32));

			Assert.IsTrue(provider.TableExists(tableName));

			provider.RemoveTable(tableName);

			Assert.IsFalse(provider.TableExists(tableName));
		}

		[Test]
		public void CanCreateTableWithNecessaryCols()
		{
			string tableName = this.GetRandomName("Mimimi");

			provider.AddTable(tableName,
				new Column("ID", DbType.Int32),
				new Column("StringColumn", DbType.String.WithSize(500)),
				new Column("DecimalColumn", DbType.Decimal.WithSize(18, 2))
			);

			provider.Insert(
				tableName,
				new[] { "ID", "StringColumn", "DecimalColumn" },
				new[] { "1984", "test moo", "123.56789" }
			);


			string sql = provider.FormatSql("select * from {0:NAME}", tableName);
			using (var reader = provider.ExecuteReader(sql))
			{
				Assert.IsTrue(reader.Read());
				Assert.AreEqual(1984, reader.GetInt32(0));
				Assert.AreEqual("test moo", reader.GetString(1));
				Assert.AreEqual(123.57m, reader.GetDecimal(2));
				Assert.IsFalse(reader.Read());
			}

			provider.RemoveTable(tableName);
		}

		[Test]
		public void CanAddTableWithCompoundPrimaryKey()
		{
			string tableName = this.GetRandomName("TableWCPK");

			provider.AddTable(tableName,
				new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
				new Column("ID2", DbType.Int32, ColumnProperty.PrimaryKey)
			);

			Assert.IsTrue(provider.ConstraintExists(tableName, "PK_" + tableName));

			provider.Insert(tableName, new[] { "ID", "ID2" }, new[] { "5", "6" });

			Assert.Throws<SQLException>(() =>
				provider.Insert(tableName, new[] { "ID", "ID2" }, new[] { "5", "6" }));

			provider.RemoveTable(tableName);
		}

		[Test]
		public void CanRenameTable()
		{
			string table1 = this.GetRandomName("tableMoo");
			string table2 = this.GetRandomName("tableHru");

			Assert.IsFalse(provider.TableExists(table1));
			Assert.IsFalse(provider.TableExists(table2));

			provider.AddTable(table1, new Column("ID", DbType.Int32));
			provider.RenameTable(table1, table2);

			Assert.IsTrue(provider.TableExists(table2));

			provider.RemoveTable(table2);
		}

		[Test]
		public void CantRemoveUnexistingTable()
		{
			string randomTableName = GetRandomName();
			Assert.Throws<SQLException>(() => provider.RemoveTable(randomTableName));
		}

		[Test]
		public void CanGetTables()
		{
			string table1 = this.GetRandomName("tableMoo");
			string table2 = this.GetRandomName("tableHru");

			var tables = provider.GetTables();
			Assert.AreEqual(0, tables.Length);

			provider.AddTable(table1, new Column("ID", DbType.Int32));
			provider.AddTable(table2, new Column("ID", DbType.Int32));

			var tables2 = provider.GetTables();

			Assert.AreEqual(2, tables2.Length);
			Assert.IsTrue(tables2.Contains(table1));
			Assert.IsTrue(tables2.Contains(table2));

			provider.RemoveTable(table1);
			provider.RemoveTable(table2);
		}

		#endregion

		#region columns

		[Test]
		public void CanAddColumn()
		{
			string tableName = this.GetRandomName("AddColumnTest");

			provider.AddTable(tableName, new Column("ID", DbType.Int32));

			provider.AddColumn(tableName, new Column("TestStringColumn", DbType.String.WithSize(7)));

			provider.Insert(tableName, new[] { "ID", "TestStringColumn" }, new[] { "2", "test" });
			provider.Insert(tableName, new[] { "ID", "TestStringColumn" }, new[] { "4", "testmoo" });

			Assert.Throws<SQLException>(() =>
				provider.Insert(tableName, new[] { "ID", "TestStringColumn" }, new[] { "6", "testmoo1" }));

			string sql = provider.FormatSql("select * from {0:NAME}", tableName);
			using (var reader = provider.ExecuteReader(sql))
			{
				Assert.IsTrue(reader.Read());
				Assert.AreEqual(2, reader.GetInt32(0));
				Assert.AreEqual("test", reader.GetString(1));

				Assert.IsTrue(reader.Read());
				Assert.AreEqual(4, reader.GetInt32(0));
				Assert.AreEqual("testmoo", reader.GetString(1));

				Assert.IsFalse(reader.Read());
			}

			provider.RemoveTable(tableName);
		}

		[Test]
		public void CanChangeColumn()
		{
			string tableName = GetRandomName("ChangeColumnTest");

			provider.AddTable(tableName, new Column("TestStringColumn", DbType.String.WithSize(4)));

			provider.Insert(tableName, new[] { "TestStringColumn" }, new[] { "moo" });
			provider.Insert(tableName, new[] { "TestStringColumn" }, new[] { "test" });

			Assert.Throws<SQLException>(() =>
				provider.Insert(tableName, new[] { "TestStringColumn" }, new[] { "moo-test" }));

			provider.ChangeColumn(tableName, new Column("TestStringColumn", DbType.String.WithSize(18)));
			provider.Insert(tableName, new[] { "TestStringColumn" }, new[] { "moo-test" });

			string sql = provider.FormatSql(
				"select count({0:NAME}) from {1:NAME} where {0:NAME} = '{2}'",
				"TestStringColumn", tableName, "moo-test");

			Assert.AreEqual(1, provider.ExecuteScalar(sql));
			provider.RemoveTable(tableName);
		}

		[Test]
		public void CanRenameColumn()
		{
			string tableName = GetRandomName("RenameColumnTest");

			provider.AddTable(tableName, new Column("TestColumn1", DbType.Int32));
			provider.RenameColumn(tableName, "TestColumn1", "TestColumn2");

			Assert.IsFalse(provider.ColumnExists(tableName, "TestColumn1"));
			Assert.IsTrue(provider.ColumnExists(tableName, "TestColumn2"));

			provider.RemoveTable(tableName);
		}

		[Test]
		public void CanRemoveColumn()
		{
			string tableName = GetRandomName("RemoveColumnTest");

			provider.AddTable(tableName,
				new Column("ID", DbType.Int32),
				new Column("TestColumn1", DbType.Int32));

			provider.RemoveColumn(tableName, "TestColumn1");

			Assert.IsFalse(provider.ColumnExists(tableName, "TestColumn1"));

			provider.RemoveTable(tableName);
		}

		[Test]
		public void CantRemoveUnexistingColumn()
		{
			string tableName = GetRandomName("RemoveUnexistingColumn");

			provider.AddTable(tableName, new Column("ID", DbType.Int32));

			Assert.Throws<SQLException>(() =>
				provider.RemoveColumn(tableName, GetRandomName()));

			provider.RemoveTable(tableName);
		}

		#endregion

		#region constraints

		#region foreign key

		[Test]
		public void CanAddForeignKey()
		{
			// создаем таблицы и добавляем внешний ключ
			string primaryTable = GetRandomName("AddForeignKey_Primary");
			string refTable = GetRandomName("AddForeignKey_Ref");
			string foreignKeyName = GetRandomName("FK_TestSimpleKey");

			provider.AddTable(refTable, new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey));
			provider.Insert(refTable, "ID".AsArray(), "17".AsArray());

			provider.AddTable(primaryTable,
				new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
				new Column("RefID", DbType.Int32));
			provider.AddForeignKey(foreignKeyName, primaryTable, "RefID", refTable, "ID");

			// пробуем нарушить ограничения внешнего ключа
			Assert.Throws<SQLException>(() =>
				provider.Insert(primaryTable, new[] { "ID", "RefID" }, new[] { "1", "111" }));
			provider.Insert(primaryTable, new[] { "ID", "RefID" }, new[] { "1", "17" });
			Assert.Throws<SQLException>(() => provider.Delete(refTable));

			// удаляем таблицы
			provider.RemoveTable(primaryTable);
			provider.RemoveTable(refTable);
		}

		[Test]
		public void CanAddComplexForeignKey()
		{
			// создаем таблицы и добавляем внешний ключ
			string primaryTable = GetRandomName("AddForeignKey_Primary");
			string refTable = GetRandomName("AddForeignKey_Ref");
			string foreignKeyName = GetRandomName("FK_TestComplexKeyabd");

			provider.AddTable(refTable,
				new Column("ID1", DbType.Int32, ColumnProperty.PrimaryKey),
				new Column("ID2", DbType.Int32, ColumnProperty.PrimaryKey));

			provider.Insert(refTable, new[] { "ID1", "ID2" }, new[] { "111", "222" });

			provider.AddTable(primaryTable,
				new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
				new Column("RefID1", DbType.Int32),
				new Column("RefID2", DbType.Int32));

			provider.AddForeignKey(foreignKeyName,
				primaryTable, new[] { "RefID1", "RefID2" },
				refTable, new[] { "ID1", "ID2" });

			// пробуем нарушить ограничения внешнего ключа
			Assert.Throws<SQLException>(() =>
				provider.Insert(primaryTable, new[] { "ID", "RefID1", "RefID2" }, new[] { "1", "123", "456" }));

			Assert.Throws<SQLException>(() =>
				provider.Insert(primaryTable, new[] { "ID", "RefID1", "RefID2" }, new[] { "1", "111", "456" }));

			Assert.Throws<SQLException>(() =>
				provider.Insert(primaryTable, new[] { "ID", "RefID1", "RefID2" }, new[] { "1", "123", "222" }));

			provider.Insert(primaryTable, new[] { "ID", "RefID1", "RefID2" }, new[] { "1", "111", "222" });

			Assert.Throws<SQLException>(() => provider.Delete(refTable));

			// удаляем таблицы
			provider.RemoveTable(primaryTable);
			provider.RemoveTable(refTable);
		}

		[Test]
		public void CanAddForeignKeyWithDeleteCascade()
		{
			string primaryTable = GetRandomName("AddForeignKey_Primary");
			string refTable = GetRandomName("AddForeignKey_Ref");
			string foreignKeyName = GetRandomName("FK_Test");

			provider.AddTable(refTable, new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey));
			provider.Insert(refTable, "ID".AsArray(), "177".AsArray());

			provider.AddTable(primaryTable,
				new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
				new Column("RefID", DbType.Int32));
			provider.AddForeignKey(foreignKeyName, primaryTable, "RefID", refTable, "ID", ForeignKeyConstraint.Cascade);

			provider.Insert(primaryTable, new[] { "ID", "RefID" }, new[] { "1", "177" });
			provider.Delete(refTable);

			string sql = provider.FormatSql("select count(*) from {0:NAME}", primaryTable);
			Assert.AreEqual(0, provider.ExecuteScalar(sql));

			// удаляем таблицы
			provider.RemoveTable(primaryTable);
			provider.RemoveTable(refTable);
		}

		[Test]
		public void CanAddForeignKeyWithUpdateCascade()
		{
			string primaryTable = GetRandomName("AddForeignKey_Primary");
			string refTable = GetRandomName("AddForeignKey_Ref");
			string foreignKeyName = GetRandomName("FK_Test");

			provider.AddTable(refTable, new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey));
			provider.Insert(refTable, "ID".AsArray(), "654".AsArray());

			provider.AddTable(primaryTable,
				new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
				new Column("RefID", DbType.Int32));

			provider.AddForeignKey(foreignKeyName,
				primaryTable, "RefID", refTable, "ID",
				ForeignKeyConstraint.NoAction, ForeignKeyConstraint.Cascade);

			provider.Insert(primaryTable, new[] { "ID", "RefID" }, new[] { "1", "654" });

			provider.Update(refTable, "ID".AsArray(), "777".AsArray());

			string sql = provider.FormatSql("select {0:NAME} from {1:NAME}", "RefID", primaryTable);
			Assert.AreEqual(777, provider.ExecuteScalar(sql));

			// удаляем таблицы
			provider.RemoveTable(primaryTable);
			provider.RemoveTable(refTable);
		}

		[Test]
		public void CanAddForeignKeyWithDeleteSetNull()
		{
			string primaryTable = GetRandomName("AddForeignKey_Primary");
			string refTable = GetRandomName("AddForeignKey_Ref");
			string foreignKeyName = GetRandomName("FK_Test");

			provider.AddTable(refTable, new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey));
			provider.Insert(refTable, "ID".AsArray(), "177".AsArray());

			provider.AddTable(primaryTable,
				new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
				new Column("RefID", DbType.Int32));
			provider.AddForeignKey(foreignKeyName, primaryTable, "RefID", refTable, "ID", ForeignKeyConstraint.SetNull);

			provider.Insert(primaryTable, new[] { "ID", "RefID" }, new[] { "1", "177" });
			provider.Delete(refTable);

			string sql = provider.FormatSql("select {0:NAME} from {1:NAME}", "RefID", primaryTable);
			Assert.AreEqual(DBNull.Value, provider.ExecuteScalar(sql));

			// удаляем таблицы
			provider.RemoveTable(primaryTable);
			provider.RemoveTable(refTable);
		}

		[Test]
		public void CanAddForeignKeyWithUpdateSetNull()
		{
			string primaryTable = GetRandomName("AddForeignKey_Primary");
			string refTable = GetRandomName("AddForeignKey_Ref");
			string foreignKeyName = GetRandomName("FK_Test");

			provider.AddTable(refTable, new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey));
			provider.Insert(refTable, "ID".AsArray(), "654".AsArray());

			provider.AddTable(primaryTable,
				new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
				new Column("RefID", DbType.Int32));

			provider.AddForeignKey(foreignKeyName,
				primaryTable, "RefID", refTable, "ID",
				ForeignKeyConstraint.NoAction, ForeignKeyConstraint.SetNull);

			provider.Insert(primaryTable, new[] { "ID", "RefID" }, new[] { "1", "654" });

			provider.Update(refTable, "ID".AsArray(), "777".AsArray());

			string sql = provider.FormatSql("select {0:NAME} from {1:NAME}", "RefID", primaryTable);
			Assert.AreEqual(DBNull.Value, provider.ExecuteScalar(sql));

			// удаляем таблицы
			provider.RemoveTable(primaryTable);
			provider.RemoveTable(refTable);
		}

		[Test]
		public void CanAddForeignKeyWithDeleteSetDefault()
		{
			string primaryTable = GetRandomName("AddForeignKey_Primary");
			string refTable = GetRandomName("AddForeignKey_Ref");
			string foreignKeyName = GetRandomName("FK_Test");

			provider.AddTable(refTable, new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey));
			provider.Insert(refTable, "ID".AsArray(), "177".AsArray());
			provider.Insert(refTable, "ID".AsArray(), "998".AsArray());

			provider.AddTable(primaryTable,
				new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
				new Column("RefID", DbType.Int32, ColumnProperty.NotNull, 998));
			provider.AddForeignKey(foreignKeyName, primaryTable, "RefID", refTable, "ID", ForeignKeyConstraint.SetDefault);

			provider.Insert(primaryTable, new[] { "ID", "RefID" }, new[] { "1", "177" });

			string whereSql = provider.FormatSql("{0:NAME} = 177", "ID");
			provider.Delete(refTable, whereSql);

			string sql = provider.FormatSql("select {0:NAME} from {1:NAME}", "RefID", primaryTable);
			Assert.AreEqual(998, provider.ExecuteScalar(sql));

			// удаляем таблицы
			provider.RemoveTable(primaryTable);
			provider.RemoveTable(refTable);
		}

		[Test]
		public void CanAddForeignKeyWithUpdateSetDefault()
		{
			string primaryTable = GetRandomName("AddForeignKey_Primary");
			string refTable = GetRandomName("AddForeignKey_Ref");
			string foreignKeyName = GetRandomName("FK_Test");

			provider.AddTable(refTable, new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey));
			provider.Insert(refTable, "ID".AsArray(), "999".AsArray());
			provider.Insert(refTable, "ID".AsArray(), "654".AsArray());

			provider.AddTable(primaryTable,
				new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
				new Column("RefID", DbType.Int32, ColumnProperty.NotNull, 999));

			provider.AddForeignKey(foreignKeyName,
				primaryTable, "RefID", refTable, "ID",
				ForeignKeyConstraint.NoAction, ForeignKeyConstraint.SetDefault);

			provider.Insert(primaryTable, new[] { "ID", "RefID" }, new[] { "1", "654" });

			string whereSql = provider.FormatSql("{0:NAME} = 654", "ID");
			provider.Update(refTable, "ID".AsArray(), "777".AsArray(), whereSql);

			string sql = provider.FormatSql("select {0:NAME} from {1:NAME}", "RefID", primaryTable);
			Assert.AreEqual(999, provider.ExecuteScalar(sql));

			// удаляем таблицы
			provider.RemoveTable(primaryTable);
			provider.RemoveTable(refTable);
		}

		#endregion

		[Test]
		public void CanAddPrimaryKey()
		{
			string tableName = GetRandomName("AddPrimaryKey");
			string pkName = GetRandomName("PK_AddPrimaryKey");

			provider.AddTable(tableName,
				new Column("ID1", DbType.Int32, ColumnProperty.NotNull),
				new Column("ID2", DbType.Int32, ColumnProperty.NotNull));

			provider.AddPrimaryKey(pkName, tableName, "ID1", "ID2");

			provider.Insert(tableName, new[] { "ID1", "ID2" }, new[] { "1", "2" });
			provider.Insert(tableName, new[] { "ID1", "ID2" }, new[] { "2", "2" });

			Assert.Throws<SQLException>(() =>
				provider.Insert(tableName, new[] { "ID1", "ID2" }, new[] { "1", "2" }));

			provider.RemoveTable(tableName);
		}

		[Test]
		public void CanCheckThatPrimaryKeyIsExist()
		{
			string tableName = GetRandomName("CheckThatPrimaryKeyIsExist");
			string pkName = GetRandomName("PK_CheckThatPrimaryKeyIsExist");

			provider.AddTable(tableName, new Column("ID", DbType.Int32, ColumnProperty.NotNull));
			Assert.IsFalse(provider.ConstraintExists(tableName, pkName));

			provider.AddPrimaryKey(pkName, tableName, "ID");
			Assert.IsTrue(provider.ConstraintExists(tableName, pkName));

			provider.RemoveConstraint(tableName, pkName);
			Assert.IsFalse(provider.ConstraintExists(tableName, pkName));

			provider.RemoveTable(tableName);
		}

		[Test]
		public void CanAddComplexUniqueConstraint()
		{
			string tableName = GetRandomName("AddComplexUniqueConstraint");
			string ucName = GetRandomName("UC_AddComplexUniqueConstraint");

			provider.AddTable(tableName,
				new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
				new Column("TestStringColumn1", DbType.String.WithSize(20)),
				new Column("TestStringColumn2", DbType.String.WithSize(20)));
			Assert.IsFalse(provider.ConstraintExists(tableName, ucName));

			provider.AddUniqueConstraint(ucName, tableName, "TestStringColumn1", "TestStringColumn2");

			// пробуем нарушить ограничения
			provider.Insert(tableName, 
				new[] { "ID", "TestStringColumn1", "TestStringColumn2" }, 
				new[] { "1", "xxx", "abc" });

			provider.Insert(tableName, 
				new[] { "ID", "TestStringColumn1", "TestStringColumn2" }, 
				new[] { "2", "111", "abc" });

			provider.Insert(tableName, 
				new[] { "ID", "TestStringColumn1", "TestStringColumn2" }, 
				new[] { "3", "xxx", "222" });

			Assert.Throws<SQLException>(() =>
				provider.Insert(tableName,
					new[] { "ID", "TestStringColumn1", "TestStringColumn2" },
					new[] { "4", "xxx", "abc" }));

			provider.RemoveTable(tableName);
		}

		[Test]
		public void CanCheckThatUniqueConstraintIsExist()
		{
			string tableName = GetRandomName("AddUniqueConstraint");
			string ucName = GetRandomName("UK_AddUniqueConstraint");

			provider.AddTable(tableName,
				new Column("ID1", DbType.Int32, ColumnProperty.PrimaryKey),
				new Column("TestStringColumn", DbType.String.WithSize(20)));
			Assert.IsFalse(provider.ConstraintExists(tableName, ucName));

			// добавляем UC и проверяем его существование
			provider.AddUniqueConstraint(ucName, tableName, "TestStringColumn");
			Assert.IsTrue(provider.ConstraintExists(tableName, ucName));

			provider.RemoveConstraint(tableName, ucName);
			Assert.IsFalse(provider.ConstraintExists(tableName, ucName));

			provider.RemoveTable(tableName);
		}
		

		#endregion

		#region index

		[Test]
		public void CanAddAndRemoveIndex()
		{
			string tableName = GetRandomName("AddIndex");

			provider.AddTable(tableName,
				new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
				new Column("Name", DbType.String.WithSize(10)));

			provider.AddIndex("ix_moo", false, tableName, new[] { "Name" });
			Assert.IsTrue(provider.IndexExists("ix_moo", tableName));

			provider.Insert(tableName, new[] { "ID", "Name" }, new[] { "1", "test-name" });
			provider.Insert(tableName, new[] { "ID", "Name" }, new[] { "2", "test-name" });

			provider.RemoveIndex("ix_moo", tableName);
			Assert.IsFalse(provider.IndexExists("ix_moo", tableName));

			provider.RemoveTable(tableName);
		}

		[Test]
		public void CanAddAndRemoveUniqueIndex()
		{
			string tableName = GetRandomName("AddUniqueIndex");

			provider.AddTable(tableName,
				new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
				new Column("Name", DbType.String.WithSize(10)));

			provider.AddIndex("ix_moo", true, tableName, new[] { "Name" });
			Assert.IsTrue(provider.IndexExists("ix_moo", tableName));

			provider.Insert(tableName, new[] { "ID", "Name" }, new[] { "1", "test-name" });
			Assert.Throws<SQLException>(() =>
				provider.Insert(tableName, new[] { "ID", "Name" }, new[] { "2", "test-name" }));

			provider.RemoveIndex("ix_moo", tableName);
			Assert.IsFalse(provider.IndexExists("ix_moo", tableName));

			provider.RemoveTable(tableName);
		}

		[Test]
		public void CanAddAndRemoveComplexIndex()
		{
			string tableName = GetRandomName("AddComplexIndex");

			provider.AddTable(tableName,
				new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
				new Column("Name1", DbType.String.WithSize(20)),
				new Column("Name2", DbType.String.WithSize(20)));

			provider.AddIndex("ix_moo", true, tableName, new[] { "Name1", "Name2" });
			Assert.IsTrue(provider.IndexExists("ix_moo", tableName));

			provider.Insert(tableName, new[] { "ID", "Name1", "Name2" }, new[] { "1", "test-name", "xxx" });
			provider.Insert(tableName, new[] { "ID", "Name1", "Name2" }, new[] { "2", "test-name-2", "xxx" });
			provider.Insert(tableName, new[] { "ID", "Name1", "Name2" }, new[] { "3", "test-name", "zzz" });
			Assert.Throws<SQLException>(() =>
				provider.Insert(tableName, new[] { "ID", "Name1", "Name2" }, new[] { "4", "test-name", "xxx" }));


			provider.RemoveIndex("ix_moo", tableName);
			Assert.IsFalse(provider.IndexExists("ix_moo", tableName));

			provider.RemoveTable(tableName);
		}

		#endregion


		#endregion

		#region helpers

		protected string GetRandomName(string baseName = "")
		{
			string guid = Guid.NewGuid().ToString().Replace("-", string.Empty).ToLower();
			return "{0}{1}".FormatWith(baseName, guid);
		}

		#endregion
	}
}
