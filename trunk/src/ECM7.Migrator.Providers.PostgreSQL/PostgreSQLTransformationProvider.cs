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
	public class PostgreSQLTransformationProvider : TransformationProvider
	{
		/// <summary>
		/// Инициализация
		/// </summary>
		/// <param name="dialect">Диалект</param>
		/// <param name="connectionString">Строка подключения</param>
		public PostgreSQLTransformationProvider(Dialect dialect, string connectionString)
			: base(dialect, new NpgsqlConnection(connectionString))
		{
		}

		public override void RemoveTable(string name)
		{
			ExecuteNonQuery(String.Format("DROP TABLE IF EXISTS {0} CASCADE", name));
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
			builder.AppendFormat("and lower(c.relname) = '{0}' ", indexName.ToLower());
			builder.AppendFormat("and lower(c2.relname) = '{0}' ", tableName.ToLower());

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

			string sql = string.Format("DROP INDEX {0}",
					Dialect.QuoteNameIfNeeded(indexName));

			ExecuteNonQuery(sql);
		}

		public override bool ConstraintExists(string table, string name)
		{
			using (IDataReader reader =
				ExecuteQuery(string.Format("SELECT constraint_name FROM information_schema.table_constraints WHERE table_schema = 'public' AND constraint_name = lower('{0}')", name)))
			{
				return reader.Read();
			}
		}

		public override bool ColumnExists(string table, string column)
		{
			if (!TableExists(table))
				return false;

			using (IDataReader reader =
				ExecuteQuery(String.Format("SELECT column_name FROM information_schema.columns WHERE table_schema = 'public' AND table_name = lower('{0}') AND column_name = lower('{1}')", table, column)))
			{
				return reader.Read();
			}
		}

		public override bool TableExists(string table)
		{

			using (IDataReader reader =
				ExecuteQuery(String.Format("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_name = lower('{0}')", table)))
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
			ExecuteQuery(String.Format("UPDATE {0} SET {1}={2}", table, column.Name, tempColumn));
			RemoveColumn(table, tempColumn);
		}

		public override string[] GetTables()
		{
			List<string> tables = new List<string>();
			using (IDataReader reader = ExecuteQuery("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'"))
			{
				while (reader.Read())
				{
					tables.Add((string)reader[0]);
				}
			}
			return tables.ToArray();
		}

		public override Column[] GetColumns(string table)
		{
			List<Column> columns = new List<Column>();
			using (
				IDataReader reader =
					ExecuteQuery(
						String.Format("select COLUMN_NAME, IS_NULLABLE from information_schema.columns where table_schema = 'public' AND table_name = lower('{0}');", table)))
			{
				// FIXME: Mostly duplicated code from the Transformation provider just to support stupid case-insensitivty of Postgre
				while (reader.Read())
				{
					Column column = new Column(reader[0].ToString(), DbType.String);
					bool isNullable = reader.GetString(1) == "YES";
					column.ColumnProperty |= isNullable ? ColumnProperty.Null : ColumnProperty.NotNull;

					columns.Add(column);
				}
			}

			return columns.ToArray();
		}

		public override Column GetColumnByName(string table, string columnName)
		{
			// Duplicate because of the lower case issue
			return Array.Find(GetColumns(table), column => column.Name == columnName.ToLower());
		}
	}
}