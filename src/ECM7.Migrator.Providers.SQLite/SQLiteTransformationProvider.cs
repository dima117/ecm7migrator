namespace ECM7.Migrator.Providers.SQLite
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.SQLite;

	using ECM7.Migrator.Framework;

	using ForeignKeyConstraint = ECM7.Migrator.Framework.ForeignKeyConstraint;

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

			propertyMap.RegisterProperty(ColumnProperty.Identity, "AUTOINCREMENT");
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
			ECM7.Migrator.Framework.ForeignKeyConstraint onDeleteConstraint = ForeignKeyConstraint.NoAction,
			ECM7.Migrator.Framework.ForeignKeyConstraint onUpdateConstraint = ForeignKeyConstraint.NoAction)
		{
			// todo: написать тесты на отсутствие поддержки внешних ключей в SQLite
			// todo: проверить, что при отдельном добавлении внешнего ключа генерируется исключение
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

		public override void ChangeColumn(string table, Column column)
		{
			throw new NotSupportedException("SLQite не поддерживает изменение колонок");
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

		public override bool ColumnExists(string table, string column)
		{
			throw new NotImplementedException("Нужно реализовать проверку существования колонки");
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
