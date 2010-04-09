using System.Reflection;
using ECM7.Migrator.Framework;
using ECM7.Migrator.Framework.Loggers;
using ECM7.Migrator.Loader;
using NUnit.Framework;
using NUnit.Mocks;

namespace ECM7.Migrator.Tests.TestClasses.Common
{
	[TestFixture]
	public class MigrationLoaderTest
	{

		private MigrationLoader migrationLoader;

		[SetUp]
		public void SetUp()
		{
			SetUpCurrentVersion(0, false);
		}

		[Test]
		public void LastVersion()
		{
			Assert.AreEqual(7, migrationLoader.LastVersion);
		}

		[Test]
		public void ZeroIfNoMigrations()
		{
			migrationLoader.MigrationsTypes.Clear();
			Assert.AreEqual(0, migrationLoader.LastVersion);
		}

		[Test]
		public void NullIfNoMigrationForVersion()
		{
			Assert.IsNull(migrationLoader.GetMigration(99999999));
		}

		[Test, ExpectedException(typeof(DuplicatedVersionException))]
		public void CheckForDuplicatedVersion()
		{
			migrationLoader.MigrationsTypes.Add(
				new MigrationInfo(typeof(MigratorTest.FirstMigration)));
			migrationLoader.CheckForDuplicatedVersion();

		}

		private void SetUpCurrentVersion(int version, bool assertRollbackIsCalled)
		{
			DynamicMock providerMock = new DynamicMock(typeof(ITransformationProvider));

			providerMock.SetReturnValue("get_CurrentVersion", version);
			providerMock.SetReturnValue("get_Logger", new Logger(false));
			if (assertRollbackIsCalled)
				providerMock.Expect("Rollback");
			else
				providerMock.ExpectNoCall("Rollback");

			migrationLoader = new MigrationLoader((ITransformationProvider)providerMock.MockInstance, true);
			migrationLoader.MigrationsTypes.Add(new MigrationInfo(typeof(MigratorTest.FirstMigration)));
			migrationLoader.MigrationsTypes.Add(new MigrationInfo(typeof(MigratorTest.SecondMigration)));
			migrationLoader.MigrationsTypes.Add(new MigrationInfo(typeof(MigratorTest.ThirdMigration)));
			migrationLoader.MigrationsTypes.Add(new MigrationInfo(typeof(MigratorTest.ForthMigration)));
			migrationLoader.MigrationsTypes.Add(new MigrationInfo(typeof(MigratorTest.BadMigration)));
			migrationLoader.MigrationsTypes.Add(new MigrationInfo(typeof(MigratorTest.SixthMigration)));
			migrationLoader.MigrationsTypes.Add(new MigrationInfo(typeof(MigratorTest.NonIgnoredMigration)));
		}
	}
}