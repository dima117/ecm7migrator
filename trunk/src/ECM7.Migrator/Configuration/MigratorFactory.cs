namespace ECM7.Migrator.Configuration
{
	using System;
	using System.Configuration;
	using System.Reflection;

	using log4net;

	/// <summary>
	/// Инициализация мигратора
	/// </summary>
	public static class MigratorFactory
	{
		#region config file

		/// <summary>
		/// Создание мигратора и его инициализация из конфига
		/// </summary>
		public static Migrator InitByConfigFile(ILog logger)
		{
			return InitByConfigFile("migrator", logger);
		}

		/// <summary>
		/// Создание мигратора и его инициализация из конфига
		/// </summary>
		public static Migrator InitByConfigFile(string configSectionName, ILog logger)
		{
			Require.IsNotNullOrEmpty(configSectionName, true, "Не задана секция конфигурационногог файла");
			var config = ConfigurationManager.GetSection(configSectionName) as MigratorConfigurationSection;
			return CreateMigrator(config, logger);
		}

		#endregion

		#region создание и инициализация мигратора

		/// <summary>
		/// Создание экземпляра мигратора, инициализированного заданными настройками
		/// </summary>
		/// <param name="config">Конфигурация мигратора</param>
		/// <param name="logger">Логгер</param>
		public static Migrator CreateMigrator(IMigratorConfiguration config, ILog logger)
		{
			Require.IsNotNull(logger, "Не задан логгер");
			Require.IsNotNull(config, "Конфигурация не задана");
			Require.IsNotNullOrEmpty(config.Dialect, "Не задан используемый диалект");

			Assembly assembly = GetAssembly(config);

			string connectionString = GetConnectionString(config);

			return new Migrator(config.Dialect.Trim(), connectionString, assembly, logger);
		}

		/// <summary>
		/// Строка подключения
		/// </summary>
		/// <param name="config">Конфигурация мигратора</param>
		private static string GetConnectionString(IMigratorConfiguration config)
		{
			string connectionString = null;

			if (!config.ConnectionString.IsNullOrEmpty(true))
			{
				connectionString = config.ConnectionString.Trim();
			}
			else if (!config.ConnectionStringName.IsNullOrEmpty(true))
			{
				string cstringName = config.ConnectionStringName.Trim();
				connectionString = ConfigurationManager.ConnectionStrings[cstringName].ConnectionString;
			}

			Require.IsNotNullOrEmpty(connectionString, true, "Не задана строка подключения");

			return connectionString;
		}

		/// <summary>
		/// Загрузка сборки с миграциями
		/// </summary>
		/// <param name="config">Конфигурация мигратора</param>
		private static Assembly GetAssembly(IMigratorConfiguration config)
		{
			Assembly assembly = null;

			if (!config.Assembly.IsNullOrEmpty(true))
			{
				assembly = Assembly.Load(config.Assembly);
			}
			else
			{
				if (!config.AssemblyFile.IsNullOrEmpty(true))
				{
					assembly = Assembly.LoadFrom(config.AssemblyFile);
				}
			}

			Require.IsNotNull(assembly, "Не задана сборка, содержащая миграции");
			return assembly;
		}

		#endregion
	}
}
