using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using ECM7.Migrator.Framework;
using Npgsql;

namespace ECM7.Migrator.Providers.PostgreSQL
{
	using ECM7.Migrator.Framework.Logging;

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
			RegisterColumnType(DbType.AnsiStringFixedLength, "char(255)");
			RegisterColumnType(DbType.AnsiStringFixedLength, 8000, "char($l)");
			RegisterColumnType(DbType.AnsiString, "varchar(255)");
			RegisterColumnType(DbType.AnsiString, 8000, "varchar($l)");
			RegisterColumnType(DbType.AnsiString, 2147483647, "text");
			RegisterColumnType(DbType.Binary, "bytea");
			RegisterColumnType(DbType.Binary, 2147483647, "bytea");
			RegisterColumnType(DbType.Boolean, "boolean");
			RegisterColumnType(DbType.Byte, "int2");
			RegisterColumnType(DbType.Currency, "decimal(16,4)");
			RegisterColumnType(DbType.Date, "date");
			RegisterColumnType(DbType.DateTime, "timestamp");
			RegisterColumnType(DbType.Decimal, "decimal(19,5)");
			RegisterColumnType(DbType.Decimal, 19, "decimal(18, $l)");
			RegisterColumnType(DbType.Double, "float8");
			RegisterColumnType(DbType.Int16, "int2");
			RegisterColumnType(DbType.Int32, "int4");
			RegisterColumnType(DbType.Int64, "int8");
			RegisterColumnType(DbType.Single, "float4");
			RegisterColumnType(DbType.StringFixedLength, "char(255)");
			RegisterColumnType(DbType.StringFixedLength, 4000, "char($l)");
			RegisterColumnType(DbType.String, "varchar(255)");
			RegisterColumnType(DbType.String, 4000, "varchar($l)");
			RegisterColumnType(DbType.String, 1073741823, "text");
			RegisterColumnType(DbType.Time, "time");

			RegisterProperty(ColumnProperty.Identity, "serial");
		}

		#region Overrides of SqlGenerator

		public override bool IdentityNeedsType
		{
			get { return false; }
		}

		public override bool NeedsNotNullForIdentity
		{
			get { return true; }
		}

		public override bool SupportsIndex
		{
			get { return true; }
		}

		public override string NamesQuoteTemplate
		{
			get { return "\"{0}\""; }
		}

		public override string BatchSeparator
		{
			get { return null; }
		}

		#endregion

		#region custom sql

		public override void RemoveTable(string name)
		{
			string sql = FormatSql("DROP TABLE IF EXISTS {0:NAME} CASCADE", name);
			ExecuteNonQuery(sql);
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

		public override void RemoveIndex(string indexName, string tableName)
		{
			if (!IndexExists(indexName, tableName))
			{
				MigratorLogManager.Log.WarnFormat("Index {0} is not exists", indexName);
				return;
			}

			string sql = FormatSql("DROP INDEX {0:NAME}", indexName);

			ExecuteNonQuery(sql);
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
			if (!TableExists(table))
			{
				return false;
			}

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

		public override void ChangeColumn(string table, Column column)
		{
			if (!ColumnExists(table, column.Name))
			{
				MigratorLogManager.Log.WarnFormat("Column {0}.{1} does not exist", table, column.Name);
				return;
			}

			string tempColumn = "temp_" + column.Name;
			RenameColumn(table, column.Name, tempColumn);
			AddColumn(table, column);

			string sql = FormatSql("UPDATE {0:NAME} SET {1:NAME}={2:NAME}", table, column.Name, tempColumn);
			ExecuteReader(sql);
			RemoveColumn(table, tempColumn);
		}

		public override string[] GetTables()
		{
			List<string> tables = new List<string>();
			using (IDataReader reader = ExecuteReader("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'"))
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