namespace ECM7.Migrator.Tests2
{
	using ECM7.Common.Utils.Exceptions;
	using ECM7.Migrator.Framework;
	using ECM7.Migrator.Loader;

	using Moq;

	using NUnit.Framework;

	/// <summary>
	/// Тестирование загрузчика миграций
	/// </summary>
	[TestFixture]
	public class MigrationLoaderTest
	{
		/// <summary>
		/// Проверка, что при отсутствии миграции с заданным номером версии возвращается null
		/// </summary>
		[Test]
		public void NullIfNoMigrationForVersion()
		{
			MigrationLoader loader = new MigrationLoader(string.Empty, null);
			Mock<ITransformationProvider> provider = new Moq.Mock<ITransformationProvider>();

			Assert.IsNull(loader.GetMigration(99999999, provider.Object));
		}

		/// <summary>
		/// Проверка генерации исключения, если не указан провайдер СУБД
		/// </summary>
		[Test]
		public void ForNullProviderShouldThrowException()
		{
			Assert.Throws<RequirementNotCompliedException>(delegate
				{
					new MigrationLoader(string.Empty, null).GetMigration(1, null);
				});
		}



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

		[Test, ExpectedException(typeof(DuplicatedVersionException))]
		public void CheckForDuplicatedVersion()
		{
			//migrationLoader.MigrationsTypes.Add(
			//    new MigrationInfo(typeof(MigratorTest.FirstMigration)));
			//migrationLoader.CheckForDuplicatedVersion();

		}

		private void SetUpCurrentVersion(int version, bool assertRollbackIsCalled)
		{
			//DynamicMock providerMock = new DynamicMock(typeof(ITransformationProvider));

			//providerMock.SetReturnValue("get_CurrentVersion", version);
			//providerMock.SetReturnValue("get_Logger", new Logger(false));
			//if (assertRollbackIsCalled)
			//    providerMock.Expect("Rollback");
			//else
			//    providerMock.ExpectNoCall("Rollback");

			//migrationLoader = new MigrationLoader((ITransformationProvider)providerMock.MockInstance, true);
			//migrationLoader.MigrationsTypes.Add(new MigrationInfo(typeof(MigratorTest.FirstMigration)));
			//migrationLoader.MigrationsTypes.Add(new MigrationInfo(typeof(MigratorTest.SecondMigration)));
			//migrationLoader.MigrationsTypes.Add(new MigrationInfo(typeof(MigratorTest.ThirdMigration)));
			//migrationLoader.MigrationsTypes.Add(new MigrationInfo(typeof(MigratorTest.ForthMigration)));
			//migrationLoader.MigrationsTypes.Add(new MigrationInfo(typeof(MigratorTest.BadMigration)));
			//migrationLoader.MigrationsTypes.Add(new MigrationInfo(typeof(MigratorTest.SixthMigration)));
			//migrationLoader.MigrationsTypes.Add(new MigrationInfo(typeof(MigratorTest.NonIgnoredMigration)));
		}
	}
}