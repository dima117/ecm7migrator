namespace ECM7.Migrator.Configuration
{
	using System.Configuration;

	/// <summary>
	/// Настройки мигратора
	/// </summary>
	public class MigratorConfigurationSection : ConfigurationSection, IMigratorConfiguration
	{
		/// <summary>
		/// Диалект
		/// </summary>
		[ConfigurationProperty("dialect", IsRequired = true)]
		public string Dialect
		{
			get { return (string)base["dialect"]; }
		}

		/// <summary>
		/// Строка подключения
		/// </summary>
		[ConfigurationProperty("connectionString")]
		public string ConnectionString
		{
			get { return (string)base["connectionString"]; }
		}

		/// <summary>
		/// Название строки подключения
		/// </summary>
		[ConfigurationProperty("connectionStringName")]
		public string ConnectionStringName
		{
			get { return (string)base["connectionStringName"]; }
		}

		/// <summary>
		/// Сборка с миграциями
		/// </summary>
		[ConfigurationProperty("assembly")]
		public string Assembly
		{
			get { return (string)base["assembly"]; }
		}

		/// <summary>
		/// Путь к файлу с миграциями
		/// </summary>
		[ConfigurationProperty("assemblyFile")]
		public string AssemblyFile
		{
			get { return (string)base["assemblyFile"]; }
		}

		/// <summary>
		/// Ключ миграций
		/// </summary>
		[ConfigurationProperty("key")]
		public string Key
		{
			get { return (string)base["key"]; }
		}
	}
}
