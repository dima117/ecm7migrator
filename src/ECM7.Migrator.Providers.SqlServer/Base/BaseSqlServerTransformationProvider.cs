namespace ECM7.Migrator.Providers.SqlServer.Base
{
	using System;
	using System.Collections.Generic;
	using System.Data;

	using ECM7.Migrator.Framework;

	using System.Text;

	/// <summary>
	/// Migration transformations provider for Microsoft SQL Server.
	/// </summary>
	public abstract class BaseSqlServerTransformationProvider<TConnection> : TransformationProvider<TConnection>
		where TConnection : IDbConnection
	{
		protected BaseSqlServerTransformationProvider(TConnection connection)
			: base(connection)
		{
			typeMap.Put(DbType.AnsiStringFixedLength, "CHAR(255)");
			typeMap.Put(DbType.AnsiStringFixedLength, 8000, "CHAR($l)");
			typeMap.Put(DbType.AnsiString, "VARCHAR(255)");
			typeMap.Put(DbType.AnsiString, 8000, "VARCHAR($l)");
			typeMap.Put(DbType.AnsiString, int.MaxValue, "VARCHAR(MAX)");
			typeMap.Put(DbType.Binary, "VARBINARY(8000)");
			typeMap.Put(DbType.Binary, 8000, "VARBINARY($l)");
			typeMap.Put(DbType.Binary, int.MaxValue, "VARBINARY(MAX)");
			typeMap.Put(DbType.Boolean, "BIT");
			typeMap.Put(DbType.Byte, "TINYINT");
			typeMap.Put(DbType.Currency, "MONEY");
			typeMap.Put(DbType.Date, "DATETIME");
			typeMap.Put(DbType.DateTime, "DATETIME");
			typeMap.Put(DbType.Decimal, "DECIMAL");
			typeMap.Put(DbType.Decimal, 38, "DECIMAL($l, $s)", 2);
			typeMap.Put(DbType.Double, "DOUBLE PRECISION"); //synonym for FLOAT(53)
			typeMap.Put(DbType.Guid, "UNIQUEIDENTIFIER");
			typeMap.Put(DbType.Int16, "SMALLINT");
			typeMap.Put(DbType.Int32, "INT");
			typeMap.Put(DbType.Int64, "BIGINT");
			typeMap.Put(DbType.Single, "REAL"); //synonym for FLOAT(24) 
			typeMap.Put(DbType.StringFixedLength, "NCHAR(255)");
			typeMap.Put(DbType.StringFixedLength, 4000, "NCHAR($l)");
			typeMap.Put(DbType.String, "NVARCHAR(255)");
			typeMap.Put(DbType.String, 4000, "NVARCHAR($l)");
			typeMap.Put(DbType.String, int.MaxValue, "NVARCHAR(MAX)");
			typeMap.Put(DbType.Time, "DATETIME");
			typeMap.Put(DbType.Xml, "XML");

			propertyMap.RegisterProperty(ColumnProperty.Identity, "IDENTITY");
		}

		#region Overrides of SqlGenerator

		public override string NamesQuoteTemplate
		{
			get { return "[{0}]"; }
		}

		public override string BatchSeparator
		{
			get { return "GO"; }
		}

		public override string Default(object defaultValue)
		{
			if (defaultValue.GetType().Equals(typeof(bool)))
			{
				defaultValue = ((bool)defaultValue) ? 1 : 0;
			}

			return String.Format("DEFAULT {0}", defaultValue);
		}


		#endregion

		#region custom sql

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

			using (IDataReader reader = ExecuteReader(sql))
			{
				return reader.Read();
			}
		}

		protected override string GetSqlAddColumn(string table, string columnSql)
		{
			return FormatSql("ALTER TABLE {0:NAME} ADD {1}", table, columnSql);
		}

		public override bool ColumnExists(string table, string column)
		{
			using (IDataReader reader =
				ExecuteReader(String.Format("SELECT * FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE [TABLE_NAME]='{0}' AND [COLUMN_NAME]='{1}'", table, column)))
			{
				return reader.Read();
			}
		}

		public override bool TableExists(string table)
		{
			using (IDataReader reader =
				ExecuteReader(String.Format("SELECT * FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME]='{0}'", table)))
			{
				return reader.Read();
			}
		}

		public override void RemoveColumn(string table, string column)
		{
			DeleteColumnConstraints(table, column);
			base.RemoveColumn(table, column);
		}

		protected override string GetSqlRenameColumn(string tableName, string oldColumnName, string newColumnName)
		{
			return FormatSql("EXEC sp_rename '{0}.{1}', '{2}', 'COLUMN'", tableName, oldColumnName, newColumnName);
		}

		protected override string GetSqlRenameTable(string oldName, string newName)
		{
			return FormatSql("EXEC sp_rename '{0}', '{1}'", oldName, newName);
		}

		// Deletes all constraints linked to a column. Sql Server
		// doesn't seems to do this.
		private void DeleteColumnConstraints(string table, string column)
		{
			string sqlContrainte = FindConstraints(table, column);
			List<string> constraints = new List<string>();
			using (IDataReader reader = ExecuteReader(sqlContrainte))
			{
				while (reader.Read())
				{
					constraints.Add(reader.GetString(0));
				}
			}
			// Can't share the connection so two phase modif
			foreach (string constraint in constraints)
			{
				RemoveConstraint(table, constraint);
			}
		}

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

		#endregion
	}
}
