using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using ECM7.Migrator.Framework;
using Npgsql;

namespace ECM7.Migrator.Providers.PostgreSQL
{
	/// <summary>
	/// Migration transformations provider for PostgreSQL
	/// </summary>
	public class PostgreSQLTransformationProvider : TransformationProvider<NpgsqlConnection>
	{
		/// <summary>
		/// Инициализация
		/// </summary>
		/// <param name="connection"></param>
		public PostgreSQLTransformationProvider(NpgsqlConnection connection)
			: base(connection)
		{
			typeMap.Put(DbType.AnsiStringFixedLength, "char(255)");
			typeMap.Put(DbType.AnsiStringFixedLength, 8000, "char($l)");
			typeMap.Put(DbType.AnsiString, "varchar(255)");
			typeMap.Put(DbType.AnsiString, 8000, "varchar($l)");
			typeMap.Put(DbType.AnsiString, 2147483647, "text");
			typeMap.Put(DbType.Binary, "bytea");
			typeMap.Put(DbType.Binary, 2147483647, "bytea");
			typeMap.Put(DbType.Boolean, "boolean");
			typeMap.Put(DbType.Byte, "int2");
			typeMap.Put(DbType.Currency, "decimal(16,4)");
			typeMap.Put(DbType.Date, "date");
			typeMap.Put(DbType.DateTime, "timestamp");
			typeMap.Put(DbType.Decimal, "decimal(18,5)");
			typeMap.Put(DbType.Decimal, 18, "decimal($l, $s)");
			typeMap.Put(DbType.Double, "float8");
			typeMap.Put(DbType.Int16, "int2");
			typeMap.Put(DbType.Int32, "int4");
			typeMap.Put(DbType.Int64, "int8");
			typeMap.Put(DbType.Single, "float4");
			typeMap.Put(DbType.StringFixedLength, "char(255)");
			typeMap.Put(DbType.StringFixedLength, 4000, "char($l)");
			typeMap.Put(DbType.String, "varchar(255)");
			typeMap.Put(DbType.String, 4000, "varchar($l)");
			typeMap.Put(DbType.String, 1073741823, "text");
			typeMap.Put(DbType.Time, "time");

			propertyMap.RegisterPropertySql(ColumnProperty.Identity, "serial");
		}

		#region Особенности СУБД

		public override bool IdentityNeedsType
		{
			get { return false; }
		}

		public override string BatchSeparator
		{
			get { return null; }
		}

		#endregion

		#region custom sql

		protected override string GetSqlRemoveTable(string table)
		{
			return FormatSql("DROP TABLE {0:NAME} CASCADE", table);
		}

		protected override string GetSqlRemoveIndex(string indexName, string tableName)
		{
			return FormatSql("DROP INDEX {0:NAME}", indexName);
		}

		protected override string GetSqlChangeColumnType(string table, string column, ColumnType columnType)
		{
			string columnTypeSql = typeMap.Get(columnType);

			return FormatSql("ALTER TABLE {0:NAME} ALTER COLUMN {1:NAME} TYPE {2}", table, column, columnTypeSql);
		}

		protected override string GetSqlChangeNotNullConstraint(string table, string column, NotNullConstraint notNullConstraint, ref string sqlChangeColumnType)
		{
			// если изменение типа колонки и признака NOT NULL происходит одним запросом,
			// то изменяем параметр sqlChangeColumnType и возвращаем NULL
			// иначе возвращаем запрос, меняющий признак NOT NULL

			const string SQL_TEMPLATE = "ALTER TABLE {0:NAME} ALTER COLUMN {1:NAME} {2}";

			switch (notNullConstraint)
			{
				case NotNullConstraint.Null:
					return FormatSql(SQL_TEMPLATE, table, column, "DROP NOT NULL");
				case NotNullConstraint.NotNull:
					return FormatSql(SQL_TEMPLATE, table, column, "SET NOT NULL");
				case NotNullConstraint.Undefined:
					return null;
				default:
					throw new NotSupportedException("Некорректное значение параметра notNullConstraint");
			}
		}


		public override bool IndexExists(string indexName, string tableName)
		{
			StringBuilder builder = new StringBuilder();

			builder.Append("SELECT count(*) FROM pg_class c ");
			builder.Append("JOIN pg_index i ON i.indexrelid = c.oid ");
			builder.Append("JOIN pg_class c2 ON i.indrelid = c2.oid ");
			builder.Append("LEFT JOIN pg_user u ON u.usesysid = c.relowner ");
			builder.Append("LEFT JOIN pg_namespace n ON n.oid = c.relnamespace ");
			builder.Append("WHERE c.relkind = 'i' ");
			builder.Append("AND n.nspname NOT IN ('pg_catalog', 'pg_toast') ");
			builder.Append("AND pg_table_is_visible(c.oid) ");
			builder.AppendFormat("and c.relname = '{0}' ", indexName);
			builder.AppendFormat("and c2.relname = '{0}' ", tableName);

			int count = Convert.ToInt32(ExecuteScalar(builder.ToString()));
			return count > 0;
		}

		public override bool ConstraintExists(string table, string name)
		{
			string sql = string.Format("SELECT \"constraint_name\" FROM \"information_schema\".\"table_constraints\" WHERE \"table_schema\" = 'public' AND \"constraint_name\" = '{0}'", name);

			using (IDataReader reader = ExecuteReader(sql))
			{
				return reader.Read();
			}
		}

		public override bool ColumnExists(string table, string column)
		{
			string sql = String.Format("SELECT \"column_name\" FROM \"information_schema\".\"columns\" WHERE \"table_schema\" = 'public' AND \"table_name\" = '{0}' AND \"column_name\" = '{1}'", table, column);

			using (IDataReader reader = ExecuteReader(sql))
			{
				return reader.Read();
			}
		}

		public override bool TableExists(string table)
		{
			string sql = String.Format("SELECT \"table_name\" FROM \"information_schema\".\"tables\" WHERE \"table_schema\" = 'public' AND \"table_name\" = '{0}'", table);

			using (IDataReader reader = ExecuteReader(sql))
			{
				return reader.Read();
			}
		}

		public override string[] GetTables()
		{
			List<string> tables = new List<string>();
			using (IDataReader reader = ExecuteReader("SELECT \"table_name\" FROM \"information_schema\".\"tables\" WHERE \"table_schema\" = 'public'"))
			{
				while (reader.Read())
				{
					tables.Add((string)reader[0]);
				}
			}
			return tables.ToArray();
		}

		#endregion
	}
}