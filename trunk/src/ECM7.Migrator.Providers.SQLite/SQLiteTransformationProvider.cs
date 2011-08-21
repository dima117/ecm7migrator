namespace ECM7.Migrator.Providers.SQLite
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.SQLite;

	using ECM7.Migrator.Framework;
	using ECM7.Migrator.Framework.Logging;

	/// <summary>
	/// Summary description for SQLiteTransformationProvider.
	/// </summary>
	public class SQLiteTransformationProvider : TransformationProvider<SQLiteConnection>
	{
		/// <summary>
		/// Инициализация
		/// </summary>
		/// <param name="connection">Подключение к БД</param>
		public SQLiteTransformationProvider(SQLiteConnection connection)
			: base(connection)
		{
			RegisterColumnType(DbType.Binary, "BLOB");
			RegisterColumnType(DbType.Byte, "INTEGER");
			RegisterColumnType(DbType.Int16, "INTEGER");
			RegisterColumnType(DbType.Int32, "INTEGER");
			RegisterColumnType(DbType.Int64, "INTEGER");
			RegisterColumnType(DbType.SByte, "INTEGER");
			RegisterColumnType(DbType.UInt16, "INTEGER");
			RegisterColumnType(DbType.UInt32, "INTEGER");
			RegisterColumnType(DbType.UInt64, "INTEGER");
			RegisterColumnType(DbType.Currency, "NUMERIC");
			RegisterColumnType(DbType.Decimal, "NUMERIC");
			RegisterColumnType(DbType.Double, "NUMERIC");
			RegisterColumnType(DbType.Single, "NUMERIC");
			RegisterColumnType(DbType.VarNumeric, "NUMERIC");
			RegisterColumnType(DbType.String, "TEXT");
			RegisterColumnType(DbType.AnsiString, "TEXT");
			RegisterColumnType(DbType.AnsiStringFixedLength, "TEXT");
			RegisterColumnType(DbType.StringFixedLength, "TEXT");
			RegisterColumnType(DbType.DateTime, "DATETIME");
			RegisterColumnType(DbType.Time, "DATETIME");
			RegisterColumnType(DbType.Boolean, "INTEGER");
			RegisterColumnType(DbType.Guid, "UNIQUEIDENTIFIER");

			RegisterProperty(ColumnProperty.Identity, "AUTOINCREMENT");
		}

		#region Overrides of SqlGenerator

		public override bool NeedsNotNullForIdentity
		{
			get { return false; }
		}

		public override string NamesQuoteTemplate
		{
			get { return "[{0}]"; }
		}

		public override string BatchSeparator
		{
			get { return "GO"; }
		}

		#endregion

		#region custom sql

		/// <summary>
		/// Check that the index with the specified name already exists
		/// </summary>
		public override bool IndexExists(string indexName, string tableName)
		{
			throw new NotSupportedException("SQLite не поддерживает индексы");
		}

		/// <summary>
		/// Добавление внешнего ключа
		/// </summary>
		public override void AddForeignKey(
			string name,
			string primaryTable,
			string[] primaryColumns,
			string refTable,
			string[] refColumns,
			ECM7.Migrator.Framework.ForeignKeyConstraint onDeleteConstraint,
			ECM7.Migrator.Framework.ForeignKeyConstraint onUpdateConstraint)
		{
			// todo: написать тесты на отсутствие поддержки внешних ключей в SQLite
			// todo: проверить, что при отдельном добавлении внешнего ключа генерируется исключение
			throw new NotSupportedException("SQLite не поддерживает внешние ключи");
		}

		/// <summary>
		/// Remove an existing foreign key constraint
		/// </summary>
		/// <param name="name">The name of the foreign key to remove</param>
		/// <param name="table">The table that contains the foreign key.</param>
		public override void RemoveForeignKey(string name, string table)
		{
			throw new NotSupportedException("SQLite не поддерживает внешние ключи");
		}

		/// <summary>
		/// Remove an existing column from a table
		/// </summary>
		/// <param name="table">The name of the table to remove the column from</param>
		/// <param name="column">The column to remove</param>
		public override void RemoveColumn(string table, string column)
		{
			throw new NotSupportedException("SQLite не поддерживает удаление колонок");
		}

		/// <summary>
		/// Rename an existing table
		/// </summary>
		/// <param name="tableName">The name of the table</param>
		/// <param name="oldColumnName">The old name of the column</param>
		/// <param name="newColumnName">The new name of the column</param>
		public override void RenameColumn(string tableName, string oldColumnName, string newColumnName)
		{
			throw new NotSupportedException("SLQite не поддерживает переименование колонок");
		}

		/// <summary>
		/// Change the definition of an existing column.
		/// </summary>
		/// <param name="table">The name of the table that will get the new column</param>
		/// <param name="column">
		/// An instance of a <see cref="Column">Column</see> with the specified properties and the name of an existing column</param>
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
			ExecuteReader(FormatSql("UPDATE {0:NAME} SET {1:NAME}={2:NAME}", table, column.Name, tempColumn));
			RemoveColumn(table, tempColumn);
		}

		/// <summary>
		/// Check if a table already exists
		/// </summary>
		/// <param name="table">The name of the table that you want to check on.</param>
		public override bool TableExists(string table)
		{
			using (IDataReader reader =
				ExecuteReader(String.Format("SELECT [name] FROM [sqlite_master] WHERE [type]='table' and [name]='{0}'", table)))
			{
				return reader.Read();
			}
		}

		/// <summary>
		/// Determines if a constraint exists.
		/// </summary>
		/// <param name="table">Table owning the constraint</param>
		/// <param name="name">Constraint name</param>
		/// <returns><c>true</c> if the constraint exists.</returns>
		public override bool ConstraintExists(string table, string name)
		{
			return false;
		}

		/// <summary>
		/// Get the names of all of the tables
		/// </summary>
		/// <returns>The names of all the tables.</returns>
		public override string[] GetTables()
		{
			List<string> tables = new List<string>();

			const string SQL = "SELECT [name] FROM [sqlite_master] WHERE [type]='table' AND [name] <> 'sqlite_sequence' ORDER BY [name]";
			using (IDataReader reader = ExecuteReader(SQL))
			{
				while (reader.Read())
				{
					tables.Add((string)reader[0]);
				}
			}

			return tables.ToArray();
		}

		#endregion
	}
}
