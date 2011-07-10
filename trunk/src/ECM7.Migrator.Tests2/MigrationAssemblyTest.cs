namespace ECM7.Migrator.Tests2
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	using ECM7.Common.Utils.Exceptions;
	using ECM7.Migrator.Framework;
	using ECM7.Migrator.Loader;
	using ECM7.Migrator.TestAssembly;

	using Moq;

	using NUnit.Framework;

	/// <summary>
	/// ������������ ���������� ��������
	/// </summary>
	[TestFixture]
	public class MigrationLoaderTest
	{
		/// <summary>
		/// �������� �������� ����� ������ � ��������
		/// </summary>
		[Test]
		public void CanLoadMigrationsWithKey()
		{
			Assembly assembly = Assembly.Load("ECM7.Migrator.TestAssembly");
			var migrationAssembly = new MigrationAssembly(assembly);

			IList<long> list = migrationAssembly.MigrationsTypes.Select(x => x.Version).ToList();

			Assert.AreEqual("test-key111", migrationAssembly.Key);

			Assert.AreEqual(2, list.Count);
			Assert.IsTrue(list.Contains(1));
			Assert.IsTrue(list.Contains(2));

			Assert.AreEqual(2, migrationAssembly.LastVersion);
		}

		/// <summary>
		/// ��������, ��� ��� ���������� �������� � �������� ������� ������ ������������ null
		/// </summary>
		[Test]
		public void NullIfNoMigrationForVersion()
		{
			Assembly assembly = Assembly.Load("ECM7.Migrator.TestAssembly");

			MigrationAssembly migrationAssembly = new MigrationAssembly(assembly);
			Mock<ITransformationProvider> provider = new Moq.Mock<ITransformationProvider>();

			Assert.IsNull(migrationAssembly.InstantiateMigration(99999999, provider.Object));
		}

		/// <summary>
		/// �������� ��������� ����������, ���� �� ������ ��������� ����
		/// </summary>
		[Test]
		public void ForNullProviderShouldThrowException()
		{
			Assembly assembly = Assembly.Load("ECM7.Migrator.TestAssembly");

			var loader = new MigrationAssembly(assembly);
			Assert.Throws<RequirementNotCompliedException>(() => loader.InstantiateMigration(1, null));
		}


		/// <summary>
		/// �������� ������������ ����������� ��������� ��������� ������
		/// </summary>
		[Test]
		public void LastVersion()
		{
			Assembly assembly = Assembly.Load("ECM7.Migrator.TestAssembly");
			MigrationAssembly migrationAssembly = new MigrationAssembly(assembly);
			Assert.AreEqual(2, migrationAssembly.LastVersion);
		}

		/// <summary>
		/// ��������, ��� ��� ���������� �������� ��������� ��������� ������ == 0
		/// (��������� �� ��������������� �����)
		/// </summary>
		[Test]
		public void LaseVersionIsZeroIfNoMigrations()
		{
			Assembly assembly = this.GetType().Assembly; // ��������� ������� ������ - � ��� ��� ��������
			MigrationAssembly migrationAssembly = new MigrationAssembly(assembly);
			Assert.AreEqual(0, migrationAssembly.LastVersion);
		}

		/// <summary>
		/// �������� ����������� �� ���������� ������� ������
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
		/// �������� �������� ������� �������� �� ������ ������
		/// </summary>
		[Test]
		public void CanCreateMigrationObject()
		{
			Assembly assembly = Assembly.Load("ECM7.Migrator.TestAssembly");
			MigrationAssembly migrationAssembly = new MigrationAssembly(assembly);

			Mock<ITransformationProvider> provider = new Moq.Mock<ITransformationProvider>();

			IMigration migration = migrationAssembly.InstantiateMigration(2, provider.Object);

			Assert.IsNotNull(migration);
			Assert.That(migration is SecondTestMigration);
			Assert.AreSame(provider.Object, migration.Database);
		}
	}
}
