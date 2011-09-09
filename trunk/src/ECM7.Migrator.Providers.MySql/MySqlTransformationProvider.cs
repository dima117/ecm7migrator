using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using ECM7.Migrator.Framework;
using MySql.Data.MySqlClient;

namespace ECM7.Migrator.Providers.MySql
{
	using ForeignKeyConstraint = ECM7.Migrator.Framework.ForeignKeyConstraint;

	/// <summary>
	/// Summary description for MySqlTransformationProvider.
	/// </summary>
	public class MySqlTransformationProvider : TransformationProvider<MySqlConnection>
	{
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

			propertyMap.RegisterPropertySql(ColumnProperty.Unsigned, "UNSIGNED");
			propertyMap.RegisterPropertySql(ColumnProperty.Identity, "AUTO_INCREMENT");

		}

		#region Особенности СУБД

		public override bool IdentityNeedsType
		{
			get { return false; }
		}

		protected override string NamesQuoteTemplate
		{
			get { return "`{0}`"; }
		}

		#endregion

		#region custom sql

		protected override string GetSqlChangeColumn(string table, string columnSql)
		{
			return FormatSql("ALTER TABLE {0:NAME} MODIFY {1}", table, columnSql);
		}

		protected override string GetSqlDefaultValue(object defaultValue)
		{
			if (defaultValue is bool)
			{
				defaultValue = ((bool)defaultValue) ? 1 : 0;
			}

			return String.Format("DEFAULT {0}", defaultValue);
		}

		protected override string GetSqlRemoveConstraint(string table, string name)
		{
			return FormatSql("ALTER TABLE {0:NAME} DROP KEY {1:NAME}", table, name);
		}

		protected override string GetSqlAddTable(string table, string engine, string columnsSql)
		{
			string dbEngine = engine.Nvl("INNODB");
			return FormatSql("CREATE TABLE {0:NAME} ({1}) ENGINE = {2}", table, columnsSql, dbEngine);
		}

		#endregion

			#region DDL

		public override bool IndexExists(string indexName, string tableName)
		{
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
			string sql = FormatSql("SHOW TABLES LIKE '{0}'", table);
			using (IDataReader reader = ExecuteReader(sql))
			{
				return reader.Read();
			}

		}

		public override bool ColumnExists(string table, string column)
		{
			string sql = FormatSql("SHOW COLUMNS FROM {0:NAME} WHERE Field='{1}'", table, column);
			using (IDataReader reader = ExecuteReader(sql))
			{
				return reader.Read();
			}
		}

		public override void RenameColumn(string tableName, string oldColumnName, string newColumnName)
		{
			throw new NotSupportedException("MySql doesn't support column rename");
		}

		public override void AddCheckConstraint(string name, string table, string checkSql)
		{
			throw new NotSupportedException("MySql doesn't support check constraints");
		}

		public override void AddForeignKey(string name, string primaryTable, string[] primaryColumns, string refTable, string[] refColumns, Framework.ForeignKeyConstraint onDeleteConstraint = ForeignKeyConstraint.NoAction, Framework.ForeignKeyConstraint onUpdateConstraint = ForeignKeyConstraint.NoAction)
		{
			if (onDeleteConstraint == ForeignKeyConstraint.SetDefault || 
				onUpdateConstraint == ForeignKeyConstraint.SetDefault)
			{
				throw new NotSupportedException("MySQL не поддерживает SET DEFAULT для внешних ключей");
			}

			base.AddForeignKey(name, primaryTable, primaryColumns, refTable, refColumns, onDeleteConstraint, onUpdateConstraint);
		}

		#endregion
	}
}