using System;
using System.Data;
using ECM7.Migrator.Framework;
using System.Collections.Generic;
using ECM7.Migrator.Framework.SchemaBuilder;
using ForeignKeyConstraint = ECM7.Migrator.Framework.ForeignKeyConstraint;

namespace ECM7.Migrator.Providers
{
	/// <summary>
	/// No Op (Null Object Pattern) implementation of the ITransformationProvider
	/// </summary>
	public class NoOpTransformationProvider : ITransformationProvider
	{

		public static readonly NoOpTransformationProvider Instance = new NoOpTransformationProvider();

		private NoOpTransformationProvider()
		{

		}

		public virtual ILogger Logger
		{
			get { return null; }
			set { }
		}

		public Dialect Dialect
		{
			get { return null; }
		}

		public string[] GetTables()
		{
			return null;
		}

		public Column[] GetColumns(string table)
		{
			return null;
		}

		public Column GetColumnByName(string table, string column)
		{
			return null;
		}

		public void RemoveForeignKey(string table, string name)
		{
			// No Op
		}

		public void RemoveConstraint(string table, string name)
		{
			// No Op
		}

		public void AddTable(string name, params Column[] columns)
		{
			// No Op
		}

		public void AddTable(string name, string engine, params Column[] columns)
		{
			// No Op
		}

		public void RemoveTable(string name)
		{
			// No Op
		}

		public void RenameTable(string oldName, string newName)
		{
			// No Op
		}

		public void RenameColumn(string tableName, string oldColumnName, string newColumnName)
		{
			// No Op
		}

		public void AddColumn(string table, string sqlColumn)
		{
			// No Op
		}

		public void RemoveColumn(string table, string column)
		{
			// No Op
		}

		public bool ColumnExists(string table, string column)
		{
			return false;
		}

		public bool TableExists(string table)
		{
			return false;
		}

		public void AddColumn(string table, string columnName, DbType type, int size, ColumnProperty property, object defaultValue)
		{
			// No Op
		}

		public void AddColumn(string table, string columnName, ColumnType type, ColumnProperty property, object defaultValue)
		{
			// No Op
		}

		public void AddColumn(string table, string column, DbType type)
		{
			// No Op
		}

		public void AddColumn(string table, string column, DbType type, object defaultValue)
		{
			// No Op
		}

		public void AddColumn(string table, string column, DbType type, int size)
		{
			// No Op
		}

		public void AddColumn(string table, string column, DbType type, ColumnProperty property)
		{
			// No Op
		}

		public void AddColumn(string table, string column, DbType type, int size, ColumnProperty property)
		{
			// No Op
		}

		public void AddPrimaryKey(string name, string table, params string[] columns)
		{
			// No Op
		}

		public void GenerateForeignKey(string primaryTable, string primaryColumn, string refTable, string refColumn)
		{
			// No Op
		}

		public void GenerateForeignKey(string primaryTable, string[] primaryColumns, string refTable, string[] refColumns)
		{
			// No Op
		}

		public void GenerateForeignKey(string primaryTable, string primaryColumn, string refTable, string refColumn, ForeignKeyConstraint constraint)
		{
			// No Op
		}

		public void GenerateForeignKey(string primaryTable, string[] primaryColumns, string refTable,
											   string[] refColumns, ForeignKeyConstraint constraint)
		{
			// No Op
		}

		public void AddForeignKey(string name, string primaryTable, string primaryColumn, string refTable,
										  string refColumn)
		{
			// No Op
		}

		public void AddForeignKey(string name, string primaryTable, string[] primaryColumns, string refTable, string[] refColumns)
		{
			// No Op
		}

		public void AddForeignKey(string name, string primaryTable, string primaryColumn, string refTable, string refColumn, ForeignKeyConstraint constraint)
		{
			// No Op
		}

		public void AddForeignKey(string name, string primaryTable, string[] primaryColumns, string refTable,
										  string[] refColumns, ForeignKeyConstraint constraint)
		{
			// No Op
		}

		public void AddUniqueConstraint(string name, string table, params string[] columns)
		{
			// No Op
		}

		public void AddCheckConstraint(string name, string table, string checkSql)
		{
			// No Op
		}

		public bool ConstraintExists(string table, string name)
		{
			return false;
		}

		public void ChangeColumn(string table, Column column)
		{
			// No Op
		}


		public bool PrimaryKeyExists(string table, string name)
		{
			return false;
		}

		public int ExecuteNonQuery(string sql)
		{
			return 0;
		}

		public IDataReader ExecuteQuery(string sql)
		{
			return null;
		}

		public object ExecuteScalar(string sql)
		{
			return null;
		}

		public IDataReader Select(string what, string from)
		{
			return null;
		}

		public IDataReader Select(string what, string from, string where)
		{
			return null;
		}

		public object SelectScalar(string what, string from)
		{
			return null;
		}

		public object SelectScalar(string what, string from, string where)
		{
			return null;
		}

		public int Update(string table, string[] columns, string[] columnValues)
		{
			return 0;
		}

		public int Update(string table, string[] columns, string[] columnValues, string where)
		{
			return 0;
		}

		public int Insert(string table, string[] columns, string[] columnValues)
		{
			return 0;
		}

		public int Delete(string table, string[] columns, string[] columnValues)
		{
			return 0;
		}

		public int Delete(string table, string column, string value)
		{
			return 0;
		}

		public void BeginTransaction()
		{
			// No Op
		}

		public void Rollback()
		{
			// No Op
		}

		public void Commit()
		{
			// No Op
		}

		public void MigrationApplied(long version)
		{
			//no op
		}

		public void MigrationUnApplied(long version)
		{
			//no op
		}

		#region For

		public ITransformationProvider For<TDialect>()
		{
			return this;
		}

		public ITransformationProvider For(Type dialectType)
		{
			return this;
		}

		public ITransformationProvider For(string dialectTypeName)
		{
			return this;
		}

		public void For<TDialect>(Action<ITransformationProvider> actions)
		{
		}

		public void For(Type dialectType, Action<ITransformationProvider> actions)
		{
		}
		public void For(string dialectTypeName, Action<ITransformationProvider> actions)
		{
		}

		public void AddForeignKey(string name, string primaryTable, string[] primaryColumns, string refTable, string[] refColumns, ForeignKeyConstraint onDeleteConstraint, ForeignKeyConstraint onUpdateConstraint)
		{
		}

		#endregion

		public List<long> AppliedMigrations
		{
			get { return new List<long>(); }
		}

		protected void CreateSchemaInfoTable()
		{
			// No Op
		}

		public void AddColumn(string table, Column column)
		{
			// No Op
		}

		public void GenerateForeignKey(string primaryTable, string refTable)
		{
			// No Op
		}

		public void GenerateForeignKey(string primaryTable, string refTable, ForeignKeyConstraint constraint)
		{
			// No Op
		}

		public IDbCommand GetCommand()
		{
			return null;
		}

		public void ExecuteSchemaBuilder(SchemaBuilder schemaBuilder)
		{
			// No Op
		}

		public bool TypeIsSupported(DbType type)
		{
			return false;
		}
	}
}
