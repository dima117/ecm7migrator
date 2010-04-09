using System;
using ECM7.Migrator.Framework;

namespace ECM7.Migrator.Loader
{
	/// <summary>
	/// Информация о миграции. Обладает следующими свойствами:
	/// - обязательно  содержит класс миграции (!= null)
	/// - обязательно  содержит версию, соответствующую данному классу
	/// - обязательно  содержит значение свойства Ignore для данного класса
	/// </summary>
	public struct MigrationInfo
	{
		// todo: написать тесты
		public MigrationInfo(Type type)
		{
			Require.IsNotNull(type, "Не задан обрабатываемый класс");
			Require.That(typeof(IMigration).IsAssignableFrom(type), "Класс миграции должен реализовывать интерфейс IMigration");

			MigrationAttribute attribute = Attribute.GetCustomAttribute(
				type, typeof(MigrationAttribute)) as MigrationAttribute;
			Require.IsNotNull(attribute, "Не найден атрибут Migration");


			this.type = type;
			version = attribute.Version;
			ignore = attribute.Ignore;
		}

		private readonly Type type;
		private readonly long version;
		private readonly bool ignore;

		public Type Type { get { return type; } }
		public long Version { get { return version; } }
		public bool Ignore { get { return ignore; } }
	}
}