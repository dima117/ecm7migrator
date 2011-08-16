using System;
using System.Data;
using System.Data.SqlServerCe;
using ECM7.Migrator.Framework;

namespace ECM7.Migrator.Providers.SqlServer
{
	using ECM7.Migrator.Framework.Logging;
	using ECM7.Migrator.Providers.SqlServer.Base;

	/// <summary>
	/// Migration transformations provider for Microsoft SQL Server.
	/// </summary>
	public class SqlServerCeTransformationProvider : BaseSqlServerTransformationProvider<SqlCeConnection>
	{
		// todo: написать для всех провайдеров тесты на типы данных
		// todo: добавить тесты поддержки топов данных для Firebird
		// todo: проверить, что для всех создаваемых провайдеров и миграторов вызывается Dispose
		
		#region custom sql

		public SqlServerCeTransformationProvider(SqlCeConnection connection)
			: base(connection)
		{
			RegisterColumnType(DbType.AnsiStringFixedLength, "NCHAR(255)");
			RegisterColumnType(DbType.AnsiStringFixedLength, 4000, "NCHAR($l)");
			RegisterColumnType(DbType.AnsiString, "VARCHAR(255)");
			RegisterColumnType(DbType.AnsiString, 4000, "VARCHAR($l)");
			RegisterColumnType(DbType.AnsiString, int.MaxValue, "TEXT");

			RegisterColumnType(DbType.String, "NVARCHAR(255)");
			RegisterColumnType(DbType.String, 4000, "NVARCHAR($l)");
			RegisterColumnType(DbType.String, int.MaxValue, "NTEXT");

			RegisterColumnType(DbType.Binary, int.MaxValue, "IMAGE");

			RegisterColumnType(DbType.Decimal, "NUMERIC(19,5)");
			RegisterColumnType(DbType.Decimal, 19, "NUMERIC(19, $l)");
			RegisterColumnType(DbType.Double, "FLOAT");
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

		#endregion
	}
}
