using ECM7.Migrator.Exceptions;
using System;
using System.Linq;
using System.Data;
using ECM7.Migrator.Framework;
using ECM7.Migrator.Providers.MySql;
using NUnit.Framework;

namespace ECM7.Migrator.Providers.Tests.ImplementationTest.NoSchema
{
	[TestFixture]
	public class MySqlTransformationProviderTest
		: TransformationProviderTestBase<MySqlTransformationProvider>
	{
		#region Overrides of TransformationProviderTestBase<MySqlTransformationProvider>

		public override string ConnectionStrinSettingsName
		{
			get { return "MySqlConnectionString"; }
		}

		protected override string BatchSql
		{
			get
			{
				return @"
				insert into `BatchSqlTest` (`Id`, `TestId`) values (11, 111);
				insert into `BatchSqlTest` (`Id`, `TestId`) values (22, 222);
				insert into `BatchSqlTest` (`Id`, `TestId`) values (33, 333);
				insert into `BatchSqlTest` (`Id`, `TestId`) values (44, 444);
				insert into `BatchSqlTest` (`Id`, `TestId`) values (55, 555);";
			}
		}

		#endregion

		#region override tests


		[Test]
		public override void CanVerifyThatCheckConstraintIsExist()
		{
			// todo: пройтись по всем тестам с NotSupportedException и проверить необходимость выдачи исключения
			Assert.Throws<NotSupportedException>(base.CanVerifyThatCheckConstraintIsExist);
		}

		[Test]
		public override void CanAddCheckConstraint()
		{
			Assert.Throws<NotSupportedException>(base.CanAddCheckConstraint);
		}

		[Test]
		public override void CanRenameColumn()
		{
			Assert.Throws<NotSupportedException>(base.CanRenameColumn);
		}

		[Test]
		public override void CanAddForeignKeyWithDeleteSetDefault()
		{
			Assert.Throws<NotSupportedException>(base.CanAddForeignKeyWithDeleteSetDefault);
		}

		[Test]
		public override void CanAddForeignKeyWithUpdateSetDefault()
		{
			Assert.Throws<NotSupportedException>(base.CanAddForeignKeyWithUpdateSetDefault);
		}

		[Test]
		public void AddTableWithMyISAMEngine()
		{
			string tableName = GetRandomName("MyISAMTable");

			Assert.IsFalse(provider.TableExists(tableName));

			provider.AddTable(tableName, "MyISAM", new Column("ID", DbType.Int32));

			Assert.IsTrue(provider.TableExists(tableName));

			string sql = provider.FormatSql("SELECT ENGINE FROM `information_schema`.`TABLES` WHERE `TABLE_NAME` = '{0}'", tableName);
			object engine = provider.ExecuteScalar(sql);
			Assert.AreEqual("MyISAM", engine);

			provider.RemoveTable(tableName);

			Assert.IsFalse(provider.TableExists(tableName));
		}

		[Test]
		public override void CanAddAndDropTable()
		{
			// в стандартный тест добавлена проверка выбранной подсистемы низкого уровня MySQL (по умолчанию InnoDB)
			SchemaQualifiedObjectName tableName = GetRandomTableName("InnoDBTable");

			Assert.IsFalse(provider.TableExists(tableName));

			provider.AddTable(tableName, new Column("ID", DbType.Int32));
			Assert.IsTrue(provider.TableExists(tableName));

			string sql = provider.FormatSql("SELECT ENGINE FROM `information_schema`.`TABLES` WHERE `TABLE_NAME` = '{0}'", tableName);
			object engine = provider.ExecuteScalar(sql);
			Assert.AreEqual("InnoDB", engine);


			provider.RemoveTable(tableName);
			Assert.IsFalse(provider.TableExists(tableName));
		}

		/// <summary>
		/// MySql возвращает имена таблиц в нижнем регистре, поэтому в стандартный тест 
		/// добавлено сравнение имен таблиц без учета регистра
		/// </summary>
		[Test]
		public override void CanGetTables()
		{
			var table1 = GetRandomTableName("tableMoo");
			var table2 = GetRandomTableName("tableHru");

			var tables = provider.GetTables(DefaultSchema);
			Assert.IsFalse(tables.Contains(table1));
			Assert.IsFalse(tables.Contains(table2));

			provider.AddTable(table1, new Column("ID", DbType.Int32));
			provider.AddTable(table2, new Column("ID", DbType.Int32));

			var tables2 = provider.GetTables(DefaultSchema);


			Assert.AreEqual(tables.Length + 2, tables2.Length);
			Assert.IsTrue(tables.All(tab => tab.Schema == DefaultSchema));
			Assert.IsTrue(tables2.Select(tab => tab.Name).Contains(table1.Name, StringComparer.InvariantCultureIgnoreCase));
			Assert.IsTrue(tables2.Select(tab => tab.Name).Contains(table2.Name, StringComparer.InvariantCultureIgnoreCase));

			provider.RemoveTable(table1);
			provider.RemoveTable(table2);
		}

		#region primary key

		// исправление проверки наличия первичного ключа
		// т.к. в MySQL первичный ключ всегда называется "PRIMARY"
		[Test]
		public override void CanAddTableWithCompoundPrimaryKey()
		{
			// в отличие от стандартного теста, сравнение имен ключей происходит без учета регистра
			string tableName = GetRandomName("TableWCPK");

			provider.AddTable(tableName,
				new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
				new Column("ID2", DbType.Int32, ColumnProperty.PrimaryKey)
			);

			Assert.IsTrue(provider.ConstraintExists(tableName, "PRIMARY"));

			provider.Insert(tableName, new[] { "ID", "ID2" }, new[] { "5", "6" });

			Assert.Throws<SQLException>(() =>
				provider.Insert(tableName, new[] { "ID", "ID2" }, new[] { "5", "6" }));

			provider.RemoveTable(tableName);
		}

		[Test]
		public override void CanCheckThatPrimaryKeyIsExist()
		{
			string tableName = GetRandomName("CheckThatPrimaryKeyIsExist");
			string pkName = GetRandomName("PK_CheckThatPrimaryKeyIsExist");

			provider.AddTable(tableName, new Column("ID", DbType.Int32, ColumnProperty.NotNull));
			Assert.IsFalse(provider.ConstraintExists(tableName, pkName));

			provider.AddPrimaryKey(pkName, tableName, "ID");
			Assert.IsTrue(provider.ConstraintExists(tableName, "PRIMARY"));

			provider.RemoveConstraint(tableName, "PRIMARY");
			Assert.IsFalse(provider.ConstraintExists(tableName, "PRIMARY"));

			provider.RemoveTable(tableName);
		}

		#endregion		
		
		#endregion
	}
}
