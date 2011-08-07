namespace ECM7.Migrator.Providers
{
	using System;
	using System.Collections.Generic;
	using System.Data;

	using ECM7.Migrator.Framework;

	public class TransformationProviderFactory
	{
		public static ITransformationProvider Create<TProvider>(string connectionString)
			where TProvider : ITransformationProvider
		{
			return Create(typeof(TProvider), connectionString);
		}

		public static ITransformationProvider Create<TProvider>(IDbConnection connection)
			where TProvider : ITransformationProvider
		{
			return Create(typeof(TProvider), connection);
		}

		public static ITransformationProvider Create(string providerTypeName, string connectionString)
		{
			Type providerType = GetProviderType(providerTypeName);
			return Create(providerType, connectionString);
		}

		public static ITransformationProvider Create(string providerTypeName, IDbConnection connection)
		{
			Type providerType = GetProviderType(providerTypeName);
			return Create(providerType, connection);
		}

		public static ITransformationProvider Create(Type providerType, string connectionString)
		{
			Require.IsNotNullOrEmpty(connectionString, "Не задана строка подключения");

			Type connectionType = GetConnectionType(providerType);
			Require.That(typeof(IDbConnection).IsAssignableFrom(connectionType), "Тип подключения ({0}) должен реализовывать интерфейс IDbConnection", connectionType.FullName);

			IDbConnection connection = Activator.CreateInstance(connectionType) as IDbConnection;
			Require.IsNotNull(connection, "Не удалось создать подключение к БД");
			// ReSharper disable PossibleNullReferenceException
			connection.ConnectionString = connectionString;
			// ReSharper restore PossibleNullReferenceException

			return Create(providerType, connection);
		}

		public static ITransformationProvider Create(Type providerType, IDbConnection connection)
		{
			Require.IsNotNull(connection, "Не инициализировано подключение к БД");

			Require.IsNotNull(providerType, "Не задан тип создаваемого провайдера");
			Require.That(typeof(ITransformationProvider).IsAssignableFrom(providerType), "Тип провайдера ({0}) должен реализовывать интерфейс ITransformationProvider", providerType.FullName);

			ITransformationProvider provider = Activator.CreateInstance(providerType, connection) as ITransformationProvider;
			Require.IsNotNull(provider, "Не удалось создать экземпляр провайдера");

			return provider;
		}
		
		#region Helpers

		#region Shortcuts

		// todo: поменять шорткаты
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
			for (Type current = providerType; current != null; current = current.BaseType)
			{

				if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(TransformationProvider<>))
				{
					var connectionType = current.GetGenericArguments()[0];
					Require.That(typeof(IDbConnection).IsAssignableFrom(connectionType), "Тип подключения ({0}) должен реализовывать интерфейс IDbConnection", connectionType.FullName);

					return connectionType;
				}
			}

			Require.Throw("Заданный тип провайдера ({0}) должен быть унаследован от класса TransformationProvider<TConnection>", providerType);
			return null;
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

		#endregion
	}
}
