using System;
using System.Collections.Generic;
using System.Data;
using ECM7.Migrator.Framework;
using MySql.Data.MySqlClient;

namespace ECM7.Migrator.Providers.MySql
{
	/// <summary>
	/// Summary description for MySqlTransformationProvider.
	/// </summary>
	public class MySqlTransformationProvider : TransformationProvider<MySqlConnection>
	{
		/// <summary>
		/// Инициализация
		/// </summary>
		/// <param name="connection">Подключение к БД</param>
		public MySqlTransformationProvider(MySqlConnection connection)
			: base(connection)
		{
			RegisterColumnType(DbType.AnsiStringFixedLength, "CHAR(255)");
			RegisterColumnType(DbType.AnsiStringFixedLength, 255, "CHAR($l)");
			RegisterColumnType(DbType.AnsiStringFixedLength, 65535, "TEXT");
			RegisterColumnType(DbType.AnsiStringFixedLength, 16777215, "MEDIUMTEXT");
			RegisterColumnType(DbType.AnsiString, "VARCHAR(255)");
			RegisterColumnType(DbType.AnsiString, 255, "VARCHAR($l)");
			RegisterColumnType(DbType.AnsiString, 65535, "TEXT");
			RegisterColumnType(DbType.AnsiString, 16777215, "MEDIUMTEXT");
			RegisterColumnType(DbType.Binary, "LONGBLOB");
			RegisterColumnType(DbType.Binary, 127, "TINYBLOB");
			RegisterColumnType(DbType.Binary, 65535, "BLOB");
			RegisterColumnType(DbType.Binary, 16777215, "MEDIUMBLOB");
			RegisterColumnType(DbType.Boolean, "TINYINT(1)");
			RegisterColumnType(DbType.Byte, "TINYINT UNSIGNED");
			RegisterColumnType(DbType.Currency, "MONEY");
			RegisterColumnType(DbType.Date, "DATE");
			RegisterColumnType(DbType.DateTime, "DATETIME");
			RegisterColumnType(DbType.Decimal, "NUMERIC");
			RegisterColumnType(DbType.Decimal, 38, "NUMERIC($l, $s)", 2);
			RegisterColumnType(DbType.Double, "DOUBLE");
			RegisterColumnType(DbType.Guid, "VARCHAR(40)");
			RegisterColumnType(DbType.Int16, "SMALLINT");
			RegisterColumnType(DbType.Int32, "INTEGER");
			RegisterColumnType(DbType.Int64, "BIGINT");
			RegisterColumnType(DbType.Single, "FLOAT");
			RegisterColumnType(DbType.StringFixedLength, "CHAR(255)");
			RegisterColumnType(DbType.StringFixedLength, 255, "CHAR($l)");
			RegisterColumnType(DbType.StringFixedLength, 65535, "TEXT");
			RegisterColumnType(DbType.StringFixedLength, 16777215, "MEDIUMTEXT");
			RegisterColumnType(DbType.String, "VARCHAR(255)");
			RegisterColumnType(DbType.String, 255, "VARCHAR($l)");
			RegisterColumnType(DbType.String, 65535, "TEXT");
			RegisterColumnType(DbType.String, 16777215, "MEDIUMTEXT");
			RegisterColumnType(DbType.Time, "TIME");

			RegisterProperty(ColumnProperty.Unsigned, "UNSIGNED");
			RegisterProperty(ColumnProperty.Identity, "AUTO_INCREMENT");

		}

		#region Overrides of SqlGenerator

		public override bool IdentityNeedsType
		{
			get { return false; }
		}

		public override string NamesQuoteTemplate
		{
			get { return "`{0}`"; }
		}

		public override string Default(object defaultValue)
		{
			if (defaultValue.GetType().Equals(typeof(bool)))
			{
				defaultValue = ((bool)defaultValue) ? 1 : 0;
			}
			return String.Format("DEFAULT {0}", defaultValue);
		}

		#endregion

		#region custom sql

		public override void RemoveForeignKey(string table, string name)
		{
			if (ConstraintExists(table, name))
			{
				ExecuteNonQuery(FormatSql("ALTER TABLE {0:NAME} DROP FOREIGN KEY {1:NAME}", table, name));
				ExecuteNonQuery(FormatSql("ALTER TABLE {0:NAME} DROP KEY {1:NAME}", table, name));
			}
		}

