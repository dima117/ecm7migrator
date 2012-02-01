using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using ECM7.Migrator.Framework;
using ECM7.Migrator.Providers.Validation;

namespace ECM7.Migrator.Providers.SqlServer
{
	using Base;

	/// <summary>
	/// Migration transformations provider for Microsoft SQL Server.
	/// </summary>
	[ProviderValidation(typeof(SqlCeConnection), false)]
	public class SqlServerCeTransformationProvider : BaseSqlServerTransformationProvider
	{
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
			typeMap.Put(DbType.Decimal, 19, "NUMERIC($l, $s)");
			typeMap.Put(DbType.Double, "FLOAT");
		}

		public override bool ConstraintExists(SchemaQualifiedObjectName table, string name)
		{
			string sql = FormatSql(
				"SELECT [CONSTRAINT_NAME] FROM [INFORMATION_SCHEMA].[TABLE_CONSTRAINTS] " +
				"WHERE [CONSTRAINT_NAME] = '{0}' AND [TABLE_NAME] = '{1}'", name, table.Name);

			using (IDataReader reader = ExecuteReader(sql))
			{
				return reader.Read();
			}
		}

		public override void RenameColumn(SchemaQualifiedObjectName tableName, string oldColumnName, string newColumnName)
		{
			throw new NotSupportedException("SqlServerCe doesn't support column renaming");
		}

		public override void AddCheckConstraint(string name, SchemaQualifiedObjectName table, string checkSql)
		{
			throw new NotSupportedException("SqlServerCe doesn't support check constraints");
		}

		public override void ChangeDefaultValue(SchemaQualifiedObjectName table, string column, object newDefaultValue)
		{
			if (newDefaultValue != null)
			{
				base.ChangeDefaultValue(table, column, null);
			}

			base.ChangeDefaultValue(table, column, newDefaultValue);
		}

		public override SchemaQualifiedObjectName[] GetTables(string schema = null)
		{
			var tables = new List<SchemaQualifiedObjectName>();

			string sql = FormatSql("SELECT {0:NAME} FROM {1:NAME}.{2:NAME}",
				"TABLE_NAME", "INFORMATION_SCHEMA", "TABLES");

			using (IDataReader reader = ExecuteReader(sql))
			{
				while (reader.Read())
				{
					string tableName = reader.GetString(0);
					tables.Add(tableName.WithSchema(schema));
				}
			}

			return tables.ToArray();
		}

		public override bool TableExists(SchemaQualifiedObjectName table)
		{
			string sql = FormatSql(
				"SELECT * FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME]='{0}'", table.Name);

			using (IDataReader reader = ExecuteReader(sql))
			{
				return reader.Read();
			}
		}

		public override bool IndexExists(string indexName, SchemaQualifiedObjectName tableName)
		{
			string sql = FormatSql(
				"SELECT COUNT(*) FROM [INFORMATION_SCHEMA].[INDEXES] " +
				"WHERE [TABLE_NAME] = '{0}' and [INDEX_NAME] = '{1}'", tableName.Name, indexName);

			int count = Convert.ToInt32(ExecuteScalar(sql));
			return count > 0;
		}

		protected override string GetSqlRemoveIndex(string indexName, SchemaQualifiedObjectName tableName)
		{
			return FormatSql("DROP INDEX {0:NAME}.{1:NAME}", tableName.Name, indexName);
		}

		protected override string FindConstraints(SchemaQualifiedObjectName table, string column)
		{
			return
				string.Format("SELECT [CONSTRAINT_NAME] FROM [INFORMATION_SCHEMA].[KEY_COLUMN_USAGE] "
					+ "WHERE [TABLE_NAME]='{0}' AND [COLUMN_NAME] = '{1}'", table.Name, column);
		}

		#endregion
	}
}
