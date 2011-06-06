namespace ECM7.Migrator
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using Framework;
	using Framework.Loggers;
	using Loader;

	/// <summary>
	/// Migrations mediator.
	/// </summary>
	public class Migrator
	{
		/// <summary>
		/// Провайдер
		/// </summary>
		private readonly ITransformationProvider provider;

		/// <summary>
		/// Загрузчик информации о миграциях
		/// </summary>
		private readonly MigrationLoader migrationLoader;

		/// <summary>
		/// Логгер
		/// </summary>
		private readonly ILogger logger = new Logger(false);

		/// <summary>
		/// Ключ для фильтрации миграций
		/// </summary>
		private readonly string key; 

		// todo: проверить работу с мигрэйшнами из нескольких сборок
		#region constructors

		/// <summary>
		/// Инициализация
		/// </summary>
		/// <param name="dialectTypeName">Диалект</param>
		/// <param name="connectionString">Строка подключения</param>
		/// <param name="assemblies">Сборки с миграциями</param>
		public Migrator(string dialectTypeName, string connectionString, params Assembly[] assemblies)
			: this(dialectTypeName, connectionString, string.Empty, null, assemblies)
		{
		}

		/// <summary>
		/// Инициализация
		/// </summary>
		public Migrator(string dialectTypeName, string connectionString, string key, ILogger logger, params Assembly[] assemblies)
			: this(ProviderFactory.Create(dialectTypeName, connectionString), key, logger, assemblies)
		{
		}

		/// <summary>
		/// Инициализация
		/// </summary>
		public Migrator(ITransformationProvider provider, string key, ILogger logger, params Assembly[] assemblies)
		{
			this.key = key ?? string.Empty;

			// TODO!!! ОСТАВИТЬ ЗДЕСЬ ТОЛЬКО ИНИЦИАЛИЗАЦИЮ, ОСТАЛЬНОЕ ПЕРЕНЕСТИ В МЕТОД MIGRATE
			Require.IsNotNull(provider, "Не задан провайдер СУБД");
			this.provider = provider;

			migrationLoader = new MigrationLoader(key, logger, assemblies);

			this.logger = logger;
		}

		#endregion

		/// <summary>
		/// Returns registered migration <see cref="System.Type">types</see>.
		/// </summary>
		public List<MigrationInfo> AvailableMigrations
		{
			get { return migrationLoader.MigrationsTypes; }
		}

		/// <summary>
		/// Returns the current migrations applied to the database.
		/// </summary>
		public IList<long> GetAppliedMigrations()
		{
			return provider.GetAppliedMigrations(key);
		}

		/// <summary>
		/// Get or set the event logger.
		/// </summary>
		public ILogger Logger
		{
			get
			{
				return logger;
			}
		}

		/// <summary>
		/// Migrate the database to a specific version.
		/// Runs all migration between the actual version and the
		/// specified version.
		/// If <c>version</c> is greater then the current version,
		/// the <c>Up()</c> method will be invoked.
		/// If <c>version</c> lower then the current version,
		/// the <c>Down()</c> method of previous migration will be invoked.
		/// If <c>dryrun</c> is set, don't write any changes to the database.
		/// </summary>
		/// <param name="databaseVersion">The version that must became the current one</param>
		public void Migrate(long databaseVersion = -1)
		{
			long targetVersion = databaseVersion < 0 ? migrationLoader.LastVersion : databaseVersion;

			IList<long> appliedMigrations = provider.GetAppliedMigrations(key);
			IList<long> availableMigrations = migrationLoader.MigrationsTypes
				.Select(mInfo => mInfo.Version).ToList();

			MigrationPlan plan = BuildMigrationPlan(targetVersion, appliedMigrations, availableMigrations);

			Logger.Started(appliedMigrations, targetVersion);

			long currentDatabaseVersion = plan.StartVersion;

			foreach (long currentExecutedVersion in plan)
			{
				ExecuteMigration(currentExecutedVersion, currentDatabaseVersion);

				currentDatabaseVersion = currentExecutedVersion;
			}
		}

		/// <summary>
		/// Выполнение миграции
		/// </summary>
		/// <param name="targetVersion">Версия выполняемой миграции</param>
		/// <param name="currentDatabaseVersion">Текущая версия БД</param>
		public void ExecuteMigration(long targetVersion, long currentDatabaseVersion)
		{
			IMigration migration = this.migrationLoader.GetMigration(targetVersion, this.provider);
			Require.IsNotNull(migration, "Не найдена миграция версии {0}", targetVersion);

			try
			{
				this.provider.BeginTransaction();

				if (targetVersion <= currentDatabaseVersion)
				{
					this.logger.MigrateDown(targetVersion, migration.Name);
					migration.Down();
					this.provider.MigrationUnApplied(targetVersion, this.key);
				}
				else
				{
					this.logger.MigrateUp(targetVersion, migration.Name);
					migration.Up();
					this.provider.MigrationApplied(targetVersion, this.key);
				}

				this.provider.Commit();
			}
			catch (Exception ex)
			{
				this.Logger.Exception(targetVersion, migration.Name, ex);

				// при ошибке откатываем изменения
				this.provider.Rollback();
				this.Logger.RollingBack(currentDatabaseVersion);
				throw;
			}
		}

		/// <summary>
		/// Получить список версий для выполнения
		/// </summary>
		/// <param name="target">Версия назначения</param>
		/// <param name="appliedMigrations">Список версий выполненных миграций</param>
		/// <param name="availableMigrations">Список версий доступных миграций</param>
		public static MigrationPlan BuildMigrationPlan(long target, IEnumerable<long> appliedMigrations, IEnumerable<long> availableMigrations)
		{
			long startVersion = appliedMigrations.IsEmpty() ? 0 : appliedMigrations.Max();
			HashSet<long> set = new HashSet<long>(appliedMigrations);

			// проверки
			var list = availableMigrations.Where(x => x < startVersion && !set.Contains(x));
			if (!list.IsEmpty())
			{
				throw new VersionException(
					"Доступны невыполненные миграции, версия которых меньше текущей версии БД", list);
			}

			set.UnionWith(availableMigrations);

			var versions = target < startVersion
			               	? set.Where(n => n <= startVersion && n > target).OrderByDescending(x => x).ToList()
			               	: set.Where(n => n > startVersion && n <= target).OrderBy(x => x).ToList();

			return new MigrationPlan(versions, startVersion);
		}
	}
}
