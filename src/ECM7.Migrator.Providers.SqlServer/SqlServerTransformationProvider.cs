namespace ECM7.Migrator.Providers.SqlServer
{
	using System.Data.SqlClient;

	public class SqlServerTransformationProvider : BaseSqlServerTransformationProvider<SqlConnection>
	{
		public SqlServerTransformationProvider(SqlConnection connection)
			: base(connection)
		{
		}
	}
}
