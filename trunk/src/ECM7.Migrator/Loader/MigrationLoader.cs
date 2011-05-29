namespace ECM7.Migrator.Loader
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using ECM7.Migrator.Framework;

	/// <summary>
	/// Класс для работы с миграциями в сборке
	/// </summary>
	public class MigrationLoader
	{
		/// <summary>
		/// Список загруженных типов миграций
		/// </summary>
		private readonly List<MigrationInfo> migrationsTypes;

		/// <summary>
		/// Ключ для фильтрации миграций
		/// </summary>
		public string Key { get; private set; }

		/// <summary>
		/// Инициализация
		/// </summary>
		/// <param name="key">Ключ для фильтрации миграций</param>
		/// <param name="logger">Логгер для записи сообщений трассировки</param>
		/// <param name="migrationAssemblies">Список сборок с миграциями</param>
		public MigrationLoader(string key, ILogger logger, params Assembly[] migrationAssemblies)
		{
			this.Key = key;

			this.migrationsTypes = LoadMigrations(key, migrationAssemblies);

			if (logger != null)
			{
				logger.Trace("Loaded migrations:");
				foreach (var m in migrationsTypes)
				{
					logger.Trace("{0} {1}", m.Version.ToString().PadLeft(5), StringUtils.ToHumanName(m.Type.Name));
				}
			}

			CheckForDuplicatedVersion();
		}

		/// <summary>
		/// Загрузить доступные миграции
		/// </summary>
		/// <param name="key">Ключ для фильтрации загружаемых миграций</param>
		/// <param name="assemblies">Список сборок с миграциями</param>
		private static List<MigrationInfo> LoadMigrations(string key, params Assembly[] assemblies)
		{
			List<MigrationInfo> migrationsTypes = new List<MigrationInfo>();

			foreach (Assembly assembly in assemblies)
			{
				if (assembly != null && AssemblyHasTargetKey(assembly, key))
				{
					List<MigrationInfo> collection = GetMigrationInfoList(assembly);
					migrationsTypes.AddRange(collection);
				}
			}

			return migrationsTypes;
		}

		/// <summary>
		/// Проверка, помечена ли сборка с миграциями заданным ключем
		/// </summary>
		/// <param name="assembly">Проверяемая сборка</param>
		/// <param name="key">Ключ</param>
		private static bool AssemblyHasTargetKey(Assembly assembly, string key)
		{
			MigrationAssemblyAttribute asmAttribute = 
				assembly.GetCustomAttribute<MigrationAssemblyAttribute>();
			
			string targetKey = key ?? string.Empty;

			string assemblyKey = asmAttribute == null 
				? string.Empty 
				: asmAttribute.Key ?? string.Empty;

			return targetKey == assemblyKey;
		}

		/// <summary>
		/// Returns registered migration <see cref="System.Type">types</see>.
		/// </summary>
		public List<MigrationInfo> MigrationsTypes
		{
			get { return migrationsTypes; }
		}

		/// <summary>
		/// Returns the last version of the migrations.
		/// </summary>
		public long LastVersion
		{
			get
			{
				return migrationsTypes.IsEmpty() ? 0 
					: migrationsTypes.Select(info => info.Version).Max();
			}
		}

		/// <summary>
		/// Check for duplicated version in migrations.
		/// </summary>
		/// <exception cref="CheckForDuplicatedVersion">CheckForDuplicatedVersion</exception>
		public void CheckForDuplicatedVersion()
		{
			HashSet<long> versions = new HashSet<long>();

			foreach (var info in migrationsTypes)
			{
				if (versions.Contains(info.Version))
				{
					throw new DuplicatedVersionException(info.Version);
				}

				versions.Add(info.Version);
			}
		}

		/// <summary>
		/// Collect migrations in one <c>Assembly</c>.
		/// </summary>
		/// <param name="asm">The <c>Assembly</c> to browse.</param>
		/// <returns>The migrations collection</returns>
		public static List<MigrationInfo> GetMigrationInfoList(Assembly asm)
		{
			List<MigrationInfo> migrations = new List<MigrationInfo>();

			foreach (Type type in asm.GetExportedTypes())
			{
				MigrationAttribute attribute = type.GetCustomAttribute<MigrationAttribute>();

				if (attribute != null
					&& typeof(IMigration).IsAssignableFrom(type)
					&& !attribute.Ignore)
				{
					migrations.Add(new MigrationInfo(type));
				}
			}

			migrations.Sort(new MigrationInfoComparer(true));
			return migrations;
		}

		/// <summary>
		/// Получить список номеров доступных миграций
		/// </summary>
		public List<long> GetAvailableMigrations()
		{
			migrationsTypes.Sort(new MigrationInfoComparer(true));
			return migrationsTypes.Select(mInfo => mInfo.Version).ToList();
		}

		/// <summary>
		/// Получить миграцию по номеру версии
		/// </summary>
		public IMigration GetMigration(long version, ITransformationProvider provider)
		{
			Require.IsNotNull(provider, "Не задан провайдер СУБД");

			var list = migrationsTypes.Where(info => info.Version == version).ToList();

			if (list.Count == 0)
			{
				return null;
			}

			IMigration migration = (IMigration)Activator.CreateInstance(list[0].Type);
			migration.Database = provider;
			return migration;
		}
	}
}