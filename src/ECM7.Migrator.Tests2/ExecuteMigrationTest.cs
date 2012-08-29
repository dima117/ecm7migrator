namespace ECM7.Migrator.Tests2
{
	using System;
	using System.Reflection;

	using ECM7.Migrator.Framework;

	using Moq;

	using NUnit.Framework;

	/// <summary>
	/// Проверка выполнения миграций
	/// </summary>
	[TestFixture]
	public class ExecuteMigrationTest
	{
		#region with transaction

		/// <summary>
		/// Проверка выполнения миграции "вверх"
		/// </summary>
		[Test]
		public void CanMoveUp()
		{
			var provider = new Mock<ITransformationProvider>();
			Assembly asm = Assembly.Load("ECM7.Migrator.TestAssembly");

			using (var migrator = new Migrator(provider.Object, asm))
			{
				migrator.ExecuteMigration(2, 1);

				provider.Verify(db => db.BeginTransaction());
				provider.Verify(db => db.ExecuteNonQuery("up"));
				provider.Verify(db => db.MigrationApplied(2, "test-key111"));
				provider.Verify(db => db.Commit());
			}
		}

		/// <summary>
		/// Проверка выполнения миграции "вниз"
		/// </summary>
		[Test]
		public void CanMoveDown()
		{
			var provider = new Mock<ITransformationProvider>();
			Assembly asm = Assembly.Load("ECM7.Migrator.TestAssembly");

			using (var migrator = new Migrator(provider.Object, asm))
			{
				migrator.ExecuteMigration(2, 2);

				provider.Verify(db => db.BeginTransaction());
				provider.Verify(db => db.ExecuteNonQuery("down"));
				provider.Verify(db => db.MigrationUnApplied(2, "test-key111"));
				provider.Verify(db => db.Commit());
			}
		}

		/// <summary>
		/// Проверка, что при возникновении ошибки выполняется откат транзакции
		/// </summary>
		[Test]
		public void ShouldPerformRollbackWhenException()
		{
			Assembly asm = Assembly.Load("ECM7.Migrator.TestAssembly");
			var provider = new Mock<ITransformationProvider>();

			provider
				.Setup(db => db.MigrationUnApplied(It.IsAny<long>(), It.IsAny<string>()))
				.Throws<Exception>();

			using (var migrator = new Migrator(provider.Object, asm))
			{
				Assert.Throws<Exception>(() => migrator.ExecuteMigration(2, 2));

				provider.Verify(db => db.BeginTransaction());
				provider.Verify(db => db.MigrationUnApplied(2, It.IsAny<string>()));
				provider.Verify(db => db.Rollback());
			}
		}

		#endregion

		#region without transaction

		/// <summary>
		/// Проверка выполнения миграции "вверх" (без транзакции)
		/// </summary>
		[Test]
		public void CanMoveUpWithoutTransaction()
		{
			var provider = new Mock<ITransformationProvider>();
			Assembly asm = Assembly.Load("ECM7.Migrator.TestAssembly");

			using (var migrator = new Migrator(provider.Object, asm))
			{
				migrator.ExecuteMigration(4, 3);

				provider.Verify(db => db.ExecuteNonQuery("up4"));
				provider.Verify(db => db.MigrationApplied(4, "test-key111"));
			}
		}

		/// <summary>
		/// Проверка выполнения миграции "вниз" (без транзакции)
		/// </summary>
		[Test]
		public void CanMoveDownWithoutTransaction()
		{
			var provider = new Mock<ITransformationProvider>();
			Assembly asm = Assembly.Load("ECM7.Migrator.TestAssembly");

			using (var migrator = new Migrator(provider.Object, asm))
			{
				migrator.ExecuteMigration(4, 4);

				provider.Verify(db => db.ExecuteNonQuery("down4"));
				provider.Verify(db => db.MigrationUnApplied(4, "test-key111"));
			}
		}

		/// <summary>
		/// Проверка, что при возникновении ошибки (без транзакции) не выполняется откат транзакции
		/// </summary>
		[Test]
		public void ShouldNoRollbackWhenExceptionWithoutTransaction()
		{
			Assembly asm = Assembly.Load("ECM7.Migrator.TestAssembly");
			var provider = new Mock<ITransformationProvider>();

			provider
				.Setup(db => db.MigrationUnApplied(It.IsAny<long>(), It.IsAny<string>()))
				.Throws<Exception>();

			using (var migrator = new Migrator(provider.Object, asm))
			{
				Assert.Throws<Exception>(() => migrator.ExecuteMigration(2, 2));

				provider.Verify(db => db.MigrationUnApplied(2, It.IsAny<string>()));
			}
		}

		#endregion
	}
}
