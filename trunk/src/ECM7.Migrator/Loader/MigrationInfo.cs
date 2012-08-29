using System;
using ECM7.Migrator.Framework;

namespace ECM7.Migrator.Loader
{
	/// <summary>
	/// Информация о миграции. Обладает следующими свойствами:
	/// - обязательно  содержит класс миграции (!= null)
	/// - обязательно  содержит версию, соответствующую данному классу
	/// - обязательно  содержит значение свойства Ignore для данного класса
	/// - обязательно  содержит значение свойства WithoutTransaction для данного класса
	/// </summary>
	public struct MigrationInfo
	{
		/// <summary>
		/// Инициализация
		/// </summary>
		/// <param name="type">Тип, из которого извлекается информация о миграции</param>
		public MigrationInfo(Type type)
		{
			Require.IsNotNull(type, "Не задан обрабатываемый класс");
			Require.That(typeof(IMigration).IsAssignableFrom(type), "Класс миграции должен реализовывать интерфейс IMigration");

			MigrationAttribute attribute = type.GetCustomAttribute<MigrationAttribute>();
			Require.IsNotNull(attribute, "Не найден атрибут Migration");

			Type = type;
			Version = attribute.Version;
			Ignore = attribute.Ignore;
			WithoutTransaction = attribute.WithoutTransaction;
		}

		/// <summary>
		/// Тип миграции
		/// </summary>
		public readonly Type Type;

		/// <summary>
		/// Версия
		/// </summary>
		public readonly long Version;

		/// <summary>
		/// Признак: пропустить миграцию при выполнении
		/// </summary>
		public readonly bool Ignore;

		/// <summary>
		/// Признак: выполнить миграцию без транзакции
		/// </summary>
		public readonly bool WithoutTransaction;
	}
}
