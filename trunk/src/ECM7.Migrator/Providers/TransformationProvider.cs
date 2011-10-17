using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using ECM7.Migrator.Compatibility;
using ECM7.Migrator.Framework;

using ForeignKeyConstraint = ECM7.Migrator.Framework.ForeignKeyConstraint;

namespace ECM7.Migrator.Providers
{
	/// <summary>
	/// Base class for every transformation providers.
	/// A 'tranformation' is an operation that modifies the database.
	/// </summary>
	public abstract class TransformationProvider<TConnection> : SqlRunner<TConnection>, ITransformationProvider
		where TConnection : IDbConnection
	{
		private const string SCHEMA_INFO_TABLE = "SchemaInfo";

		protected readonly IFormatProvider sqlFormatProvider;
		protected readonly PropertyMap propertyMap = new PropertyMap();
		protected readonly TypeMap typeMap = new TypeMap();
		protected readonly ForeignKeyActionMap fkActionMap = new ForeignKeyActionMap();


		protected TransformationProvider(TConnection connection)
			: base(connection)
		{
			sqlFormatProvider = new SqlFormatter(obj => string.Format(NamesQuoteTemplate, obj));

			propertyMap.RegisterPropertySql(ColumnProperty.Null, "NULL");
			propertyMap.RegisterPropertySql(ColumnProperty.NotNull, "NOT NULL");
			propertyMap.RegisterPropertySql(ColumnProperty.Unique, "UNIQUE");
			propertyMap.RegisterPropertySql(ColumnProperty.PrimaryKey, "PRIMARY KEY");

			fkActionMap.RegisterSql(ForeignKeyConstraint.Cascade, "CASCADE");
			fkActionMap.RegisterSql(ForeignKeyConstraint.SetDefault, "SET DEFAULT");
			fkActionMap.RegisterSql(ForeignKeyConstraint.SetNull, "SET NULL");
			fkActionMap.RegisterSql(ForeignKeyConstraint.NoAction, "NO ACTION");
		}

		#region common

		public string FormatSql(string format, params object[] args)
		{
			return string.Format(sqlFormatProvider, format, args);
		}

		public IConditionByProvider ConditionalExecuteAction()
		{
			return new ConditionByProvider(this);
		}

		#endregion

		#region Особенности СУБД

		public virtual bool IdentityNeedsType
		{
			get { return true; }
		}

		public virtual bool NeedsNotNullForIdentity
		{
			get { return true; }
		}

		public bool TypeIsSupported(DbType type)
		{
			return typeMap.HasType(type);
		}

		protected virtual string NamesQuoteTemplate
		{
			get { return "\"{0}\""; }
		}

		#endregion

		#region generate sql

		protected virtual string GetSqlAddTable(string table, string engine, string columnsSql)
		{
			return FormatSql("CREATE TABLE {0:NAME} ({1})", table, columnsSql);
		}

		protected virtual string GetSqlRemoveTable(string table)
		{
			return FormatSql("DROP TABLE {0:NAME}", table);
		}

		public virtual string GetSqlColumnDef(Column column, bool compoundPrimaryKey)
		{
			ColumnSqlBuilder sqlBuilder = new ColumnSqlBuilder(column, typeMap, propertyMap);

			sqlBuilder.AddColumnName(NamesQuoteTemplate);
			sqlBuilder.AddColumnType(IdentityNeedsType);

			// identity не нуждается в типе
			sqlBuilder.AddSqlForIdentityWhichNotNeedsType(IdentityNeedsType);
			sqlBuilder.AddUnsignedSql();
			sqlBuilder.AddNotNullSql(NeedsNotNullForIdentity);
			sqlBuilder.AddPrimaryKeySql(compoundPrimaryKey);

			// identity нуждается в типе
			sqlBuilder.AddSqlForIdentityWhichNeedsType(IdentityNeedsType);
			sqlBuilder.AddUniqueSql();
			sqlBuilder.AddDefaultValueSql(GetSqlDefaultValue);

			return sqlBuilder.ToString();
		}

		protected virtual string GetSqlPrimaryKey(string tableName, List<string> primaryKeyColumns)
		{
			string pkName = "PK_" + tableName;

			return FormatSql("CONSTRAINT {0:NAME} PRIMARY KEY ({1:COLS})", pkName, primaryKeyColumns);

		}

		protected virtual string GetSqlDefaultValue(object defaultValue)
		{
			return string.Format("DEFAULT {0}", defaultValue);
		}

		protected virtual string GetSqlAddColumn(string table, string columnSql)
		{
			return FormatSql("ALTER TABLE {0:NAME} ADD COLUMN {1}", table, columnSql);
		}

		protected virtual string GetSqlChangeColumnType(string table, string column, ColumnType columnType)
		{
			string columnTypeSql = typeMap.Get(columnType);

			return FormatSql("ALTER TABLE {0:NAME} ALTER COLUMN {1:NAME} {2}", table, column, columnTypeSql);
		}

		protected virtual string GetSqlChangeNotNullConstraint(string table, string column, bool notNull, ref string sqlChangeColumnType)
		{
			// если изменение типа колонки и признака NOT NULL происходит одним запросом,
			// то изменяем параметр sqlChangeColumnType и возвращаем NULL
			// иначе возвращаем запрос, меняющий признак NOT NULL

			sqlChangeColumnType += notNull ? " NOT NULL" : " NULL";

			return null;
		}
		
		protected virtual string GetSqlRenameColumn(string tableName, string oldColumnName, string newColumnName)
		{
			return FormatSql("ALTER TABLE {0:NAME} RENAME COLUMN {1:NAME} TO {2:NAME}",
					tableName, oldColumnName, newColumnName);
		}

		protected virtual string GetSqlRemoveColumn(string table, string column)
		{
			return FormatSql("ALTER TABLE {0:NAME} DROP COLUMN {1:NAME} ", table, column);
		}

		protected virtual string GetSqlRenameTable(string oldName, string newName)
		{
			return FormatSql("ALTER TABLE {0:NAME} RENAME TO {1:NAME}", oldName, newName);
		}

		protected virtual string GetSqlRemoveIndex(string indexName, string tableName)
		{
			return FormatSql("DROP INDEX {0:NAME} ON {1:NAME}", indexName, tableName);
		}

		protected virtual string GetSqlRemoveConstraint(string table, string name)
		{
			return FormatSql("ALTER TABLE {0:NAME} DROP CONSTRAINT {1:NAME}", table, name);
		}

		#endregion

		#region DDL

		#region tables

		public void AddTable(string name, params Column[] columns)
		{
			AddTable(name, null, columns);
		}

		public virtual void AddTable(string name, string engine, params Column[] columns)
		{
			// список колонок, входящих в первичный ключ
			List<string> pks = columns
				.Where(column => column.IsPrimaryKey)
				.Select(column => column.Name)
				.ToList();

			bool compoundPrimaryKey = pks.Count > 1;

			List<string> querySections = new List<string>();

			// SQL для колонок таблицы
			foreach (Column column in columns)
			{
				// Remove the primary key notation if compound primary key because we'll add it back later
				if (compoundPrimaryKey && column.IsPrimaryKey)
				{
					column.ColumnProperty |= ColumnProperty.PrimaryKey;
				}

				string columnSql = GetSqlColumnDef(column, compoundPrimaryKey);
				querySections.Add(columnSql);
			}

			// SQL для составного первичного ключа
			if (compoundPrimaryKey)
			{
				string primaryKeyQuerySection = GetSqlPrimaryKey(name, pks);
				querySections.Add(primaryKeyQuerySection);
			}

			string sqlQuerySections = querySections.ToCommaSeparatedString();
			string createTableSql = GetSqlAddTable(name, engine, sqlQuerySections);

			ExecuteNonQuery(createTableSql);
		}

		public abstract string[] GetTables();

		public abstract bool TableExists(string table);

		public virtual void RenameTable(string oldName, string newName)
		{
			string sql = GetSqlRenameTable(oldName, newName);
			ExecuteNonQuery(sql);
		}

		public virtual void RemoveTable(string name)
		{
			string sql = GetSqlRemoveTable(name);
			ExecuteNonQuery(sql);
		}

		#endregion

		#region columns

		public virtual void AddColumn(string table, Column column)
		{
			string sqlColumnDef = GetSqlColumnDef(column, false);
			string sqlAddColumn = GetSqlAddColumn(table, sqlColumnDef);

			ExecuteNonQuery(sqlAddColumn);
		}

		public virtual void ChangeColumn(string table, string column, ColumnType columnType, bool notNull)
		{
			string sqlChangeColumn = GetSqlChangeColumnType(table, column, columnType);
			string sqlChangeNotNullConstraint = GetSqlChangeNotNullConstraint(
				table, column, notNull, ref sqlChangeColumn);

			if (!sqlChangeColumn.IsNullOrEmpty(true))
			{
				ExecuteNonQuery(sqlChangeColumn);
			}

			if (!sqlChangeNotNullConstraint.IsNullOrEmpty(true))
			{
				ExecuteNonQuery(sqlChangeNotNullConstraint);
			}
		}

		public virtual void ChangeDefaultValue(string table, string column, object newDefaultValue)
		{
		}

		public virtual void RenameColumn(string tableName, string oldColumnName, string newColumnName)
		{
			string sql = GetSqlRenameColumn(tableName, oldColumnName, newColumnName);
			ExecuteNonQuery(sql);
		}

		public abstract bool ColumnExists(string table, string column);

		public virtual void RemoveColumn(string table, string column)
		{
			string sql = GetSqlRemoveColumn(table, column);
			ExecuteNonQuery(sql);
		}

		#endregion

		#region constraints

		public void AddForeignKey(string name,
			string primaryTable, string primaryColumn, string refTable, string refColumn,
			ForeignKeyConstraint onDeleteConstraint = ForeignKeyConstraint.NoAction,
			ForeignKeyConstraint onUpdateConstraint = ForeignKeyConstraint.NoAction)
		{
			AddForeignKey(name,
				primaryTable, primaryColumn.AsArray(),
				refTable, refColumn.AsArray(),
				onDeleteConstraint, onUpdateConstraint);
		}

		public virtual void AddForeignKey(string name,
			string primaryTable, string[] primaryColumns,
			string refTable, string[] refColumns,
			ForeignKeyConstraint onDeleteConstraint = ForeignKeyConstraint.NoAction,
			ForeignKeyConstraint onUpdateConstraint = ForeignKeyConstraint.NoAction)
		{
			string onDeleteConstraintResolved = fkActionMap.GetSqlOnDelete(onDeleteConstraint);
			string onUpdateConstraintResolved = fkActionMap.GetSqlOnUpdate(onUpdateConstraint);

			string sql = FormatSql(
				"ALTER TABLE {0:NAME} ADD CONSTRAINT {1:NAME} FOREIGN KEY ({2:COLS}) REFERENCES {3:NAME} ({4:COLS}) {5} {6}",
				primaryTable, name, primaryColumns, refTable, refColumns, onUpdateConstraintResolved, onDeleteConstraintResolved);

			ExecuteNonQuery(sql);
		}

		public virtual void AddPrimaryKey(string name, string table, params string[] columns)
		{
			string sql = FormatSql(
				"ALTER TABLE {0:NAME} ADD CONSTRAINT {1:NAME} PRIMARY KEY ({2:COLS})", table, name, columns);

			ExecuteNonQuery(sql);
		}

		public virtual void AddUniqueConstraint(string name, string table, params string[] columns)
		{
			string sql = FormatSql(
				"ALTER TABLE {0:NAME} ADD CONSTRAINT {1:NAME} UNIQUE({2:COLS})", table, name, columns);

			ExecuteNonQuery(sql);
		}

		public virtual void AddCheckConstraint(string name, string table, string checkSql)
		{
			string sql = FormatSql(
				"ALTER TABLE {0:NAME} ADD CONSTRAINT {1:NAME} CHECK ({2}) ", table, name, checkSql);

			ExecuteNonQuery(sql);
		}

		public abstract bool ConstraintExists(string table, string name);

		public virtual void RemoveConstraint(string table, string name)
		{
			string format = GetSqlRemoveConstraint(table, name);

			ExecuteNonQuery(format);
		}

		#endregion

		#region indexes

		public virtual void AddIndex(string name, bool unique, string table, params string[] columns)
		{
			Require.That(columns.Length > 0, "Not specified columns of the table to create an index");

			string uniqueString = unique ? "UNIQUE" : string.Empty;
			string sql = FormatSql("CREATE {0} INDEX {1:NAME} ON {2:NAME} ({3:COLS})",
				uniqueString, name, table, columns);

			ExecuteNonQuery(sql);
		}

		public abstract bool IndexExists(string indexName, string tableName);

		public virtual void RemoveIndex(string indexName, string tableName)
		{
			string sql = GetSqlRemoveIndex(indexName, tableName);

			ExecuteNonQuery(sql);
		}

		#endregion

		#endregion

		#region DML

		public virtual int Insert(string table, string[] columns, string[] values)
		{
			var quotedValues = values.Select(val =>
				null == val
					? "null"
					: string.Format("'{0}'", val.Replace("'", "''")));

			// todo: сделать, чтобы методы insert и update получали массив object
			string sql = FormatSql("INSERT INTO {0:NAME} ({1:COLS}) VALUES ({2})",
				table, columns, quotedValues.ToCommaSeparatedString());

			return ExecuteNonQuery(sql);
		}

		public virtual int Update(string table, string[] columns, string[] values, string whereSql = null)
		{
			var quotedValues = values
				.Select(val =>
					null == val
						? "null"
						: string.Format("'{0}'", val.Replace("'", "''")))
				.ToList();

			string namesAndValues = columns
				.Select((col, i) => FormatSql("{0:NAME}={1}", col, quotedValues[i]))
				.ToCommaSeparatedString();

			string query = whereSql.IsNullOrEmpty(true)
								? "UPDATE {0:NAME} SET {1}"
								: "UPDATE {0:NAME} SET {1} WHERE {2}";

			string sql = FormatSql(query, table, namesAndValues, whereSql);
			return ExecuteNonQuery(sql);
		}

		public virtual int Delete(string table, string whereSql = null)
		{
			string format = whereSql.IsNullOrEmpty(true)
								? "DELETE FROM {0:NAME}"
								: "DELETE FROM {0:NAME} WHERE {1}";

			string sql = FormatSql(format, table, whereSql);

			return ExecuteNonQuery(sql);
		}

		#endregion

		#region methods for migrator core

		/// <summary>
		/// The list of Migrations currently applied to the database.
		/// </summary>
		public List<long> GetAppliedMigrations(string key = "")
		{
			Require.IsNotNull(key, "Не указан ключ миграциий");
			var appliedMigrations = new List<long>();

			CreateSchemaInfoTable();

			string sql = FormatSql("SELECT {0:NAME} FROM {1:NAME} WHERE {2:NAME} = '{3}'",
				"Version", SCHEMA_INFO_TABLE, "Key", key.Replace("'", "''"));

			// todo: написать тест, который выполняет миграцию, а потом проверяет, что она сохранилась в БД
			using (IDataReader reader = ExecuteReader(sql))
			{
				while (reader.Read())
				{
					appliedMigrations.Add(reader.GetInt64(0));
				}
			}

			appliedMigrations.Sort();

			return appliedMigrations;
		}

		/// <summary>
		/// Marks a Migration version number as having been applied
		/// </summary>
		/// <param name="version">The version number of the migration that was applied</param>
		/// <param name="key">Key of migration series</param>
		public void MigrationApplied(long version, string key)
		{
			CreateSchemaInfoTable();
			Insert(SCHEMA_INFO_TABLE, new[] { "Version", "Key" }, new[] { version.ToString(), key });
		}

		/// <summary>
		/// Marks a Migration version number as having been rolled back from the database
		/// </summary>
		/// <param name="version">The version number of the migration that was removed</param>
		/// <param name="key">Key of migration series</param>
		public void MigrationUnApplied(long version, string key)
		{
			CreateSchemaInfoTable();

			string whereSql = FormatSql("{0:NAME} = {1} AND {2:NAME} = '{3}'",
				"Version", version, "Key", key);

			Delete(SCHEMA_INFO_TABLE, whereSql);
		}

		protected void CreateSchemaInfoTable()
		{
			EnsureHasConnection();

			if (!TableExists(SCHEMA_INFO_TABLE))
			{
				AddTable(
					SCHEMA_INFO_TABLE,
					new Column("Version", DbType.Int64, ColumnProperty.PrimaryKey),
					new Column("Key", DbType.String.WithSize(200), ColumnProperty.PrimaryKey, "''"));
			}
			else
			{
				if (!ColumnExists(SCHEMA_INFO_TABLE, "Key"))
				{
					// TODO: Удалить код совместимости для старой таблицы SchemaInfo в следующих версиях
					UpdateSchemaInfo.Update(this);
				}
			}
		}

		#endregion
	}
}
