using System;
using System.Collections.Generic;
using System.Reflection;
using ECM7.Migrator.Framework;
using ECM7.Migrator.Framework.Loggers;
using ECM7.Migrator.Loader;
using NUnit.Framework;
using NUnit.Mocks;
using ECM7.Common.Utils.Exceptions;

namespace ECM7.Migrator.Tests.TestClasses.Common
{
	[TestFixture]
	public class MigratorTest
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
			SetUpCurrentVersion(1);
			migrator.MigrateTo(3);

			Assert.AreEqual(2, UpCalled.Count);
			Assert.AreEqual(0, DownCalled.Count);

			Assert.AreEqual(2, UpCalled[0]);
			Assert.AreEqual(3, UpCalled[1]);
		}

		[Test]
		public void MigrateBackward()
		{
			SetUpCurrentVersion(3);
			migrator.MigrateTo(1);

			Assert.AreEqual(0, UpCalled.Count);
			Assert.AreEqual(2, DownCalled.Count);

			Assert.AreEqual(3, DownCalled[0]);
			Assert.AreEqual(2, DownCalled[1]);
		}

		[Test]
		public void MigrateUpwardWithRollback()
		{
			SetUpCurrentVersion(3, true);

			try
			{
				migrator.MigrateTo(6);
				Assert.Fail("La migration 5 devrait lancer une exception");
			}
			catch { }

			Assert.AreEqual(1, UpCalled.Count);
			Assert.AreEqual(0, DownCalled.Count);

			Assert.AreEqual(4, UpCalled[0]);
		}

		[Test]
		public void MigrateDownwardWithRollback()
		{
			SetUpCurrentVersion(6, true);

			try
			{
				migrator.MigrateTo(3);
				Assert.Fail("La migration 5 devrait lancer une exception");
			}
			catch { }

			Assert.AreEqual(0, UpCalled.Count);
			Assert.AreEqual(1, DownCalled.Count);

			Assert.AreEqual(6, DownCalled[0]);
		}

		[Test]
		public void MigrateToCurrentVersion()
		{
			SetUpCurrentVersion(3);

			migrator.MigrateTo(3);

			Assert.AreEqual(0, UpCalled.Count);
			Assert.AreEqual(0, DownCalled.Count);
		}

		[Test]
		public void MigrateToLastVersion()
		{
			SetUpCurrentVersion(3, false, false);

			migrator.MigrateToLastVersion();

			Assert.AreEqual(2, UpCalled.Count);
			Assert.AreEqual(0, DownCalled.Count);
		}

		[Test]
		public void CanCheckMigrationNumbers()
		{
			Assert.DoesNotThrow(() => Migrator.CheckMigrationNumbers(new List<long> { 1, 2, 3, 4 }, new List<long> { 1, 2 }));
			Assert.DoesNotThrow(() => Migrator.CheckMigrationNumbers(new List<long> { 1, 2, 3, 4 }, new List<long>()));
			Assert.DoesNotThrow(() => Migrator.CheckMigrationNumbers(new List<long> { 1 }, new List<long> { 1, 2 }));
			Assert.DoesNotThrow(() => Migrator.CheckMigrationNumbers(new List<long> { 1, 3, 4 }, new List<long> { 1, 2 }));

			Assert.Throws<RequirementNotCompliedException>(() => Migrator.CheckMigrationNumbers(new List<long> { 1, 2, 3, 4 }, new List<long> { 1, 4 }));
		}

		[Test]
		public void ToHumanName()
		{
			Assert.AreEqual("Create a table", StringUtils.ToHumanName("CreateATable"));
		}

		[Test]
		public void MigrateUpwardFrom0()
		{
			migrator.MigrateTo(3);

			Assert.AreEqual(3, UpCalled.Count);
			Assert.AreEqual(0, DownCalled.Count);

			Assert.AreEqual(1, UpCalled[0]);
			Assert.AreEqual(2, UpCalled[1]);
			Assert.AreEqual(3, UpCalled[2]);
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
			DynamicMock providerMock = new DynamicMock(typeof(ITransformationProvider));

			List<long> appliedVersions = new List<long>();
			for (long i = 1; i <= version; i++)
			{
				appliedVersions.Add(i);
			}
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
			migrator.MigrationsTypes.Add(new MigrationInfo(typeof(ForthMigration)));
			migrator.MigrationsTypes.Add(new MigrationInfo(typeof(SixthMigration)));

			if (includeBad)
			{
				migrator.MigrationsTypes.Add(new MigrationInfo(typeof(BadMigration)));
			}
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

		[Migration(1, Ignore = true)]
		public class FirstMigration : AbstractTestMigration { }
		[Migration(2, Ignore = true)]
		public class SecondMigration : AbstractTestMigration { }
		[Migration(3, Ignore = true)]
		public class ThirdMigration : AbstractTestMigration { }
		[Migration(4, Ignore = true)]
		public class ForthMigration : AbstractTestMigration { }

		[Migration(5, Ignore = true)]
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

		[Migration(6, Ignore = true)]
		public class SixthMigration : AbstractTestMigration { }

		[Migration(7)]
		public class NonIgnoredMigration : AbstractTestMigration { }

		#endregion
	}
}