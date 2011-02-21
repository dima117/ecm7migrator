#region License

//The contents of this file are subject to the Mozilla Public License
//Version 1.1 (the "License"); you may not use this file except in
//compliance with the License. You may obtain a copy of the License at
//http://www.mozilla.org/MPL/
//Software distributed under the License is distributed on an "AS IS"
//basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//License for the specific language governing rights and limitations
//under the License.

#endregion

using System;
using System.Collections.Generic;
using System.Reflection;
using ECM7.Migrator.Framework;
using ECM7.Migrator.Framework.Loggers;
using ECM7.Migrator.Loader;

namespace ECM7.Migrator
{
	using System.Configuration;
	using System.Linq;

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

		public string[] Args { get; set; }

		// todo: ��������� ������ � ����������� �� ���������� ������
		#region constructors

		/// <summary>
		/// �������� ��������� � ��� ������������� �� �������
		/// </summary>
		public static Migrator InitByConfig()
		{
			return InitByConfig("migrator");
		}

		/// <summary>
		/// �������� ��������� � ��� ������������� �� �������
		/// </summary>
		public static Migrator InitByConfig(string configSectionName)
		{
			Require.IsNotNullOrEmpty(configSectionName, true, "�� ������ ������ ������������������ �����");
			var config = ConfigurationManager.GetSection(configSectionName) as MigratorConfiguration;
			Require.IsNotNull(config, "������������ �� ������");
			Require.IsNotNullOrEmpty(config.Assembly, "�� ������ ������, ���������� ��������");
			Require.IsNotNullOrEmpty(config.Dialect, "�� ����� ������������ �������");

			// ������ � ����������
			Assembly assembly = Assembly.Load(config.Assembly);

			// ������ �����������
			string connectionString = null;

			if (!config.ConnectionString.IsNullOrEmpty(true))
			{
				config.ConnectionString.Trim();
			}
			else if (!config.ConnectionStringName.IsNullOrEmpty(true))
			{
				string cstringName = config.ConnectionStringName.Trim();
				connectionString = ConfigurationManager.ConnectionStrings[cstringName].ConnectionString;
			}
			else
			{
				Require.Throw("�� ������ ������ �����������");
			}

			return new Migrator(config.Dialect.Trim(), connectionString, assembly);
		}

		/// <summary>
		/// �������������
		/// </summary>
		/// <param name="dialectTypeName">�������</param>
		/// <param name="connectionString">������ �����������</param>
		/// <param name="assemblies">������ � ����������</param>
		public Migrator(string dialectTypeName, string connectionString, params Assembly[] assemblies)
			: this(dialectTypeName, connectionString, false, assemblies)
		{
		}

		/// <summary>
		/// �������������
		/// </summary>
		public Migrator(string dialectTypeName, string connectionString, bool trace, params Assembly[] assemblies)
			: this(ProviderFactory.Create(dialectTypeName, connectionString), trace, assemblies)
		{
		}

		/// <summary>
		/// �������������
		/// </summary>
		public Migrator(string dialectTypeName, string connectionString, bool trace, ILogger logger, params Assembly[] assemblies)
			: this(ProviderFactory.Create(dialectTypeName, connectionString), trace, logger, assemblies)
		{
		}

		/// <summary>
		/// �������������
		/// </summary>
		public Migrator(ITransformationProvider provider, bool trace, params Assembly[] assemblies)
			: this(provider, trace, new Logger(trace, new ConsoleWriter()), assemblies)
		{
		}

		/// <summary>
		/// �������������
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
		/// Run all migrations up to the latest.  Make no changes to database if
		/// dryrun is true.
		/// </summary>
		public void MigrateToLastVersion()
		{
			MigrateTo(migrationLoader.LastVersion);
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
		/// �������: ������������ SQL, �� �������� ���
		/// </summary>
		public virtual bool DryRun { get; set; }

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
		/// <param name="version">The version that must became the current one</param>
		public void MigrateTo(long version)
		{
			if (migrationLoader.MigrationsTypes.Count == 0)
			{
				logger.Warn("No public classes with the Migration attribute were found.");
				return;
			}

			bool firstRun = true;
			List<long> availableMigrations = migrationLoader.GetAvailableMigrations();
			BaseMigrate migrate = BaseMigrate.GetInstance(availableMigrations, provider, logger);
			migrate.DryRun = DryRun;

			// �������� ������������ ������� ��������
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
						migration.InitializeOnce(Args);
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
		/// ��������, ��� ��������� ��� ��������� �������� � �������� ������ �������
		/// </summary>
		/// <param name="availableMigrations">��������� ��������</param>
		/// <param name="appliedVersions">����������� ��������</param>
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
