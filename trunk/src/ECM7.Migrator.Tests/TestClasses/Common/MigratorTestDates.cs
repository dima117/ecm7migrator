namespace ECM7.Migrator.Tests.TestClasses.Common
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using ECM7.Common.Utils.Exceptions;
	using Framework;
	using Framework.Loggers;
	using Loader;
	using NUnit.Framework;
	using NUnit.Mocks;

	[TestFixture]
	public class MigratorTestDates
	{
		private Migrator migrator;

		// Collections that contain the version that are called migrating up and down
		private static readonly List<long> upCalled = new List<long>();
		private static readonly List<long> downCalled = new List<long>();

		[SetUp]
		public void SetUp()
		{
			SetUpCurrentVersion(0);
		}

		[Test]
		public void MigrateUpward()
		{
			SetUpCurrentVersion(2008010195);
			migrator.Migrate(2008030195);

			Assert.AreEqual(2, upCalled.Count);
			Assert.AreEqual(0, downCalled.Count);

			Assert.AreEqual(2008020195, upCalled[0]);
			Assert.AreEqual(2008030195, upCalled[1]);
		}

		[Test]
		public void MigrateBackward()
		{
			SetUpCurrentVersion(2008030195);
			migrator.Migrate(2008010195);

			Assert.AreEqual(0, upCalled.Count);
			Assert.AreEqual(2, downCalled.Count);

			Assert.AreEqual(2008030195, downCalled[0]);
			Assert.AreEqual(2008020195, downCalled[1]);
		}

		[Test]
		public void MigrateUpwardWithRollback()
		{
			SetUpCurrentVersion(2008030195, true);

			try
			{
				migrator.Migrate(2008060195);
				Assert.Fail("La migration 5 devrait lancer une exception");
			}
			catch (Exception) { }

			Assert.AreEqual(1, upCalled.Count);
			Assert.AreEqual(0, downCalled.Count);

			Assert.AreEqual(2008040195, upCalled[0]);
		}

		[Test]
		public void MigrateDownwardWithRollback()
		{
			SetUpCurrentVersion(2008060195, true);

			try
			{
				migrator.Migrate(3);
				Assert.Fail("La migration 5 devrait lancer une exception");
			}
			catch (Exception) { }

			Assert.AreEqual(0, upCalled.Count);
			Assert.AreEqual(1, downCalled.Count);

			Assert.AreEqual(2008060195, downCalled[0]);
		}

		[Test]
		public void MigrateToCurrentVersion()
		{
			SetUpCurrentVersion(2008030195);

			migrator.Migrate(2008030195);

			Assert.AreEqual(0, upCalled.Count);
			Assert.AreEqual(0, downCalled.Count);
		}

		[Test]
		public void MigrateToLastVersion()
		{
			SetUpCurrentVersion(2008030195, false, false);

			migrator.Migrate();

			Assert.AreEqual(2, upCalled.Count);
			Assert.AreEqual(0, downCalled.Count);
		}

		[Test]
		public void CantMigrateUpWithHoles()
		{
			List<long> migs = new List<long> { 2008010195, 2008030195 };
			SetUpCurrentVersion(2008030195, migs, false, false);
			Assert.Throws<RequirementNotCompliedException>(() => migrator.Migrate(2008040195));
		}

		[Test]
		public void CantMigrateDownWithHoles()
		{
			List<long> migs = new List<long> { 2008010195, 2008030195, 2008040195 };
			SetUpCurrentVersion(2008040195, migs, false, false);
			Assert.Throws<RequirementNotCompliedException>(() => migrator.Migrate(2008030195));
		}

		[Test]
		public void ToHumanName()
		{
			Assert.AreEqual("Create a table", StringUtils.ToHumanName("CreateATable"));
		}

		#region Helper methods and classes

		private void SetUpCurrentVersion(long version, bool assertRollbackIsCalled = false)
		{
			SetUpCurrentVersion(version, assertRollbackIsCalled, true);
		}

		private void SetUpCurrentVersion(long version, bool assertRollbackIsCalled, bool includeBad)
		{
			List<long> appliedVersions = new List<long>();
			for (long i = 2008010195; i <= version; i += 10000)
			{
				appliedVersions.Add(i);
			}
			SetUpCurrentVersion(version, appliedVersions, assertRollbackIsCalled, includeBad);
		}

		private void SetUpCurrentVersion(long version, List<long> appliedVersions, bool assertRollbackIsCalled, bool includeBad)
		{
			DynamicMock providerMock = new DynamicMock(typeof(ITransformationProvider));

			providerMock.SetReturnValue("get_MaxVersion", version);
			providerMock.SetReturnValue("get_AppliedMigrations", appliedVersions);
			providerMock.SetReturnValue("get_Logger", new Logger(false));
			if (assertRollbackIsCalled)
			{
				providerMock.Expect("Rollback");
			}
			else
			{
				providerMock.ExpectNoCall("Rollback");
			}

			ITransformationProvider provider = (ITransformationProvider)providerMock.MockInstance;
			migrator = new Migrator(provider, string.Empty, null, Assembly.GetExecutingAssembly());

			// Enlève toutes les migrations trouvée automatiquement
			migrator.AvailableMigrations.Clear();
			upCalled.Clear();
			downCalled.Clear();

			migrator.AvailableMigrations.Add(new MigrationInfo(typeof(FirstMigration)));
			migrator.AvailableMigrations.Add(new MigrationInfo(typeof(SecondMigration)));
			migrator.AvailableMigrations.Add(new MigrationInfo(typeof(ThirdMigration)));
			migrator.AvailableMigrations.Add(new MigrationInfo(typeof(FourthMigration)));
			migrator.AvailableMigrations.Add(new MigrationInfo(typeof(SixthMigration)));

			if (includeBad)
			{
				migrator.AvailableMigrations.Add(new MigrationInfo(typeof(BadMigration)));
			}
		}

		public class AbstractTestMigration : Migration
		{
			override public void Up()
			{
				upCalled.Add(new MigrationInfo(GetType()).Version);
			}
			override public void Down()
			{
				downCalled.Add(new MigrationInfo(GetType()).Version);
			}
		}

		[Migration(2008010195, Ignore = true)]
		public class FirstMigration : AbstractTestMigration { }
		[Migration(2008020195, Ignore = true)]
		public class SecondMigration : AbstractTestMigration { }
		[Migration(2008030195, Ignore = true)]
		public class ThirdMigration : AbstractTestMigration { }
		[Migration(2008040195, Ignore = true)]
		public class FourthMigration : AbstractTestMigration { }

		[Migration(2008050195, Ignore = true)]
		public class BadMigration : AbstractTestMigration
		{
			override public void Up()
			{
				throw new Exception("oh uh!");
			}
			override public void Down()
			{
				throw new Exception("oh uh!");
			}
		}

		[Migration(2008060195, Ignore = true)]
		public class SixthMigration : AbstractTestMigration { }

		[Migration(2008070195)]
		public class NonIgnoredMigration : AbstractTestMigration { }

		#endregion
	}
}