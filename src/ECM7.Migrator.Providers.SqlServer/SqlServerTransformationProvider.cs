using System.Text;
using ECM7.Migrator.Framework;
using ECM7.Migrator.Providers.Validation;

namespace ECM7.Migrator.Providers.SqlServer
{
	using System;
	using System.Data.SqlClient;

	using Base;

	[ProviderValidation(typeof(SqlConnection), false)]
	public class SqlServerTransformationProvider : BaseSqlServerTransformationProvider<SqlConnection>
	{
		public SqlServerTransformationProvider(SqlConnection connection)
			: base(connection)
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
	}
}
