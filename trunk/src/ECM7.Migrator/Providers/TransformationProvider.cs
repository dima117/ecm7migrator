#region License

//The contents of this file are subject to the Mozilla Public License
//Version 1.1 (the "License"); you may not use this file except in
//compliance with the License. You may obtain a copy of the License at
//http://www.mozilla.org/MPL/
//Software distributed under the License is distributed on an "AS IS"
//basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//License for the specific language governing rights and limitations
//under the License.

#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;

using ECM7.Migrator.Compatibility;
using ECM7.Migrator.Framework;

using ForeignKeyConstraint = ECM7.Migrator.Framework.ForeignKeyConstraint;

namespace ECM7.Migrator.Providers
{
	using ECM7.Migrator.Framework.Logging;

	/// <summary>
	/// Base class for every transformation providers.
	/// A 'tranformation' is an operation that modifies the database.
	/// </summary>
	public abstract class TransformationProvider : ITransformationProvider
	{
		private const string SCHEMA_INFO_TABLE = "SchemaInfo";
		protected IDbConnection connection;
		private IDbTransaction transaction;

		protected Dialect dialect;

		private readonly ForeignKeyConstraintMapper constraintMapper = new ForeignKeyConstraintMapper();

		protected TransformationProvider(Dialect dialect, IDbConnection connection)
		{
			Require.IsNotNull(dialect, "Не задан диалект");
			this.dialect = dialect;

			Require.IsNotNull(connection, "Не инициализировано подключение к БД");
			this.connection = connection;
		}

		public bool TypeIsSupported(DbType type)
		{
			return dialect.TypeIsRegistred(type);
		}

		public Dialect Dialect
		{
			get { return dialect; }
		}

		public virtual Column[] GetColumns(string table)
		{
			List<Column> columns = new List<Column>();
			using (
				IDataReader reader =
					ExecuteQuery(
						String.Format("select COLUMN_NAME, IS_NULLABLE from information_schema.columns where table_name = '{0}'", table)))
			{
				while (reader.Read())
				{
					Column column = new Column(reader.GetString(0), DbType.String);
					string nullableStr = reader.GetString(1);
					bool isNullable = nullableStr == "YES";
					column.ColumnProperty |= isNullable ? ColumnProperty.Null : ColumnProperty.NotNull;

					columns.Add(column);
				}
			}

			return columns.ToArray();
		}

		public virtual Column GetColumnByName(string table, string columnName)
		{
			return Array.Find(GetColumns(table),
				column => column.Name.ToLower() == columnName.ToLower());
		}

		public virtual string[] GetTables()
		{
			List<string> tables = new List<string>();
			using (IDataReader reader = ExecuteQuery("SELECT table_name FROM information_schema.tables"))
			{
				while (reader.Read())
				{
					tables.Add((string)reader[0]);
				}
			}
			return tables.ToArray();
		}

		public virtual void RemoveForeignKey(string table, string name)
		{
			RemoveConstraint(table, name);
		}

		public virtual void RemoveConstraint(string table, string name)
		{
			if (TableExists(table) && ConstraintExists(table, name))
			{
				table = dialect.QuoteNameIfNeeded(table);
				name = dialect.QuoteNameIfNeeded(name);
				ExecuteNonQuery(String.Format("ALTER TABLE {0} DROP CONSTRAINT {1}", table, name));
			}
		}

		public virtual void AddTable(string table, string engine, string columns)
		{
			table = dialect.QuoteNameIfNeeded(table);
			string sqlCreate = String.Format("CREATE TABLE {0} ({1})", table, columns);
			ExecuteNonQuery(sqlCreate);
		}

		/// <summary>
		/// Add a new table
		/// </summary>
		/// <param name="name">Table name</param>
		/// <param name="columns">Columns</param>
		/// <example>
		/// Adds the Test table with two columns:
		/// <code>
		/// Database.AddTable("Test",
		///	                  new Column("Id", typeof(int), ColumnProperty.PrimaryKey),
		///	                  new Column("Title", typeof(string), 100)
		///	                 );
		/// </code>
		/// </example>
		public virtual void AddTable(string name, params Column[] columns)
		{
			// Most databases don't have the concept of a storage engine, so default is to not use it.
			AddTable(name, null, columns);
		}

