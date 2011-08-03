namespace ECM7.Migrator.Providers.SqlServer
{
	using System.Data;
	using System.Data.SqlClient;

	public class SqlServerTransformationProviderFactory
		: ITransformationProviderFactory<SqlServerTransformationProvider>
	{
		#region Implementation of ITransformationProviderFactory<out MySqlTransformationProvider>

		public SqlServerTransformationProvider CreateProvider(IDbConnection connection)
		{
			Require.IsNotNull(connection, "Не задано подключение");

			SqlConnection typedConnection = connection as SqlConnection;
			Require.IsNotNull(typedConnection,
				"Подключение должно иметь тип SqlConnection, передано подключение типа {0}", connection.GetType());

			return new SqlServerTransformationProvider(typedConnection);
		}

		public SqlServerTransformationProvider CreateProvider(string connectionString)
		{
			SqlConnection connection = new SqlConnection(connectionString);
			return this.CreateProvider(connection);
		}

		#endregion
	}
}
