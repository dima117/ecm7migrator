using System;
using System.Collections.Generic;
using System.Reflection;
using ECM7.Migrator.Framework;
using ECM7.Migrator.Framework.Loggers;
using ECM7.Migrator.Loader;
using NUnit.Framework;
using NUnit.Mocks;

namespace ECM7.Migrator.Tests.TestClasses.Common
{
	[TestFixture]
	public class MigratorTestDates
	{
		private Migrator migrator;

		// Collections that contain the version that are called migrating up and down
		private static readonly List<long> UpCalled = new List<long>();
		private static readonly List<long> DownCalled = new List<long>();

		[SetUp]
		public void SetUp()
		{
			SetUpCurrentVersion(0);
		}

		[Test]
		public void MigrateUpward()
		{
			SetUpCurrentVersion(2008010195);
			migrator.MigrateTo(2008030195);

			Assert.AreEqual(2, UpCalled.Count);
			Assert.AreEqual(0, DownCalled.Count);

			Assert.AreEqual(2008020195, UpCalled[0]);
			Assert.AreEqual(2008030195, UpCalled[1]);
		}

		[Test]
		public void MigrateBackward()
		{
			SetUpCurrentVersion(2008030195);
			migrator.MigrateTo(2008010195);

			Assert.AreEqual(0, UpCalled.Count);
			Assert.AreEqual(2, DownCalled.Count);

			Assert.AreEqual(2008030195, DownCalled[0]);
			Assert.AreEqual(2008020195, DownCalled[1]);
		}

		[Test]
		public void MigrateUpwardWithRollback()
		{
			SetUpCurrentVersion(2008030195, true);

			try
			{
				migrator.MigrateTo(2008060195);
				Assert.Fail("La migration 5 devrait lancer une exception");
			}
			catch (Exception) { }

			Assert.AreEqual(1, UpCalled.Count);
			Assert.AreEqual(0, DownCalled.Count);

			Assert.AreEqual(2008040195, UpCalled[0]);
		}

		[Test]
		public void MigrateDownwardWithRollback()
		{
			SetUpCurrentVersion(2008060195, true);

			try
			{
				migrator.MigrateTo(3);
				Assert.Fail("La migration 5 devrait lancer une exception");
			}
			catch (Exception) { }

			Assert.AreEqual(0, UpCalled.Count);
			Assert.AreEqual(1, DownCalled.Count);

			Assert.AreEqual(2008060195, DownCalled[0]);
		}

		[Test]
		public void MigrateToCurrentVersion()
		{
			SetUpCurrentVersion(2008030195);

			migrator.MigrateTo(2008030195);

			Assert.AreEqual(0, UpCalled.Count);
			Assert.AreEqual(0, DownCalled.Count);
		}

		[Test]
		public void MigrateToLastVersion()
		{
			SetUpCurrentVersion(2008030195, false, false);

			migrator.MigrateToLastVersion();

			Assert.AreEqual(2, UpCalled.Count);
			Assert.AreEqual(0, DownCalled.Count);
		}

		[Test]
		public void MigrateUpWithHoles()
		{
			List<long> migs = new List<long> {2008010195, 2008030195};
			SetUpCurrentVersion(2008030195, migs, false, false);
			migrator.MigrateTo(2008040195);


			Assert.AreEqual(2, UpCalled.Count);
			Assert.AreEqual(0, DownCalled.Count);

			Assert.AreEqual(2008020195, UpCalled[0]);
			Assert.AreEqual(2008040195, UpCalled[1]);

		}

		[Test]
		public void MigrateDownWithHoles()
		{
			List<long> migs = new List<long> { 2008010195, 2008030195, 2008040195 };
			SetUpCurrentVersion(2008040195, migs, false, false);
			migrator.MigrateTo(2008030195);

			Assert.AreEqual(1, UpCalled.Count);
			Assert.AreEqual(1, DownCalled.Count);

			Assert.AreEqual(2008020195, UpCalled[0]);
			Assert.AreEqual(2008040195, DownCalled[0]);

		}

		[Test]
		public void PostMergeMigrateDown()
		{
			// Assume trunk had versions 1 2 and 4.  A branch is merged with 3, then 
			// rollback to version 2.  v3 should be untouched, and v4 should be rolled back
			List<long> migs = new List<long> {2008010195, 2008020195, 2008040195};
			SetUpCurrentVersion(2008040195, migs, false, false);
			migrator.MigrateTo(2008020195);

			Assert.AreEqual(0, UpCalled.Count);
			Assert.AreEqual(1, DownCalled.Count);

			Assert.AreEqual(2008040195, DownCalled[0]);

		}

		[Test]
		public void PostMergeOldAndMigrateLatest()
		{
			// Assume trunk had versions 1 2 and 4.  A branch is merged with 3, then 
			// we migrate to Latest.  v3 should be applied and nothing else done.
			List<long> migs = new List<long> {2008010195, 2008020195, 2008040195};
			SetUpCurrentVersion(2008040195, migs, false, false);
			migrator.MigrateTo(2008040195);

			Assert.AreEqual(1, UpCalled.Count);
			Assert.AreEqual(0, DownCalled.Count);

			Assert.AreEqual(2008030195, UpCalled[0]);

		}



		[Test]
		public void ToHumanName()
		{
			Assert.AreEqual("Create a table", StringUtils.ToHumanName("CreateATable"));
		}

		#region Helper methods and classes

		private void SetUpCurrentVersion(long version)
		{
			SetUpCurrentVersion(version, false);
		}

		private void SetUpCurrentVersion(long version, bool assertRollbackIsCalled)
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
				providerMock.Expect("Rollback");
			else
				providerMock.ExpectNoCall("Rollback");

			migrator = new Migrator((ITransformationProvider)providerMock.MockInstance, false, Assembly.GetExecutingAssembly());

			// Enlève toutes les migrations trouvée automatiquement
			migrator.MigrationsTypes.Clear();
			UpCalled.Clear();
			DownCalled.Clear();

			migrator.MigrationsTypes.Add(new MigrationInfo(typeof(FirstMigration)));
			migrator.MigrationsTypes.Add(new MigrationInfo(typeof(SecondMigration)));
			migrator.MigrationsTypes.Add(new MigrationInfo(typeof(ThirdMigration)));
			migrator.MigrationsTypes.Add(new MigrationInfo(typeof(FourthMigration)));
			migrator.MigrationsTypes.Add(new MigrationInfo(typeof(SixthMigration)));

			if (includeBad)
				migrator.MigrationsTypes.Add(new MigrationInfo(typeof(BadMigration)));

		}

		public class AbstractTestMigration : Migration
		{
			override public void Up()
			{
				UpCalled.Add(new MigrationInfo(GetType()).Version);
			}
			override public void Down()
			{
				DownCalled.Add(new MigrationInfo(GetType()).Version);
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