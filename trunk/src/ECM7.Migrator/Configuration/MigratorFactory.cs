using ECM7.Migrator.Utils;

namespace ECM7.Migrator.Configuration
{
	using System;
	using System.Configuration;
	using System.Reflection;

	/// <summary>
	/// Инициализация мигратора
	/// </summary>
	public static class MigratorFactory
	{
		#region config file

		/// <summary>
		/// Создание мигратора и его инициализация из конфига
		/// </summary>
		public static Migrator InitByConfigFile(string configSectionName = "migrator")
		{
			Require.IsNotNullOrEmpty(configSectionName, true, "Не задана секция конфигурационного файла");
			var config = ConfigurationManager.GetSection(configSectionName) as MigratorConfigurationSection;
			return CreateMigrator(config);
		}

		#endregion

		#region создание и инициализация мигратора

		/// <summary>
		/// Создание экземпляра мигратора, инициализированного заданными настройками
		/// </summary>
		/// <param name="config">Конфигурация мигратора</param>
		public static Migrator CreateMigrator(IMigratorConfiguration config)
		{
			Require.IsNotNull(config, "Конфигурация не задана");
			Require.IsNotNullOrEmpty(config.Provider, "Не задан используемый тип провайдера");

			Assembly assembly = GetAssembly(config);

			string connectionString = GetConnectionString(config);

			return new Migrator(config.Provider.Trim(), connectionString, assembly, config.CommandTimeout);
		}

		/// <summary>
		/// Строка подключения
		/// </summary>
		/// <param name="config">Конфигурация мигратора</param>
		private static string GetConnectionString(IMigratorConfiguration config)
		{
			string connectionString = null;

			if (!string.IsNullOrWhiteSpace(config.ConnectionString))
			{
				connectionString = config.ConnectionString.Trim();
			}
			else if (!string.IsNullOrWhiteSpace(config.ConnectionStringName))
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

			if (!string.IsNullOrWhiteSpace(config.Assembly))
			{
				assembly = Assembly.Load(config.Assembly);
			}
			else
			{
				if (!string.IsNullOrWhiteSpace(config.AssemblyFile))
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
