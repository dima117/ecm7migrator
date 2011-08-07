namespace ECM7.Migrator.Providers.SqlServer
{
	using System.Data.SqlClient;

	using ECM7.Migrator.Providers.SqlServer.Base;

	public class SqlServerTransformationProvider : BaseSqlServerTransformationProvider<SqlConnection>
	{
		public SqlServerTransformationProvider(SqlConnection connection)
			: base(connection)
		{
		}
	}
}
