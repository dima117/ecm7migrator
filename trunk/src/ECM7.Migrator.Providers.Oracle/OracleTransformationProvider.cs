using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using ECM7.Migrator.Framework;
using ForeignKeyConstraint = ECM7.Migrator.Framework.ForeignKeyConstraint;
using OracleConnection=Oracle.DataAccess.Client.OracleConnection;

namespace ECM7.Migrator.Providers.Oracle
{
	public class OracleTransformationProvider : TransformationProvider
	{
		/// <summary>
		/// ������� ����� oracle
		/// todo: �������� �������� ����� � ������ ����������� ������������� ��������
		/// </summary>
		public string Scheme { get; protected set; }

		public OracleTransformationProvider(Dialect dialect, string connectionString)
			: base(dialect, connectionString)
		{

			var csBuilder = new OracleConnectionStringBuilder(connectionString);
			Scheme = csBuilder.UserID;
			
			connection = new OracleConnection();
			connection.ConnectionString = base.connectionString;
			connection.Open();
		}

		public override void AddForeignKey(string name, string primaryTable, string[] primaryColumns, string refTable,
										  string[] refColumns, ForeignKeyConstraint constraint)
		{
			if (ConstraintExists(primaryTable, name))
			{
				Logger.Warn("Constraint {0} already exists", name);
				return;
			}

			List<string> command = new List<string>();
			command.Add("ALTER TABLE {0}".FormatWith(primaryTable));
			command.Add("ADD CONSTRAINT {0}".FormatWith(name));
			command.Add("FOREIGN KEY ({0})"
				.FormatWith(primaryColumns.ToCommaSeparatedString()));
			command.Add("REFERENCES {0} ({1})"
				.FormatWith(refTable, refColumns.ToCommaSeparatedString()));

			switch (constraint)
			{
				case ForeignKeyConstraint.Cascade:
					command.Add("ON DELETE CASCADE");
					break;
				//command.Add("NOVALIDATE");
			}

			string commandText = command.ToSeparatedString(" ");
			ExecuteNonQuery(commandText);
		}

		public override bool IndexExists(string indexName, string tableName)
		{
			string sql =
				("select count(*) from user_indexes where " + 
				"lower(table_owner) = '{0}' " + 
				"and lower(INDEX_NAME) = '{1}' " + 
				"and lower(TABLE_NAME) = '{2}'")
				.FormatWith(Scheme.ToLower(), indexName.ToLower(), tableName.ToLower());

			// todo: ��������� �������� ������� ��� �������� ������� ������� �� ��������� ColumnProperty.Indexed

			// todo: ������� ������, ����������� ���������� �������, � �����������
			int count = Convert.ToInt32(ExecuteScalar(sql));
			return count > 0;
		}

		public override void RemoveIndex(string indexName, string tableName)
		{
			if (!IndexExists(indexName, tableName))
			{
				Logger.Warn("Index {0} is not exists", indexName);
				return;
			}

			string sql = "DROP INDEX {0}"
				.FormatWith(Dialect.QuoteIfNeeded(indexName));

			ExecuteNonQuery(sql);
		}

		// todo: �������� ����� �� ���������� �������� ����� � ��������� �����������
		public override void AddForeignKey(string name, string primaryTable, string[] primaryColumns, string refTable, string[] refColumns, ForeignKeyConstraint onDeleteConstraint, ForeignKeyConstraint onUpdateConstraint)
		{
			throw new NotSupportedException("Oracle �� ������������ ��������� ����������");
		}

		public override void AddColumn(string table, string sqlColumn)
		{
			ExecuteNonQuery(String.Format("ALTER TABLE {0} ADD ({1})", table, sqlColumn));
		}

		public override bool ConstraintExists(string table, string name)
		{
			string sql =
				string.Format(
					"SELECT COUNT(constraint_name) FROM user_constraints WHERE lower(constraint_name) = '{0}' AND lower(table_name) = '{1}'",
					name.ToLower(), table.ToLower());
			Logger.Log(sql);
			object scalar = ExecuteScalar(sql);
			return Convert.ToInt32(scalar) == 1;
		}

		public override bool ColumnExists(string table, string column)
		{
			if (!TableExists(table))
				return false;

			string sql =
				string.Format(
					"SELECT COUNT(column_name) FROM user_tab_columns WHERE lower(table_name) = '{0}' AND lower(column_name) = '{1}'",
					table.ToLower(), column.ToLower());
			Logger.Log(sql);
			object scalar = ExecuteScalar(sql);
			return Convert.ToInt32(scalar) == 1;
		}

		public override bool TableExists(string table)
		{
			string sql = string.Format("SELECT COUNT(table_name) FROM user_tables WHERE lower(table_name) = '{0}'",
									   table.ToLower());
			Logger.Log(sql);
			object count = ExecuteScalar(sql);
			return Convert.ToInt32(count) == 1;
		}

		public override string[] GetTables()
		{
			List<string> tables = new List<string>();

			using (IDataReader reader =
				ExecuteQuery("SELECT table_name FROM user_tables"))
			{
				while (reader.Read())
				{
					tables.Add(reader[0].ToString());
				}
			}

			return tables.ToArray();
		}

		public override Column[] GetColumns(string table)
		{
			List<Column> columns = new List<Column>();


			using (
				IDataReader reader =
					ExecuteQuery(
						string.Format(
							"select column_name, data_type, data_length, data_precision, data_scale, nullable FROM USER_TAB_COLUMNS WHERE lower(table_name) = '{0}'",
							table.ToLower())))
			{
				while (reader.Read())
				{
					string colName = reader["column_name"].ToString();
					DbType colType = DbType.String;
					string dataType = reader["data_type"].ToString().ToLower();
					bool nullable = reader["nullable"].ToString() == "Y";

					if (dataType.Equals("number"))
					{
						int precision = Convert.ToInt32(reader[3]);
						int scale = Convert.ToInt32(reader[4]);
						if (scale == 0)
						{
							colType = precision <= 10 ? DbType.Int16 : DbType.Int64;
						}
						else
						{
							colType = DbType.Decimal;
						}
					}
					else if (dataType.StartsWith("timestamp") || dataType.Equals("date"))
					{
						colType = DbType.DateTime;
					}

					ColumnProperty properties = nullable
						? ColumnProperty.Null
						: ColumnProperty.NotNull;

					columns.Add(new Column(colName, colType, properties));
				}
			}

			return columns.ToArray();
		}

		public override void ChangeColumn(string table, string sqlColumn)
		{
			ExecuteNonQuery(String.Format("ALTER TABLE {0} MODIFY ({1})", table, sqlColumn));
		}
	}
}
