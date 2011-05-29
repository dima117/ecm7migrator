namespace ECM7.Migrator
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using ECM7.Migrator.Framework;

	/// <summary>
	/// �������� ������ ��
	/// </summary>
	public sealed class BaseMigrate
	{
		// TODO!!!!!!!! ������� ������ ����������� ������, ���������� ���, ������ ��� ��� ��������� �� � ���������� �� ���� ����� ������� ������!!!!!!!

		// TODO!!!! ��������� �������� �� ����� ������� � ������ ����������� ����������� �� �����

		/// <summary>
		/// ��������� ����
		/// </summary>
		private readonly ITransformationProvider provider;

		/// <summary>
		/// Logger
		/// </summary>
		private readonly ILogger logger;

		/// <summary>
		/// ��������� ��� ���������� ��������
		/// </summary>
		private readonly List<long> availableMigrations;

		/// <summary>
		/// ����������� �������� �� ������� ������
		/// </summary>
		private readonly List<long> currentAppliedMigrations;

		/// <summary>
		/// ����������� �������� �� ������ �������������
		/// </summary>
		private readonly List<long> originalAppliedMigrations;

		/// <summary>
		/// ����� ������� ��������
		/// </summary>
		public long Current
		{
			get
			{
				return currentAppliedMigrations.IsEmpty() ? 0 : currentAppliedMigrations.Max();
			}
		}

		/// <summary>
		/// �������, ��� �������� ������ ����������� � ������� �����������
		/// </summary>
		private bool goForward;

		/// <summary>
		/// �������������
		/// </summary>
		/// <param name="provider">��������� ����</param>
		/// <param name="key">���� ��� ���������� ��������</param>
		/// <param name="availableMigrations">������ ������ ��������� ��� ���������� ��������</param>
		/// <param name="logger">Logger</param>
		public BaseMigrate(ITransformationProvider provider, string key, List<long> availableMigrations, ILogger logger)
		{
			// ��������� ����������
			Require.IsNotNull(provider, "�� ����� ��������� ����");

			this.provider = provider;
			this.logger = logger;

			// ������ ��������� � ����������� ��������
			this.availableMigrations = availableMigrations;
			this.originalAppliedMigrations = provider.GetAppliedMigrations(key);
			this.currentAppliedMigrations = new List<long>(originalAppliedMigrations.ToArray()); // clone

			goForward = false;

			CheckMigrationNumbers();
		}

		/// <summary>
		/// ��������, ��� ��������� ��� ��������� �������� � �������� ������ �������
		/// </summary>
		private void CheckMigrationNumbers()
		{
			long current = this.Current;

			var skippedMigrations = availableMigrations
				.Where(m => m <= current && !currentAppliedMigrations.Contains(m));

			Require.AreEqual(
				skippedMigrations.Count(),
				0,
				"The current database version is {0}, the migration {1} are available but not used",
				current,
				skippedMigrations.ToCommaSeparatedString());
		}

		public List<long> AppliedVersions
		{
			get { return originalAppliedMigrations; }
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

		/// <summary>
		/// ���� ������ ������ ��������� ������������� ��������, ����������� ������� ������
		/// </summary>
		private long NextMigration()
		{
			// Start searching at the current index
			int pos = availableMigrations.IndexOf(Current) + 1;

			// See if we can find a migration that matches the requirement
			while (pos < availableMigrations.Count
				  && currentAppliedMigrations.Contains(availableMigrations[pos]))
			{
				pos++;
			}

			// did we exhaust the list?
			if (pos == availableMigrations.Count)
			{
				// we're at the last one.  Done!
				return availableMigrations[pos - 1] + 1;
			}

			// found one.
			return availableMigrations[pos];
		}

		/// <summary>
		/// Finds the previous migration that has been applied.  Only returns
		/// migrations that HAVE already been applied.
		/// </summary>
		/// <returns>The most recently applied Migration.</returns>
		private long PreviousMigration()
		{
			// Start searching at the current index
			int migrationSearch = availableMigrations.IndexOf(Current) - 1;

			// See if we can find a migration that matches the requirement
			while (migrationSearch > -1 && !currentAppliedMigrations.Contains(availableMigrations[migrationSearch]))
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

		#region ���������� ��������

		public void Migrate(IMigration migration)
		{
			provider.BeginTransaction();
			MigrationAttribute attr = migration.GetType().GetCustomAttribute<MigrationAttribute>();

			if (currentAppliedMigrations.Contains(attr.Version))
			{
				RemoveMigration(migration, attr);
			}
			else
			{
				ApplyMigration(migration, attr);
			}
		}

		private void ApplyMigration(IMigration migration, MigrationAttribute attr)
		{
			// ��������� � ��������, �������� ������ ��������� ������� ������
			logger.MigrateUp(Current, migration.Name);

			migration.Up();
			provider.MigrationApplied(attr.Version,);
			currentAppliedMigrations.Add(attr.Version);
			provider.Commit();
			migration.AfterUp();
		}

		private void RemoveMigration(IMigration migration, MigrationAttribute attr)
		{
			// ��������� � ��������, �������� ������ ��������� ������� ������
			logger.MigrateDown(Current, migration.Name);
			migration.Down();
			provider.MigrationUnApplied(attr.Version);
			currentAppliedMigrations.Remove(attr.Version);
			provider.Commit();
			migration.AfterDown();
		}

		#endregion
	}
}
