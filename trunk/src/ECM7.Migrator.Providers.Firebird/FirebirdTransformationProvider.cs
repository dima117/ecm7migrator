namespace ECM7.Migrator.Providers.Firebird
{
	using System;
	using System.Collections.Generic;
	using System.Data;

	using ECM7.Migrator.Framework;
	using ECM7.Migrator.Providers.Firebird.Internal;
	using FirebirdSql.Data.FirebirdClient;

	public class FirebirdTransformationProvider : TransformationProvider<FbConnection>
	{
		/// <summary>
		/// Инициализация
		/// </summary>
		/// <param name="connection">Подключение к БД</param>
		public FirebirdTransformationProvider(FbConnection connection)
			: base(connection)
		{
			typeMap.Put(DbType.AnsiStringFixedLength, "CHAR(255)");
			typeMap.Put(DbType.AnsiStringFixedLength, 32767, "CHAR($l)");
			typeMap.Put(DbType.AnsiString, "VARCHAR(255)");
			typeMap.Put(DbType.AnsiString, 32767, "VARCHAR($l)");
			typeMap.Put(DbType.Binary, "VARCHAR(8000)");
			typeMap.Put(DbType.Binary, 8000, "VARCHAR($l)");
			typeMap.Put(DbType.Boolean, "SMALLINT");
			typeMap.Put(DbType.Byte, "SMALLINT");
			typeMap.Put(DbType.Currency, "DECIMAL(18,4)");
			typeMap.Put(DbType.Date, "TIMESTAMP");
			typeMap.Put(DbType.DateTime, "TIMESTAMP");
			typeMap.Put(DbType.Decimal, "DECIMAL");
			typeMap.Put(DbType.Decimal, 38, "DECIMAL($l, $s)", 2);
			typeMap.Put(DbType.Guid, "CHAR(36)");
			typeMap.Put(DbType.Int16, "SMALLINT");
			typeMap.Put(DbType.Int32, "INTEGER");
			typeMap.Put(DbType.Int64, "BIGINT");
			typeMap.Put(DbType.Single, "DOUBLE PRECISION");
			typeMap.Put(DbType.StringFixedLength, "CHAR(255) CHARACTER SET UNICODE_FSS");
			typeMap.Put(DbType.StringFixedLength, 4000, "CHAR($l) CHARACTER SET UNICODE_FSS");
			typeMap.Put(DbType.String, "VARCHAR(255) CHARACTER SET UNICODE_FSS");
			typeMap.Put(DbType.String, 4000, "VARCHAR($l) CHARACTER SET UNICODE_FSS");
			typeMap.Put(DbType.Time, "TIMESTAMP");
		}

		#region Особенности СУБД

		public override string BatchSeparator
		{
			get { return "/"; }
		}

		#endregion

		#region override SqlRunner methods

		protected override IDataReader OpenDataReader(IDbCommand cmd)
		{
			return new InternalDataReader(cmd);
		}

		#endregion

		#region generate sql

		protected override string GetSqlDefaultValue(object defaultValue)
		{
			if (defaultValue is bool)
			{
				defaultValue = ((bool)defaultValue) ? 1 : 0;
			}

			return String.Format("DEFAULT {0}", defaultValue);
		}

		protected override string GetSqlRemoveIndex(string indexName, string tableName)
		{
			return FormatSql("DROP INDEX {0:NAME}", indexName);
		}

		protected override string GetSqlAddColumn(string table, string columnSql)
		{
			return FormatSql("ALTER TABLE {0:NAME} ADD {1}", table, columnSql);
		}

		protected override string GetSqlRenameColumn(string tableName, string oldColumnName, string newColumnName)
		{
			return FormatSql("ALTER TABLE {0:NAME} ALTER COLUMN {1:NAME} TO {2:NAME}",
				tableName, oldColumnName, newColumnName);
		}

		protected override string GetSqlRemoveColumn(string table, string column)
		{
			return FormatSql("ALTER TABLE {0:NAME} DROP {1:NAME} ", table, column);
		}

		public override string GetSqlColumnDef(Column column, bool compoundPrimaryKey)
		{
			ColumnSqlBuilder sqlBuilder = new ColumnSqlBuilder(column, typeMap, propertyMap);

			sqlBuilder.AddColumnName(NamesQuoteTemplate);
			sqlBuilder.AddColumnType(IdentityNeedsType);
			sqlBuilder.AddDefaultValueSql(GetSqlDefaultValue);
			sqlBuilder.AddNotNullSql(NeedsNotNullForIdentity);
			sqlBuilder.AddPrimaryKeySql(compoundPrimaryKey);
			sqlBuilder.AddUniqueSql();

			return sqlBuilder.ToString();
		}

		#region ChangeColumn

		protected override string GetSqlChangeColumnType(string table, string column, ColumnType columnType)
		{
			string sqlColumnType = typeMap.Get(columnType);

			return FormatSql("ALTER TABLE {0:NAME} ALTER COLUMN {1:NAME} TYPE {2}", table, column, sqlColumnType);
		}

		protected override string GetSqlChangeNotNullConstraint(string table, string column, NotNullConstraint notNullConstraint, ref string sqlChangeColumnType)
		{
			const string SQL_TEMPLATE = "UPDATE RDB$RELATION_FIELDS SET RDB$NULL_FLAG = {0} WHERE RDB$FIELD_NAME = '{1}' AND RDB$RELATION_NAME = '{2}';";

			switch (notNullConstraint)
			{
				case NotNullConstraint.Null:
					return FormatSql(SQL_TEMPLATE, "NULL", column, table);
				case NotNullConstraint.NotNull:
					return FormatSql(SQL_TEMPLATE, "1", column, table);
				case NotNullConstraint.Undefined:
					return null;
				default:
					throw new NotSupportedException("Некорректное значение параметра notNullConstraint");
			}
		}

		#endregion

		#endregion

		#region DDL

		public override string[] GetTables()
		{
			string sql = FormatSql(
				"select rdb$relation_name from rdb$relations where rdb$system_flag = 0");

			List<string> result = new List<string>();

			using (IDataReader reader = ExecuteReader(sql))
			{
				while (reader.Read())
				{
					string tableName = reader.GetString(0).Trim();
					result.Add(tableName);
				}
			}

			return result.ToArray();
		}

		public override bool ColumnExists(string table, string column)
		{
			string sql = FormatSql(
				"select count(*) from rdb$relation_fields " +
				"where rdb$relation_name = '{0}' and rdb$field_name = '{1}'", table, column);

			int cnt = Convert.ToInt32(ExecuteScalar(sql));
			return cnt > 0;
		}

		public override bool TableExists(string table)
		{
			string sql = FormatSql(
				"select count(*) from rdb$relations " +
				"where rdb$system_flag = 0 and rdb$relation_name = '{0}'", table);

			int cnt = Convert.ToInt32(ExecuteScalar(sql));
			return cnt > 0;
		}

		public override bool IndexExists(string indexName, string tableName)
		{
			string sql = FormatSql(
				"select count(*) from rdb$indices " +
				"where rdb$relation_name = '{0}' and rdb$index_name = '{1}' " +
				"and not (rdb$index_name starting with 'rdb$')", tableName, indexName);

			int cnt = Convert.ToInt32(ExecuteScalar(sql));
			return cnt > 0;
		}

		public override bool ConstraintExists(string table, string name)
		{
			string sql = FormatSql(
				"select count(*) from rdb$relation_constraints " +
				"where rdb$relation_name = '{0}' and rdb$constraint_name = '{1}'", table, name);

			int cnt = Convert.ToInt32(ExecuteScalar(sql));
			return cnt > 0;
		}

		public override void RenameTable(string oldName, string newName)
		{
			throw new NotSupportedException("Firebird не поддерживает переименование таблиц");
		}

		#endregion
	}
}
