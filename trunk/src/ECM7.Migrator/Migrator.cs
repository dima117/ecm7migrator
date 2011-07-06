namespace ECM7.Migrator
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Reflection;
	using Framework;
	using Loader;

	using log4net;

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
		private readonly MigrationAssembly migrationAssembly;

		/// <summary>
		/// Логгер
		/// </summary>
		private readonly ILog logger;

		/// <summary>
		/// Ключ для фильтрации миграций
		/// </summary>
		private string Key
		{
			get { return migrationAssembly.Key; }
		}

		#region constructors

		/// <summary>
		/// Инициализация
		/// </summary>
		public Migrator(string dialectTypeName, string connectionString, Assembly asm, ILog logger)
			: this(ProviderFactory.Create(dialectTypeName, connectionString, logger), asm, logger)
		{
		}

		/// <summary>
		/// Инициализация
		/// </summary>
		public Migrator(ITransformationProvider provider, Assembly asm, ILog logger)
		{
			Require.IsNotNull(logger, "Не задан объект, реализующий интерфейс ILog");
			this.logger = logger;

			Require.IsNotNull(provider, "Не задан провайдер СУБД");
			this.provider = provider;

			Require.IsNotNull(asm, "Не задана сборка с миграциями");
			this.migrationAssembly = new MigrationAssembly(asm, logger);
		}

		#endregion

		/// <summary>
		/// Returns registered migration <see cref="System.Type">types</see>.
		/// </summary>
		public ReadOnlyCollection<MigrationInfo> AvailableMigrations
		{
			get { return this.migrationAssembly.MigrationsTypes; }
		}

		/// <summary>
		/// Returns the current migrations applied to the database.
		/// </summary>
		public IList<long> GetAppliedMigrations()
		{
			return provider.GetAppliedMigrations(Key);
		}

		/// <summary>
		/// Get or set the event logger.
		/// </summary>
		public ILog Logger
		{
			get { return logger; }
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

			long targetVersion = databaseVersion < 0 ? this.migrationAssembly.LastVersion : databaseVersion;

			IList<long> appliedMigrations = provider.GetAppliedMigrations(Key);
			IList<long> availableMigrations = this.migrationAssembly.MigrationsTypes
				.Select(mInfo => mInfo.Version).ToList();

			MigrationPlan plan = BuildMigrationPlan(targetVersion, appliedMigrations, availableMigrations);

			long currentDatabaseVersion = plan.StartVersion;
			Logger.Started(currentDatabaseVersion, targetVersion);

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
			IMigration migration = this.migrationAssembly.InstantiateMigration(targetVersion, this.provider);
			Require.IsNotNull(migration, "Не найдена миграция версии {0}", targetVersion);

			try
			{
				this.provider.BeginTransaction();

				if (targetVersion <= currentDatabaseVersion)
				{
					this.logger.MigrateDown(targetVersion, migration.Name);
					migration.Down();
					this.provider.MigrationUnApplied(targetVersion, Key);
				}
				else
				{
					this.logger.MigrateUp(targetVersion, migration.Name);
					migration.Up();
					this.provider.MigrationApplied(targetVersion, Key);
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