		public override void RemoveConstraint(string table, string name)
		{
			if (ConstraintExists(table, name))
			{
				string sql = FormatSql("ALTER TABLE {0:NAME} DROP KEY {1:NAME}", table, name);
				ExecuteNonQuery(sql);
			}
		}

		public override bool IndexExists(string indexName, string tableName)
		{
			if (!TableExists(tableName))
				return false;

			string sql = FormatSql("SHOW INDEXES FROM {0:NAME}", tableName);

			using (IDataReader reader = ExecuteQuery(sql))
			{
				while (reader.Read())
				{
					if (reader["Key_name"].ToString() == indexName)
					{
						return true;
					}
				}
			}

			return false;
		}

		public override bool ConstraintExists(string table, string name)
		{
			if (!TableExists(table))
				return false;

			string sqlConstraint = FormatSql("SHOW KEYS FROM {0:NAME}", table);

			using (IDataReader reader = ExecuteQuery(sqlConstraint))
			{
				while (reader.Read())
				{
					if (reader["Key_name"].ToString() == name)
					{
						return true;
					}
				}
			}

			return false;
		}

		public override bool PrimaryKeyExists(string table, string name)
		{
			return ConstraintExists(table, "PRIMARY");
		}

		public override Column[] GetColumns(string table)
		{
			List<Column> columns = new List<Column>();
			using (
				IDataReader reader =
					ExecuteQuery(FormatSql("SHOW COLUMNS FROM {0:NAME}", table)))
			{
				while (reader.Read())
				{
					Column column = new Column(reader.GetString(0), DbType.String);
					string nullableStr = reader.GetString(2).ToUpper();
					bool isNullable = nullableStr == "YES";
					column.ColumnProperty |= isNullable ? ColumnProperty.Null : ColumnProperty.NotNull;

					columns.Add(column);
				}
			}

			return columns.ToArray();
		}

		public override string[] GetTables()
		{
			List<string> tables = new List<string>();
			using (IDataReader reader = ExecuteQuery("SHOW TABLES"))
			{
				while (reader.Read())
				{
					tables.Add((string)reader[0]);
				}
			}

			return tables.ToArray();
		}

		public override void ChangeColumn(string table, string columnSql)
		{
			ExecuteNonQuery(FormatSql("ALTER TABLE {0:NAME} MODIFY {1}", table, columnSql));
		}

		public override void AddTable(string name, params Column[] columns)
		{
			AddTable(name, "INNODB", columns);
		}

		public override void AddTable(string name, string engine, string columnsSql)
		{
			string sqlCreate = FormatSql("CREATE TABLE {0:NAME} ({1}) ENGINE = {2}", name, columnsSql, engine);
			ExecuteNonQuery(sqlCreate);
		}

		public override void RenameColumn(string tableName, string oldColumnName, string newColumnName)
		{
			if (ColumnExists(tableName, newColumnName))
				throw new MigrationException(String.Format("Table '{0}' has column named '{1}' already", tableName, newColumnName));

			if (ColumnExists(tableName, oldColumnName))
			{
				string definition = null;
				string sql = FormatSql("SHOW COLUMNS FROM {0:NAME} WHERE Field='{1}'", tableName, oldColumnName);
				using (IDataReader reader = ExecuteQuery(sql))
				{
					if (reader.Read())
					{
						// TODO: Could use something similar to construct the columns in GetColumns
						definition = reader["Type"].ToString();
						if ("NO" == reader["Null"].ToString())
						{
							definition += " " + "NOT NULL";
						}

						if (!reader.IsDBNull(reader.GetOrdinal("Key")))
						{
							string key = reader["Key"].ToString();
							if ("PRI" == key)
							{
								definition += " " + "PRIMARY KEY";
							}
							else if ("UNI" == key)
							{
								definition += " " + "UNIQUE";
							}
						}

						if (!reader.IsDBNull(reader.GetOrdinal("Extra")))
						{
							definition += " " + reader["Extra"];
						}
					}
				}

				if (!String.IsNullOrEmpty(definition))
				{
					ExecuteNonQuery(FormatSql("ALTER TABLE {0:NAME} CHANGE {1:NAME} {2:NAME} {3}", tableName, oldColumnName, newColumnName, definition));
				}
			}
		}

		public override void AddCheckConstraint(string name, string table, string checkSql)
		{
			throw new NotSupportedException("MySql doesn't support check constraints");
		}
		#endregion
	}
}