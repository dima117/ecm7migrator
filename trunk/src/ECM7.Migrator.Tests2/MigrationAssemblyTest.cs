using ECM7.Migrator.Exceptions;

namespace ECM7.Migrator.Tests2
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	using ECM7.Common.Utils.Exceptions;
	using ECM7.Migrator.Framework;
	using ECM7.Migrator.Framework.Logging;
	using ECM7.Migrator.Loader;
	using ECM7.Migrator.TestAssembly;

	using log4net.Appender;

	using Moq;

	using NUnit.Framework;

	/// <summary>
	/// Тестирование загрузчика миграций
	/// </summary>
	[TestFixture]
	public class MigrationLoaderTest
	{
		/// <summary>
		/// Проверка загрузки ключа сборки и миграций
		/// </summary>
		[Test]
		public void CanLoadMigrationsWithKey()
		{
			Assembly assembly = Assembly.Load("ECM7.Migrator.TestAssembly");
			var migrationAssembly = new MigrationAssembly(assembly);

			IList<long> list = migrationAssembly.MigrationsTypes.Select(x => x.Version).ToList();

			Assert.AreEqual("test-key111", migrationAssembly.Key);

			Assert.AreEqual(3, list.Count);
			Assert.IsTrue(list.Contains(1));
			Assert.IsTrue(list.Contains(2));
			Assert.IsTrue(list.Contains(4));
		}

		/// <summary>
		/// Проверка, что при отсутствии миграции с заданным номером генерируется исключение
		/// </summary>
		[Test]
		public void ThrowIfNoMigrationForVersion()
		{
			Assembly assembly = Assembly.Load("ECM7.Migrator.TestAssembly");

			MigrationAssembly migrationAssembly = new MigrationAssembly(assembly);

			Assert.Throws<RequirementNotCompliedException>(() => migrationAssembly.GetMigrationInfo(99999999));
		}

		/// <summary>
		/// Проверка генерации исключения, если не указан провайдер СУБД
		/// </summary>
		[Test]
		public void ForNullProviderShouldThrowException()
		{
			Assembly assembly = Assembly.Load("ECM7.Migrator.TestAssembly");

			var loader = new MigrationAssembly(assembly);

			var mi = loader.GetMigrationInfo(1);
			Assert.Throws<RequirementNotCompliedException>(() => loader.InstantiateMigration(mi, null));
		}


		/// <summary>
		/// Проверка корректности определения последней доступной версии
		/// </summary>
		[Test]
		public void LastVersion()
		{
			Assembly assembly = Assembly.Load("ECM7.Migrator.TestAssembly");
			MigrationAssembly migrationAssembly = new MigrationAssembly(assembly);
			Assert.AreEqual(4, migrationAssembly.LastVersion);
		}

		/// <summary>
		/// Проверка, что при отсутствии миграций последняя доступная версия == 0
		/// (загружаем по несуществующему ключу)
		/// </summary>
		[Test]
		public void LaseVersionIsZeroIfNoMigrations()
		{
			Assembly assembly = GetType().Assembly; // загружаем текущую сборку - в ней нет миграций
			MigrationAssembly migrationAssembly = new MigrationAssembly(assembly);
			Assert.AreEqual(0, migrationAssembly.LastVersion);
		}

		/// <summary>
		/// Проверка ограничения на повторнеие номеров версий
		/// </summary>
		[Test]
		public void CheckForDuplicatedVersion()
		{
			var versions = new long[] { 1, 2, 3, 4, 2, 4 };

			var ex = Assert.Throws<DuplicatedVersionException>(() =>
				MigrationAssembly.CheckForDuplicatedVersion(versions));

			Assert.AreEqual(2, ex.Versions.Count);
			Assert.That(ex.Versions.Contains(2));
			Assert.That(ex.Versions.Contains(4));
		}

		/// <summary>
		/// Проверка создания объекта миграции по номеру версии
		/// </summary>
		[Test]
		public void CanCreateMigrationObject()
		{
			Assembly assembly = Assembly.Load("ECM7.Migrator.TestAssembly");
			MigrationAssembly migrationAssembly = new MigrationAssembly(assembly);

			Mock<ITransformationProvider> provider = new Mock<ITransformationProvider>();

			var mi = migrationAssembly.GetMigrationInfo(2);
			IMigration migration = migrationAssembly.InstantiateMigration(mi, provider.Object);

			Assert.IsNotNull(migration);
			Assert.That(migration is SecondTestMigration);
			Assert.AreSame(provider.Object, migration.Database);
		}

		[Test]
		public void MigrationsMustBeSortedByNumber()
		{
			MemoryAppender appender = new MemoryAppender();
			MigratorLogManager.AddAppender(appender);

			Assembly assembly = Assembly.Load("ECM7.Migrator.TestAssembly");
			new MigrationAssembly(assembly);

			var list = appender
				.GetEvents()
				.Where(e => e.MessageObject.ToString().StartsWith("Loaded migrations:"))
				.ToList();

			Assert.AreEqual(1, list.Count);
			Assert.AreEqual(
				"Loaded migrations:\r\n    1 First test migration\r\n    2 Second test migration\r\n    4 Four test migration\r\n",
				list[0].MessageObject.ToString());
		}
	}
}
