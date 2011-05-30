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

		/// <summary>
		/// Менеджер версий
		/// </summary>
		private readonly BaseMigrate migrate; 

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
			// TODO!!! ОСТАВИТЬ ЗДЕСЬ ТОЛЬКО ИНИЦИАЛИЗАЦИЮ, ОСТАЛЬНОЕ ПЕРЕНЕСТИ В МЕТОД MIGRATE
			Require.IsNotNull(provider, "Не задан провайдер СУБД");
			this.provider = provider;

			Logger = logger;

			migrationLoader = new MigrationLoader(key, logger, assemblies);

			// TODO:!!!! ПЕРЕИМЕНОВАТЬ ПОЛЕ MIGRATE В VERSIONMANAGER!!!!!
			List<long> availableMigrations = migrationLoader.MigrationsTypes
				.Select(mInfo => mInfo.Version).ToList();
			migrate = new BaseMigrate(provider, key, availableMigrations, logger);
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
		public IList<long> AppliedMigrations
		{
			get
			{
				return migrate.CurrentAppliedVersions;
			}
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
			long targetVersion = databaseVersion < 0 ? migrationLoader.LastVersion : databaseVersion;

			if (migrationLoader.MigrationsTypes.Count == 0)
			{
				logger.Warn("No public classes with the Migration attribute were found.");
			}
			else
			{
				Logger.Started(migrate.CurrentAppliedVersions, targetVersion);

				while (migrate.Continue(targetVersion))
				{
					IMigration migration = migrationLoader.GetMigration(migrate.Current, provider);

					if (migration == null)
					{
						logger.Skipping(migrate.Current);
					}
					else
					{
						RunMigration(migration);
					}
				}

				Logger.Finished(migrate.OriginalAppliedVersions, targetVersion);
			}
		}

		private void RunMigration(IMigration migration)
		{
			try
			{
				this.provider.BeginTransaction();
				MigrationAttribute attr = migration.GetType().GetCustomAttribute<MigrationAttribute>();

				// TODO! поменять это условие
				if (this.migrate.CurrentAppliedVersions.Contains(attr.Version))
				{
					this.logger.MigrateDown(attr.Version, migration.Name);
					migration.Down();
					this.migrate.RemoveVersion(attr.Version);
				}
				else
				{
					this.logger.MigrateUp(attr.Version, migration.Name);
					migration.Up();
					this.migrate.AddVersion(attr.Version);
				}

				this.provider.Commit();
			}
			catch (Exception ex)
			{
				this.Logger.Exception(this.migrate.Current, migration.Name, ex);

				// Oho! error! We rollback changes.
				this.Logger.RollingBack(this.migrate.Previous);
				this.provider.Rollback();

				throw;
			}
		}
	}
}
