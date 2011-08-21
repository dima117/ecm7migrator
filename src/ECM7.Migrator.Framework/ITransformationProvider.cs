using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace ECM7.Migrator.Framework
{
	/// <summary>
	/// The main interface to use in Migrations to make changes on a database schema.
	/// </summary>
	public interface ITransformationProvider : IDisposable
	{
		IDbConnection Connection { get; }

		/// <summary>
		/// The list of Migrations currently applied to the database.
		/// </summary>
		List<long> GetAppliedMigrations(string key = "");
		
		/// <summary>
		/// Add a column to an existing table
		/// </summary>
		/// <param name="table">The name of the table that will get the new column</param>
		/// <param name="columnName">The name of the new column</param>
		/// <param name="type">The data type for the new column</param>
		/// <param name="size">The precision or size of the column</param>
		/// <param name="property">Properties that can be ORed together</param>
		/// <param name="defaultValue">The default value of the column if no value is given in a query</param>
		void AddColumn(string table, string columnName, DbType type, int size, ColumnProperty property, object defaultValue);

		/// <summary>
		/// Add a column to an existing table
		/// </summary>
		/// <param name="table">The name of the table that will get the new column</param>
		/// <param name="columnName">The name of the new column</param>
		/// <param name="type">The data type for the new column</param>
		/// <param name="property">Properties that can be ORed together</param>
		/// <param name="defaultValue">The default value of the column if no value is given in a query</param>
		void AddColumn(string table, string columnName, ColumnType type, ColumnProperty property, object defaultValue);

		/// <summary>
		/// Add a column to an existing table
		/// </summary>
		/// <param name="table">The name of the table that will get the new column</param>
		/// <param name="column">The name of the new column</param>
		/// <param name="type">The data type for the new columnd</param>
		void AddColumn(string table, string column, DbType type);
		
		/// <summary>
		/// Add a column to an existing table
		/// </summary>
		/// <param name="table">The name of the table that will get the new column</param>
		/// <param name="column">The name of the new column</param>
		/// <param name="type">The data type for the new columnd</param>
		/// <param name="size">The precision or size of the column</param>
		void AddColumn(string table, string column, DbType type, int size);

		/// <summary>
		/// Add a column to an existing table
		/// </summary>
		/// <param name="table">The name of the table that will get the new column</param>
		/// <param name="column">The name of the new column</param>
		/// <param name="type">The data type for the new columnd</param>
		/// <param name="size">The precision or size of the column</param>
		/// <param name="property">Properties that can be ORed together</param>
		void AddColumn(string table, string column, DbType type, int size, ColumnProperty property);

		/// <summary>
		/// Add a column to an existing table
		/// </summary>
		/// <param name="table">The name of the table that will get the new column</param>
		/// <param name="column">The name of the new column</param>
		/// <param name="type">The data type for the new columnd</param>
		/// <param name="property">Properties that can be ORed together</param>
		void AddColumn(string table, string column, DbType type, ColumnProperty property);

		/// <summary>
		/// Add a column to an existing table with the default column size.
		/// </summary>
		/// <param name="table">The name of the table that will get the new column</param>
		/// <param name="column">The name of the new column</param>
		/// <param name="type">The data type for the new columnd</param>
		/// <param name="defaultValue">The default value of the column if no value is given in a query</param>
		void AddColumn(string table, string column, DbType type, object defaultValue);

		/// <summary>
		/// Add a column to an existing table
		/// </summary>
		/// <param name="table">The name of the table that will get the new column</param>
		/// <param name="column">An instance of a <see cref="Column">Column</see> with the specified properties</param>
		void AddColumn(string table, Column column);

		/// <summary>
		/// Add a foreign key constraint
		/// </summary>
		/// <param name="name">The name of the foreign key. e.g. FK_TABLE_REF</param>
		/// <param name="foreignTable">The table that the foreign key will be created in (eg. Table.FK_id)</param>
		/// <param name="foreignColumns">The columns that are the foreign keys (eg. FK_id)</param>
		/// <param name="primaryTable">The table that holds the primary keys (eg. Table.PK_id)</param>
		/// <param name="primaryColumns">The columns that are the primary keys (eg. PK_id)</param>
		void AddForeignKey(string name, string foreignTable, string[] foreignColumns, string primaryTable, string[] primaryColumns);

		/// <summary>
		/// Add a foreign key constraint
		/// </summary>
		/// <param name="name">The name of the foreign key. e.g. FK_TABLE_REF</param>
		/// <param name="foreignTable">The table that the foreign key will be created in (eg. Table.FK_id)</param>
		/// <param name="foreignColumns">The columns that are the foreign keys (eg. FK_id)</param>
		/// <param name="primaryTable">The table that holds the primary keys (eg. Table.PK_id)</param>
		/// <param name="primaryColumns">The columns that are the primary keys (eg. PK_id)</param>
		/// <param name="constraint">Constraint parameters</param>
		void AddForeignKey(string name, string foreignTable, string[] foreignColumns, string primaryTable, string[] primaryColumns, ForeignKeyConstraint constraint);

		/// <summary>
		/// Add a foreign key constraint
		/// </summary>
		/// 
		/// <param name="name">The name of the foreign key. e.g. FK_TABLE_REF</param>
		/// <param name="foreignTable">The table that the foreign key will be created in (eg. Table.FK_id)</param>
		/// <param name="foreignColumn">The column that is the foreign key (eg. FK_id)</param>
		/// <param name="primaryTable">The table that holds the primary keys (eg. Table.PK_id)</param>
		/// <param name="primaryColumn">The column that is the primary key (eg. PK_id)</param>
		void AddForeignKey(string name, string foreignTable, string foreignColumn, string primaryTable, string primaryColumn);

		/// <summary>
		/// Add a foreign key constraint
		/// </summary>
		/// <param name="name">The name of the foreign key. e.g. FK_TABLE_REF</param>
		/// <param name="foreignTable">The table that the foreign key will be created in (eg. Table.FK_id)</param>
		/// <param name="foreignColumn">The column that is the foreign key (eg. FK_id)</param>
		/// <param name="primaryTable">The table that holds the primary key (eg. Table.PK_id)</param>
		/// <param name="primaryColumn">The column that is the primary key (eg. PK_id)</param>
		/// <param name="constraint">Constraint parameters</param>
		void AddForeignKey(string name, string foreignTable, string foreignColumn, string primaryTable, string primaryColumn, ForeignKeyConstraint constraint);

		/// <summary>
		/// Define a new index
		/// </summary>
		/// <param name="name">Name of new index</param>
		/// <param name="unique">Sign that the index is unique</param>
		/// <param name="table">Table name</param>
		/// <param name="columns">Columns</param>
		void AddIndex(string name, bool unique, string table, params string[] columns);

		/// <summary>
		/// Check that the index with the specified name already exists
		/// </summary>
		/// <param name="indexName">�������� �������</param>
		/// <param name="tableName">�������� �������</param>
		/// <returns></returns>
		bool IndexExists(string indexName, string tableName);

		/// <summary>
		/// Deleting index
		/// </summary>
		/// <param name="indexName">Index name</param>
		/// <param name="tableName">Table name</param>
		void RemoveIndex(string indexName, string tableName);


		/// <summary>
		/// Add a foreign key constraint when you don't care about the name of the constraint.
		/// Warning: This will prevent you from dropping the constraint since you won't know the name.
		/// </summary>
		/// <param name="foreignTable">The table that the foreign key will be created in (eg. Table.FK_id)</param>
		/// <param name="foreignColumn">The column that is the foreign key (eg. FK_id)</param>
		/// <param name="primaryTable">The table that holds the primary key (eg. Table.PK_id)</param>
		/// <param name="primaryColumn">The column that is the primary key (eg. PK_id)</param>
		void GenerateForeignKey(string foreignTable, string foreignColumn, string primaryTable, string primaryColumn);

		/// <summary>
		/// Add a foreign key constraint when you don't care about the name of the constraint.
		/// Warning: This will prevent you from dropping the constraint since you won't know the name.
		/// </summary>
		/// <param name="foreignTable">The table that the foreign key will be created in (eg. Table.FK_id)</param>
		/// <param name="foreignColumns">The columns that are the foreign keys (eg. FK_id)</param>
		/// <param name="primaryTable">The table that holds the primary key (eg. Table.PK_id)</param>
		/// <param name="primaryColumns">The column that is the primary key (eg. PK_id)</param>
		void GenerateForeignKey(string foreignTable, string[] foreignColumns, string primaryTable, string[] primaryColumns);

		/// <summary>
		/// Add a foreign key constraint when you don't care about the name of the constraint.
		/// Warning: This will prevent you from dropping the constraint since you won't know the name.
		/// </summary>
		/// <param name="foreignTable">The table that the foreign key will be created in (eg. Table.FK_id)</param>
		/// <param name="foreignColumns">The columns that are the foreign keys (eg. FK_id)</param>
		/// <param name="primaryTable">The table that holds the primary key (eg. Table.PK_id)</param>
		/// <param name="primaryColumns">The columns that are the primary keys (eg. PK_id)</param>
		/// <param name="constraint">Constraint parameters</param>
		void GenerateForeignKey(string foreignTable, string[] foreignColumns, string primaryTable, string[] primaryColumns, ForeignKeyConstraint constraint);

		/// <summary>
		/// Add a foreign key constraint when you don't care about the name of the constraint.
		/// Warning: This will prevent you from dropping the constraint since you won't know the name.
		/// </summary>
		/// <param name="foreignTable">The table that the foreign key will be created in (eg. Table.FK_id)</param>
		/// <param name="foreignColumn">The columns that are the foreign keys (eg. FK_id)</param>
		/// <param name="primaryTable">The table that holds the primary key (eg. Table.PK_id)</param>
		/// <param name="primaryColumn">The column that is the primary key (eg. PK_id)</param>
		/// <param name="constraint">Constraint parameters</param>
		void GenerateForeignKey(string foreignTable, string foreignColumn, string primaryTable, string primaryColumn,
								ForeignKeyConstraint constraint);

		/// <summary>
		/// Add a foreign key constraint when you don't care about the name of the constraint.
		/// Warning: This will prevent you from dropping the constraint since you won't know the name.
		///
		/// The current expectations are that there is a column named the same as the foreignTable present in
		/// the table. This is subject to change because I think it's not a good convention.
		/// </summary>
		/// <param name="foreignTable">The table that the foreign key will be created in (eg. Table.FK_id)</param>
		/// <param name="primaryTable">The table that holds the primary key (eg. Table.PK_id)</param>
		void GenerateForeignKey(string foreignTable, string primaryTable);

		/// <summary>
		/// Add a foreign key constraint when you don't care about the name of the constraint.
		/// Warning: This will prevent you from dropping the constraint since you won't know the name.
		///
		/// The current expectations are that there is a column named the same as the foreignTable present in
		/// the table. This is subject to change because I think it's not a good convention.
		/// </summary>
		/// <param name="foreignTable">The table that the foreign key will be created in (eg. Table.FK_id)</param>
		/// <param name="primaryTable">The table that holds the primary key (eg. Table.PK_id)</param>
		/// <param name="constraint"></param>
		void GenerateForeignKey(string foreignTable, string primaryTable, ForeignKeyConstraint constraint);

		/// <summary>
		/// Add a primary key to a table
		/// </summary>
		/// <param name="name">The name of the primary key to add.</param>
		/// <param name="table">The name of the table that will get the primary key.</param>
		/// <param name="columns">The name of the column or columns that are in the primary key.</param>
		void AddPrimaryKey(string name, string table, params string[] columns);

		/// <summary>
		/// Add a constraint to a table
		/// </summary>
		/// <param name="name">The name of the constraint to add.</param>
		/// <param name="table">The name of the table that will get the constraint</param>
		/// <param name="columns">The name of the column or columns that will get the constraint.</param>
		void AddUniqueConstraint(string name, string table, params string[] columns);

		/// <summary>
		/// Add a constraint to a table
		/// </summary>
		/// <param name="name">The name of the constraint to add.</param>
		/// <param name="table">The name of the table that will get the constraint</param>
		/// <param name="checkSql">The check constraint definition.</param>
		void AddCheckConstraint(string name, string table, string checkSql);

		/// <summary>
		/// Add a table
		/// </summary>
		/// <param name="name">The name of the table to add.</param>
		/// <param name="columns">The columns that are part of the table.</param>
		void AddTable(string name, params Column[] columns);

		/// <summary>
		/// Add a table
		/// </summary>
		/// <param name="name">The name of the table to add.</param>
		/// <param name="engine">The name of the database engine to use. (MySQL)</param>
		/// <param name="columns">The columns that are part of the table.</param>
		void AddTable(string name, string engine, params Column[] columns);

		/// <summary>
		/// Start a transction
		/// </summary>
		void BeginTransaction();

		/// <summary>
		/// Change the definition of an existing column.
		/// </summary>
		/// <param name="table">The name of the table that will get the new column</param>
		/// <param name="column">An instance of a <see cref="Column">Column</see> with the specified properties and the name of an existing column</param>
		void ChangeColumn(string table, Column column);

		/// <summary>
		/// Check to see if a column exists
		/// </summary>
		/// <param name="table"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		bool ColumnExists(string table, string column);

		/// <summary>
		/// Commit the running transction
		/// </summary>
		void Commit();

		/// <summary>
		/// Check to see if a constraint exists
		/// </summary>
		/// <param name="name">The name of the constraint</param>
		/// <param name="table">The table that the constraint lives on.</param>
		/// <returns></returns>
		bool ConstraintExists(string table, string name);

		/// <summary>
		/// Check to see if a primary key constraint exists on the table
		/// </summary>
		/// <param name="name">The name of the primary key</param>
		/// <param name="table">The table that the constraint lives on.</param>
		/// <returns></returns>
		bool PrimaryKeyExists(string table, string name);

		/// <summary>
		/// Execute an arbitrary SQL query
		/// </summary>
		/// <param name="sql">The SQL to execute.</param>
		/// <returns></returns>
		int ExecuteNonQuery(string sql);

		/// <summary>
		/// Execute an arbitrary SQL query
		/// </summary>
		/// <param name="sql">The SQL to execute.</param>
		/// <returns></returns>
		IDataReader ExecuteReader(string sql);

		/// <summary>
		/// Execute an arbitrary SQL query
		/// </summary>
		/// <param name="sql">The SQL to execute.</param>
		/// <returns>A single value that is returned.</returns>
		object ExecuteScalar(string sql);

		/// <summary>
		/// Get the names of all of the tables
		/// </summary>
		/// <returns>The names of all the tables.</returns>
		string[] GetTables();

		/// <summary>
		/// Insert data into a table
		/// </summary>
		/// <param name="table">The table that will get the new data</param>
		/// <param name="columns">The names of the columns</param>
		/// <param name="values">The values in the same order as the columns</param>
		/// <returns></returns>
		int Insert(string table, string[] columns, string[] values);

		/// <summary>
		/// Delete data from a table
		/// </summary>
		/// <param name="table">The table that will have the data deleted</param>
		/// <param name="whereSql">Condition for select deleting rows</param>
		/// <returns></returns>
		int Delete(string table, string whereSql = null);

		/// <summary>
		/// Marks a Migration version number as having been applied
		/// </summary>
		/// <param name="version">The version number of the migration that was applied</param>
		/// <param name="key">Key of migration series</param>
		void MigrationApplied(long version, string key);

		/// <summary>
		/// Marks a Migration version number as having been rolled back from the database
		/// </summary>
		/// <param name="version">The version number of the migration that was removed</param>
		/// <param name="key">Key of migration series</param>
		void MigrationUnApplied(long version, string key);

		/// <summary>
		/// Remove an existing column from a table
		/// </summary>
		/// <param name="table">The name of the table to remove the column from</param>
		/// <param name="column">The column to remove</param>
		void RemoveColumn(string table, string column);

		/// <summary>
		/// Remove an existing foreign key constraint
		/// </summary>
		/// <param name="table">The table that contains the foreign key.</param>
		/// <param name="name">The name of the foreign key to remove</param>
		void RemoveForeignKey(string table, string name);

		/// <summary>
		/// Remove an existing constraint
		/// </summary>
		/// <param name="table">The table that contains the foreign key.</param>
		/// <param name="name">The name of the constraint to remove</param>
		void RemoveConstraint(string table, string name);

		/// <summary>
		/// Remove an existing table
		/// </summary>
		/// <param name="tableName">The name of the table</param>
		void RemoveTable(string tableName);

		/// <summary>
		/// Rename an existing table
		/// </summary>
		/// <param name="oldName">The old name of the table</param>
		/// <param name="newName">The new name of the table</param>
		void RenameTable(string oldName, string newName);

		/// <summary>
		/// Rename an existing table
		/// </summary>
		/// <param name="tableName">The name of the table</param>
		/// <param name="oldColumnName">The old name of the column</param>
		/// <param name="newColumnName">The new name of the column</param>
		void RenameColumn(string tableName, string oldColumnName, string newColumnName);

		/// <summary>
		/// Rollback the currently running transaction.
		/// </summary>
		void Rollback();

		/// <summary>
		/// Check if a table already exists
		/// </summary>
		/// <param name="tableName">The name of the table that you want to check on.</param>
		/// <returns></returns>
		bool TableExists(string tableName);

		/// <summary>
		/// Update the values in a table
		/// </summary>
		/// <param name="table">The name of the table to update</param>
		/// <param name="columns">The names of the columns.</param>
		/// <param name="columnValues">The values for the columns in the same order as the names.</param>
		/// <returns></returns>
		int Update(string table, string[] columns, string[] columnValues);

		/// <summary>
		/// Update the values in a table
		/// </summary>
		/// <param name="table">The name of the table to update</param>
		/// <param name="columns">The names of the columns.</param>
		/// <param name="values">The values for the columns in the same order as the names.</param>
		/// <param name="where">A where clause to limit the update</param>
		/// <returns></returns>
		int Update(string table, string[] columns, string[] values, string where);

		IDbCommand GetCommand();

		bool TypeIsSupported(DbType type);

		#region For

		/// <summary>
		/// Get this provider or a NoOp provider if you are not running in the context of 'TProvider'.
		/// </summary>
		void For<TProvider>(Action<ITransformationProvider> actions);

		/// <summary>
		/// Get this provider or a NoOp provider if you are not running in the context of 'TProvider'.
		/// </summary>
		void For(Type providerType, Action<ITransformationProvider> actions);

		/// <summary>
		/// Get this provider or a NoOp provider if you are not running in the context of provider with name 'providerTypeName'.
		/// </summary>
		void For(string providerTypeName, Action<ITransformationProvider> actions); 
		#endregion

		void AddForeignKey(string name, string primaryTable, string[] primaryColumns, string refTable,
		                                   string[] refColumns, ForeignKeyConstraint onDeleteConstraint, ForeignKeyConstraint onUpdateConstraint);

		void ExecuteFromResource(Assembly assembly, string path);

		string QuoteName(string name);

		string FormatSql(string format, params object[] args);
	}
}