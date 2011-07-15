
using System;
using System.Collections.Generic;
using System.Data;
using ECM7.Migrator.Framework;

namespace ECM7.Migrator.Providers.Oracle
{
	public class OracleDialect : Dialect
	{
		public OracleDialect()
		{
			RegisterColumnType(DbType.AnsiStringFixedLength, "CHAR(255)");
			RegisterColumnType(DbType.AnsiStringFixedLength, 2000, "CHAR($l)");
			RegisterColumnType(DbType.AnsiString, "VARCHAR2(255)");
			RegisterColumnType(DbType.AnsiString, 2000, "VARCHAR2($l)");
			RegisterColumnType(DbType.AnsiString, 2147483647, "CLOB"); // should use the IType.ClobType
			RegisterColumnType(DbType.Binary, "RAW(2000)");
			RegisterColumnType(DbType.Binary, 2000, "RAW($l)");
			RegisterColumnType(DbType.Binary, 2147483647, "BLOB");
			RegisterColumnType(DbType.Boolean, "NUMBER(1,0)");
			RegisterColumnType(DbType.Byte, "NUMBER(3,0)");
			RegisterColumnType(DbType.Currency, "NUMBER(19,1)");
			RegisterColumnType(DbType.Date, "DATE");
			RegisterColumnType(DbType.DateTime, "TIMESTAMP(4)");
			RegisterColumnType(DbType.Decimal, "NUMBER");
			RegisterColumnType(DbType.Decimal, 38, "NUMBER($l, $s)", 2);
			// having problems with both ODP and OracleClient from MS not being able
			// to read values out of a field that is DOUBLE PRECISION
			RegisterColumnType(DbType.Double, "BINARY_DOUBLE");
			RegisterColumnType(DbType.Guid, "RAW(16)");
			RegisterColumnType(DbType.Int16, "NUMBER(5,0)");
			RegisterColumnType(DbType.Int32, "NUMBER(10,0)");
			RegisterColumnType(DbType.Int64, "NUMBER(18,0)");
			RegisterColumnType(DbType.Single, "FLOAT(24)");
			RegisterColumnType(DbType.StringFixedLength, "NCHAR(255)");
			RegisterColumnType(DbType.StringFixedLength, 2000, "NCHAR($l)");
			RegisterColumnType(DbType.String, "NVARCHAR2(255)");
			RegisterColumnType(DbType.String, 2000, "NVARCHAR2($l)");
			RegisterColumnType(DbType.String, 1073741823, "NCLOB");
			RegisterColumnType(DbType.Time, "DATE");

			RegisterProperty(ColumnProperty.Null, String.Empty);
		}

		public override string Default(object defaultValue)
		{
			// convert boolean to number (1, 0)
			if (defaultValue is bool)
			{
			    defaultValue = (bool)defaultValue ? 1 : 0;
			}

			return base.Default(defaultValue);
		}

		public override Type TransformationProviderType { get { return typeof(OracleTransformationProvider); } }

		protected override void BuildColumnSql(List<string> vals, Column column, bool compoundPrimaryKey)
		{
			AddColumnName(vals, column);
			AddColumnType(vals, column);
			AddDefaultValueSql(vals, column);
			AddNotNullSql(vals, column);
			AddPrimaryKeySql(vals, column, compoundPrimaryKey);
			AddUniqueSql(vals, column);
		}

        public override bool NamesNeedsQuote
        {
            get { return true; }
        }

        public override string NamesQuoteTemplate
        {
            get { return "\"{0}\""; }
        }
	}
}