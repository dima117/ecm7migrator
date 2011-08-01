using System.Data;

namespace ECM7.Migrator.Providers.Firebird
{
	using FirebirdSql.Data.FirebirdClient;

    public class FirebirdTransformationProvider : TransformationProvider
    {
        protected FirebirdTransformationProvider(Dialect dialect, IDbConnection connection)
            : base(dialect, connection)
        {
        }

        public FirebirdTransformationProvider(Dialect dialect,  string connectionString)
            : base(dialect,  new FbConnection(connectionString))
        {
        }

        public override bool IndexExists(string indexName, string tableName)
        {
            throw new System.NotImplementedException();
        }

        public override bool ConstraintExists(string table, string name)
        {
            throw new System.NotImplementedException();
        }

        public override void AddColumn(string table, string columnSql)
        {
            string sql = FormatSql("ALTER TABLE {0:NAME} ADD {1}", table, columnSql);
            ExecuteNonQuery(sql);
        }
    }
}
