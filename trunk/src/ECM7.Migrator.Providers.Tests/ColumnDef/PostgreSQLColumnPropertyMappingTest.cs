namespace ECM7.Migrator.Providers.Tests.ColumnDef
{
	using System;

	using ECM7.Migrator.Providers.PostgreSQL;

	using Npgsql;

	using NUnit.Framework;

	[TestFixture]
	public class PostgreSQLColumnPropertyMappingTest
		: ColumnPropertyMappingTest<PostgreSQLTransformationProvider, NpgsqlConnection>
	{
		#region Overrides of ColumnPropertyMappingTest<PostgreSQLTransformationProvider,NpgsqlConnection>

		public override string CStringName
		{
			get { return "NpgsqlConnectionString"; }
		}

		/// <summary>
		/// new Column("Foo", DbType.String.WithSize(30))
		/// </summary>
		public override string SimpleColumnSql
		{
			get { return "\"Foo\" varchar(30)"; }
		}

		/// <summary>
		/// new Column("Moo", DbType.Decimal.WithSize(10, 4), ColumnProperty.NotNull | ColumnProperty.Unique, 124)
		/// </summary>
		public override string FullColumnSql
		{
			get { return "\"Moo\" decimal(18, 10) NOT NULL UNIQUE DEFAULT 124"; }
		}

		/// <summary>
		/// new Column("Bar", DbType.Int64, ColumnProperty.PrimaryKey)
		/// </summary>
		public override string ColumnSqlWithPrimaryKey
		{
			get { return "\"Bar\" int8 NOT NULL PRIMARY KEY"; }
		}

		/// <summary>
		/// new Column("Boo", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity)
		/// </summary>
		public override string ColumnSqlWithPrimaryKeyAndIdentity
		{
			get { return "\"Boo\" serial NOT NULL PRIMARY KEY"; }
		}

		public override string ColumnSqlWithCompoundPrimaryKey
		{
			get { return "\"Mimimi\" int2 NOT NULL"; }
		}

		public override string BooleanColumnSqlWithDefaultValueSql
		{
			get { return "\"Xxx\" boolean DEFAULT True"; }
		}

		#endregion
	}
}