		/// <summary>
		/// Add a new table
		/// </summary>
		/// <param name="name">Table name</param>
		/// <param name="columns">Columns</param>
		/// <param name="engine">the database storage engine to use</param>
		/// <example>
		/// Adds the Test table with two columns:
		/// <code>
		/// Database.AddTable("Test", "INNODB",
		///	                  new Column("Id", typeof(int), ColumnProperty.PrimaryKey),
		///	                  new Column("Title", typeof(string), 100)
		///	                 );
		/// </code>
		/// </example>
		public virtual void AddTable(string name, string engine, params Column[] columns)
		{

			if (TableExists(name))
			{
				MigratorLogManager.Log.WarnFormat("Table {0} already exists", name);
				return;
			}

			List<string> pks = GetPrimaryKeys(columns);
			bool compoundPrimaryKey = pks.Count > 1;

			List<string> listQuerySections = new List<string>(columns.Length);
			foreach (Column column in columns)
			{
				// Remove the primary key notation if compound primary key because we'll add it back later
				if (compoundPrimaryKey && column.IsPrimaryKey)
					column.ColumnProperty |= ColumnProperty.NotNull;

				string columnSql = dialect.GetColumnSql(column, compoundPrimaryKey);
				listQuerySections.Add(columnSql);
			}

			if (compoundPrimaryKey)
			{
				string primaryKeyQuerySection = BuildPrimaryKeyQuerySection(name, pks);
				listQuerySections.Add(primaryKeyQuerySection);
			}

			string sectionsSql = listQuerySections.ToCommaSeparatedString();
			AddTable(name, engine, sectionsSql);
		}

		protected virtual string BuildPrimaryKeyQuerySection(string tableName, List<string> primaryKeyColumns)
		{
			string pkName = this.QuoteName("PK_" + tableName);
			string columnNames = primaryKeyColumns.Select(QuoteName).ToCommaSeparatedString();

			string sql = "CONSTRAINT {0} PRIMARY KEY ({1})".FormatWith(pkName, columnNames);
			
			return sql;
		}

		public List<string> GetPrimaryKeys(IEnumerable<Column> columns)
		{
			return columns
				.Where(column => column.IsPrimaryKey)
				.Select(column => column.Name)
				.ToList();
		}

		public virtual void RemoveTable(string name)
		{
			if (TableExists(name))
				ExecuteNonQuery(String.Format("DROP TABLE {0}", name));
		}

		public virtual void RenameTable(string oldName, string newName)
		{
			if (TableExists(newName))
				throw new MigrationException(String.Format("Table with name '{0}' already exists", newName));

			if (TableExists(oldName))
				ExecuteNonQuery(String.Format("ALTER TABLE {0} RENAME TO {1}", oldName, newName));
		}

		public virtual void RenameColumn(string tableName, string oldColumnName, string newColumnName)
		{
			if (ColumnExists(tableName, newColumnName))
				throw new MigrationException(String.Format("Table '{0}' has column named '{1}' already", tableName, newColumnName));

			if (ColumnExists(tableName, oldColumnName))
				ExecuteNonQuery(String.Format("ALTER TABLE {0} RENAME COLUMN {1} TO {2}", tableName, oldColumnName, newColumnName));
		}

		public virtual void AddColumn(string table, string sqlColumn)
		{
			string tableName = this.QuoteName(table);
			ExecuteNonQuery(String.Format("ALTER TABLE {0} ADD COLUMN {1}", tableName, sqlColumn));
		}

		public virtual void RemoveColumn(string table, string column)
		{
			if (ColumnExists(table, column))
			{
				string columnName = this.QuoteName(column);
				string tableName = this.QuoteName(table);
				ExecuteNonQuery(String.Format("ALTER TABLE {0} DROP COLUMN {1} ", tableName, columnName));
			}
		}

