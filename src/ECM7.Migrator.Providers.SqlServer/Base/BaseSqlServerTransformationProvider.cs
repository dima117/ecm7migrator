namespace ECM7.Migrator.Providers.SqlServer.Base
{
	using System;
	using System.Collections.Generic;
	using System.Data;

	using Framework;

	using System.Text;

	/// <summary>
	/// Migration transformations provider for Microsoft SQL Server.
	/// </summary>
	public abstract class BaseSqlServerTransformationProvider : TransformationProvider
	{
		protected BaseSqlServerTransformationProvider(IDbConnection connection)
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

			propertyMap.RegisterPropertySql(ColumnProperty.Identity, "IDENTITY");
		}

		#region generate sql

		protected override string GetSqlDefaultValue(object defaultValue)
		{
			if (defaultValue is bool)
			{
				defaultValue = ((bool)defaultValue) ? 1 : 0;
			}

			return String.Format("DEFAULT {0}", defaultValue);
		}

		#endregion

		#region Особенности СУБД

		protected override string NamesQuoteTemplate
		{
			get { return "[{0}]"; }
		}

		public override string BatchSeparator
		{
			get { return "GO"; }
		}

		#endregion

		#region generate sql

		protected override string GetSqlAddColumn(SchemaQualifiedObjectName table, string columnSql)
		{
			return FormatSql("ALTER TABLE {0:NAME} ADD {1}", table, columnSql);
		}

		protected override string GetSqlRenameColumn(SchemaQualifiedObjectName tableName, string oldColumnName, string newColumnName)
		{
			return FormatSql("EXEC sp_rename '{0}.{1}', '{2}', 'COLUMN'", tableName, oldColumnName, newColumnName);
		}

		protected override string GetSqlRenameTable(SchemaQualifiedObjectName oldName, string newName)
		{
			return FormatSql("EXEC sp_rename '{0}', '{1}'", oldName, newName);
		}

		#endregion

		#region DDL

		public override bool IndexExists(string indexName, SchemaQualifiedObjectName tableName)
		{
			string sql = FormatSql(
				"SELECT COUNT(*) FROM [sys].[indexes] WHERE [name] = '{0}' AND [object_id] = object_id(N'{1:NAME}')", indexName, tableName);
			int count = Convert.ToInt32(ExecuteScalar(sql));
			return count > 0;
		}

		public override bool ConstraintExists(SchemaQualifiedObjectName table, string name)
		{
			SchemaQualifiedObjectName fullConstraintName = name.WithSchema(table.Schema);

			string sql = FormatSql(
				"SELECT TOP 1 [name] FROM [sys].[objects] " +
				"WHERE [parent_object_id] = object_id('{0:NAME}') " +
				"AND [object_id] = object_id('{1:NAME}') " +
				"AND [type] IN ('D', 'F', 'PK', 'UQ')" +
				"UNION ALL " +
				"SELECT TOP 1 [name] FROM [sys].[check_constraints] " +
				"WHERE [parent_object_id] = OBJECT_ID(N'{0:NAME}') AND " +
				"[object_id] = OBJECT_ID(N'{1:NAME}')", table, fullConstraintName);

			using (IDataReader reader = ExecuteReader(sql))
			{
				return reader.Read();
			}
		}

		public override bool ColumnExists(SchemaQualifiedObjectName table, string column)
		{
			string sql = FormatSql(
				"SELECT * FROM [INFORMATION_SCHEMA].[COLUMNS] " +
				"WHERE [TABLE_NAME]='{0}' AND [COLUMN_NAME]='{1}'",
				table.Name, column);

			if (!table.Schema.IsNullOrEmpty(true))
			{
				sql += FormatSql(" AND [TABLE_SCHEMA] = '{0}'", table.Schema);
			}

			using (IDataReader reader = ExecuteReader(sql))
			{
				return reader.Read();
			}
		}

		public override bool TableExists(SchemaQualifiedObjectName table)
		{
			string sql = FormatSql(
				"SELECT * FROM [INFORMATION_SCHEMA].[TABLES] " +
				"WHERE [TABLE_NAME]='{0}'", table.Name);

			if (!table.Schema.IsNullOrEmpty(true))
			{
				sql += FormatSql(" AND [TABLE_SCHEMA] = '{0}'", table.Schema);
			}

			using (IDataReader reader = ExecuteReader(sql))
			{
				return reader.Read();
			}
		}

		public override SchemaQualifiedObjectName[] GetTables(string schema = null)
		{
			string nspname = schema.IsNullOrEmpty(true) ? "dbo" : schema;

			var tables = new List<SchemaQualifiedObjectName>();

			string sql = FormatSql("SELECT {0:NAME}, {1:NAME} FROM {2:NAME}.{3:NAME} where {4:NAME} = '{5}'",
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

		// todo: написать тесты на схемы 

		public override void RemoveColumn(SchemaQualifiedObjectName table, string column)
		{
			DeleteColumnConstraints(table, column);
			base.RemoveColumn(table, column);
		}

		// Deletes all constraints linked to a column. 
		// Sql Server doesn't seems to do this.
		private void DeleteColumnConstraints(SchemaQualifiedObjectName table, string column)
		{
			string sqlContraints = FindConstraints(table, column);
			List<string> constraints = new List<string>();
			using (IDataReader reader = ExecuteReader(sqlContraints))
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

		protected virtual string FindConstraints(SchemaQualifiedObjectName table, string column)
		{
			StringBuilder sqlBuilder = new StringBuilder();

			sqlBuilder.Append("SELECT [CONSTRAINT_NAME] ");
			sqlBuilder.Append("FROM [INFORMATION_SCHEMA].[CONSTRAINT_COLUMN_USAGE] ");
			sqlBuilder.AppendFormat("WHERE [TABLE_NAME] = '{0}' and [COLUMN_NAME] = '{1}' ", table.Name, column);

			if (!table.Schema.IsNullOrEmpty(true))
			{
				sqlBuilder.AppendFormat("AND [TABLE_SCHEMA] = '{0}' ", table.Schema);
			}

			sqlBuilder.Append("UNION ALL ");
			sqlBuilder.Append("SELECT [dobj].[name] as [CONSTRAINT_NAME] ");
			sqlBuilder.Append("FROM [sys].[columns] [col] ");
			sqlBuilder.Append("INNER JOIN [sys].[objects] [dobj] ");
			sqlBuilder.Append("ON [dobj].[object_id] = [col].[default_object_id] AND [dobj].[type] = 'D' ");
			sqlBuilder.Append(FormatSql("WHERE [col].[object_id] = object_id(N'{0:NAME}') AND [col].[name] = '{1}'", table, column));

			return sqlBuilder.ToString();
		}

		#endregion
	}
}
