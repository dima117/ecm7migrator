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
		/// ���������
		/// </summary>
		private readonly ITransformationProvider provider;

		/// <summary>
		/// ��������� ���������� � ���������
		/// </summary>
		private readonly MigrationLoader migrationLoader;

		/// <summary>
		/// ������
		/// </summary>
		private ILogger logger = new Logger(false);

		/// <summary>
		/// ���� ��� ���������� ��������
		/// </summary>
		private readonly string key; 

		// todo: ��������� ������ � ����������� �� ���������� ������
		#region constructors

		/// <summary>
		/// �������������
		/// </summary>
		/// <param name="dialectTypeName">�������</param>
		/// <param name="connectionString">������ �����������</param>
		/// <param name="assemblies">������ � ����������</param>
		public Migrator(string dialectTypeName, string connectionString, params Assembly[] assemblies)
			: this(dialectTypeName, connectionString, string.Empty, null, assemblies)
		{
		}

		/// <summary>
		/// �������������
		/// </summary>
		public Migrator(string dialectTypeName, string connectionString, string key, ILogger logger, params Assembly[] assemblies)
			: this(ProviderFactory.Create(dialectTypeName, connectionString), key, logger, assemblies)
		{
		}

		/// <summary>
		/// �������������
		/// </summary>
		public Migrator(ITransformationProvider provider, string key, ILogger logger, params Assembly[] assemblies)
		{
			this.key = key;

			// TODO!!! �������� ����� ������ �������������, ��������� ��������� � ����� MIGRATE
			Require.IsNotNull(provider, "�� ����� ��������� ����");
			this.provider = provider;

			migrationLoader = new MigrationLoader(key, logger, assemblies);

			Logger = logger;
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

			// TODO:!!!!!!!!!!! ������ migrationLoader.Key
			IList<long> appliedMigrations = provider.GetAppliedMigrations(migrationLoader.Key);
			IList<long> availableMigrations = migrationLoader.MigrationsTypes
				.Select(mInfo => mInfo.Version).ToList();

			IList<long> versionsToRun = BuildMigrationPlan(targetVersion, appliedMigrations, availableMigrations);
			long startVersion = appliedMigrations.IsEmpty() ? 0 : appliedMigrations.Max();

			Logger.Started(appliedMigrations, targetVersion);

			for (int index = 0; index < versionsToRun.Count; index++)
			{
				long currentVersion = versionsToRun[index];
				long previousVersion = index == 0 ? startVersion : versionsToRun[index - 1];
				IMigration migration = this.migrationLoader.GetMigration(currentVersion, this.provider);

				Require.IsNotNull(migration, "�� ������� �������� ������ {0}", currentVersion);

				try
				{
					this.provider.BeginTransaction();
					MigrationAttribute attr = migration.GetType().GetCustomAttribute<MigrationAttribute>();

					// TODO! �������� ��� �������
					if (appliedMigrations.Contains(attr.Version))
					{
						this.logger.MigrateDown(attr.Version, migration.Name);
						migration.Down();
						this.provider.MigrationUnApplied(attr.Version, this.migrationLoader.Key);
					}
					else
					{
						this.logger.MigrateUp(attr.Version, migration.Name);
						migration.Up();
						this.provider.MigrationApplied(attr.Version, this.migrationLoader.Key);
					}

					this.provider.Commit();
				}
				catch (Exception ex)
				{
					this.Logger.Exception(currentVersion, migration.Name, ex);

					// ��� ������ ���������� ���������
					this.provider.Rollback();
					this.Logger.RollingBack(previousVersion);

					throw;
				}
			}
		}

		/// <summary>
		/// �������� ������ ������ ��� ����������
		/// </summary>
		/// <param name="target">������ ����������</param>
		/// <param name="appliedMigrations">������ ������ ����������� ��������</param>
		/// <param name="availableMigrations">������ ������ ��������� ��������</param>
		public static IList<long> BuildMigrationPlan(long target, IEnumerable<long> appliedMigrations, IEnumerable<long> availableMigrations)
		{
			long current = appliedMigrations.IsEmpty() ? 0 : appliedMigrations.Max();
			HashSet<long> set = new HashSet<long>(appliedMigrations);

			// ��������
			var list = availableMigrations.Where(x => x < current && !set.Contains(x));
			if (!list.IsEmpty())
			{
				throw new VersionException(
					"�������� ������������� ��������, ������ ������� ������ ������� ������ ��", list);
			}

			set.UnionWith(availableMigrations);

			return target >= current
			    ? set.Where(n => n > current && n <= target).OrderBy(x => x).ToList()
			    : set.Where(n => n <= current && n > target).OrderByDescending(x => x).ToList();
		}
	}
}
