namespace ECM7.Migrator.Providers
{
	using System;

	public class TransformationProviderFactory
	{
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
				if (current == typeof(TransformationProvider<>))
				{
					
				}
			}

			Require.Throw("Заданный тип провайдера () должен быть унаследован от класса TransformationProvider<TConnection>");
			return null;
		}
	}
}
