
using System;
using System.Data;
using ECM7.Migrator.Framework;

namespace ECM7.Migrator.Providers.SQLite
{
	public class SQLiteDialect : Dialect
	{
		public SQLiteDialect()
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

		public override Type TransformationProviderType
		{
			get { return typeof(SQLiteTransformationProvider); }
		}

		// todo: ������� ������������ ������������� ��������
		public override bool NamesNeedsQuote
		{
			get { return true; }
		}

		// todo: ��� ��������� ���������� ���������� ����� ������������ ����������
		public override string NamesQuoteTemplate
		{
			get { return "[{0}]"; }
		}

		public override bool NeedsNotNullForIdentity
		{
			get { return false; }
		}
	}
}