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
	using System.Text;

	using ECM7.Migrator.Framework.Logging;

	/// <summary>
	/// Base class for every transformation providers.
	/// A 'tranformation' is an operation that modifies the database.
	/// </summary>
	public abstract class TransformationProvider<TConnection> : SqlGenerator, ITransformationProvider
		where TConnection : IDbConnection
	{
		private const string SCHEMA_INFO_TABLE = "SchemaInfo";
		protected IDbConnection connection;
		private IDbTransaction transaction;

		protected TransformationProvider(TConnection connection)
		{
			Require.IsNotNull(connection, "Не инициализировано подключение к БД");
			this.connection = connection;

			RegisterProperty(ColumnProperty.Null, "NULL");
			RegisterProperty(ColumnProperty.NotNull, "NOT NULL");
			RegisterProperty(ColumnProperty.Unique, "UNIQUE");
			RegisterProperty(ColumnProperty.PrimaryKey, "PRIMARY KEY");
		}

		public virtual Column[] GetColumns(string table)
		{
			List<Column> columns = new List<Column>();

			string sql = FormatSql("select {0:NAME}, {1:NAME} from {2:NAME}.{3:NAME} where {4:NAME} = '{5}'",
							"column_name", "is_nullable", "information_schema", "columns", "table_name", table);
			using (IDataReader reader = ExecuteQuery(sql))
			{
				while (reader.Read())
				{
					Column column = new Column(reader.GetString(0), DbType.String);
					string nullableStr = reader.GetString(1).ToUpper();
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
				column => column.Name == columnName);
		}

		public virtual string[] GetTables()
		{
			List<string> tables = new List<string>();
			string sql = FormatSql("SELECT {0:NAME} FROM {1:NAME}.{2:NAME}",
				"table_name", "information_schema", "tables");

			using (IDataReader reader = ExecuteQuery(sql))
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
				string format = this.FormatSql("ALTER TABLE {0:NAME} DROP CONSTRAINT {1:NAME}", table, name);
				ExecuteNonQuery(format);
			}
		}

		public virtual void AddTable(string table, string engine, string columnsSql)
		{
			string sqlCreate = this.FormatSql("CREATE TABLE {0:NAME} ({1})", table, columnsSql);
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

				string columnSql = GetColumnSql(column, compoundPrimaryKey);
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
			string pkName = "PK_" + tableName;

			return FormatSql("CONSTRAINT {0:NAME} PRIMARY KEY ({1:COLS})", pkName, primaryKeyColumns);

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
			{
				ExecuteNonQuery(FormatSql("DROP TABLE {0:NAME}", name));
			}
		}

		public virtual void RenameTable(string oldName, string newName)
		{
			if (TableExists(newName))
			{
				throw new MigrationException(String.Format("Table with name '{0}' already exists", newName));
			}

			if (TableExists(oldName))
			{
				string sql = FormatSql("ALTER TABLE {0:NAME} RENAME TO {1:NAME}", oldName, newName);
				this.ExecuteNonQuery(sql);
			}
		}

		public virtual void RenameColumn(string tableName, string oldColumnName, string newColumnName)
		{
			if (ColumnExists(tableName, newColumnName))
				throw new MigrationException(String.Format("Table '{0}' has column named '{1}' already", tableName, newColumnName));

			if (ColumnExists(tableName, oldColumnName))
			{
				string sql = FormatSql("ALTER TABLE {0:NAME} RENAME COLUMN {1:NAME} TO {2:NAME}",
					tableName, oldColumnName, newColumnName);
				ExecuteNonQuery(sql);
			}
		}

		public virtual void RemoveColumn(string table, string column)
		{
			if (ColumnExists(table, column))
			{
				string sql = this.FormatSql("ALTER TABLE {0:NAME} DROP COLUMN {1:NAME} ", table, column);
				ExecuteNonQuery(sql);
			}
		}

		public virtual bool ColumnExists(string table, string column)
		{
			try
			{
				string sql = FormatSql("SELECT {0:NAME} FROM {1:NAME}", column, table);
				ExecuteNonQuery(sql);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public virtual void ChangeColumn(string table, Column column)
		{
			if (!ColumnExists(table, column.Name))
			{
				MigratorLogManager.Log.WarnFormat("Column {0}.{1} does not exist", table, column.Name);
				return;
			}

			string columnSql = GetColumnSql(column, false);
			ChangeColumn(table, columnSql);
		}

		public virtual void ChangeColumn(string table, string columnSql)
		{
			string sql = FormatSql("ALTER TABLE {0:NAME} ALTER COLUMN {1}", table, columnSql);
			ExecuteNonQuery(sql);
		}

		public virtual bool TableExists(string table)
		{
			try
			{
				string sql = FormatSql("SELECT COUNT(*) FROM {0:NAME}", table);
				ExecuteNonQuery(sql);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		#region AddColumn

		public virtual void AddColumn(string table, string columnSql)
		{
			string sql = FormatSql("ALTER TABLE {0:NAME} ADD COLUMN {1}", table, columnSql);
			ExecuteNonQuery(sql);
		}

		public void AddColumn(string table, Column column)
		{
			if (ColumnExists(table, column.Name))
			{
				MigratorLogManager.Log.WarnFormat("Column {0}.{1} already exists", table, column.Name);
				return;
			}

			string columnSql = GetColumnSql(column, false);

			AddColumn(table, columnSql);
		}

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
			string sql = FormatSql(
				"ALTER TABLE {0:NAME} ADD CONSTRAINT {1:NAME} PRIMARY KEY ({2:COLS}) ",
				table, name, columns);

			ExecuteNonQuery(sql);
		}

		public virtual void AddUniqueConstraint(string name, string table, params string[] columns)
		{
			if (ConstraintExists(table, name))
			{
				MigratorLogManager.Log.WarnFormat("Constraint {0} already exists", name);
				return;
			}

			string sql = FormatSql(
				"ALTER TABLE {0:NAME} ADD CONSTRAINT {1:NAME} UNIQUE({2:COLS})",
				table, name, columns);

			ExecuteNonQuery(sql);
		}

		public virtual void AddCheckConstraint(string name, string table, string checkSql)
		{
			if (ConstraintExists(table, name))
			{
				MigratorLogManager.Log.WarnFormat("Constraint {0} already exists", name);
				return;
			}

			string sql = FormatSql("ALTER TABLE {0:NAME} ADD CONSTRAINT {1:NAME} CHECK ({2}) ", table, name, checkSql);
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
			string sql = FormatSql("CREATE {0} INDEX {1:NAME} ON {2:NAME} ({3:COLS})",
				uniqueString, name, table, columns);

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

			string sql = FormatSql("DROP INDEX {0:NAME} ON {1:NAME}", indexName, tableName);
			ExecuteNonQuery(sql);
		}

		#endregion

		#region ForeignKeys

		public void GenerateForeignKey(string primaryTable, string refTable)
		{
			GenerateForeignKey(primaryTable, refTable, ForeignKeyConstraint.NoAction);
		}

		public void GenerateForeignKey(string primaryTable, string refTable, ForeignKeyConstraint constraint)
		{
			GenerateForeignKey(primaryTable, refTable + "Id", refTable, "Id", constraint);
		}

		/// <summary>
		/// Guesses the name of the foreign key and add it
		/// </summary>
		public virtual void GenerateForeignKey(string primaryTable, string primaryColumn, string refTable, string refColumn)
		{
			string fkName = "FK_" + primaryTable + "_" + refTable;
			AddForeignKey(fkName, primaryTable, primaryColumn, refTable, refColumn);
		}

		/// <summary>
		/// Guesses the name of the foreign key and add it
		/// </summary>
		public virtual void GenerateForeignKey(string primaryTable, string[] primaryColumns, string refTable,
											   string[] refColumns)
		{
			string fkName = "FK_" + primaryTable + "_" + refTable;
			AddForeignKey(fkName, primaryTable, primaryColumns, refTable, refColumns);
		}

		/// <summary>
		/// Guesses the name of the foreign key and add it
		/// </summary>
		public virtual void GenerateForeignKey(string primaryTable, string primaryColumn, string refTable,
											   string refColumn, ForeignKeyConstraint constraint)
		{
			string fkName = "FK_" + primaryTable + "_" + refTable;
			AddForeignKey(fkName, primaryTable, primaryColumn, refTable, refColumn, constraint);
		}

		/// <summary>
		/// Guesses the name of the foreign key and add it
		/// </summary>
		public virtual void GenerateForeignKey(string primaryTable, string[] primaryColumns, string refTable,
											   string[] refColumns, ForeignKeyConstraint constraint)
		{
			string fkName = "FK_" + primaryTable + "_" + refTable;
			AddForeignKey(fkName, primaryTable, primaryColumns, refTable, refColumns, constraint);
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
			AddForeignKey(name, primaryTable, new[] { primaryColumn }, refTable, new[] { refColumn }, constraint);
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

			string onDeleteConstraintResolved = SqlForConstraint(onDeleteConstraint);
			string onUpdateConstraintResolved = SqlForConstraint(onUpdateConstraint);

			string sql = FormatSql(
				"ALTER TABLE {0:NAME} ADD CONSTRAINT {1:NAME} FOREIGN KEY ({2:COLS}) REFERENCES {3:NAME} ({4:COLS}) ON UPDATE {5} ON DELETE {6}",
				primaryTable, name, primaryColumns, refTable, refColumns, onUpdateConstraintResolved, onDeleteConstraintResolved);

			ExecuteNonQuery(sql);
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
			int result = 0;

			try
			{
				if (!BatchSeparator.IsNullOrEmpty(true) &&
					sql.IndexOf(BatchSeparator, StringComparison.CurrentCultureIgnoreCase) >= 0)
				{
					// если задан разделитель пакетов запросов, запускаем пакеты по очереди
					sql += "\n" + BatchSeparator.Trim(); // make sure last batch is executed.
					string[] lines = sql.Split(new[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
					StringBuilder sqlBatch = new StringBuilder();

					foreach (string line in lines)
					{
						if (line.ToUpperInvariant().Trim() == BatchSeparator.ToUpperInvariant())
						{
							string query = sqlBatch.ToString();
							if (!query.IsNullOrEmpty(true))
							{
								MigratorLogManager.Log.ExecuteSql(query);
								IDbCommand cmd = this.BuildCommand(query);
								result = cmd.ExecuteNonQuery();
							}
							sqlBatch.Clear();
						}
						else
						{
							sqlBatch.AppendLine(line.Trim());
						}
					}
				}
				else
				{
					MigratorLogManager.Log.ExecuteSql(sql);
					IDbCommand cmd = this.BuildCommand(sql);
					result = cmd.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				MigratorLogManager.Log.Warn(ex.Message, ex);
				throw;
			}

			return result;
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

		public virtual int Update(string table, string[] columns, string[] values)
		{
			return Update(table, columns, values, null);
		}

		public virtual int Update(string table, string[] columns, string[] values, string where)
		{
			string namesAndValues = JoinColumnsAndValues(columns, values);

			string query = where.IsNullOrEmpty(true)
								? "UPDATE {0:NAME} SET {1}"
								: "UPDATE {0:NAME} SET {1} WHERE {2}";

			string sql = FormatSql(query, table, namesAndValues, where);
			return ExecuteNonQuery(sql);
		}

		public virtual int Insert(string table, string[] columns, string[] values)
		{
			string sql = FormatSql("INSERT INTO {0:NAME} ({1:COLS}) VALUES ({2})",
				table, columns, QuoteValues(values).ToCommaSeparatedString());

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
		/// Get this provider or a NoOp provider if you are not running in the context of 'TTargetProvider'.
		/// </summary>
		public void For<TDialect>(Action<ITransformationProvider> actions)
		{
			For(typeof(TDialect), actions);
		}

		/// <summary>
		/// Get this provider or a NoOp provider if you are not running in the context of 'TTargetProvider'.
		/// </summary>
		public void For(Type providerType, Action<ITransformationProvider> actions)
		{
			if (GetType() == providerType)
				actions(this);
		}

		/// <summary>
		/// Get this provider or a NoOp provider if you are not running in the context of dialect with name 'providerTypeName'.
		/// </summary>
		public void For(string providerTypeName, Action<ITransformationProvider> actions)
		{
			Type providerType = Type.GetType(providerTypeName);
			Require.IsNotNull(providerType, "Не удалось загрузить тип провайдера: {0}".FormatWith(providerTypeName.Nvl("null")));
			For(providerType, actions);
		}

		#endregion
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

			using (IDataReader reader = ExecuteQuery(sql))
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

		public IDbCommand GetCommand()
		{
			return BuildCommand(null);
		}

		public virtual string[] QuoteValues(params string[] values)
		{
			return Array.ConvertAll(values,
				val => null == val ? "null" : String.Format("'{0}'", val.Replace("'", "''")));
		}

		public string JoinColumnsAndValues(string[] columns, string[] values, string separator = ",")
		{
			Require.IsNotNull(separator, "Не задан разделитель");

			string processedSeparator = " " + separator.Trim() + " ";

			string[] quotedValues = QuoteValues(values);
			string[] namesAndValues = columns
				.Select((col, i) => FormatSql("{0:NAME}={1}", col, quotedValues[i]))
				.ToArray();

			return string.Join(processedSeparator, namesAndValues);
		}

		public void ExecuteFromResource(Assembly assembly, string path)
		{
			Require.IsNotNull(assembly, "Incorrect assembly");

			using (Stream stream = assembly.GetManifestResourceStream(path))
			{
				Require.IsNotNull(stream, "Не удалось загрузить указанный файл ресурсов");

				// ReSharper disable AssignNullToNotNullAttribute
				using (StreamReader reader = new StreamReader(stream))
				{
					string sql = reader.ReadToEnd();
					ExecuteNonQuery(sql);
				}
				// ReSharper restore AssignNullToNotNullAttribute
			}
		}
	}
}
