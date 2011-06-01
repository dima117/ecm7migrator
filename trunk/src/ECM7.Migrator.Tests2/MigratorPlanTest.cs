namespace ECM7.Migrator.Tests2
{
	using System.Collections.Generic;

	using NUnit.Framework;

	/// <summary>
	/// Тестирование построения плана выполнения миграций
	/// </summary>
	[TestFixture]
	public class MigratorPlanTest
	{
		/// <summary>
		/// Проверка, что в нормальных условиях корректно формируется план для движения назад
		/// </summary>
		[Test]
		public void CanBuildCorrectMigrationPlanDown()
		{
			var applied = new List<long> { 1, 2, 3 };
			var available = new List<long> { 1, 2, 3, 4, 77, 88 };

			var res = Migrator.BuildMigrationPlan(1, applied, available);
			Assert.AreEqual(2, res.Count);
			Assert.AreEqual(3, res[0]);
			Assert.AreEqual(2, res[1]);
		}

		/// <summary>
		/// Проверка, что в нормальных условиях корректно формируется план для движения назад
		/// </summary>
		[Test]
		public void CanBuildCorrectMigrationPlanUp()
		{
			var applied = new List<long> { 1, 2, 3 };
			var available = new List<long> { 1, 2, 3, 4, 77, 88 };

			var res = Migrator.BuildMigrationPlan(88, applied, available);
			Assert.AreEqual(3, res.Count);
			Assert.AreEqual(4, res[0]);
			Assert.AreEqual(77, res[1]);
			Assert.AreEqual(88, res[2]);
		}

		/// <summary>
		/// Проверка, что план формируется корректно для несуществующих версий (идем вверх)
		/// </summary>
		[Test]
		public void CanBuildPlanForNotExistsVersionUp()
		{
			var applied = new List<long> { 5, 10 };
			var available = new List<long> { 5, 10, 15, 20 };

			var res = Migrator.BuildMigrationPlan(12, applied, available);
			Assert.AreEqual(0, res.Count);

			var res2 = Migrator.BuildMigrationPlan(17, applied, available);
			Assert.AreEqual(1, res2.Count);
			Assert.AreEqual(15, res2[0]);

			var res3 = Migrator.BuildMigrationPlan(23, applied, available);
			Assert.AreEqual(2, res3.Count);
			Assert.AreEqual(15, res3[0]);
			Assert.AreEqual(20, res3[1]);
		}

		/// <summary>
		/// Проверка, что план формируется корректно для несуществующих версий (идем вниз)
		/// </summary>
		[Test]
		public void CanBuildPlanForNotExistsVersionDown()
		{
			var applied = new List<long> { 5, 10, 15 };
			var available = new List<long> { 5, 10, 15, 20 };

			var res = Migrator.BuildMigrationPlan(12, applied, available);
			Assert.AreEqual(1, res.Count);
			Assert.AreEqual(15, res[0]);

			var res2 = Migrator.BuildMigrationPlan(7, applied, available);
			Assert.AreEqual(2, res2.Count);
			Assert.AreEqual(15, res2[0]);
			Assert.AreEqual(10, res2[1]);
		}

		/// <summary>
		/// Построение плана для текущей версии
		/// </summary>
		[Test]
		public void CanBuildCorrectMigrationPlanForCurrentVersion()
		{
			var applied = new List<long> { 1, 2, 3 };
			var available = new List<long> { 1, 2, 3, 4, 77, 88 };

			var res = Migrator.BuildMigrationPlan(3, applied, available);
			Assert.AreEqual(0, res.Count);
		}

		/// <summary>
		/// Отсутствие классов миграций для выполненных версий
		/// </summary>
		[Test]
		public void CanBuildPlanForVersionsHasntMigrationClass()
		{
			var applied = new List<long> { 1, 2, 3, 4 };
			var available = new List<long> { 1, 2, 4, 77, 88 };

			var res = Migrator.BuildMigrationPlan(1, applied, available);
			Assert.AreEqual(3, res.Count);
			Assert.AreEqual(4, res[0]);
			Assert.AreEqual(3, res[1]);
			Assert.AreEqual(2, res[2]);
		}

		/// <summary>
		/// Проверка, что при невыполненных миграциях ниже текущей версии генерируется исключение
		/// </summary>
		[Test]
		public void ShouldThrowExceptionIfExistsNotAppliedMigrationLessThatCurrent()
		{
			var applied = new List<long> { 1, 4 };
			var available = new List<long> { 1, 2, 3, 4, 5 };

			var ex = Assert.Throws<VersionException>(() =>
				Migrator.BuildMigrationPlan(5, applied, available));

			Assert.AreEqual(2, ex.Versions.Count);
			Assert.That(ex.Versions.Contains(3));
			Assert.That(ex.Versions.Contains(2));
		}
	}
}
