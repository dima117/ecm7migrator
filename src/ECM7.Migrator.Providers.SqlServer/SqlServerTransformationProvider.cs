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
		public SqlServerTransformationProvider(SqlConnection connection)
			: base(connection)
		{
		}

		#region change default value

		protected override string GetSqlChangeDefaultValue(SchemaQualifiedObjectName table, string column, object newDefaultValue)
		{
			string dfConstraintName = string.Format("DF_{0}", Guid.NewGuid().ToString("N"));
			string sqlDefaultValue = GetSqlDefaultValue(newDefaultValue);
			return FormatSql("ALTER TABLE {0:NAME} ADD CONSTRAINT {1:NAME} {2} FOR {3:NAME}", table, dfConstraintName, sqlDefaultValue, column);
		}

		public virtual string GetDefaultConstraintName(SchemaQualifiedObjectName table, string column)
		{
			var sqlBuilder = new StringBuilder();

            sqlBuilder.Append(FormatSql("SELECT {0:NAME}.{1:NAME} AS {2:NAME} ", "dobj", "name", "CONSTRAINT_NAME"));
            sqlBuilder.Append(FormatSql("FROM {0:NAME} {1:NAME} ", "columns".WithSchema("sys"), "col"));
            sqlBuilder.Append(FormatSql("INNER JOIN {0:NAME} {1:NAME} ", "objects".WithSchema("sys"), "dobj"));
            sqlBuilder.Append(FormatSql("ON {0:NAME}.{1:NAME} = {2:NAME}.{3:NAME} AND {0:NAME}.{4:NAME} = 'D' ",
                "dobj", "object_id", "col", "default_object_id", "type"));
            sqlBuilder.Append(FormatSql("WHERE {0:NAME}.{1:NAME} = object_id(N'{2}') AND {0:NAME}.{3:NAME} = '{4}'",
                "col", "object_id", table, "name", column));

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

			if (!string.IsNullOrWhiteSpace(defaultConstraintName))
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
			string nspname = string.IsNullOrWhiteSpace(schema) ? "SCHEMA_NAME()" : string.Format("'{0}'", schema);

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
                "SELECT * FROM {0:NAME} WHERE {1:NAME}='{2}' AND {3:NAME} = {4}",
                "TABLES".WithSchema("INFORMATION_SCHEMA"), "TABLE_NAME", table.Name, "TABLE_SCHEMA", nspname);

			using (IDataReader reader = ExecuteReader(sql))
			{
				return reader.Read();
			}
		}
	}
}
