using System;
using System.Data;

namespace ECM7.Migrator.Providers.SqlServer
{
    public class SqlServer2005Dialect : SqlServerDialect
    {
        public SqlServer2005Dialect()
        {
            RegisterColumnType(DbType.AnsiString, int.MaxValue, "VARCHAR(MAX)");
			RegisterColumnType(DbType.Binary, int.MaxValue, "VARBINARY(MAX)");
			RegisterColumnType(DbType.String, int.MaxValue, "NVARCHAR(MAX)");
            RegisterColumnType(DbType.Xml, "XML");
        }

        public override Type TransformationProviderType { get { return typeof (SqlServerTransformationProvider); } }

    }
}
