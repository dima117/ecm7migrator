using System.Data;
using FirebirdSql.Data.FirebirdClient;

namespace ECM7.Migrator.Providers.Firebird
{
	public class FirebirdTransformationProviderFactory
		: ITransformationProviderFactory<FirebirdTransformationProvider>
	{
		#region Implementation of ITransformationProviderFactory<out MySqlTransformationProvider>

		public FirebirdTransformationProvider CreateProvider(IDbConnection connection)
		{
			Require.IsNotNull(connection, "Не задано подключение");

			FbConnection typedConnection = connection as FbConnection;
			Require.IsNotNull(typedConnection,
				"Подключение должно иметь тип FbConnection, передано подключение типа {0}", connection.GetType());

			return new FirebirdTransformationProvider(typedConnection);
		}

		public FirebirdTransformationProvider CreateProvider(string connectionString)
		{
			FbConnection connection = new FbConnection(connectionString);
			return CreateProvider(connection);
		}

		#endregion
	}
}
