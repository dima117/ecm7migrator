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

namespace ECM7.Migrator
{
	/// <summary>
	/// Migrations mediator.
	/// </summary>
	public class Migrator
	{
		private readonly ITransformationProvider provider;

		private readonly MigrationLoader migrationLoader;

		private ILogger logger = new Logger(false);
		protected bool dryrun;

		public string[] Args { get; set; }

		//todo: проверить работу с мигрэйшнами из нескольких сборок

		public Migrator(string dialectTypeName, string connectionString, params Assembly[] assemblies)
			: this(dialectTypeName, connectionString, false, assemblies)
		{
		}

		public Migrator(string dialectTypeName, string connectionString, bool trace, params Assembly[] assemblies)
			: this(ProviderFactory.Create(dialectTypeName, connectionString), trace, assemblies)
		{
		}

		public Migrator(string dialectTypeName, string connectionString, bool trace, ILogger logger, params Assembly[] assemblies)
			: this(ProviderFactory.Create(dialectTypeName, connectionString), trace, logger, assemblies)
		{
		}

		public Migrator(ITransformationProvider provider, bool trace, params Assembly[] assemblies)
			: this(provider, trace, new Logger(trace, new ConsoleWriter()), assemblies)
		{
		}

		public Migrator(ITransformationProvider provider, bool trace, ILogger logger, params Assembly[] assemblies)
		{
			this.provider = provider;
			Logger = logger;

			migrationLoader = new MigrationLoader(provider, trace, assemblies);
			migrationLoader.CheckForDuplicatedVersion();
		}


		/// <summary>
		/// Returns registered migration <see cref="System.Type">types</see>.
		/// </summary>
		public List<Type> MigrationsTypes
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
			get { return logger; }
			set
			{
				logger = value;
				provider.Logger = value;
			}
		}

		public virtual bool DryRun
		{
			get { return dryrun; }
			set { dryrun = value; }
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
		/// <param name="version">The version that must became the current one</param>
		public void MigrateTo(long version)
		{

			if (migrationLoader.MigrationsTypes.Count == 0)
			{
				logger.Warn("No public classes with the Migration attribute were found.");
				return;
			}

			bool firstRun = true;
			BaseMigrate migrate = BaseMigrate.GetInstance(migrationLoader.GetAvailableMigrations(), provider, logger);
			migrate.DryRun = DryRun;
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
	}
}
