using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;

using ECM7.Migrator.Framework;
using Oracle.DataAccess.Client;
using ForeignKeyConstraint = ECM7.Migrator.Framework.ForeignKeyConstraint;

namespace ECM7.Migrator.Providers.Oracle
{
	using Framework.Logging;

	/// <summary>
	/// Провайдер трансформации для Oracle
	/// </summary>
	public class OracleTransformationProvider : TransformationProvider<OracleConnection>
	{
		/// <summary>
		/// Инициализация
		/// </summary>
		/// <param name="connection">Подключение к БД</param>
		public OracleTransformationProvider(OracleConnection connection)
			: base(connection)
		{
			RegisterColumnType(DbType.AnsiStringFixedLength, "CHAR(255)");
			RegisterColumnType(DbType.AnsiStringFixedLength, 2000, "CHAR($l)");
			RegisterColumnType(DbType.AnsiString, "VARCHAR2(255)");
			RegisterColumnType(DbType.AnsiString, 2000, "VARCHAR2($l)");
			RegisterColumnType(DbType.AnsiString, 2147483647, "CLOB"); // should use the IType.ClobType
			RegisterColumnType(DbType.Binary, "RAW(2000)");
			RegisterColumnType(DbType.Binary, 2000, "RAW($l)");
			RegisterColumnType(DbType.Binary, 2147483647, "BLOB");
			RegisterColumnType(DbType.Boolean, "NUMBER(1,0)");
			RegisterColumnType(DbType.Byte, "NUMBER(3,0)");
			RegisterColumnType(DbType.Currency, "NUMBER(19,1)");
			RegisterColumnType(DbType.Date, "DATE");
			RegisterColumnType(DbType.DateTime, "TIMESTAMP(4)");
			RegisterColumnType(DbType.Decimal, "NUMBER");
			RegisterColumnType(DbType.Decimal, 38, "NUMBER($l, $s)", 2);
			// having problems with both ODP and OracleClient from MS not being able
			// to read values out of a field that is DOUBLE PRECISION
			RegisterColumnType(DbType.Double, "BINARY_DOUBLE");
			RegisterColumnType(DbType.Guid, "RAW(16)");
			RegisterColumnType(DbType.Int16, "NUMBER(5,0)");
			RegisterColumnType(DbType.Int32, "NUMBER(10,0)");
			RegisterColumnType(DbType.Int64, "NUMBER(18,0)");
			RegisterColumnType(DbType.Single, "FLOAT(24)");
			RegisterColumnType(DbType.StringFixedLength, "NCHAR(255)");
			RegisterColumnType(DbType.StringFixedLength, 2000, "NCHAR($l)");
			RegisterColumnType(DbType.String, "NVARCHAR2(255)");
			RegisterColumnType(DbType.String, 2000, "NVARCHAR2($l)");
			RegisterColumnType(DbType.String, 1073741823, "NCLOB");
			RegisterColumnType(DbType.Time, "DATE");

			RegisterProperty(ColumnProperty.Null, String.Empty);
		}

		#region Overrides of SqlGenerator

		public override string NamesQuoteTemplate
		{
			get { return "\"{0}\""; }
		}

		public override string BatchSeparator
		{
			get { return "/"; }
		}

		public override string Default(object defaultValue)
		{
			// convert boolean to number (1, 0)
			if (defaultValue is bool)
			{
				defaultValue = (bool)defaultValue ? 1 : 0;
			}

			return base.Default(defaultValue);
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

		#region custom sql

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
					FormatSql("ALTER TABLE {0:NAME}", primaryTable),
					FormatSql("ADD CONSTRAINT {0:NAME}", name),
					FormatSql("FOREIGN KEY ({0:COLS})", primaryColumns.ToList()),
					FormatSql("REFERENCES {0:NAME} ({1:COLS})", refTable, refColumns)
				};

			switch (constraint)
			{
				case ForeignKeyConstraint.Cascade:
					command.Add("ON DELETE CASCADE");
					break;
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

			string sql = FormatSql("DROP INDEX {0:NAME}", indexName);

			ExecuteNonQuery(sql);
		}

		// todo: написать тесты на добавление внешнего ключа с каскадным обновлением
		public override void AddForeignKey(string name, string primaryTable, string[] primaryColumns, string refTable, string[] refColumns, ForeignKeyConstraint onDeleteConstraint, ForeignKeyConstraint onUpdateConstraint)
		{
			throw new NotSupportedException("Oracle не поддерживает каскадное обновление");
		}

		public override void AddColumn(string table, string columnSql)
		{
			string sql = FormatSql("ALTER TABLE {0:NAME} ADD ({1})", table, columnSql);
			ExecuteNonQuery(sql);
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
				ExecuteReader("SELECT table_name FROM user_tables"))
			{
				while (reader.Read())
				{
					tables.Add(reader[0].ToString());
				}
			}

			return tables.ToArray();
		}

		public override void ChangeColumn(string table, string columnSql)
		{
			string sql = FormatSql("ALTER TABLE {0:NAME} MODIFY ({1})", table, columnSql);
			ExecuteNonQuery(sql);
		}

		#endregion
	}
}
