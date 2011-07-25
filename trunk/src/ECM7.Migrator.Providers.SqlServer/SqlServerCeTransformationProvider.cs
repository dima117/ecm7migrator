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
using System.Data;
using System.Data.SqlServerCe;
using ECM7.Migrator.Framework;

namespace ECM7.Migrator.Providers.SqlServer
{
	using ECM7.Migrator.Framework.Logging;

	/// <summary>
	/// Migration transformations provider for Microsoft SQL Server.
	/// </summary>
	public class SqlServerCeTransformationProvider : SqlServerTransformationProvider
	{
		public SqlServerCeTransformationProvider(Dialect dialect, string connectionString)
			: base(dialect, new SqlCeConnection(connectionString))
		{
		}

		public override bool ConstraintExists(string table, string name)
		{
			string sql = string.Format("SELECT [cont].[constraint_name] FROM [INFORMATION_SCHEMA].[TABLE_CONSTRAINTS] [cont] WHERE [cont].[Constraint_Name]='{0}'", name);
			using (IDataReader reader = ExecuteQuery(sql))
			{
				return reader.Read();
			}
		}

		public override void RenameColumn(string tableName, string oldColumnName, string newColumnName)
		{
			throw new MigrationException("SqlServerCe doesn't support column renaming");
		}

		public override void RenameTable(string oldName, string newName)
		{
			if (TableExists(newName))
			{
				throw new MigrationException("The specified table already exists");
			}

			string sql = FormatSql("exec sp_rename '{0}', '{1}'", oldName, newName);
			ExecuteNonQuery(sql);
		}

		public override void AddCheckConstraint(string name, string table, string checkSql)
		{
			throw new MigrationException("SqlServerCe doesn't support check constraints");
		}

		public override bool IndexExists(string indexName, string tableName)
		{
			string sql = string.Format(
				"select count(*) from [INFORMATION_SCHEMA].[INDEXES] where [TABLE_NAME] = '{0}' and [INDEX_NAME] = '{1}'", tableName, indexName);

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

			string sql = FormatSql("DROP INDEX {0:NAME}.{1:NAME}", tableName, indexName);

			ExecuteNonQuery(sql);

		}

		protected override string FindConstraints(string table, string column)
		{
			return
				string.Format("SELECT [cont].[constraint_name] FROM [INFORMATION_SCHEMA].[KEY_COLUMN_USAGE] [cont] "
					+ "WHERE [cont].[Table_Name]='{0}' AND [cont].[column_name] = '{1}'", table, column);
		}
	}
}
