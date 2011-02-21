namespace ECM7.Migrator
{
	using System.Collections.Generic;
	using ECM7.Migrator.Framework;

	public abstract class BaseMigrate
	{
		protected readonly ITransformationProvider provider;
		protected ILogger logger;
		protected List<long> availableMigrations;
		protected List<long> original;
		protected long current;

		protected BaseMigrate(List<long> availableMigrations, ITransformationProvider provider, ILogger logger)
		{
			this.provider = provider;
			this.availableMigrations = availableMigrations;
			this.original = new List<long>(this.provider.AppliedMigrations.ToArray()); // clone
			this.logger = logger;
		}

		public static BaseMigrate GetInstance(List<long> availableMigrations, ITransformationProvider provider, ILogger logger)
		{
			return new MigrateAnywhere(availableMigrations, provider, logger);
		}

		public List<long> AppliedVersions
		{
			get { return original; }
		}

		public virtual long Current
		{
			get { return current; }
			protected set { current = value; }
		}

		public abstract long Previous { get; }
		public abstract long Next { get; }

		public void Iterate()
		{
			Current = Next;
		}

		public abstract bool Continue(long targetVersion);

		public abstract void Migrate(IMigration migration);

		/// <summary>
		/// Finds the next migration available to be applied.  Only returns
		/// migrations that have NOT already been applied.
		/// </summary>
		/// <returns>The migration number of the next available Migration.</returns>
		protected long NextMigration()
		{
			// Start searching at the current index
			int migrationSearch = availableMigrations.IndexOf(Current) + 1;

			// See if we can find a migration that matches the requirement
			while (migrationSearch < availableMigrations.Count
				  && provider.AppliedMigrations.Contains(availableMigrations[migrationSearch]))
				migrationSearch++;

			// did we exhaust the list?
			if (migrationSearch == availableMigrations.Count)
			{
				// we're at the last one.  Done!
				return availableMigrations[migrationSearch - 1] + 1;
			}
			// found one.
			return availableMigrations[migrationSearch];
		}

		/// <summary>
		/// Finds the previous migration that has been applied.  Only returns
		/// migrations that HAVE already been applied.
		/// </summary>
		/// <returns>The most recently applied Migration.</returns>
		protected long PreviousMigration()
		{
			// Start searching at the current index
			int migrationSearch = availableMigrations.IndexOf(Current) - 1;

			// See if we can find a migration that matches the requirement
			while (migrationSearch > -1
				  && !provider.AppliedMigrations.Contains(availableMigrations[migrationSearch]))
			{
				migrationSearch--;
			}

			// did we exhaust the list?
			if (migrationSearch < 0)
			{
				// we're at the first one.  Done!
				return 0;
			}

			// found one.
			return availableMigrations[migrationSearch];
		}
	}
}
