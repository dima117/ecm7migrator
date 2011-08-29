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
			const string TABLE_NAME = "MooTable45844f8a2a794b75ac8c80adbe0d20da";

			Assert.IsFalse(provider.TableExists(TABLE_NAME));

			provider.AddTable(TABLE_NAME, new Column("ID", DbType.Int32));

			Assert.IsTrue(provider.TableExists(TABLE_NAME));

			provider.RemoveTable(TABLE_NAME);

			Assert.IsFalse(provider.TableExists(TABLE_NAME));
		}

		[Test]
		public void CanCreateTableWithNecessaryCols()
		{
			const string TABLE_NAME = "Mimimi639257bbbfcc46b9b13e8c585945809e";

			provider.AddTable(TABLE_NAME,
				new Column("ID", DbType.Int32),
				new Column("StringColumn", DbType.String.WithSize(500)),
				new Column("DecimalColumn", DbType.Decimal.WithSize(18, 2))
			);

			provider.Insert(
				TABLE_NAME,
				new[] { "ID", "StringColumn", "DecimalColumn" },
				new[] { "1984", "test moo", "123.56789" }
			);


			string sql = provider.FormatSql("select * from {0:NAME}", TABLE_NAME);
			using (var reader = provider.ExecuteReader(sql))
			{
				Assert.IsTrue(reader.Read());
				Assert.AreEqual(1984, reader.GetInt32(0));
				Assert.AreEqual("test moo", reader.GetString(1));
				Assert.AreEqual(123.57m, reader.GetDecimal(2));
				Assert.IsFalse(reader.Read());
			}

			provider.RemoveTable(TABLE_NAME);
		}

		[Test]
		public void CanAddTableWithCompoundPrimaryKey()
		{
			const string TABLE_NAME = "TableWCPK63c36e04c1ca4c6d924cb9d4167b75a9";

			provider.AddTable(TABLE_NAME,
				new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
				new Column("ID2", DbType.Int32, ColumnProperty.PrimaryKey)
			);

			Assert.IsTrue(provider.ConstraintExists(TABLE_NAME, "PK_" + TABLE_NAME));

			provider.Insert(TABLE_NAME, new[] { "ID", "ID2" }, new[] { "5", "6" });

			Assert.Throws<SQLException>(() =>
				provider.Insert(TABLE_NAME, new[] { "ID", "ID2" }, new[] { "5", "6" }));

			provider.RemoveTable(TABLE_NAME);
		}

		[Test]
		public void CanRenameTable()
		{
			const string TABLE1 = "tableMoo7784ec9fbb864355ad8d76e5f1fd1313";
			const string TABLE2 = "tableHru64fadbfa2368462bb417e94ca6a9fcd7";

			Assert.IsFalse(provider.TableExists(TABLE1));
			Assert.IsFalse(provider.TableExists(TABLE2));

			provider.AddTable(TABLE1, new Column("ID", DbType.Int32));
			provider.RenameTable(TABLE1, TABLE2);

			Assert.IsTrue(provider.TableExists(TABLE2));

			provider.RemoveTable(TABLE2);
		}

		[Test]
		public void CantRemoveUnexistingTable()
		{
			Assert.Throws<SQLException>(() => provider.RemoveTable("1929e6db3b4f43dbba0fb6530b822068"));
		}

		[Test]
		public void CanGetTables()
		{
			const string TABLE1 = "tableMoo01f352bfffb9498f8c60660b9107814d";
			const string TABLE2 = "tableHruc526fc9c3d6a4ea4880b4430ca3c6b0d";

			var tables = provider.GetTables();
			Assert.AreEqual(0, tables.Length);

			provider.AddTable(TABLE1, new Column("ID", DbType.Int32));
			provider.AddTable(TABLE2, new Column("ID", DbType.Int32));

			var tables2 = provider.GetTables();

			Assert.AreEqual(2, tables2.Length);
			Assert.IsTrue(tables2.Contains(TABLE1));
			Assert.IsTrue(tables2.Contains(TABLE2));

			provider.RemoveTable(TABLE1);
			provider.RemoveTable(TABLE2);
		}

		#endregion

		#region columns

		[Test]
		public void CanAddColumn()
		{
			const string TABLE_NAME = "AddColumnTest238b7423cd4f49b8a0bd8d93f432a4ec";

			provider.AddTable(TABLE_NAME, new Column("ID", DbType.Int32));

			provider.AddColumn(TABLE_NAME, new Column("TestStringColumn", DbType.String.WithSize(7)));

			provider.Insert(TABLE_NAME, new[] { "ID", "TestStringColumn" }, new[] { "2", "test" });
			provider.Insert(TABLE_NAME, new[] { "ID", "TestStringColumn" }, new[] { "4", "testmoo" });

			Assert.Throws<SQLException>(() =>
				provider.Insert(TABLE_NAME, new[] { "ID", "TestStringColumn" }, new[] { "6", "testmoo1" }));

			string sql = provider.FormatSql("select * from {0:NAME}", TABLE_NAME);
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

			provider.RemoveTable(TABLE_NAME);
		}

		[Test]
		public void CanChangeColumn()
		{
			const string TABLE_NAME = "ChangeColumnTest10b19f64359941099ccb424d0adfa686";

			provider.AddTable(TABLE_NAME, new Column("TestStringColumn", DbType.String.WithSize(4)));

			provider.Insert(TABLE_NAME, new[] { "TestStringColumn" }, new[] { "moo" });
			provider.Insert(TABLE_NAME, new[] { "TestStringColumn" }, new[] { "test" });

			Assert.Throws<SQLException>(() =>
				provider.Insert(TABLE_NAME, new[] { "TestStringColumn" }, new[] { "moo-test" }));

			provider.ChangeColumn(TABLE_NAME, new Column("TestStringColumn", DbType.String.WithSize(18)));
			provider.Insert(TABLE_NAME, new[] { "TestStringColumn" }, new[] { "moo-test" });

			string sql = provider.FormatSql(
				"select count({0:NAME}) from {1:NAME} where {0:NAME} = '{2}'",
				"TestStringColumn", TABLE_NAME, "moo-test");

			Assert.AreEqual(1, provider.ExecuteScalar(sql));
			provider.RemoveTable(TABLE_NAME);
		}

		[Test]
		public void CanRenameColumn()
		{
			const string TABLE_NAME = "RenameColumnTest7fd566e427c74657a9ee3fede59bdb37";

			provider.AddTable(TABLE_NAME, new Column("TestColumn1", DbType.Int32));
			provider.RenameColumn(TABLE_NAME, "TestColumn1", "TestColumn2");

			Assert.IsFalse(provider.ColumnExists(TABLE_NAME, "TestColumn1"));
			Assert.IsTrue(provider.ColumnExists(TABLE_NAME, "TestColumn2"));

			provider.RemoveTable(TABLE_NAME);
		}

		[Test]
		public void CanRemoveColumn()
		{
			const string TABLE_NAME = "RemoveColumnTest7172daac54f34fb9be1fed5718d3bac6";

			provider.AddTable(TABLE_NAME,
				new Column("ID", DbType.Int32),
				new Column("TestColumn1", DbType.Int32));

			provider.RemoveColumn(TABLE_NAME, "TestColumn1");

			Assert.IsFalse(provider.ColumnExists(TABLE_NAME, "TestColumn1"));

			provider.RemoveTable(TABLE_NAME);
		}

		[Test]
		public void CantRemoveUnexistingColumn()
		{
			const string TABLE_NAME = "RemoveUnexistingColumn75124a19c5724c209189480644e3c7cb";

			provider.AddTable(TABLE_NAME, new Column("ID", DbType.Int32));

			Assert.Throws<SQLException>(() =>
				provider.RemoveColumn(TABLE_NAME, "9d41bdb2b6ae4e1abcf656c5681b3763"));

			provider.RemoveTable(TABLE_NAME);
		}

		#endregion

		#region constraints

		#region foreign key

		[Test]
		public void CanAddForeignKey()
		{
			// создаем таблицы и добавляем внешний ключ
			const string PRIMARY_TABLE = "AddForeignKey_Primaryf1c55eda62994cc38bc90b6282430a65";
			const string REF_TABLE = "AddForeignKey_Ref606c825dfc0d4117b1f905550adcbec5";
			const string FOREIGN_KEY_NAME = "FK_TestSimpleKeyd6bcf94ba1f246fc9c591f39e02b8ed4";

			provider.AddTable(REF_TABLE, new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey));
			provider.Insert(REF_TABLE, "ID".AsArray(), "17".AsArray());

			provider.AddTable(PRIMARY_TABLE,
				new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
				new Column("RefID", DbType.Int32));
			provider.AddForeignKey(FOREIGN_KEY_NAME, PRIMARY_TABLE, "RefID", REF_TABLE, "ID");

			// пробуем нарушить ограничения внешнего ключа
			Assert.Throws<SQLException>(() =>
				provider.Insert(PRIMARY_TABLE, new[] { "ID", "RefID" }, new[] { "1", "111" }));
			provider.Insert(PRIMARY_TABLE, new[] { "ID", "RefID" }, new[] { "1", "17" });
			Assert.Throws<SQLException>(() => provider.Delete(REF_TABLE));

			// удаляем таблицы
			provider.RemoveTable(PRIMARY_TABLE);
			provider.RemoveTable(REF_TABLE);
		}

		[Test]
		public void CanAddComplexForeignKey()
		{
			// создаем таблицы и добавляем внешний ключ
			const string PRIMARY_TABLE = "AddForeignKey_Primary9e205ec9f984453bb424f77b1a2db000";
			const string REF_TABLE = "AddForeignKey_Ref26f8aff53f6c4b2994e28ac981f56c59";
			const string FOREIGN_KEY_NAME = "FK_TestComplexKeyabd758075a754c0088dcefa60a88a107";

			provider.AddTable(REF_TABLE,
				new Column("ID1", DbType.Int32, ColumnProperty.PrimaryKey),
				new Column("ID2", DbType.Int32, ColumnProperty.PrimaryKey));

			provider.Insert(REF_TABLE, new[] { "ID1", "ID2" }, new[] { "111", "222" });

			provider.AddTable(PRIMARY_TABLE,
				new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
				new Column("RefID1", DbType.Int32),
				new Column("RefID2", DbType.Int32));

			provider.AddForeignKey(FOREIGN_KEY_NAME,
				PRIMARY_TABLE, new[] { "RefID1", "RefID2" },
				REF_TABLE, new[] { "ID1", "ID2" });

			// пробуем нарушить ограничения внешнего ключа
			Assert.Throws<SQLException>(() =>
				provider.Insert(PRIMARY_TABLE, new[] { "ID", "RefID1", "RefID2" }, new[] { "1", "123", "456" }));

			Assert.Throws<SQLException>(() =>
				provider.Insert(PRIMARY_TABLE, new[] { "ID", "RefID1", "RefID2" }, new[] { "1", "111", "456" }));

			Assert.Throws<SQLException>(() =>
				provider.Insert(PRIMARY_TABLE, new[] { "ID", "RefID1", "RefID2" }, new[] { "1", "123", "222" }));

			provider.Insert(PRIMARY_TABLE, new[] { "ID", "RefID1", "RefID2" }, new[] { "1", "111", "222" });

			Assert.Throws<SQLException>(() => provider.Delete(REF_TABLE));

			// удаляем таблицы
			provider.RemoveTable(PRIMARY_TABLE);
			provider.RemoveTable(REF_TABLE);
		}

		[Test]
		public void CanAddForeignKeyWithDeleteCascade()
		{
			const string PRIMARY_TABLE = "AddForeignKey_Primarye1d7661e88764d41b1d1aea668842001";
			const string REF_TABLE = "AddForeignKey_Reface89f21336b47189420dadc3aea3a4b";
			const string FOREIGN_KEY_NAME = "FK_Test457a1e3993824b9daba30465de464459";

			provider.AddTable(REF_TABLE, new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey));
			provider.Insert(REF_TABLE, "ID".AsArray(), "177".AsArray());

			provider.AddTable(PRIMARY_TABLE,
				new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
				new Column("RefID", DbType.Int32));
			provider.AddForeignKey(FOREIGN_KEY_NAME, PRIMARY_TABLE, "RefID", REF_TABLE, "ID", ForeignKeyConstraint.Cascade);

			provider.Insert(PRIMARY_TABLE, new[] { "ID", "RefID" }, new[] { "1", "177" });
			provider.Delete(REF_TABLE);

			string sql = provider.FormatSql("select count(*) from {0:NAME}", PRIMARY_TABLE);
			Assert.AreEqual(0, provider.ExecuteScalar(sql));

			// удаляем таблицы
			provider.RemoveTable(PRIMARY_TABLE);
			provider.RemoveTable(REF_TABLE);
		}

		[Test]
		public void CanAddForeignKeyWithUpdateCascade()
		{
			const string PRIMARY_TABLE = "AddForeignKey_Primary030942db757c4ef6a12f01488b5a5a84";
			const string REF_TABLE = "AddForeignKey_Ref4d5e9b0d68ea42b28b9d3329b3638678";
			const string FOREIGN_KEY_NAME = "FK_Testb418aa8f282b42b893022d28a61feff6";

			provider.AddTable(REF_TABLE, new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey));
			provider.Insert(REF_TABLE, "ID".AsArray(), "654".AsArray());

			provider.AddTable(PRIMARY_TABLE,
				new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
				new Column("RefID", DbType.Int32));

			provider.AddForeignKey(FOREIGN_KEY_NAME,
				PRIMARY_TABLE, "RefID", REF_TABLE, "ID",
				ForeignKeyConstraint.NoAction, ForeignKeyConstraint.Cascade);

			provider.Insert(PRIMARY_TABLE, new[] { "ID", "RefID" }, new[] { "1", "654" });

			provider.Update(REF_TABLE, "ID".AsArray(), "777".AsArray());

			string sql = provider.FormatSql("select {0:NAME} from {1:NAME}", "RefID", PRIMARY_TABLE);
			Assert.AreEqual(777, provider.ExecuteScalar(sql));

			// удаляем таблицы
			provider.RemoveTable(PRIMARY_TABLE);
			provider.RemoveTable(REF_TABLE);
		}

		[Test]
		public void CanAddForeignKeyWithUpdateSetNull()
		{
			const string PRIMARY_TABLE = "AddForeignKey_Primary030942db757c4ef6a12f01488b5a5a84";
			const string REF_TABLE = "AddForeignKey_Ref4d5e9b0d68ea42b28b9d3329b3638678";
			const string FOREIGN_KEY_NAME = "FK_Testb418aa8f282b42b893022d28a61feff6";

			provider.AddTable(REF_TABLE, new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey));
			provider.Insert(REF_TABLE, "ID".AsArray(), "654".AsArray());

			provider.AddTable(PRIMARY_TABLE,
				new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
				new Column("RefID", DbType.Int32));

			provider.AddForeignKey(FOREIGN_KEY_NAME,
				PRIMARY_TABLE, "RefID", REF_TABLE, "ID",
				ForeignKeyConstraint.NoAction, ForeignKeyConstraint.SetNull);

			provider.Insert(PRIMARY_TABLE, new[] { "ID", "RefID" }, new[] { "1", "654" });

			provider.Update(REF_TABLE, "ID".AsArray(), "777".AsArray());

			string sql = provider.FormatSql("select {0:NAME} from {1:NAME}", "RefID", PRIMARY_TABLE);
			Assert.AreEqual(DBNull.Value, provider.ExecuteScalar(sql));

			// удаляем таблицы
			provider.RemoveTable(PRIMARY_TABLE);
			provider.RemoveTable(REF_TABLE);
		}

		[Test]
		public void CanAddForeignKeyWithUpdateSetDefault()
		{
			const string PRIMARY_TABLE = "AddForeignKey_Primary030942db757c4ef6a12f01488b5a5a84";
			const string REF_TABLE = "AddForeignKey_Ref4d5e9b0d68ea42b28b9d3329b3638678";
			const string FOREIGN_KEY_NAME = "FK_Testb418aa8f282b42b893022d28a61feff6";

			provider.AddTable(REF_TABLE, new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey));
			provider.Insert(REF_TABLE, "ID".AsArray(), "999".AsArray());
			provider.Insert(REF_TABLE, "ID".AsArray(), "654".AsArray());

			provider.AddTable(PRIMARY_TABLE,
				new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
				new Column("RefID", DbType.Int32, ColumnProperty.NotNull, 999));

			provider.AddForeignKey(FOREIGN_KEY_NAME,
				PRIMARY_TABLE, "RefID", REF_TABLE, "ID",
				ForeignKeyConstraint.NoAction, ForeignKeyConstraint.SetDefault);

			provider.Insert(PRIMARY_TABLE, new[] { "ID", "RefID" }, new[] { "1", "654" });

			string whereSql = provider.FormatSql("{0:NAME} = 654", "ID");
			provider.Update(REF_TABLE, "ID".AsArray(), "777".AsArray(), whereSql);

			string sql = provider.FormatSql("select {0:NAME} from {1:NAME}", "RefID", PRIMARY_TABLE);
			Assert.AreEqual(999, provider.ExecuteScalar(sql));

			// удаляем таблицы
			provider.RemoveTable(PRIMARY_TABLE);
			provider.RemoveTable(REF_TABLE);
		}

		#endregion

		[Test]
		public void CanAddPrimaryKey()
		{
			const string TABLE_NAME = "AddPrimaryKey2cc2d87087894b34af4f02add192fea9";
			const string PK_NAME = "PK_AddPrimaryKey2cc2d87087894b34af4f02add192fea9";

			provider.AddTable(TABLE_NAME,
				new Column("ID1", DbType.Int32, ColumnProperty.NotNull),
				new Column("ID2", DbType.Int32, ColumnProperty.NotNull));

			provider.AddPrimaryKey(PK_NAME, TABLE_NAME, "ID1", "ID2");

			provider.Insert(TABLE_NAME, new[] { "ID1", "ID2" }, new[] { "1", "2" });
			provider.Insert(TABLE_NAME, new[] { "ID1", "ID2" }, new[] { "2", "2" });

			Assert.Throws<SQLException>(() =>
				provider.Insert(TABLE_NAME, new[] { "ID1", "ID2" }, new[] { "1", "2" }));

			provider.RemoveTable(TABLE_NAME);
		}

		[Test]
		public void CanCheckThatPrimaryKeyIsExist()
		{
			const string TABLE_NAME = "CheckThatPrimaryKeyIsExist471f6847ea4c44d8a201b420a2a49926";
			const string PK_NAME = "PK_CheckThatPrimaryKeyIsExist471f6847ea4c44d8a201b420a2a49926";

			provider.AddTable(TABLE_NAME, new Column("ID", DbType.Int32, ColumnProperty.NotNull));
			Assert.IsFalse(provider.ConstraintExists(TABLE_NAME, PK_NAME));

			provider.AddPrimaryKey(PK_NAME, TABLE_NAME, "ID");
			Assert.IsTrue(provider.ConstraintExists(TABLE_NAME, PK_NAME));

			provider.RemoveConstraint(TABLE_NAME, PK_NAME);
			Assert.IsFalse(provider.ConstraintExists(TABLE_NAME, PK_NAME));

			provider.RemoveTable(TABLE_NAME);
		}

		#endregion

		#region index

		[Test]
		public void CanAddAndRemoveIndex()
		{
			const string TABLE_NAME = "AddIndex62c7e42ed3af4361b4636664e380d377";

			provider.AddTable(TABLE_NAME,
				new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
				new Column("Name", DbType.String.WithSize(10)));

			provider.AddIndex("ix_moo", false, TABLE_NAME, new[] { "Name" });
			Assert.IsTrue(provider.IndexExists("ix_moo", TABLE_NAME));

			provider.Insert(TABLE_NAME, new[] { "ID", "Name" }, new[] { "1", "test-name" });
			provider.Insert(TABLE_NAME, new[] { "ID", "Name" }, new[] { "2", "test-name" });

			provider.RemoveIndex("ix_moo", TABLE_NAME);
			Assert.IsFalse(provider.IndexExists("ix_moo", TABLE_NAME));

			provider.RemoveTable(TABLE_NAME);
		}

		[Test]
		public void CanAddAndRemoveUniqueIndex()
		{
			const string TABLE_NAME = "AddUniqueIndex76d66f21e9a545d4bfedd690b95bdc5d";

			provider.AddTable(TABLE_NAME,
				new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
				new Column("Name", DbType.String.WithSize(10)));

			provider.AddIndex("ix_moo", true, TABLE_NAME, new[] { "Name" });
			Assert.IsTrue(provider.IndexExists("ix_moo", TABLE_NAME));

			provider.Insert(TABLE_NAME, new[] { "ID", "Name" }, new[] { "1", "test-name" });
			Assert.Throws<SQLException>(() =>
				provider.Insert(TABLE_NAME, new[] { "ID", "Name" }, new[] { "2", "test-name" }));

			provider.RemoveIndex("ix_moo", TABLE_NAME);
			Assert.IsFalse(provider.IndexExists("ix_moo", TABLE_NAME));

			provider.RemoveTable(TABLE_NAME);
		}

		[Test]
		public void CanAddAndRemoveComplexIndex()
		{
			const string TABLE_NAME = "AddComplexIndex9732b8a809bf45d38f51506252b7ec70";

			provider.AddTable(TABLE_NAME,
				new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
				new Column("Name1", DbType.String.WithSize(20)),
				new Column("Name2", DbType.String.WithSize(20)));

			provider.AddIndex("ix_moo", true, TABLE_NAME, new[] { "Name1", "Name2" });
			Assert.IsTrue(provider.IndexExists("ix_moo", TABLE_NAME));

			provider.Insert(TABLE_NAME, new[] { "ID", "Name1", "Name2" }, new[] { "1", "test-name", "xxx" });
			provider.Insert(TABLE_NAME, new[] { "ID", "Name1", "Name2" }, new[] { "2", "test-name-2", "xxx" });
			provider.Insert(TABLE_NAME, new[] { "ID", "Name1", "Name2" }, new[] { "3", "test-name", "zzz" });
			Assert.Throws<SQLException>(() =>
				provider.Insert(TABLE_NAME, new[] { "ID", "Name1", "Name2" }, new[] { "4", "test-name", "xxx" }));


			provider.RemoveIndex("ix_moo", TABLE_NAME);
			Assert.IsFalse(provider.IndexExists("ix_moo", TABLE_NAME));

			provider.RemoveTable(TABLE_NAME);
		}

		#endregion


		#endregion
	}
}
