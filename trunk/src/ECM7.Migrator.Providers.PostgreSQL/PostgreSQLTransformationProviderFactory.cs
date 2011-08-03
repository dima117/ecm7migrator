namespace ECM7.Migrator.Providers.PostgreSQL
{
	using System.Data;

	using Npgsql;

	public class PostgreSQLTransformationProviderFactory 
		: ITransformationProviderFactory<PostgreSQLTransformationProvider>
	{
		#region Implementation of ITransformationProviderFactory<out PostgreSQLTransformationProvider>

		public PostgreSQLTransformationProvider CreateProvider(IDbConnection connection)
		{
			Require.IsNotNull(connection, "Не задано подключение");

			NpgsqlConnection typedConnection = connection as NpgsqlConnection;
			Require.IsNotNull(typedConnection, 
				"Подключение должно иметь тип NpgsqlConnection, передано подключение типа {0}", connection.GetType());
		
			return new PostgreSQLTransformationProvider(typedConnection);
		}

		public PostgreSQLTransformationProvider CreateProvider(string connectionString)
		{
			NpgsqlConnection connection = new NpgsqlConnection(connectionString);
			return CreateProvider(connection);
		}

		#endregion
	}
}
