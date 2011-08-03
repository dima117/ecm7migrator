namespace ECM7.Migrator.Providers.MySql
{
	using System.Data;

	using global::MySql.Data.MySqlClient;

	public class MySqlTransformationProviderFactory
		: ITransformationProviderFactory<MySqlTransformationProvider>
	{
		#region Implementation of ITransformationProviderFactory<out MySqlTransformationProvider>

		public MySqlTransformationProvider CreateProvider(IDbConnection connection)
		{
			Require.IsNotNull(connection, "Не задано подключение");

			MySqlConnection typedConnection = connection as MySqlConnection;
			Require.IsNotNull(typedConnection,
				"Подключение должно иметь тип MySqlConnection, передано подключение типа {0}", connection.GetType());

			return new MySqlTransformationProvider(typedConnection);
		}

		public MySqlTransformationProvider CreateProvider(string connectionString)
		{
			MySqlConnection connection = new MySqlConnection(connectionString);
			return this.CreateProvider(connection);
		}

		#endregion
	}
}
