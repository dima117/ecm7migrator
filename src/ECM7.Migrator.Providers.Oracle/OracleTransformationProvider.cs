using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ECM7.Migrator.Framework;
using ForeignKeyConstraint = ECM7.Migrator.Framework.ForeignKeyConstraint;
using OracleConnection = Oracle.DataAccess.Client.OracleConnection;

namespace ECM7.Migrator.Providers.Oracle
{
	using Framework.Logging;

	/// <summary>
	/// Провайдер трансформации для Oracle
	/// </summary>
	public class OracleTransformationProvider : TransformationProvider
	{
		/// <summary>
		/// Инициализация
		/// </summary>
		/// <param name="dialect">Диалект</param>
		/// <param name="connectionString">Строка подключения</param>
		public OracleTransformationProvider(Dialect dialect, string connectionString)
			: base(dialect, new OracleConnection(connectionString))
		{
		}

		public override void AddForeignKey(string name, string primaryTable, string[] primaryColumns, string refTable,
										  string[] refColumns, ForeignKeyConstraint constraint)
		{
			if (ConstraintExists(primaryTable, name))
			{
				MigratorLogManager.Log.WarnFormat("Constraint {0} already exists", name);
				return;
			}

			List<string> command = new List<string>
				{
					"ALTER TABLE {0}".FormatWith(QuoteName(primaryTable)),
					"ADD CONSTRAINT {0}".FormatWith(QuoteName(name)),
					"FOREIGN KEY ({0})".FormatWith(primaryColumns.Select(QuoteName).ToCommaSeparatedString()),
					"REFERENCES {0} ({1})".FormatWith(QuoteName(refTable), refColumns.Select(QuoteName).ToCommaSeparatedString())
				};

			switch (constraint)
			{
				case ForeignKeyConstraint.Cascade:
					command.Add("ON DELETE CASCADE");
					break;
				//command.Add("NOVALIDATE");
			}

			string commandText = command.ToSeparatedString(" ");
			ExecuteNonQuery(commandText);
		}

		public override bool IndexExists(string indexName, string tableName)
		{
			string sql =
				("select count(*) from user_indexes " +
				"where INDEX_NAME = '{0}' " +
				"and TABLE_NAME = '{1}'")
				.FormatWith(indexName, tableName);

			int count = Convert.ToInt32(ExecuteScalar(sql));
			return count > 0;
		}

		public override void RemoveIndex(string indexName, string tableName)
		{
			if (!IndexExists(indexName, tableName))
			{
				MigratorLogManager.Log.WarnFormat("Index {0} is not exists", indexName);
				return;
			}

            string sql = "DROP INDEX {0}".FormatWith(QuoteName(indexName));

			ExecuteNonQuery(sql);
		}

		// todo: написать тесты на добавление внешнего ключа с каскадным обновлением
		public override void AddForeignKey(string name, string primaryTable, string[] primaryColumns, string refTable, string[] refColumns, ForeignKeyConstraint onDeleteConstraint, ForeignKeyConstraint onUpdateConstraint)
		{
			throw new NotSupportedException("Oracle не поддерживает каскадное обновление");
		}

		public override void AddColumn(string table, string columnSql)
		{
			ExecuteNonQuery(String.Format("ALTER TABLE {0} ADD ({1})", QuoteName(table), columnSql));
		}

		public override bool ConstraintExists(string table, string name)
		{
			string sql =
				string.Format(
					"SELECT COUNT(constraint_name) FROM user_constraints WHERE constraint_name = '{0}' AND table_name = '{1}'",
					name, table);

			MigratorLogManager.Log.ExecuteSql(sql);
			object scalar = ExecuteScalar(sql);
			return Convert.ToInt32(scalar) == 1;
		}

		public override bool ColumnExists(string table, string column)
		{
			if (!TableExists(table))
				return false;

			string sql =
				string.Format(
					"SELECT COUNT(column_name) FROM user_tab_columns WHERE table_name = '{0}' AND column_name = '{1}'",
					table, column);

			MigratorLogManager.Log.ExecuteSql(sql);
			object scalar = ExecuteScalar(sql);
			return Convert.ToInt32(scalar) == 1;
		}

		public override bool TableExists(string table)
		{
			string sql = string.Format(
                "SELECT COUNT(table_name) FROM user_tables WHERE table_name = '{0}'", table);
			MigratorLogManager.Log.ExecuteSql(sql);
			object count = ExecuteScalar(sql);
			return Convert.ToInt32(count) == 1;
		}

		public override string[] GetTables()
		{
			List<string> tables = new List<string>();

			using (IDataReader reader =
				ExecuteQuery("SELECT table_name FROM user_tables"))
			{
				while (reader.Read())
				{
					tables.Add(reader[0].ToString());
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
						string.Format(
							"select column_name, data_type, data_length, data_precision, data_scale, nullable FROM USER_TAB_COLUMNS WHERE table_name = '{0}'",
							table)))
			{
				while (reader.Read())
				{
					string colName = reader["column_name"].ToString();
					DbType colType = DbType.String;
					string dataType = reader["data_type"].ToString().ToLower();
					bool nullable = reader["nullable"].ToString() == "Y";

					if (dataType.Equals("number"))
					{
						int precision = Convert.ToInt32(reader[3]);
						int scale = Convert.ToInt32(reader[4]);
						if (scale == 0)
						{
							colType = precision <= 10 ? DbType.Int16 : DbType.Int64;
						}
						else
						{
							colType = DbType.Decimal;
						}
					}
					else if (dataType.StartsWith("timestamp") || dataType.Equals("date"))
					{
						colType = DbType.DateTime;
					}

					ColumnProperty properties = nullable
						? ColumnProperty.Null
						: ColumnProperty.NotNull;

					columns.Add(new Column(colName, colType, properties));
				}
			}

			return columns.ToArray();
		}

		public override void ChangeColumn(string table, string columnSql)
		{
			ExecuteNonQuery(String.Format("ALTER TABLE {0} MODIFY ({1})", QuoteName(table), columnSql));
		}
	}
}
