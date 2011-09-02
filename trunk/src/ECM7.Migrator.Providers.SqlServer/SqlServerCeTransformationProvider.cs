using System;
using System.Data;
using System.Data.SqlServerCe;
using ECM7.Migrator.Exceptions;

namespace ECM7.Migrator.Providers.SqlServer
{
	using ECM7.Migrator.Framework.Logging;
	using ECM7.Migrator.Providers.SqlServer.Base;

	/// <summary>
	/// Migration transformations provider for Microsoft SQL Server.
	/// </summary>
	public class SqlServerCeTransformationProvider : BaseSqlServerTransformationProvider<SqlCeConnection>
	{
		// todo: добавить тесты поддержки типов данных для Firebird
		
		#region custom sql

		public SqlServerCeTransformationProvider(SqlCeConnection connection)
			: base(connection)
		{
			typeMap.Put(DbType.AnsiStringFixedLength, "NCHAR(255)");
			typeMap.Put(DbType.AnsiStringFixedLength, 4000, "NCHAR($l)");
			typeMap.Put(DbType.AnsiString, "VARCHAR(255)");
			typeMap.Put(DbType.AnsiString, 4000, "VARCHAR($l)");
			typeMap.Put(DbType.AnsiString, int.MaxValue, "TEXT");

			typeMap.Put(DbType.String, "NVARCHAR(255)");
			typeMap.Put(DbType.String, 4000, "NVARCHAR($l)");
			typeMap.Put(DbType.String, int.MaxValue, "NTEXT");

			typeMap.Put(DbType.Binary, int.MaxValue, "IMAGE");

			typeMap.Put(DbType.Decimal, "NUMERIC(19,5)");
			typeMap.Put(DbType.Decimal, 19, "NUMERIC(19, $l)");
			typeMap.Put(DbType.Double, "FLOAT");
		}

		public override bool ConstraintExists(string table, string name)
		{
			string sql = string.Format("SELECT [cont].[constraint_name] FROM [INFORMATION_SCHEMA].[TABLE_CONSTRAINTS] [cont] WHERE [cont].[Constraint_Name]='{0}'", name);
			using (IDataReader reader = ExecuteReader(sql))
			{
				return reader.Read();
			}
		}

		public override void RenameColumn(string tableName, string oldColumnName, string newColumnName)
		{
			throw new MigrationException("SqlServerCe doesn't support column renaming");
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
