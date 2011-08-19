using System;
using System.Collections.Generic;
using System.Data;
using ECM7.Migrator.Framework;
using ECM7.Migrator.Framework.Logging;

namespace ECM7.Migrator.Providers.Firebird
{
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
			// todo: написать тесты на закрытие подключения
			RegisterColumnType(DbType.AnsiStringFixedLength, "CHAR(255)");
			RegisterColumnType(DbType.AnsiStringFixedLength, 32767, "CHAR($l)");
			RegisterColumnType(DbType.AnsiString, "VARCHAR(255)");
			RegisterColumnType(DbType.AnsiString, 32767, "VARCHAR($l)");
			RegisterColumnType(DbType.Binary, "VARCHAR(8000)");
			RegisterColumnType(DbType.Binary, 8000, "VARCHAR($l)");
			RegisterColumnType(DbType.Boolean, "SMALLINT");
			RegisterColumnType(DbType.Byte, "SMALLINT");
			RegisterColumnType(DbType.Currency, "DECIMAL(18,4)");
			RegisterColumnType(DbType.Date, "TIMESTAMP");
			RegisterColumnType(DbType.DateTime, "TIMESTAMP");
			RegisterColumnType(DbType.Decimal, "DECIMAL");
			RegisterColumnType(DbType.Decimal, 38, "DECIMAL($l, $s)", 2);
			RegisterColumnType(DbType.Guid, "CHAR(36)");
			RegisterColumnType(DbType.Int16, "SMALLINT");
			RegisterColumnType(DbType.Int32, "INTEGER");
			RegisterColumnType(DbType.Int64, "BIGINT");
			RegisterColumnType(DbType.Single, "DOUBLE PRECISION");
			RegisterColumnType(DbType.StringFixedLength, "CHAR(255) CHARACTER SET UNICODE_FSS");
			RegisterColumnType(DbType.StringFixedLength, 4000, "CHAR($l) CHARACTER SET UNICODE_FSS");
			RegisterColumnType(DbType.String, "VARCHAR(255) CHARACTER SET UNICODE_FSS");
			RegisterColumnType(DbType.String, 4000, "VARCHAR($l) CHARACTER SET UNICODE_FSS");
			RegisterColumnType(DbType.Time, "TIMESTAMP");
		}

		#region Overrides of SqlGenerator

		public override string BatchSeparator
		{
			get { return ";"; }
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

		public override void AddColumn(string table, string columnSql)
		{
			string sql = FormatSql("ALTER TABLE {0:NAME} ADD {1}", table, columnSql);
			ExecuteNonQuery(sql);
		}

		protected override void BuildColumnSql(List<string> vals, Column column, bool compoundPrimaryKey)
		{
			AddColumnName(vals, column);
			AddColumnType(vals, column);
			AddDefaultValueSql(vals, column);
			AddNotNullSql(vals, column);
			AddPrimaryKeySql(vals, column, compoundPrimaryKey);
			AddUniqueSql(vals, column);
		}

		#endregion
	}
}
