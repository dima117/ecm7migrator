namespace ECM7.Migrator.Configuration
{
	/// <summary>
	/// Настрйоки мигратора
	/// </summary>
	public class MigratorConfiguration : IMigratorConfiguration
	{
		#region Implementation of IMigratorConfiguration

		/// <summary>
		/// Диалект
		/// </summary>
		public string Dialect { get;  set; }

		/// <summary>
		/// Строка подключения
		/// </summary>
		public string ConnectionString { get;  set; }

		/// <summary>
		/// Название строки подключения
		/// </summary>
		public string ConnectionStringName { get;  set; }

		/// <summary>
		/// Сборка с миграциями
		/// </summary>
		public string Assembly { get;  set; }

		/// <summary>
		/// Путь к файлу с миграциями
		/// </summary>
		public string AssemblyFile { get; set; }

		/// <summary>
		/// Ключ миграций
		/// </summary>
		public string Key { get; set; }

		#endregion
	}
}
