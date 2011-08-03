namespace ECM7.Migrator.Providers.SqlServer
{
	using System.Data;
	using System.Data.SqlServerCe;

	public class SqlServerCeTransformationProviderFactory
		: ITransformationProviderFactory<SqlServerCeTransformationProvider>
	{
		#region Implementation of ITransformationProviderFactory<out MySqlTransformationProvider>

		public SqlServerCeTransformationProvider CreateProvider(IDbConnection connection)
		{
			Require.IsNotNull(connection, "Не задано подключение");

			SqlCeConnection typedConnection = connection as SqlCeConnection;
			Require.IsNotNull(typedConnection,
				"Подключение должно иметь тип SqlCeConnection, передано подключение типа {0}", connection.GetType());

			return new SqlServerCeTransformationProvider(typedConnection);
		}

		public SqlServerCeTransformationProvider CreateProvider(string connectionString)
		{
			SqlCeConnection connection = new SqlCeConnection(connectionString);
			return this.CreateProvider(connection);
		}

		#endregion
	}
}
