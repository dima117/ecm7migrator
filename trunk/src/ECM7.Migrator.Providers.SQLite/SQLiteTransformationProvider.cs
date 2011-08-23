namespace ECM7.Migrator.Providers.SQLite
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.SQLite;

	using ECM7.Migrator.Framework;

	/// <summary>
	/// Summary description for SQLiteTransformationProvider.
	/// </summary>
	public class SQLiteTransformationProvider : TransformationProvider<SQLiteConnection>
	{
		/// <summary>
		/// �������������
		/// </summary>
		/// <param name="connection">����������� � ��</param>
		public SQLiteTransformationProvider(SQLiteConnection connection)
			: base(connection)
		{
			RegisterColumnType(DbType.Binary, "BLOB");
			RegisterColumnType(DbType.Byte, "INTEGER");
			RegisterColumnType(DbType.Int16, "INTEGER");
			RegisterColumnType(DbType.Int32, "INTEGER");
			RegisterColumnType(DbType.Int64, "INTEGER");
			RegisterColumnType(DbType.SByte, "INTEGER");
			RegisterColumnType(DbType.UInt16, "INTEGER");
			RegisterColumnType(DbType.UInt32, "INTEGER");
			RegisterColumnType(DbType.UInt64, "INTEGER");
			RegisterColumnType(DbType.Currency, "NUMERIC");
			RegisterColumnType(DbType.Decimal, "NUMERIC");
			RegisterColumnType(DbType.Double, "NUMERIC");
			RegisterColumnType(DbType.Single, "NUMERIC");
			RegisterColumnType(DbType.VarNumeric, "NUMERIC");
			RegisterColumnType(DbType.String, "TEXT");
			RegisterColumnType(DbType.AnsiString, "TEXT");
			RegisterColumnType(DbType.AnsiStringFixedLength, "TEXT");
			RegisterColumnType(DbType.StringFixedLength, "TEXT");
			RegisterColumnType(DbType.DateTime, "DATETIME");
			RegisterColumnType(DbType.Time, "DATETIME");
			RegisterColumnType(DbType.Boolean, "INTEGER");
			RegisterColumnType(DbType.Guid, "UNIQUEIDENTIFIER");

			RegisterProperty(ColumnProperty.Identity, "AUTOINCREMENT");
		}

		#region Overrides of SqlGenerator

		public override bool NeedsNotNullForIdentity
		{
			get { return false; }
		}

		public override string NamesQuoteTemplate
		{
			get { return "[{0}]"; }
		}

		public override string BatchSeparator
		{
			get { return "GO"; }
		}

		#endregion

		#region custom sql

		/// <summary>
		/// Check that the index with the specified name already exists
		/// </summary>
		public override bool IndexExists(string indexName, string tableName)
		{
			throw new NotSupportedException("SQLite �� ������������ �������");
		}

		/// <summary>
		/// ���������� �������� �����
		/// </summary>
		public override void AddForeignKey(
			string name,
			string primaryTable,
			string[] primaryColumns,
			string refTable,
			string[] refColumns,
			ECM7.Migrator.Framework.ForeignKeyConstraint onDeleteConstraint,
			ECM7.Migrator.Framework.ForeignKeyConstraint onUpdateConstraint)
		{
			// todo: �������� ����� �� ���������� ��������� ������� ������ � SQLite
			// todo: ���������, ��� ��� ��������� ���������� �������� ����� ������������ ����������
			throw new NotSupportedException("SQLite �� ������������ ������� �����");
		}

		/// <summary>
		/// Remove an existing foreign key constraint
		/// </summary>
		/// <param name="name">The name of the foreign key to remove</param>
		/// <param name="table">The table that contains the foreign key.</param>
		public override void RemoveForeignKey(string name, string table)
		{
			throw new NotSupportedException("SQLite �� ������������ ������� �����");
		}

		/// <summary>
		/// Remove an existing column from a table
		/// </summary>
		/// <param name="table">The name of the table to remove the column from</param>
		/// <param name="column">The column to remove</param>
		public override void RemoveColumn(string table, string column)
		{
			throw new NotSupportedException("SQLite �� ������������ �������� �������");
		}

		/// <summary>
		/// Rename an existing table
		/// </summary>
		/// <param name="tableName">The name of the table</param>
		/// <param name="oldColumnName">The old name of the column</param>
		/// <param name="newColumnName">The new name of the column</param>
		public override void RenameColumn(string tableName, string oldColumnName, string newColumnName)
		{
			throw new NotSupportedException("SLQite �� ������������ �������������� �������");
		}

		public override void ChangeColumn(string table, Column column)
		{
			throw new NotSupportedException("SLQite �� ������������ ��������� �������");
		}

		/// <summary>
		/// Check if a table already exists
		/// </summary>
		/// <param name="table">The name of the table that you want to check on.</param>
		public override bool TableExists(string table)
		{
			using (IDataReader reader =
				ExecuteReader(String.Format("SELECT [name] FROM [sqlite_master] WHERE [type]='table' and [name]='{0}'", table)))
			{
				return reader.Read();
			}
		}

		/// <summary>
		/// Determines if a constraint exists.
		/// </summary>
		/// <param name="table">Table owning the constraint</param>
		/// <param name="name">Constraint name</param>
		/// <returns><c>true</c> if the constraint exists.</returns>
		public override bool ConstraintExists(string table, string name)
		{
			return false;
		}

		/// <summary>
		/// Get the names of all of the tables
		/// </summary>
		/// <returns>The names of all the tables.</returns>
		public override string[] GetTables()
		{
			List<string> tables = new List<string>();

			const string SQL = "SELECT [name] FROM [sqlite_master] WHERE [type]='table' AND [name] <> 'sqlite_sequence' ORDER BY [name]";
			using (IDataReader reader = ExecuteReader(SQL))
			{
				while (reader.Read())
				{
					tables.Add((string)reader[0]);
				}
			}

			return tables.ToArray();
		}

		#endregion
	}
}
