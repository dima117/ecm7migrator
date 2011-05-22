namespace ECM7.Migrator.Configuration
{
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Linq;
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
		public static Migrator InitByConfigFile()
		{
			return InitByConfigFile("migrator");
		}

		/// <summary>
		/// Создание мигратора и его инициализация из конфига
		/// </summary>
		public static Migrator InitByConfigFile(string configSectionName)
		{
			Require.IsNotNullOrEmpty(configSectionName, true, "Не задана секция конфигурационногог файла");
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
			Require.IsNotNullOrEmpty(config.Dialect, "Не задан используемый диалект");

			IEnumerable<Assembly> assemblies = GetAssemblies(config);

			string connectionString = GetConnectionString(config);

			return new Migrator(config.Dialect.Trim(), connectionString, config.Key, assemblies.ToArray());
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
		/// Загрузка сборок с миграциями
		/// </summary>
		/// <param name="config">Конфигурация мигратора</param>
		private static IEnumerable<Assembly> GetAssemblies(IMigratorConfiguration config)
		{
			IList<Assembly> assemblies = new List<Assembly>();
			if (!config.Assembly.IsNullOrEmpty(true))
			{
				assemblies.Add(Assembly.Load(config.Assembly));
			}

			if (!config.AssemblyFile.IsNullOrEmpty(true))
			{
				assemblies.Add(Assembly.LoadFrom(config.AssemblyFile));
			}

			return assemblies;
		}

		#endregion
	}
}
