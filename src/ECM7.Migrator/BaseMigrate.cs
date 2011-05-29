namespace ECM7.Migrator
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using ECM7.Migrator.Framework;

	public class BaseMigrate
	{
		// TODO!!!!!!!! ÑÄÅËÀÒÜ ÑÏÈÑÎÊ ÂÛÏÎËÍÅÍÍÛÕ ÂÅĞÑÈÉ, ÊİØÈĞÎÂÀÒÜ ÅÃÎ, ÌÅÍßÒÜ ÅÃÎ ÏĞÈ ÈÇÌÅÍÅÍÈÈ ÁÄ È ÎÏĞÅÄÅËßÒÜ ÏÎ ÍÅÌÓ ÍÎÌÅĞ ÒÅÊÓÙÅÉ ÂÅĞÑÈÈ!!!!!!!

		// TODO!!!! ÇÀÃĞÓÆÀÒÜ ÌÈÃĞÀÖÈÈ ÑÎ ÂÑÅÌÈ ÊËŞ×ÀÌÈ È ÄÀÂÀÒÜ ÂÎÇÌÎÆÍÎÑÒÜ ÔÈËÜÒĞÎÂÀÒÜ ÏÎ ÊËŞ×Ó
		protected readonly ITransformationProvider provider;
		protected ILogger logger;
		protected List<long> availableMigrations;
		protected List<long> original;
		protected long current;

		private bool goForward;

		protected BaseMigrate(List<long> availableMigrations, ITransformationProvider provider, ILogger logger)
		{
			this.provider = provider;
			this.availableMigrations = availableMigrations;
			this.original = new List<long>(this.provider.AppliedMigrations.ToArray()); // clone
			this.logger = logger;

			current = 0;
			if (provider.AppliedMigrations.Count > 0)
			{
				current = provider.AppliedMigrations[provider.AppliedMigrations.Count - 1];
			}

			goForward = false;
		}

		/// <summary>
		/// Ïğîâåğêà, ÷òî âûïîëíåíû âñå äîñòóïíûå ìèãğàöèè ñ íîìåğàìè ìåíüøå òåêóùåé
		/// </summary>
		/// <param name="availableMigrations">Äîñòóïíûå ìèãğàöèè</param>
		public void CheckMigrationNumbers(IList<long> availableMigrations)
		{
			
			long current = appliedVersions.IsEmpty() ? 0 : appliedVersions.Max();
			var skippedMigrations = availableMigrations.Where(m => m <= current && !appliedVersions.Contains(m));

			string errorMessage = "The current database version is {0}, the migration {1} are available but not used"
				.FormatWith(current, skippedMigrations.ToCommaSeparatedString());
			Require.AreEqual(skippedMigrations.Count(), 0, errorMessage);
		}

		public static BaseMigrate GetInstance(List<long> availableMigrations, ITransformationProvider provider, ILogger logger)
		{
			return new BaseMigrate(availableMigrations, provider, logger);
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

		public long Previous
		{
			get { return goForward ? PreviousMigration() : NextMigration(); }
		}

		public long Next
		{
			get { return goForward ? NextMigration() : PreviousMigration(); }
		}

		public void Iterate()
		{
			Current = Next;
		}

		public bool Continue(long targetVersion)
		{
			// If we're going backwards and our current is less than the target, 
			// reverse direction.  Also, start over at zero to make sure we catch
			// any merged migrations that are less than the current target.
			if (!goForward && targetVersion >= Current)
			{
				goForward = true;
				Current = 0;
				Iterate();
			}

			// We always finish on going forward. So continue if we're still 
			// going backwards, or if there are no migrations left in the forward direction.
			return !goForward || Current <= targetVersion;
		}

		public void Migrate(IMigration migration)
		{
			provider.BeginTransaction();
			MigrationAttribute attr = (MigrationAttribute)Attribute.GetCustomAttribute(migration.GetType(), typeof(MigrationAttribute));

			if (provider.AppliedMigrations.Contains(attr.Version))
			{
				RemoveMigration(migration, attr);
			}
			else
			{
				ApplyMigration(migration, attr);
			}

		}

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
			{
				migrationSearch++;
			}

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
			while (migrationSearch > -1 && !provider.AppliedMigrations.Contains(availableMigrations[migrationSearch]))
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

		private void ApplyMigration(IMigration migration, MigrationAttribute attr)
		{
			// we're adding this one
			logger.MigrateUp(Current, migration.Name);

			migration.Up();
			provider.MigrationApplied(attr.Version);
			provider.Commit();
			migration.AfterUp();
		}

		private void RemoveMigration(IMigration migration, MigrationAttribute attr)
		{
			// we're removing this one
			logger.MigrateDown(Current, migration.Name);
			migration.Down();
			provider.MigrationUnApplied(attr.Version);
			provider.Commit();
			migration.AfterDown();
		}

	}
}
