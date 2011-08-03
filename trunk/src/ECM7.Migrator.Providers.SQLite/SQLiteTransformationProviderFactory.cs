namespace ECM7.Migrator.Providers.SQLite
{
	using System.Data;
	using System.Data.SQLite;

	public class SQLiteTransformationProviderFactory
		: ITransformationProviderFactory<SQLiteTransformationProvider>
	{
		#region Implementation of ITransformationProviderFactory<out MySqlTransformationProvider>

		public SQLiteTransformationProvider CreateProvider(IDbConnection connection)
		{
			Require.IsNotNull(connection, "Не задано подключение");

			SQLiteConnection typedConnection = connection as SQLiteConnection;
			Require.IsNotNull(typedConnection,
				"Подключение должно иметь тип SQLiteConnection, передано подключение типа {0}", connection.GetType());

			return new SQLiteTransformationProvider(typedConnection);
		}

		public SQLiteTransformationProvider CreateProvider(string connectionString)
		{
			SQLiteConnection connection = new SQLiteConnection(connectionString);
			return this.CreateProvider(connection);
		}

		#endregion
	}
}
