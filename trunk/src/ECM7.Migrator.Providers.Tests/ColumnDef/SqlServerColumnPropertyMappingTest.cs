using ECM7.Migrator.Providers.SqlServer;
using NUnit.Framework;

namespace ECM7.Migrator.Providers.Tests.ColumnDef
{
	[TestFixture]
	public class SqlServerColumnPropertyMappingTest
		: ColumnPropertyMappingTest<SqlServerTransformationProvider>
	{
		public override string CStringName
		{
			get { return "SqlServerConnectionString"; }
		}

		#region Тестовые фрагменты SQL

		/// <summary>
		/// new Column("Foo", DbType.String.WithSize(30))
		/// </summary>
		public override string SimpleColumnSql
		{
			get { return "[Foo] NVARCHAR(30)"; }
		}

		/// <summary>
		/// new Column("Moo", DbType.Decimal.WithSize(10, 4), ColumnProperty.NotNull | ColumnProperty.Unique, 124)
		/// </summary>
		public override string FullColumnSql
		{
			get { return "[Moo] DECIMAL(10, 4) NOT NULL UNIQUE DEFAULT 124"; }
		}

		/// <summary>
		/// new Column("Bar", DbType.Int64, ColumnProperty.PrimaryKey)
		/// </summary>
		public override string ColumnSqlWithPrimaryKey
		{
			get { return "[Bar] BIGINT NOT NULL PRIMARY KEY"; }
		}

		/// <summary>
		/// new Column("Boo", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity)
		/// </summary>
		public override string ColumnSqlWithPrimaryKeyAndIdentity
		{
			get { return "[Boo] INT NOT NULL PRIMARY KEY IDENTITY"; }
		}

		/// <summary>
		/// new Column("Mimimi", DbType.Int16, ColumnProperty.PrimaryKey)
		/// </summary>
		public override string ColumnSqlWithCompoundPrimaryKey
		{
			get { return "[Mimimi] SMALLINT NOT NULL"; }
		}

		/// <summary>
		/// new Column("Xxx", DbType.Boolean, ColumnProperty.Null, true)
		/// </summary>
		public override string BooleanColumnSqlWithDefaultValueSql
		{
			get { return "[Xxx] BIT DEFAULT 1"; }
		}

		#endregion
	}
}
