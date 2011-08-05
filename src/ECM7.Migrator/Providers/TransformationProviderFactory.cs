namespace ECM7.Migrator.Providers
{
	using System;
	using System.Data;

	using ECM7.Migrator.Framework;

	public class TransformationProviderFactory
	{
		public static ITransformationProvider Create(Type providerType, string connectionString)
		{
			Require.IsNotNullOrEmpty(connectionString, "Не задана строка подключения");

			Type connectionType = GetConnectionType(providerType);

			IDbConnection connection = Activator.CreateInstance(connectionType) as IDbConnection;
			Require.IsNotNull(connection, "Не удалось создать подключение к БД");
			// ReSharper disable PossibleNullReferenceException
			connection.ConnectionString = connectionString;
			// ReSharper restore PossibleNullReferenceException

			return Create(providerType, connection);
		}

		public static ITransformationProvider Create(Type providerType, IDbConnection connection)
		{
			Require.IsNotNull(providerType, "Не тзадан тип создаваемого провайдера");
			Require.IsNotNull(connection, "Не инициализировано подключение к БД");

			ITransformationProvider provider = Activator.CreateInstance(providerType, connection) as ITransformationProvider;
			Require.IsNotNull(provider, "Не удалось создать экземпляр провайдера");

			return provider;
		}

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
	}
}
