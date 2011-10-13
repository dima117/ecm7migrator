using ECM7.Migrator.Exceptions;

namespace ECM7.Migrator.Providers.Tests.ImplementationTest
{
	using System;
	using System.Data;

	using ECM7.Migrator.Framework;
	using ECM7.Migrator.Providers.MySql;

	using NUnit.Framework;

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
			Assert.Throws<NotSupportedException>(() =>
				base.CanVerifyThatCheckConstraintIsExist());
		}

		[Test]
		public override void CanAddCheckConstraint()
		{
			Assert.Throws<NotSupportedException>(() =>
				base.CanAddCheckConstraint());
		}

		[Test]
		public override void CanRenameColumn()
		{
			Assert.Throws<NotSupportedException>(() =>
				base.CanRenameColumn());
		}

		[Test]
		public override void CanAddForeignKeyWithDeleteSetDefault()
		{
			Assert.Throws<NotSupportedException>(() =>
				base.CanAddForeignKeyWithDeleteSetDefault());
		}

		[Test]
		public override void CanAddForeignKeyWithUpdateSetDefault()
		{
			Assert.Throws<NotSupportedException>(() =>
				base.CanAddForeignKeyWithUpdateSetDefault());
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
			string tableName = this.GetRandomName("InnoDBTable");

			Assert.IsFalse(provider.TableExists(tableName));

			provider.AddTable(tableName, new Column("ID", DbType.Int32));
			Assert.IsTrue(provider.TableExists(tableName));

			string sql = provider.FormatSql("SELECT ENGINE FROM `information_schema`.`TABLES` WHERE `TABLE_NAME` = '{0}'", tableName);
			object engine = provider.ExecuteScalar(sql);
			Assert.AreEqual("InnoDB", engine);


			provider.RemoveTable(tableName);
			Assert.IsFalse(provider.TableExists(tableName));
		}

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

		#endregion
	}
}
