using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ECM7.Migrator.Framework;

namespace ECM7.Migrator.Providers.SqlServer
{
	using System.Text;

	/// <summary>
	/// Migration transformations provider for Microsoft SQL Server.
	/// </summary>
	public class SqlServerTransformationProvider : TransformationProvider
	{
		protected SqlServerTransformationProvider(Dialect dialect, IDbConnection connection)
			: base(dialect, connection)
		{
		}

		public SqlServerTransformationProvider(Dialect dialect, string connectionString)
			: base(dialect, new SqlConnection(connectionString))
		{
		}

		// FIXME: We should look into implementing this with INFORMATION_SCHEMA if possible
		// so that it would be usable by all the SQL Server implementations
		public override bool IndexExists(string indexName, string tableName)
		{
			string sql = string.Format("SELECT COUNT(*) FROM [sys].[indexes] WHERE [name] = '{0}'", indexName);
			int count = Convert.ToInt32(ExecuteScalar(sql));
			return count > 0;
		}

		public override bool ConstraintExists(string table, string name)
		{
			string sql = string.Format(
				"SELECT TOP 1 [name] FROM [sys].[objects] " +
				"WHERE [parent_object_id] = object_id('{0}') " +
				"AND [object_id] = object_id('{1}') " +
				"AND [type] IN ('D', 'F', 'PK', 'UQ')" +
				"UNION ALL " +
				"select [CONSTRAINT_NAME] AS [name] " +
				"from [INFORMATION_SCHEMA].[CHECK_CONSTRAINTS]" +
				"WHERE [CONSTRAINT_NAME] = '{1}'", table, name);

			using (IDataReader reader = ExecuteQuery(sql))
			{
				return reader.Read();
			}
		}

		public override void AddColumn(string table, string columnSql)
		{
			ExecuteNonQuery(FormatSql("ALTER TABLE {0:NAME} ADD {1}", table, columnSql));
		}

		public override bool ColumnExists(string table, string column)
		{
			if (!TableExists(table))
				return false;

			using (IDataReader reader =
				ExecuteQuery(String.Format("SELECT * FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME]='{0}' AND [COLUMN_NAME]='{1}'", table, column)))
			{
				return reader.Read();
			}
		}

		public override bool TableExists(string table)
		{
			using (IDataReader reader =
				ExecuteQuery(String.Format("SELECT * FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME]='{0}'", table)))
			{
				return reader.Read();
			}
		}

		public override void RemoveColumn(string table, string column)
		{
			DeleteColumnConstraints(table, column);
			base.RemoveColumn(table, column);
		}

		public override void RenameColumn(string tableName, string oldColumnName, string newColumnName)
		{
			if (ColumnExists(tableName, newColumnName))
				throw new MigrationException(String.Format("Table '{0}' has column named '{1}' already", tableName, newColumnName));

			if (ColumnExists(tableName, oldColumnName))
				ExecuteNonQuery(String.Format("EXEC sp_rename '{0}.{1}', '{2}', 'COLUMN'", tableName, oldColumnName, newColumnName));
		}

		public override void RenameTable(string oldName, string newName)
		{
			if (TableExists(newName))
				throw new MigrationException(String.Format("Table with name '{0}' already exists", newName));

			if (TableExists(oldName))
				ExecuteNonQuery(String.Format("EXEC sp_rename '{0}', '{1}'", oldName, newName));
		}

		// Deletes all constraints linked to a column. Sql Server
		// doesn't seems to do this.
		private void DeleteColumnConstraints(string table, string column)
		{
			string sqlContrainte = FindConstraints(table, column);
			List<string> constraints = new List<string>();
			using (IDataReader reader = ExecuteQuery(sqlContrainte))
			{
				while (reader.Read())
				{
					constraints.Add(reader.GetString(0));
				}
			}
			// Can't share the connection so two phase modif
			foreach (string constraint in constraints)
			{
				RemoveForeignKey(table, constraint);
			}
		}

		// FIXME: We should look into implementing this with INFORMATION_SCHEMA if possible
		// so that it would be usable by all the SQL Server implementations
		protected virtual string FindConstraints(string table, string column)
		{
			StringBuilder sqlBuilder = new StringBuilder();

			sqlBuilder.Append("SELECT [CONSTRAINT_NAME] ");
			sqlBuilder.Append("FROM [INFORMATION_SCHEMA].[CONSTRAINT_COLUMN_USAGE] ");
			sqlBuilder.AppendFormat("WHERE [TABLE_NAME] = '{0}' and [COLUMN_NAME] = '{1}' ", table, column);
			
			sqlBuilder.Append("UNION ALL ");
			sqlBuilder.Append("SELECT [dobj].[name] as [CONSTRAINT_NAME] ");
			sqlBuilder.Append("FROM [sys].[columns] [col] ");
			sqlBuilder.Append("INNER JOIN [sys].[objects] [dobj] ");
			sqlBuilder.Append("ON [dobj].[object_id] = [col].[default_object_id] AND [dobj].[type] = 'D' ");
			sqlBuilder.AppendFormat("WHERE [col].[object_id] = object_id(N'{0}') AND [col].[name] = '{1}'", table, column);

			return sqlBuilder.ToString();
		}
	}
}
