using System.Collections.Generic;
using System.Data;
using System.Text;
using ECM7.Migrator.Framework;
using ECM7.Migrator.Providers.Validation;
using System;

namespace ECM7.Migrator.Providers.SqlServer
{
	using System.Data.SqlClient;

	using Base;

	[ProviderValidation(typeof(SqlConnection), true)]
	public class SqlServerTransformationProvider : BaseSqlServerTransformationProvider
	{
		public SqlServerTransformationProvider(SqlConnection connection, int? commandTimeout)
			: base(connection, commandTimeout)
		{
		}

		#region change default value

		protected override string GetSqlChangeDefaultValue(SchemaQualifiedObjectName table, string column, object newDefaultValue)
		{
			string dfConstraintName = "DF_{0}".FormatWith(Guid.NewGuid().ToString("N"));
			string sqlDefaultValue = GetSqlDefaultValue(newDefaultValue);
			return FormatSql("ALTER TABLE {0:NAME} ADD CONSTRAINT {1:NAME} {2} FOR {3:NAME}", table, dfConstraintName, sqlDefaultValue, column);
		}

		public virtual string GetDefaultConstraintName(SchemaQualifiedObjectName table, string column)
		{
			StringBuilder sqlBuilder = new StringBuilder();

			sqlBuilder.Append("SELECT [dobj].[name] AS [CONSTRAINT_NAME] ");
			sqlBuilder.Append("FROM [sys].[columns] [col] ");
			sqlBuilder.Append("INNER JOIN [sys].[objects] [dobj] ");
			sqlBuilder.Append("ON [dobj].[object_id] = [col].[default_object_id] AND [dobj].[type] = 'D' ");
			sqlBuilder.AppendFormat("WHERE [col].[object_id] = object_id(N'{0}') AND [col].[name] = '{1}'", table, column);

			using (var reader = ExecuteReader(sqlBuilder.ToString()))
			{
				if (reader.Read())
				{
					return reader.GetString(0);
				}

				return null;
			}
		}

		public override void ChangeDefaultValue(SchemaQualifiedObjectName table, string column, object newDefaultValue)
		{
			string defaultConstraintName = GetDefaultConstraintName(table, column);

			if (!defaultConstraintName.IsNullOrEmpty(true))
			{
				RemoveConstraint(table, defaultConstraintName);
			}

			if (newDefaultValue != null)
			{
				string sql = GetSqlChangeDefaultValue(table, column, newDefaultValue);
				ExecuteNonQuery(sql);
			}
		}

		#endregion

		public override SchemaQualifiedObjectName[] GetTables(string schema = null)
		{
			string nspname = schema.IsNullOrEmpty(true) ? "SCHEMA_NAME()" : string.Format("'{0}'", schema);

			var tables = new List<SchemaQualifiedObjectName>();

			string sql = FormatSql("SELECT {0:NAME}, {1:NAME} FROM {2:NAME}.{3:NAME} where {4:NAME} = {5}",
				"TABLE_NAME", "TABLE_SCHEMA", "INFORMATION_SCHEMA", "TABLES", "TABLE_SCHEMA", nspname);

			using (IDataReader reader = ExecuteReader(sql))
			{
				while (reader.Read())
				{
					string tableName = reader.GetString(0);
					string tableSchema = reader.GetString(1);
					tables.Add(tableName.WithSchema(tableSchema));
				}
			}

			return tables.ToArray();
		}

		public override bool TableExists(SchemaQualifiedObjectName table)
		{
			string nspname = table.SchemaIsEmpty ? "SCHEMA_NAME()" : string.Format("'{0}'", table.Schema);

			string sql = FormatSql(
				"SELECT * FROM [INFORMATION_SCHEMA].[TABLES] " +
				"WHERE [TABLE_NAME]='{0}' AND [TABLE_SCHEMA] = {1}", table.Name, nspname);

			using (IDataReader reader = ExecuteReader(sql))
			{
				return reader.Read();
			}
		}
	}
}
