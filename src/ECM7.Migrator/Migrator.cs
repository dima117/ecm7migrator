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
		private ILogger logger = new Logger(false);

		// todo: проверить работу с мигрэйшнами из нескольких сборок
		#region constructors

		/// <summary>
		/// Инициализация
		/// </summary>
		/// <param name="dialectTypeName">Диалект</param>
		/// <param name="connectionString">Строка подключения</param>
		/// <param name="assemblies">Сборки с миграциями</param>
		public Migrator(string dialectTypeName, string connectionString, params Assembly[] assemblies)
			: this(dialectTypeName, connectionString, false, assemblies)
		{
		}

		/// <summary>
		/// Инициализация
		/// </summary>
		public Migrator(string dialectTypeName, string connectionString, bool trace, params Assembly[] assemblies)
			: this(ProviderFactory.Create(dialectTypeName, connectionString), trace, assemblies)
		{
		}

		/// <summary>
		/// Инициализация
		/// </summary>
		public Migrator(string dialectTypeName, string connectionString, bool trace, ILogger logger, params Assembly[] assemblies)
			: this(ProviderFactory.Create(dialectTypeName, connectionString), trace, logger, assemblies)
		{
		}

		/// <summary>
		/// Инициализация
		/// </summary>
		public Migrator(ITransformationProvider provider, bool trace, params Assembly[] assemblies)
			: this(provider, trace, new Logger(trace, new ConsoleWriter()), assemblies)
		{
		}

		/// <summary>
		/// Инициализация
		/// </summary>
		public Migrator(ITransformationProvider provider, bool trace, ILogger logger, params Assembly[] assemblies)
		{
			this.provider = provider;
			Logger = logger;

			migrationLoader = new MigrationLoader(provider, trace, assemblies);
			migrationLoader.CheckForDuplicatedVersion();
		}

		#endregion

		/// <summary>
		/// Returns registered migration <see cref="System.Type">types</see>.
		/// </summary>
		public List<MigrationInfo> MigrationsTypes
		{
			get { return migrationLoader.MigrationsTypes; }
		}

		/// <summary>
		/// Returns the current migrations applied to the database.
		/// </summary>
		public List<long> AppliedMigrations
		{
			get { return provider.AppliedMigrations; }
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

			set
			{
				logger = value;
				provider.Logger = value;
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
			long version = databaseVersion == -1 ? migrationLoader.LastVersion : databaseVersion;

			Require.That(version > 0, "Версия БД должна быть больше 0 или иметь значение -1 (соответствует последней доступной версии)");

			if (migrationLoader.MigrationsTypes.Count == 0)
			{
				logger.Warn("No public classes with the Migration attribute were found.");
				return;
			}

			bool firstRun = true;
			List<long> availableMigrations = migrationLoader.GetAvailableMigrations();
			BaseMigrate migrate = BaseMigrate.GetInstance(availableMigrations, provider, logger);

			// проверка корректности номеров миграций
			CheckMigrationNumbers(availableMigrations, migrate.AppliedVersions);

			Logger.Started(migrate.AppliedVersions, version);

			while (migrate.Continue(version))
			{
				IMigration migration = migrationLoader.GetMigration(migrate.Current);
				if (null == migration)
				{
					logger.Skipping(migrate.Current);
					migrate.Iterate();
					continue;
				}

				try
				{
					if (firstRun)
					{
						migration.InitializeOnce();
						firstRun = false;
					}

					migrate.Migrate(migration);
				}
				catch (Exception ex)
				{
					Logger.Exception(migrate.Current, migration.Name, ex);

					// Oho! error! We rollback changes.
					Logger.RollingBack(migrate.Previous);
					provider.Rollback();

					throw;
				}

				migrate.Iterate();
			}

			Logger.Finished(migrate.AppliedVersions, version);
		}

		/// <summary>
		/// Проверка, что выполнены все доступные миграции с номерами меньше текущей
		/// </summary>
		/// <param name="availableMigrations">Доступные миграции</param>
		/// <param name="appliedVersions">Выполненные миграции</param>
		public static void CheckMigrationNumbers(IList<long> availableMigrations, IList<long> appliedVersions)
		{
			long current = appliedVersions.IsEmpty() ? 0 : appliedVersions.Max();
			var skippedMigrations = availableMigrations.Where(m => m <= current && !appliedVersions.Contains(m));

			string errorMessage = "The current database version is {0}, the migration {1} are available but not used"
				.FormatWith(current, skippedMigrations.ToCommaSeparatedString());
			Require.AreEqual(skippedMigrations.Count(), 0, errorMessage);
		}
	}
}