		public virtual bool ColumnExists(string table, string column)
		{
			try
			{
				string columnName = this.QuoteName(column);
				string tableName = this.QuoteName(table);
				ExecuteNonQuery(String.Format("SELECT {0} FROM {1}", columnName, tableName));
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public virtual string QuoteName(string name)
		{
			return dialect.QuoteName(name);
		}

		public virtual void ChangeColumn(string table, Column column)
		{
			if (!ColumnExists(table, column.Name))
			{
				MigratorLogManager.Log.WarnFormat("Column {0}.{1} does not exist", table, column.Name);
				return;
			}

			string columnSql = dialect.GetColumnSql(column, false);
			ChangeColumn(table, columnSql);
		}

		public virtual void ChangeColumn(string table, string sqlColumn)
		{
			ExecuteNonQuery(String.Format(
				"ALTER TABLE {0} ALTER COLUMN {1}", dialect.QuoteNameIfNeeded(table), sqlColumn));
		}

		public virtual bool TableExists(string table)
		{
			try
			{
				ExecuteNonQuery("SELECT COUNT(*) FROM " + dialect.QuoteNameIfNeeded(table));
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		#region AddColumn

		/// <summary>
		/// Add a new column to an existing table.
		/// </summary>
		/// <param name="table">Table to which to add the column</param>
		/// <param name="columnName">Column name</param>
		/// <param name="type">Date type of the column</param>
		/// <param name="size">Max length of the column</param>
		/// <param name="property">Properties of the column, see <see cref="ColumnProperty">ColumnProperty</see>,</param>
		/// <param name="defaultValue">Default value</param>
		public virtual void AddColumn(string table, string columnName, DbType type, int size, ColumnProperty property,
									  object defaultValue)
		{
			Column column = new Column(columnName, type, size, property, defaultValue);
			AddColumn(table, column);
		}

		public void AddColumn(string table, Column column)
		{
			if (ColumnExists(table, column.Name))
			{
				MigratorLogManager.Log.WarnFormat("Column {0}.{1} already exists", table, column.Name);
				return;
			}

			string columnSql = dialect.GetColumnSql(column, false);

			AddColumn(table, columnSql);
		}

		public void AddColumn(string table, string columnName, ColumnType type, ColumnProperty property, object defaultValue)
		{
			Column column = new Column(columnName, type, property, defaultValue);
			AddColumn(table, column);
		}

		/// <summary>
		/// Добавление колонки
		/// </summary>
		public virtual void AddColumn(string table, string column, DbType type)
		{
			AddColumn(table, column, type, 0, ColumnProperty.Null, null);
		}

		/// <summary>
		/// Добавление колонки
		/// </summary>
		public virtual void AddColumn(string table, string column, DbType type, int size)
		{
			AddColumn(table, column, type, size, ColumnProperty.Null, null);
		}

		public void AddColumn(string table, string column, DbType type, object defaultValue)
		{
			if (ColumnExists(table, column))
			{
				MigratorLogManager.Log.WarnFormat("Column {0}.{1} already exists", table, column);
				return;
			}

			Column newColumn = new Column(column, type, defaultValue);

			AddColumn(table, newColumn);

		}

		/// <summary>
		/// Добавление колонки
		/// </summary>
		public virtual void AddColumn(string table, string column, DbType type, ColumnProperty property)
		{
			AddColumn(table, column, type, 0, property, null);
		}

		/// <summary>
		/// Добавление колонки
		/// </summary>
		public virtual void AddColumn(string table, string column, DbType type, int size, ColumnProperty property)
		{
			AddColumn(table, column, type, size, property, null);
		}

		// todo: проверить, чтобы все имена колонок и таблиц заключались в кавычки, если необходимо

		#endregion

		/// <summary>
		/// Append a primary key to a table.
		/// </summary>
		/// <param name="name">Constraint name</param>
		/// <param name="table">Table name</param>
		/// <param name="columns">Primary column names</param>
		public virtual void AddPrimaryKey(string name, string table, params string[] columns)
		{
			if (ConstraintExists(table, name))
			{
				MigratorLogManager.Log.WarnFormat("Primary key {0} already exists", name);
				return;
			}
			string sqlTableName = QuoteName(table);
			string sqlConstraintName = QuoteName(name);
			string sql = String.Format("ALTER TABLE {0} ADD CONSTRAINT {1} PRIMARY KEY ({2}) ",
							sqlTableName, sqlConstraintName,
							columns.Select(col => Dialect.QuoteNameIfNeeded(col)).ToCommaSeparatedString());

			ExecuteNonQuery(sql);
		}

		public virtual void AddUniqueConstraint(string name, string table, params string[] columns)
		{
			if (ConstraintExists(table, name))
			{
				MigratorLogManager.Log.WarnFormat("Constraint {0} already exists", name);
				return;
			}
			ExecuteNonQuery(String.Format("ALTER TABLE {0} ADD CONSTRAINT {1} UNIQUE({2}) ",
				table, name,
				columns.Select(col => Dialect.QuoteNameIfNeeded(col)).ToCommaSeparatedString()));
		}

		public virtual void AddCheckConstraint(string name, string table, string checkSql)
		{
			if (ConstraintExists(table, name))
			{
				MigratorLogManager.Log.WarnFormat("Constraint {0} already exists", name);
				return;
			}
			string sql = "ALTER TABLE {0} ADD CONSTRAINT {1} CHECK ({2}) ".FormatWith(table, name, checkSql);
			ExecuteNonQuery(sql);
		}

		#region indexes

		public void AddIndex(string name, bool unique, string table, params string[] columns)
		{
			Require.That(columns.Length > 0, "Not specified columns of the table to create an index");

			if (IndexExists(name, table))
			{
				MigratorLogManager.Log.WarnFormat("Index {0} already exists", name);
				return;
			}

			string uniqueString = unique ? "UNIQUE" : string.Empty;
			string sql = "CREATE {0} INDEX {1} ON {2} ({3})"
				.FormatWith(
					uniqueString,
					Dialect.QuoteNameIfNeeded(name),
					Dialect.QuoteNameIfNeeded(table),
					columns.Select(column => Dialect.QuoteNameIfNeeded(column)).ToCommaSeparatedString());

			ExecuteNonQuery(sql);
		}

		public abstract bool IndexExists(string indexName, string tableName);

		public virtual void RemoveIndex(string indexName, string tableName)
		{
			if (!IndexExists(indexName, tableName))
			{
				MigratorLogManager.Log.WarnFormat("Index {0} is not exists", indexName);
				return;
			}

			string sql = "DROP INDEX {0} ON {1}"
				.FormatWith(
					Dialect.QuoteNameIfNeeded(indexName),
					Dialect.QuoteNameIfNeeded(tableName));

			ExecuteNonQuery(sql);
		}

		#endregion

		#region ForeignKeys

		/// <summary>
		/// Guesses the name of the foreign key and add it
		/// </summary>
		public virtual void GenerateForeignKey(string primaryTable, string primaryColumn, string refTable, string refColumn)
		{
			AddForeignKey("FK_" + primaryTable + "_" + refTable, primaryTable, primaryColumn, refTable, refColumn);
		}

		/// <summary>
		/// Guesses the name of the foreign key and add it
		/// </summary>
		public virtual void GenerateForeignKey(string primaryTable, string[] primaryColumns, string refTable,
											   string[] refColumns)
		{
			AddForeignKey("FK_" + primaryTable + "_" + refTable, primaryTable, primaryColumns, refTable, refColumns);
		}

		/// <summary>
		/// Guesses the name of the foreign key and add it
		/// </summary>
		public virtual void GenerateForeignKey(string primaryTable, string primaryColumn, string refTable,
											   string refColumn, ForeignKeyConstraint constraint)
		{
			AddForeignKey("FK_" + primaryTable + "_" + refTable, primaryTable, primaryColumn, refTable, refColumn,
						  constraint);
		}

		/// <summary>
		/// Guesses the name of the foreign key and add it
		/// </summary>
		public virtual void GenerateForeignKey(string primaryTable, string[] primaryColumns, string refTable,
											   string[] refColumns, ForeignKeyConstraint constraint)
		{
			AddForeignKey("FK_" + primaryTable + "_" + refTable, primaryTable, primaryColumns, refTable, refColumns,
						  constraint);
		}

		/// <summary>
		/// Append a foreign key (relation) between two tables.
		/// tables.
		/// </summary>
		/// <param name="name">Constraint name</param>
		/// <param name="primaryTable">Table name containing the primary key</param>
		/// <param name="primaryColumn">Primary key column name</param>
		/// <param name="refTable">Foreign table name</param>
		/// <param name="refColumn">Foreign column name</param>
		public virtual void AddForeignKey(string name, string primaryTable, string primaryColumn, string refTable,
										  string refColumn)
		{
			AddForeignKey(name, primaryTable, new[] { primaryColumn }, refTable, new[] { refColumn });
		}

		/// <summary>
		/// <see cref="ITransformationProvider.AddForeignKey(string, string, string, string, string)">
		/// AddForeignKey(string, string, string, string, string)
		/// </see>
		/// </summary>
		public virtual void AddForeignKey(string name, string primaryTable, string[] primaryColumns, string refTable, string[] refColumns)
		{
			AddForeignKey(name, primaryTable, primaryColumns, refTable, refColumns, ForeignKeyConstraint.NoAction);
		}

		public virtual void AddForeignKey(string name, string primaryTable, string primaryColumn, string refTable, string refColumn, ForeignKeyConstraint constraint)
		{
			AddForeignKey(name, primaryTable, new[] { primaryColumn }, refTable, new[] { refColumn },
						  constraint);
		}

		public virtual void AddForeignKey(string name, string primaryTable, string[] primaryColumns, string refTable,
			string[] refColumns, ForeignKeyConstraint constraint)
		{
			AddForeignKey(name, primaryTable, primaryColumns, refTable, refColumns, constraint, ForeignKeyConstraint.NoAction);
		}

		public virtual void AddForeignKey(string name, string primaryTable, string[] primaryColumns, string refTable,
			string[] refColumns, ForeignKeyConstraint onDeleteConstraint, ForeignKeyConstraint onUpdateConstraint)
		{
			if (ConstraintExists(primaryTable, name))
			{
				MigratorLogManager.Log.WarnFormat("Constraint {0} already exists", name);
				return;
			}

			string onDeleteConstraintResolved = constraintMapper.SqlForConstraint(onDeleteConstraint);
			string onUpdateConstraintResolved = constraintMapper.SqlForConstraint(onUpdateConstraint);
			ExecuteNonQuery(
				String.Format(
					"ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3} ({4}) ON UPDATE {5} ON DELETE {6}",
					primaryTable, name, String.Join(",", primaryColumns),
					refTable, String.Join(",", refColumns), onUpdateConstraintResolved, onDeleteConstraintResolved));
		}

		#endregion

		/// <summary>
		/// Determines if a constraint exists.
		/// </summary>
		/// <param name="name">Constraint name</param>
		/// <param name="table">Table owning the constraint</param>
		/// <returns><c>true</c> if the constraint exists.</returns>
		public abstract bool ConstraintExists(string table, string name);

		public virtual bool PrimaryKeyExists(string table, string name)
		{
			return ConstraintExists(table, name);
		}

		public int ExecuteNonQuery(string sql)
		{
			MigratorLogManager.Log.ExecuteSql(sql);
			IDbCommand cmd = BuildCommand(sql);
			try
			{
				return cmd.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				MigratorLogManager.Log.Warn(ex.Message, ex);
				throw;
			}
		}

		private IDbCommand BuildCommand(string sql)
		{
			this.EnsureHasConnection();
			IDbCommand cmd = connection.CreateCommand();
			cmd.CommandText = sql;
			cmd.CommandType = CommandType.Text;
			if (transaction != null)
			{
				cmd.Transaction = transaction;
			}
			return cmd;
		}

		/// <summary>
		/// Execute an SQL query returning results.
		/// </summary>
		/// <param name="sql">The SQL command.</param>
		/// <returns>A data iterator, <see cref="System.Data.IDataReader">IDataReader</see>.</returns>
		public IDataReader ExecuteQuery(string sql)
		{
			MigratorLogManager.Log.ExecuteSql(sql);
			IDbCommand cmd = BuildCommand(sql);
			try
			{
				return cmd.ExecuteReader();
			}
			catch
			{
				MigratorLogManager.Log.WarnFormat("query failed: {0}", cmd.CommandText);
				throw;
			}
		}

		public object ExecuteScalar(string sql)
		{
			MigratorLogManager.Log.ExecuteSql(sql);
			IDbCommand cmd = BuildCommand(sql);
			try
			{
				return cmd.ExecuteScalar();
			}
			catch
			{
				MigratorLogManager.Log.WarnFormat("Query failed: {0}", cmd.CommandText);
				throw;
			}
		}

		public IDataReader Select(string what, string from)
		{
			return Select(what, from, "1=1");
		}

		public virtual IDataReader Select(string what, string from, string where)
		{
			return ExecuteQuery(String.Format("SELECT {0} FROM {1} WHERE {2}", what, from, where));
		}

		public object SelectScalar(string what, string from)
		{
			return SelectScalar(what, from, "1=1");
		}

		public virtual object SelectScalar(string what, string from, string where)
		{
			return ExecuteScalar(String.Format("SELECT {0} FROM {1} WHERE {2}", what, from, where));
		}

		public virtual int Update(string table, string[] columns, string[] values)
		{
			return Update(table, columns, values, null);
		}

		public virtual int Update(string table, string[] columns, string[] values, string where)
		{
			string namesAndValues = JoinColumnsAndValues(columns, values);

			string query = "UPDATE {0} SET {1}";
			if (!String.IsNullOrEmpty(where))
			{
				query += " WHERE " + where;
			}

			return ExecuteNonQuery(String.Format(query, table, namesAndValues));
		}

		public virtual int Insert(string table, string[] columns, string[] values)
		{
			return ExecuteNonQuery(String.Format("INSERT INTO {0} ({1}) VALUES ({2})", table, String.Join(", ", columns), String.Join(", ", QuoteValues(values))));
		}

		public virtual int Delete(string table)
		{
			return Delete(table, (string[])null, null);
		}

		public virtual int Delete(string table, string[] columns, string[] values)
		{
			return null == columns || null == values ?
					ExecuteNonQuery(String.Format("DELETE FROM {0}", table)) :
					ExecuteNonQuery(String.Format("DELETE FROM {0} WHERE ({1})",
						table, JoinColumnsAndValues(columns, values, "and")));
		}

		public virtual int Delete(string table, string wherecolumn, string wherevalue)
		{
			return ExecuteNonQuery(String.Format("DELETE FROM {0} WHERE {1} = {2}", table, wherecolumn, QuoteValues(wherevalue)));
		}

		/// <summary>
		/// Starts a transaction. Called by the migration mediator.
		/// </summary>
		public void BeginTransaction()
		{
			if (transaction == null && connection != null)
			{
				EnsureHasConnection();
				transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
			}
		}

		protected void EnsureHasConnection()
		{
			if (connection.State != ConnectionState.Open)
			{
				connection.Open();
			}
		}

		/// <summary>
		/// Rollback the current migration. Called by the migration mediator.
		/// </summary>
		public virtual void Rollback()
		{
			if (transaction != null && connection != null && connection.State == ConnectionState.Open)
			{
				try
				{
					transaction.Rollback();
				}
				finally
				{
					connection.Close();
				}
			}
			transaction = null;
		}

		/// <summary>
		/// Commit the current transaction. Called by the migrations mediator.
		/// </summary>
		public void Commit()
		{
			if (transaction != null && connection != null && connection.State == ConnectionState.Open)
			{
				try
				{
					transaction.Commit();
				}
				finally
				{
					connection.Close();
				}
			}
			transaction = null;
		}

		#region For

		/// <summary>
		/// Get this provider or a NoOp provider if you are not running in the context of 'TDialect'.
		/// </summary>
		public ITransformationProvider For<TDialect>()
		{
			return For(typeof(TDialect));
		}

		/// <summary>
		/// Get this provider or a NoOp provider if you are not running in the context of 'TDialect'.
		/// </summary>
		public ITransformationProvider For(Type dialectType)
		{
			ProviderFactory.ValidateDialectType(dialectType);
			if (Dialect.GetType() == dialectType)
				return this;

			return NoOpTransformationProvider.Instance;
		}

		/// <summary>
		/// Get this provider or a NoOp provider if you are not running in the context of 'TDialect'.
		/// </summary>
		public ITransformationProvider For(string dialectTypeName)
		{
			Type dialectType = Type.GetType(dialectTypeName);
			Require.IsNotNull(dialectType, "Не удалось загрузить тип диалекта: {0}".FormatWith(dialectTypeName.Nvl("null")));
			return For(dialectType);
		}

		/// <summary>
		/// Get this provider or a NoOp provider if you are not running in the context of 'TDialect'.
		/// </summary>
		public void For<TDialect>(Action<ITransformationProvider> actions)
		{
			For(typeof(TDialect), actions);
		}

		/// <summary>
		/// Get this provider or a NoOp provider if you are not running in the context of 'TDialect'.
		/// </summary>
		public void For(Type dialectType, Action<ITransformationProvider> actions)
		{
			ProviderFactory.ValidateDialectType(dialectType);
			if (Dialect.GetType() == dialectType)
				actions(this);
		}

		/// <summary>
		/// Get this provider or a NoOp provider if you are not running in the context of dialect with name 'dialectTypeName'.
		/// </summary>
		public void For(string dialectTypeName, Action<ITransformationProvider> actions)
		{
			Type dialectType = Type.GetType(dialectTypeName);
			Require.IsNotNull(dialectType, "Не удалось загрузить тип диалекта: {0}".FormatWith(dialectTypeName.Nvl("null")));
			For(dialectType, actions);
		}



		#endregion
		/// <summary>
		/// The list of Migrations currently applied to the database.
		/// </summary>
		public List<long> GetAppliedMigrations(string key)
		{
			Require.IsNotNull(key, "Не указан ключ миграциий");
			var appliedMigrations = new List<long>();

			CreateSchemaInfoTable();
			
			string keyColumnName = dialect.QuoteName("Key");
			string where = string.Format("{0} = '{1}'", keyColumnName, key.Replace("'", "''")); // TODO: проверить на других СУБД

			// переделать на DataTable
			using (IDataReader reader = Select("Version", SCHEMA_INFO_TABLE, where))
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
			Insert(SCHEMA_INFO_TABLE, new[] { dialect.QuoteName("Version"), dialect.QuoteName("Key") }, new[] { version.ToString(), key });
		}

		/// <summary>
		/// Marks a Migration version number as having been rolled back from the database
		/// </summary>
		/// <param name="version">The version number of the migration that was removed</param>
		/// <param name="key">Key of migration series</param>
		public void MigrationUnApplied(long version, string key)
		{
			CreateSchemaInfoTable();
			// TODO:!!!!!!!!!!!!!!!!!!!!
			Delete(SCHEMA_INFO_TABLE, new[] { "Version", "Key" }, new[] { version.ToString(), key });
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
				// AddPrimaryKey("PK_SchemaInfo", SCHEMA_INFO_TABLE, "Version", "Key");
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

		public void GenerateForeignKey(string primaryTable, string refTable)
		{
			GenerateForeignKey(primaryTable, refTable, ForeignKeyConstraint.NoAction);
		}

		public void GenerateForeignKey(string primaryTable, string refTable, ForeignKeyConstraint constraint)
		{
			GenerateForeignKey(primaryTable, refTable + "Id", refTable, "Id", constraint);
		}

		public IDbCommand GetCommand()
		{
			return BuildCommand(null);
		}

		public virtual string QuoteValues(string values)
		{
			return QuoteValues(new[] { values })[0];
		}

		public virtual string[] QuoteValues(string[] values)
		{
			return Array.ConvertAll(values,
				val => null == val ? "null" : String.Format("'{0}'", val.Replace("'", "''")));
		}

		public string JoinColumnsAndValues(string[] columns, string[] values)
		{
			return JoinColumnsAndValues(columns, values, ",");
		}

		public string JoinColumnsAndValues(string[] columns, string[] values, string separator)
		{
			Require.IsNotNull(separator, "Не задан разделитель");

			string processedSeparator = " " + separator.Trim() + " ";

			string[] quotedValues = QuoteValues(values);
			string[] namesAndValues = columns.Select((str, i) =>
				"{0}={1}".FormatWith(str, quotedValues[i])).ToArray();

			return string.Join(processedSeparator, namesAndValues);
		}

		public void ExecuteFromResource(Assembly assembly, string path)
		{
			Require.IsNotNull(assembly, "Incorrect assembly");

			using (Stream stream = assembly.GetManifestResourceStream(path))
			{
				Require.IsNotNull(stream, "Не удалось загрузить указанный файл ресурсов");

				using (StreamReader reader = new StreamReader(stream))
				{
					string sql = reader.ReadToEnd();
					ExecuteNonQuery(sql);
				}
			}
		}
	}
}
