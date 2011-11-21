using System;
using System.Collections.Generic;
using ECM7.Migrator.Framework;

namespace ECM7.Migrator
{
    /// <summary>
    /// Description of MigrateAnywhere.
    /// </summary>
    public class MigrateAnywhere : BaseMigrate
    {
        private bool goForward;

        public MigrateAnywhere(List<long> availableMigrations, ITransformationProvider provider, ILogger logger)
            : base(availableMigrations, provider, logger)
        {
			current = 0;
			if (provider.AppliedMigrations.Count > 0) {
				current = provider.AppliedMigrations[provider.AppliedMigrations.Count - 1];
			}
			goForward = false;
        }

        public override long Next
        {
            get
            {
                return goForward
                           ? NextMigration()
                           : PreviousMigration();
            }
        }

        public override long Previous
        {
            get
            {
                return goForward
                           ? PreviousMigration()
                           : NextMigration();
            }
        }

        public override bool Continue(long version)
        {
            // If we're going backwards and our current is less than the target, 
            // reverse direction.  Also, start over at zero to make sure we catch
            // any merged migrations that are less than the current target.
            if (!goForward && version >= Current)
            {
                goForward = true;
                Current = 0;
                Iterate();
            }

            // We always finish on going forward. So continue if we're still 
            // going backwards, or if there are no migrations left in the forward direction.
            return !goForward || Current <= version;
        }

        public override void Migrate(IMigration migration)
        {
            provider.BeginTransaction();
            MigrationAttribute attr = (MigrationAttribute)Attribute.GetCustomAttribute(migration.GetType(), typeof(MigrationAttribute));
            
            if (provider.AppliedMigrations.Contains(attr.Version)) {
            	RemoveMigration(migration, attr);
            } else {
            	ApplyMigration(migration, attr);
            }
        }

        private void ApplyMigration(IMigration migration, MigrationAttribute attr)
        {
            // we're adding this one
            logger.MigrateUp(Current, migration.Name);
            if(! DryRun)
            {
                migration.Up();
                provider.MigrationApplied(attr.Version);
                provider.Commit();
                migration.AfterUp();
            }
        }

        private void RemoveMigration(IMigration migration, MigrationAttribute attr)
        {
            // we're removing this one
            logger.MigrateDown(Current, migration.Name);
            if (! DryRun)
            {
                migration.Down();
                provider.MigrationUnApplied(attr.Version);
                provider.Commit();
                migration.AfterDown();
            }
        }
    }
}
