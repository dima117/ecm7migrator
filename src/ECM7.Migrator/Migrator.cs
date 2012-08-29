using ECM7.Migrator.Exceptions;

namespace ECM7.Migrator
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Data;
	using System.Linq;
	using System.Reflection;

	using ECM7.Migrator.Framework.Logging;
	using ECM7.Migrator.Providers;

	using Framework;
	using Loader;

	/// <summary>
	/// Migrations mediator.
	/// </summary>
	public class Migrator : IDisposable
	{
		/// <summary>
		/// Провайдер
		/// </summary>
		private readonly ITransformationProvider provider;

		public ITransformationProvider Provider
		{
			get { return provider; }
		}

		/// <summary>
		/// Загрузчик информации о миграциях
		/// </summary>
		private readonly MigrationAssembly migrationAssembly;

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
		public Migrator(string providerTypeName, IDbConnection connection, Assembly asm, int? commandTimeout = null)
			: this(ProviderFactory.Create(providerTypeName, connection, commandTimeout), asm)
		{
		}

		/// <summary>
		/// Инициализация
		/// </summary>
		public Migrator(string providerTypeName, string connectionString, Assembly asm, int? commandTimeout = null)
			: this(ProviderFactory.Create(providerTypeName, connectionString, commandTimeout), asm)
		{
		}

		/// <summary>
		/// Инициализация
		/// </summary>
		public Migrator(ITransformationProvider provider, Assembly asm)
		{
			Require.IsNotNull(provider, "Не задан провайдер трансформации");
			this.provider = provider;

			Require.IsNotNull(asm, "Не задана сборка с миграциями");
			migrationAssembly = new MigrationAssembly(asm);
		}

		#endregion

		/// <summary>
		/// Returns registered migration <see cref="System.Type">types</see>.
		/// </summary>
		public ReadOnlyCollection<MigrationInfo> AvailableMigrations
		{
			get { return migrationAssembly.MigrationsTypes; }
		}

		/// <summary>
		/// Returns the current migrations applied to the database.
		/// </summary>
		public IList<long> GetAppliedMigrations()
		{
			return provider.GetAppliedMigrations(Key);
		}

		/// <summary>
		/// Migrate the database to a specific version.
		/// Runs all migration between the actual version and the
		/// specified version.
		/// If <c>version</c> is greater then the current version,
		/// the <c>Apply()</c> method will be invoked.
		/// If <c>version</c> lower then the current version,
		/// the <c>Revert()</c> method of previous migration will be invoked.
		/// If <c>dryrun</c> is set, don't write any changes to the database.
		/// </summary>
		/// <param name="databaseVersion">The version that must became the current one</param>
		public void Migrate(long databaseVersion = -1)
		{

			long targetVersion = databaseVersion < 0 ? migrationAssembly.LastVersion : databaseVersion;

			IList<long> appliedMigrations = provider.GetAppliedMigrations(Key);
			IList<long> availableMigrations = migrationAssembly.MigrationsTypes
				.Select(mInfo => mInfo.Version).ToList();

			MigrationPlan plan = BuildMigrationPlan(targetVersion, appliedMigrations, availableMigrations);

			long currentDatabaseVersion = plan.StartVersion;
			MigratorLogManager.Log.Started(currentDatabaseVersion, targetVersion);

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
			var migrationInfo = migrationAssembly.GetMigrationInfo(targetVersion);

			IMigration migration = migrationAssembly.InstantiateMigration(migrationInfo, provider);
			
			try
			{
				if (!migrationInfo.WithoutTransaction)
				{
					provider.BeginTransaction();
				}

				if (targetVersion <= currentDatabaseVersion)
				{
					MigratorLogManager.Log.MigrateDown(targetVersion, migration.Name);
					migration.Revert();
					provider.MigrationUnApplied(targetVersion, Key);
				}
				else
				{
					MigratorLogManager.Log.MigrateUp(targetVersion, migration.Name);
					migration.Apply();
					provider.MigrationApplied(targetVersion, Key);
				}

				if (!migrationInfo.WithoutTransaction)
				{
					provider.Commit();
				}
			}
			catch (Exception ex)
			{
				MigratorLogManager.Log.Exception(targetVersion, migration.Name, ex);

				if (!migrationInfo.WithoutTransaction)
				{
					// при ошибке откатываем изменения
					provider.Rollback();
					MigratorLogManager.Log.RollingBack(currentDatabaseVersion);
				}

				throw;
			}
		}

		/// <summary>
		/// Получить список версий для выполнения
		/// </summary>
		/// <param name="target">Версия назначения</param>
		/// <param name="appliedMigrations">Список версий выполненных миграций</param>
		/// <param name="availableMigrations">Список версий доступных миграций</param>
		public static MigrationPlan BuildMigrationPlan(long target, IList<long> appliedMigrations, IList<long> availableMigrations)
		{
			long startVersion = appliedMigrations.IsEmpty() ? 0 : appliedMigrations.Max();
			HashSet<long> set = new HashSet<long>(appliedMigrations);

			// проверки
			var list = availableMigrations.Where(x => x < startVersion && !set.Contains(x)).ToList();
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

		#region Implementation of IDisposable

		public void Dispose()
		{
			provider.Dispose();
		}

		#endregion
	}
}
