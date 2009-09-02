using System;
using System.Data;

namespace ECM7.Migrator.Providers.SqlServer
{
	// todo: написать для всех провайдеров тесты на типы данных
    public class SqlServerCeDialect : SqlServerDialect
    {
		public SqlServerCeDialect()
        {
			RegisterColumnType(DbType.AnsiStringFixedLength, "NCHAR(255)");
			RegisterColumnType(DbType.AnsiStringFixedLength, 4000, "NCHAR($l)");
			RegisterColumnType(DbType.AnsiString, "NVARCHAR(255)");
			RegisterColumnType(DbType.AnsiString, 4000, "NVARCHAR($l)");
			RegisterColumnType(DbType.AnsiString, 1073741823, "TEXT");

			RegisterColumnType(DbType.Decimal, "NUMERIC(19,5)");
			RegisterColumnType(DbType.Decimal, 19, "NUMERIC(19, $l)");
			RegisterColumnType(DbType.Double, "FLOAT");
			
        }

        public override Type TransformationProvider { get { return typeof (SqlServerCeTransformationProvider); } }

    }
}
