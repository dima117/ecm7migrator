using System;
using System.Data;

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
			RegisterColumnType(DbType.Int64, "INT64");
			RegisterColumnType(DbType.Single, "DOUBLE PRECISION");
			RegisterColumnType(DbType.StringFixedLength, "CHAR(255) CHARACTER SET UNICODE_FSS");
			RegisterColumnType(DbType.StringFixedLength, 4000, "CHAR($l) CHARACTER SET UNICODE_FSS");
			RegisterColumnType(DbType.String, "VARCHAR(255) CHARACTER SET UNICODE_FSS");
			RegisterColumnType(DbType.String, 4000, "VARCHAR($l) CHARACTER SET UNICODE_FSS");
			RegisterColumnType(DbType.Time, "TIMESTAMP");
		}

		#region Overrides of SqlGenerator

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

		public override bool IndexExists(string indexName, string tableName)
		{
			throw new System.NotImplementedException();
		}

		public override bool ConstraintExists(string table, string name)
		{
			throw new System.NotImplementedException();
		}

		public override void AddColumn(string table, string columnSql)
		{
			string sql = FormatSql("ALTER TABLE {0:NAME} ADD {1}", table, columnSql);
			ExecuteNonQuery(sql);
		}

		#endregion
	}
}
