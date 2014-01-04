using ECM7.Migrator.Providers.Validation;
using ECM7.Migrator.Utils;

namespace ECM7.Migrator.Providers.SQLite
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.SQLite;

	using Framework;

	using ForeignKeyConstraint = Framework.ForeignKeyConstraint;

	/// <summary>
	/// Summary description for SQLiteTransformationProvider.
	/// </summary>
	[ProviderValidation(typeof(SQLiteConnection), false)]
	public class SQLiteTransformationProvider : TransformationProvider
	{
		/// <summary>
		/// Инициализация
		/// </summary>
		/// <param name="connection">Подключение к БД</param>
		public SQLiteTransformationProvider(SQLiteConnection connection)
			: base(connection)
		{
			typeMap.Put(DbType.Binary, "BLOB");
			typeMap.Put(DbType.Byte, "INTEGER");
			typeMap.Put(DbType.Int16, "INTEGER");
			typeMap.Put(DbType.Int32, "INTEGER");
			typeMap.Put(DbType.Int64, "INTEGER");
			typeMap.Put(DbType.SByte, "INTEGER");
			typeMap.Put(DbType.UInt16, "INTEGER");
			typeMap.Put(DbType.UInt32, "INTEGER");
			typeMap.Put(DbType.UInt64, "INTEGER");
			typeMap.Put(DbType.Currency, "NUMERIC");
			typeMap.Put(DbType.Decimal, "NUMERIC");

			typeMap.Put(DbType.Double, "NUMERIC");
			typeMap.Put(DbType.Single, "NUMERIC");
			typeMap.Put(DbType.VarNumeric, "NUMERIC");
			typeMap.Put(DbType.String, "TEXT");
			typeMap.Put(DbType.AnsiString, "TEXT");
			typeMap.Put(DbType.AnsiStringFixedLength, "TEXT");
			typeMap.Put(DbType.StringFixedLength, "TEXT");
			typeMap.Put(DbType.DateTime, "DATETIME");
			typeMap.Put(DbType.Time, "DATETIME");
			typeMap.Put(DbType.Boolean, "INTEGER");
			typeMap.Put(DbType.Guid, "UNIQUEIDENTIFIER");

			propertyMap.RegisterPropertySql(ColumnProperty.Identity, "AUTOINCREMENT");
		}

		#region Особенности СУБД

		public override bool NeedsNotNullForIdentity
		{
			get { return false; }
		}

		protected override string NamesQuoteTemplate
		{
			get { return "[{0}]"; }
		}

		public override string BatchSeparator
		{
			get { return "GO"; }
		}

		protected override string GetSqlDefaultValue(object defaultValue)
		{
			if (defaultValue is bool)
			{
				defaultValue = ((bool)defaultValue) ? 1 : 0;
			}

			return String.Format("DEFAULT {0}", defaultValue);
		}

		#endregion

		#region custom sql

		public override bool IndexExists(string indexName, SchemaQualifiedObjectName tableName)
		{
			string sql = FormatSql(
				"SELECT [name] FROM [sqlite_master] WHERE [type]='index' and [name]='{0}' and [tbl_name] = '{1}'",
				indexName, tableName.Name);

			using (IDataReader reader = ExecuteReader(sql))
			{
				return reader.Read();
			}
		}

		protected override string GetSqlRemoveIndex(string indexName, SchemaQualifiedObjectName tableName)
		{
			return FormatSql("DROP INDEX {0:NAME}", indexName);
		}

		public override void AddForeignKey(
			string name,
			SchemaQualifiedObjectName primaryTable,
			string[] primaryColumns,
			SchemaQualifiedObjectName refTable,
			string[] refColumns,
			ForeignKeyConstraint onDeleteConstraint = ForeignKeyConstraint.NoAction,
			ForeignKeyConstraint onUpdateConstraint = ForeignKeyConstraint.NoAction)
		{
			throw new NotSupportedException("SQLite не поддерживает внешние ключи");
		}

		public override void AddCheckConstraint(string name, SchemaQualifiedObjectName table, string checkSql)
		{
			throw new NotSupportedException("SQLite не поддерживает создание CHECK CONSTRAINTS после создания колонки");
		}

		public override void AddPrimaryKey(string name, SchemaQualifiedObjectName table, params string[] columns)
		{
			throw new NotSupportedException("SLQite не поддерживает добавление ограничений после создания колонки");
		}

		public override void AddUniqueConstraint(string name, SchemaQualifiedObjectName table, params string[] columns)
		{
			throw new NotSupportedException("SLQite не поддерживает добавление ограничений после создания колонки");
		}

		public override void RemoveColumn(SchemaQualifiedObjectName table, string column)
		{
			throw new NotSupportedException("SQLite не поддерживает удаление колонок");
		}

		public override void RenameColumn(SchemaQualifiedObjectName tableName, string oldColumnName, string newColumnName)
		{
			throw new NotSupportedException("SLQite не поддерживает переименование колонок");
		}

		public override void ChangeColumn(SchemaQualifiedObjectName table, string column, ColumnType columnType, bool notNull)
		{
			throw new NotSupportedException("SLQite не поддерживает изменение колонок");
		}

		public override void ChangeDefaultValue(SchemaQualifiedObjectName table, string column, object newDefaultValue)
		{
			throw new NotSupportedException("SLQite не поддерживает изменение колонок");
		}

		/// <summary>
		/// Check if a table already exists
		/// </summary>
		/// <param name="table">The name of the table that you want to check on.</param>
		public override bool TableExists(SchemaQualifiedObjectName table)
		{
			string sql = FormatSql("SELECT [name] FROM [sqlite_master] WHERE [type]='table' and [name]='{0}'", table.Name);

			using (IDataReader reader = ExecuteReader(sql))
			{
				return reader.Read();
			}
		}

		public override bool ColumnExists(SchemaQualifiedObjectName table, string column)
		{
			string sql = FormatSql("SELECT {0:NAME} FROM {1:NAME}", column, table.Name);

			try
			{
				using (ExecuteReader(sql))
				{
					return true;
				}
			}
			catch (Exception)
			{
				return false;
			}

		}

		/// <summary>
		/// Determines if a constraint exists.
		/// </summary>
		/// <param name="table">Table owning the constraint</param>
		/// <param name="name">Constraint name</param>
		/// <returns><c>true</c> if the constraint exists.</returns>
		public override bool ConstraintExists(SchemaQualifiedObjectName table, string name)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Get the names of all of the tables
		/// </summary>
		/// <returns>The names of all the tables.</returns>
		public override SchemaQualifiedObjectName[] GetTables(string schema = null)
		{
			Require.That(string.IsNullOrWhiteSpace(schema), "SQLite не поддерживает схемы");

			var tables = new List<SchemaQualifiedObjectName>();

			const string SQL = "SELECT [name] FROM [sqlite_master] WHERE [type]='table' AND [name] <> 'sqlite_sequence' ORDER BY [name]";

			using (IDataReader reader = ExecuteReader(SQL))
			{
				while (reader.Read())
				{
					string tableName = reader.GetString(0);
					tables.Add(new SchemaQualifiedObjectName { Name = tableName });
				}
			}

			return tables.ToArray();
		}

		#endregion
	}
}
