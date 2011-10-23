using System.Data;
using ECM7.Migrator.Framework;

namespace ECM7.Migrator.Providers.Tests.ImplementationTest
{
	using System;

	using ECM7.Migrator.Providers.SQLite;

	using NUnit.Framework;

	[TestFixture]
	public class SQLiteTransformationProviderTest
		: TransformationProviderTestBase<SQLiteTransformationProvider>
	{
		#region Overrides of TransformationProviderTestBase<SQLiteTransformationProvider>

		public override string ConnectionStrinSettingsName
		{
			get { return "SQLiteConnectionString"; }
		}

		protected override string BatchSql
		{
			get
			{
				return @"
				insert into [BatchSqlTest] ([Id], [TestId]) values (11, 111)
				GO
				insert into [BatchSqlTest] ([Id], [TestId]) values (22, 222)
				GO
				insert into [BatchSqlTest] ([Id], [TestId]) values (33, 333)
				GO
				insert into [BatchSqlTest] ([Id], [TestId]) values (44, 444)
				GO
				go
				insert into [BatchSqlTest] ([Id], [TestId]) values (55, 555)
				";
			}
		}

		#endregion

		#region foreign keys

		[Test]
		public override void CanAddForeignKey()
		{
			Assert.Throws<NotSupportedException>(base.CanAddForeignKey);
		}

		[Test]
		public override void CanAddForeignKeyWithDeleteCascade()
		{
			Assert.Throws<NotSupportedException>(base.CanAddForeignKeyWithDeleteCascade);
		}

		[Test]
		public override void CanAddForeignKeyWithDeleteSetDefault()
		{
			Assert.Throws<NotSupportedException>(base.CanAddForeignKeyWithDeleteSetDefault);
		}

		[Test]
		public override void CanAddForeignKeyWithDeleteSetNull()
		{
			Assert.Throws<NotSupportedException>(base.CanAddForeignKeyWithDeleteSetNull);
		}

		[Test]
		public override void CanAddForeignKeyWithUpdateCascade()
		{
			Assert.Throws<NotSupportedException>(base.CanAddForeignKeyWithUpdateCascade);
		}

		[Test]
		public override void CanAddForeignKeyWithUpdateSetDefault()
		{
			Assert.Throws<NotSupportedException>(base.CanAddForeignKeyWithUpdateSetDefault);
		}

		[Test]
		public override void CanAddForeignKeyWithUpdateSetNull()
		{
			Assert.Throws<NotSupportedException>(base.CanAddForeignKeyWithUpdateSetNull);
		}

		// check constraints
		[Test]
		public override void CanAddCheckConstraint()
		{
			Assert.Throws<NotSupportedException>(base.CanAddCheckConstraint);
		}

		[Test]
		public override void CanVerifyThatCheckConstraintIsExist()
		{
			Assert.Throws<NotSupportedException>(base.CanVerifyThatCheckConstraintIsExist);
		}

		[Test]
		public override void CanAddComplexForeignKey()
		{
			Assert.Throws<NotSupportedException>(base.CanAddComplexForeignKey);
		}

		[Test]
		public override void CanAddComplexUniqueConstraint()
		{
			Assert.Throws<NotSupportedException>(base.CanAddComplexUniqueConstraint);
		}

		[Test]
		public override void CanAddPrimaryKey()
		{
			Assert.Throws<NotSupportedException>(base.CanAddPrimaryKey);
		}

		[Test]
		public override void CanCheckThatPrimaryKeyIsExist()
		{
			Assert.Throws<NotSupportedException>(base.CanCheckThatPrimaryKeyIsExist);
		}
		
		[Test]
		public override void CanCheckThatUniqueConstraintIsExist()
		{
			Assert.Throws<NotSupportedException>(base.CanCheckThatUniqueConstraintIsExist);
		}

		#endregion

		#region change column

		[Test]
		public override void CanChangeColumnType()
		{
			Assert.Throws<NotSupportedException>(base.CanChangeColumnType);
		}

		[Test]
		public override void CanChangeDefaultValueForColumn()
		{
			Assert.Throws<NotSupportedException>(base.CanChangeDefaultValueForColumn);
		}

		[Test]
		public override void CanChangeNotNullProperty()
		{
			Assert.Throws<NotSupportedException>(base.CanChangeNotNullProperty);
		}

		[Test]
		public override void CanRemoveColumn()
		{
			Assert.Throws<NotSupportedException>(base.CanRemoveColumn);
		}

		[Test]
		public override void CanRenameColumn()
		{
			Assert.Throws<NotSupportedException>(base.CanRenameColumn);
		}

		[Test]
		public override void CanSetNotNullRepeatedly()
		{
			Assert.Throws<NotSupportedException>(base.CanSetNotNullRepeatedly);
		}

		[Test]
		public override void CantRemoveUnexistingColumn()
		{
			string tableName = GetRandomName("RemoveUnexistingColumn");

			provider.AddTable(tableName, new Column("ID", DbType.Int32));

			Assert.Throws<NotSupportedException>(() =>
				provider.RemoveColumn(tableName, GetRandomName()));

			provider.RemoveTable(tableName);
		}


		#endregion
	}
}
