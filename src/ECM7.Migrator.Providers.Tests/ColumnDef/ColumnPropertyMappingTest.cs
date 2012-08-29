namespace ECM7.Migrator.Providers.Tests.ColumnDef
{
	using System.Configuration;

	using Framework;
	using Providers;

	using System.Data;

	using NUnit.Framework;

	[TestFixture]
	public abstract class ColumnPropertyMappingTest<TProvider>
		where TProvider : TransformationProvider
	{
		public abstract string CStringName { get; }

		#region Helpers

		private TProvider CreateProvider()
		{
			string cstring = ConfigurationManager.AppSettings[CStringName];
			return ProviderFactory.Create<TProvider>(cstring, null) as TProvider;
		}

		#endregion

		#region Test SQL

		/// <summary>
		/// new Column("Foo", DbType.String.WithSize(30))
		/// </summary>
		public abstract string SimpleColumnSql { get; }

		/// <summary>
		/// new Column("Moo", DbType.Decimal.WithSize(10, 4), ColumnProperty.NotNull | ColumnProperty.Unique, 124)
		/// </summary>
		public abstract string FullColumnSql { get; }

		/// <summary>
		/// new Column("Bar", DbType.Int64, ColumnProperty.PrimaryKey)
		/// </summary>
		public abstract string ColumnSqlWithPrimaryKey { get; }

		/// <summary>
		/// new Column("Boo", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity)
		/// </summary>
		public abstract string ColumnSqlWithPrimaryKeyAndIdentity { get; }

		/// <summary>
		/// new Column("Mimimi", DbType.Int16, ColumnProperty.PrimaryKey)
		/// </summary>
		public abstract string ColumnSqlWithCompoundPrimaryKey { get; }

		/// <summary>
		/// new Column("Xxx", DbType.Boolean, ColumnProperty.Null, true)
		/// </summary>
		public abstract string BooleanColumnSqlWithDefaultValueSql { get; }



		#endregion

		//[Test]
		//public void AddUniqueColumn()
		//{
		//    provider.AddColumn("TestTwo", new Column("Test", DbType.String, 50, ColumnProperty.Unique));
		//}

		[Test]
		public void CanCreatesSimpleColumnSql()
		{
			using (var provider = CreateProvider())
			{
				Column column = new Column("Foo", DbType.String.WithSize(30));
				string columnSql = provider.GetSqlColumnDef(column, false);

				Assert.AreEqual(SimpleColumnSql, columnSql);
			}
		}

		[Test]
		public void CanCreatesFullColumnSql()
		{
			using (var provider = CreateProvider())
			{
				Column column = new Column("Moo", DbType.Decimal.WithSize(10, 4), ColumnProperty.NotNull | ColumnProperty.Unique, 124);
				string columnSql = provider.GetSqlColumnDef(column, false);

				Assert.AreEqual(FullColumnSql, columnSql);
			}
		}
		
		[Test]
		public void CanCreatesColumnSqlWithPrimaryKey()
		{
			using (var provider = CreateProvider())
			{
				Column column = new Column("Bar", DbType.Int64, ColumnProperty.PrimaryKey);
				string columnSql = provider.GetSqlColumnDef(column, false);

				Assert.AreEqual(ColumnSqlWithPrimaryKey, columnSql);
			}
		}

		[Test]
		public void CanCreatesColumnSqlWithPrimaryKeyAndIdentity()
		{
			using (var provider = CreateProvider())
			{
				Column column = new Column("Boo", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity);
				string columnSql = provider.GetSqlColumnDef(column, false);

				Assert.AreEqual(ColumnSqlWithPrimaryKeyAndIdentity, columnSql);
			}
		}

		[Test]
		public void CanCreatesColumnSqlWithCompoundPrimaryKey()
		{
			using (var provider = CreateProvider())
			{
				Column column = new Column("Mimimi", DbType.Int16, ColumnProperty.PrimaryKey);
				string columnSql = provider.GetSqlColumnDef(column, true);

				Assert.AreEqual(ColumnSqlWithCompoundPrimaryKey, columnSql);
			}
		}

		[Test]
		public void CanCreatesBooleanColumnSqlWithDefaultValue()
		{
			using (var provider = CreateProvider())
			{
				Column column = new Column("Xxx", DbType.Boolean, ColumnProperty.Null, true);
				string columnSql = provider.GetSqlColumnDef(column, false);

				Assert.AreEqual(BooleanColumnSqlWithDefaultValueSql, columnSql);
			}
		}
	}
}