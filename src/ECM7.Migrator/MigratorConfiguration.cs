namespace ECM7.Migrator
{
	using System.Configuration;

	/// <summary>
	/// Настройки мигратора
	/// </summary>
	public class MigratorConfiguration : ConfigurationSection
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
		[ConfigurationProperty("assembly", IsRequired = true)]
		public string Assembly
		{
			get { return (string)base["assembly"]; }
		}
	}
}
