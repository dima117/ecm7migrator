using System;
using System.Collections.Generic;
using System.Data;
using ECM7.Migrator.Exceptions;
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
			typeMap.Put(DbType.AnsiStringFixedLength, "CHAR(255)");
			typeMap.Put(DbType.AnsiStringFixedLength, 255, "CHAR($l)");
			typeMap.Put(DbType.AnsiStringFixedLength, 65535, "TEXT");
			typeMap.Put(DbType.AnsiStringFixedLength, 16777215, "MEDIUMTEXT");
			typeMap.Put(DbType.AnsiString, "VARCHAR(255)");
			typeMap.Put(DbType.AnsiString, 255, "VARCHAR($l)");
			typeMap.Put(DbType.AnsiString, 65535, "TEXT");
			typeMap.Put(DbType.AnsiString, 16777215, "MEDIUMTEXT");
			typeMap.Put(DbType.Binary, "LONGBLOB");
			typeMap.Put(DbType.Binary, 127, "TINYBLOB");
			typeMap.Put(DbType.Binary, 65535, "BLOB");
			typeMap.Put(DbType.Binary, 16777215, "MEDIUMBLOB");
			typeMap.Put(DbType.Boolean, "TINYINT(1)");
			typeMap.Put(DbType.Byte, "TINYINT UNSIGNED");
			typeMap.Put(DbType.Currency, "MONEY");
			typeMap.Put(DbType.Date, "DATE");
			typeMap.Put(DbType.DateTime, "DATETIME");
			typeMap.Put(DbType.Decimal, "NUMERIC");
			typeMap.Put(DbType.Decimal, 38, "NUMERIC($l, $s)", 2);
			typeMap.Put(DbType.Double, "DOUBLE");
			typeMap.Put(DbType.Guid, "VARCHAR(40)");
			typeMap.Put(DbType.Int16, "SMALLINT");
			typeMap.Put(DbType.Int32, "INTEGER");
			typeMap.Put(DbType.Int64, "BIGINT");
			typeMap.Put(DbType.Single, "FLOAT");
			typeMap.Put(DbType.StringFixedLength, "CHAR(255)");
			typeMap.Put(DbType.StringFixedLength, 255, "CHAR($l)");
			typeMap.Put(DbType.StringFixedLength, 65535, "TEXT");
			typeMap.Put(DbType.StringFixedLength, 16777215, "MEDIUMTEXT");
			typeMap.Put(DbType.String, "VARCHAR(255)");
			typeMap.Put(DbType.String, 255, "VARCHAR($l)");
			typeMap.Put(DbType.String, 65535, "TEXT");
			typeMap.Put(DbType.String, 16777215, "MEDIUMTEXT");
			typeMap.Put(DbType.Time, "TIME");

			propertyMap.RegisterProperty(ColumnProperty.Unsigned, "UNSIGNED");
			propertyMap.RegisterProperty(ColumnProperty.Identity, "AUTO_INCREMENT");

		}

		#region Overrides of SqlGenerator

		public override bool IdentityNeedsType
		{
			get { return false; }
		}

		protected override string NamesQuoteTemplate
		{
			get { return "`{0}`"; }
		}

		protected override string GetSqlDefaultValue(object defaultValue)
		{
			if (defaultValue.GetType().Equals(typeof(bool)))
			{
				defaultValue = ((bool)defaultValue) ? 1 : 0;
			}
			return String.Format("DEFAULT {0}", defaultValue);
		}

		#endregion

		#region custom sql

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

			using (IDataReader reader = ExecuteReader(sql))
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

			using (IDataReader reader = ExecuteReader(sqlConstraint))
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

		public override string[] GetTables()
		{
			List<string> tables = new List<string>();
			using (IDataReader reader = ExecuteReader("SHOW TABLES"))
			{
				while (reader.Read())
				{
					tables.Add((string)reader[0]);
				}
			}

			return tables.ToArray();
		}

		public override bool TableExists(string table)
		{
			throw new NotImplementedException("Нужно реализовать проверку существования таблицы в MySql");
		}

		public override bool ColumnExists(string table, string column)
		{
			throw new NotImplementedException("Нужно реализовать проверку существования колонки");
		}

		protected override string GetSqlChangeColumn(string table, string columnSql)
		{
			return FormatSql("ALTER TABLE {0:NAME} MODIFY {1}", table, columnSql);
		}

		protected override string GetSqlAddTable(string table, string engine, string columnsSql)
		{
			string dbEngine = engine.Nvl("INNODB");
			return FormatSql("CREATE TABLE {0:NAME} ({1}) ENGINE = {2}", table, columnsSql, dbEngine);
		}

		public override void RenameColumn(string tableName, string oldColumnName, string newColumnName)
		{
			if (ColumnExists(tableName, newColumnName))
				throw new MigrationException(String.Format("Table '{0}' has column named '{1}' already", tableName, newColumnName));

			if (ColumnExists(tableName, oldColumnName))
			{
				string definition = null;
				string sql = FormatSql("SHOW COLUMNS FROM {0:NAME} WHERE Field='{1}'", tableName, oldColumnName);
				using (IDataReader reader = ExecuteReader(sql))
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