namespace ECM7.Migrator.Providers.Tests.ImplementationTest
{
	using System.Configuration;
	using System.Data;
	using System.Linq;

	using ECM7.Migrator.Exceptions;
	using ECM7.Migrator.Framework;

	using log4net.Config;

	using NUnit.Framework;

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

			provider.AddTable(TABLE1, new Column("ID", DbType.Int32));
			provider.AddTable(TABLE2, new Column("ID", DbType.Int32));

			var tables = provider.GetTables();

			Assert.AreEqual(2, tables.Length);
			Assert.IsTrue(tables.Contains(TABLE1));
			Assert.IsTrue(tables.Contains(TABLE2));

			provider.RemoveTable(TABLE1);
			provider.RemoveTable(TABLE2);
		}

		#endregion

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
	}
}
