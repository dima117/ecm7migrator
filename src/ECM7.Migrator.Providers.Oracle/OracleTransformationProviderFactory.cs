using System.Data;
using Oracle.DataAccess.Client;

namespace ECM7.Migrator.Providers.Oracle
{
	public class OracleTransformationProviderFactory
		: ITransformationProviderFactory<OracleTransformationProvider>
	{
		#region Implementation of ITransformationProviderFactory<out PostgreSQLTransformationProvider>

		public OracleTransformationProvider CreateProvider(IDbConnection connection)
		{
			Require.IsNotNull(connection, "Не задано подключение");

			OracleConnection typedConnection = connection as OracleConnection;
			Require.IsNotNull(typedConnection,
				"Подключение должно иметь тип OracleConnection, передано подключение типа {0}", connection.GetType());

			return new OracleTransformationProvider(typedConnection);
		}

		public OracleTransformationProvider CreateProvider(string connectionString)
		{
			OracleConnection connection = new OracleConnection(connectionString);
			return CreateProvider(connection);
		}

		#endregion
	}
}
