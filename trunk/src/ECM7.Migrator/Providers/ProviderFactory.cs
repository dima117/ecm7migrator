using ECM7.Migrator.Providers.Validation;
using ECM7.Migrator.Utils;

namespace ECM7.Migrator.Providers
{
	using System;
	using System.Collections.Generic;
	using System.Data;

	using Framework;

	public class ProviderFactory
	{
		public static ITransformationProvider Create<TProvider>(string connectionString, int? commandTimeout)
			where TProvider : ITransformationProvider
		{
			return Create(typeof(TProvider), connectionString, commandTimeout);
		}

		public static ITransformationProvider Create<TProvider>(IDbConnection connection, int? commandTimeout)
			where TProvider : ITransformationProvider
		{
			return Create(typeof(TProvider), connection, commandTimeout);
		}

		public static ITransformationProvider Create(string providerTypeName, string connectionString, int? commandTimeout)
		{
			Type providerType = GetProviderType(providerTypeName);
			return Create(providerType, connectionString, commandTimeout);
		}

		public static ITransformationProvider Create(string providerTypeName, IDbConnection connection, int? commandTimeout)
		{
			Type providerType = GetProviderType(providerTypeName);
			return Create(providerType, connection, commandTimeout);
		}

		public static ITransformationProvider Create(Type providerType, string connectionString, int? commandTimeout)
		{
			Type connectionType = GetConnectionType(providerType);
			IDbConnection connection = CreateConnection(connectionType, connectionString);

			return Create(providerType, connection, commandTimeout);
		}

		public static ITransformationProvider Create(Type providerType, IDbConnection connection, int? commandTimeout)
		{
			Require.IsNotNull(connection, "Не инициализировано подключение к БД");

			Require.IsNotNull(providerType, "Не задан тип создаваемого провайдера");
			Require.That(typeof(ITransformationProvider).IsAssignableFrom(providerType), "Тип провайдера ({0}) должен реализовывать интерфейс ITransformationProvider", providerType.FullName);

			ITransformationProvider provider = Activator.CreateInstance(providerType, connection, commandTimeout) as ITransformationProvider;
			Require.IsNotNull(provider, "Не удалось создать экземпляр провайдера");

			return provider;
		}
		
		#region Helpers

		#region Shortcuts

		private static readonly Dictionary<string, string> shortcuts = new Dictionary<string, string>
		{
			{ "Firebird",		"ECM7.Migrator.Providers.Firebird.FirebirdTransformationProvider, ECM7.Migrator.Providers.Firebird" },
			{ "MySql",			"ECM7.Migrator.Providers.MySql.MySqlTransformationProvider, ECM7.Migrator.Providers.MySql" },
			{ "Oracle",			"ECM7.Migrator.Providers.Oracle.OracleTransformationProvider, ECM7.Migrator.Providers.Oracle" },
			{ "PostgreSQL",		"ECM7.Migrator.Providers.PostgreSQL.PostgreSQLTransformationProvider, ECM7.Migrator.Providers.PostgreSQL" },
			{ "SQLite",			"ECM7.Migrator.Providers.SQLite.SQLiteTransformationProvider, ECM7.Migrator.Providers.SQLite" },
			{ "SqlServer",		"ECM7.Migrator.Providers.SqlServer.SqlServerTransformationProvider, ECM7.Migrator.Providers.SqlServer" },
			{ "SqlServerCe",	"ECM7.Migrator.Providers.SqlServer.SqlServerCeTransformationProvider, ECM7.Migrator.Providers.SqlServer" },
		};

		#endregion

		/// <summary>
		/// Получение типа подключения для заданного типа провайдера
		/// <para>Проверки:</para>
		/// <para>- тип унаследован от базового класса провайдера</para>
		/// <para>- generic-параметр базового класса провайдера реализует интерфейс IDbConnection</para>
		/// </summary>
		/// <param name="providerType">Тип провайдера</param>
		/// <returns></returns>
		public static Type GetConnectionType(Type providerType)
		{
			Require.IsNotNull(providerType, "Не задан класс провайдера");

			ProviderValidationAttribute attr = providerType.GetCustomAttribute<ProviderValidationAttribute>(true);
			Require.IsNotNull(attr, "Для создаваемого провайдера не определен атрибут ProviderValidationAttribute");

			Type connectionType = attr.connectionType;
			Require.That(typeof(IDbConnection).IsAssignableFrom(connectionType), "Тип подключения ({0}) должен реализовывать интерфейс IDbConnection", connectionType.FullName);

			return connectionType;
		}

		/// <summary>
		/// Получение типа провайдера по строке, содержащей название класса или алиас
		/// </summary>
		/// <param name="providerName">название класса или алиас</param>
		public static Type GetProviderType(string providerName)
		{
			string providerTypeName = shortcuts.ContainsKey(providerName)
										? shortcuts[providerName]
										: providerName;

			Type providerType = Type.GetType(providerTypeName);
			Require.IsNotNull(providerType, "Не удалось загрузить класс провайдера: {0}", providerName.Nvl("null"));
			Require.That(typeof(ITransformationProvider).IsAssignableFrom(providerType), "Тип провайдера ({0}) должен реализовывать интерфейс ITransformationProvider", providerType.FullName);

			return providerType;
		}

		/// <summary>
		/// Создаем подключение к БД
		/// </summary>
		/// <param name="connectionType">Тип подключения</param>
		/// <param name="connectionString">СТрока подключения</param>
		private static IDbConnection CreateConnection(Type connectionType, string connectionString)
		{
			Require.IsNotNullOrEmpty(connectionString, "Не задана строка подключения");
		
			Require.That(typeof(IDbConnection).IsAssignableFrom(connectionType),
						 "Тип подключения ({0}) должен реализовывать интерфейс IDbConnection", connectionType.FullName);

			IDbConnection connection = Activator.CreateInstance(connectionType) as IDbConnection;
			Require.IsNotNull(connection, "Не удалось создать подключение к БД");
			// ReSharper disable PossibleNullReferenceException
			connection.ConnectionString = connectionString;
			// ReSharper restore PossibleNullReferenceException
			return connection;
		}

		#endregion
	}
}
