namespace ECM7.Migrator.Configuration
{
	/// <summary>
	/// Настройки мигратора
	/// </summary>
	public interface IMigratorConfiguration
	{
		/// <summary>
		/// Диалект
		/// </summary>
		string Dialect { get; }

		/// <summary>
		/// Строка подключения
		/// </summary>
		string ConnectionString { get; }

		/// <summary>
		/// Название строки подключения
		/// </summary>
		string ConnectionStringName { get; }

		/// <summary>
		/// Сборка с миграциями
		/// </summary>
		string Assembly { get; }

		/// <summary>
		/// Ключ миграций
		/// </summary>
		string Key { get; }

		/// <summary>
		/// Путь к файлу с миграциями
		/// </summary>
		string AssemblyFile { get; }
	}
}
