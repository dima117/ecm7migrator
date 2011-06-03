namespace ECM7.Migrator.Tests2
{
	using System;
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
		/// �������� �������� ��������
		/// </summary>
		[Test]
		public void CanLoadMigrationsByKey()
		{
			Assembly assembly = Assembly.Load("ECM7.Migrator.TestAssembly");
			MigrationLoader loader = new MigrationLoader("test-key111", null, assembly);

			IList<long> list = loader.MigrationsTypes.Select(x => x.Version).ToList();
			Assert.AreEqual(2, list.Count);
			Assert.IsTrue(list.Contains(1));
			Assert.IsTrue(list.Contains(2));
		}

		// todo: ��������� �������� ������� ��������

		/// <summary>
		/// ��������, ��� �� ����������� ��������, �� ����������� � ��������� �����
		/// </summary>
		[Test]
		public void CantLoadMigrationsByInvalidKey()
		{
			Assembly assembly = Assembly.Load("ECM7.Migrator.TestAssembly");
			MigrationLoader loader = new MigrationLoader("moo-moo-moo", null, assembly);

			IList<MigrationInfo> list = loader.MigrationsTypes;
			Assert.IsTrue(list.IsEmpty());
		}

		/// <summary>
		/// ��������, ��� ��� ���������� �������� � �������� ������� ������ ������������ null
		/// </summary>
		[Test]
		public void NullIfNoMigrationForVersion()
		{
			MigrationLoader loader = new MigrationLoader(string.Empty, null);
			Mock<ITransformationProvider> provider = new Moq.Mock<ITransformationProvider>();

			Assert.IsNull(loader.GetMigration(99999999, provider.Object));
		}

		/// <summary>
		/// �������� ��������� ����������, ���� �� ������ ��������� ����
		/// </summary>
		[Test]
		public void ForNullProviderShouldThrowException()
		{
			var loader = new MigrationLoader(string.Empty, null);
			Assert.Throws<RequirementNotCompliedException>(() => loader.GetMigration(1, null));
		}

		/// <summary>
		/// �������� ������������ ����������� ��������� ��������� ������
		/// </summary>
		[Test]
		public void LastVersion()
		{
			Assembly assembly = Assembly.Load("ECM7.Migrator.TestAssembly");
			MigrationLoader loader = new MigrationLoader("test-key111", null, assembly);
			Assert.AreEqual(2, loader.LastVersion);
		}

		/// <summary>
		/// ��������, ��� ��� ���������� �������� ��������� ��������� ������ == 0
		/// (��������� �� ��������������� �����)
		/// </summary>
		[Test]
		public void LaseVersionIsZeroIfNoMigrations()
		{
			Assembly assembly = Assembly.Load("ECM7.Migrator.TestAssembly");
			MigrationLoader loader = new MigrationLoader("moo-moo-moo", null, assembly);
			Assert.AreEqual(0, loader.LastVersion);
		}

		/// <summary>
		/// �������� ����������� �� ���������� ������� ������
		/// </summary>
		[Test]
		public void CheckForDuplicatedVersion()
		{
			var versions = new long[] { 1, 2, 3, 4, 2, 4 };

			var ex = Assert.Throws<DuplicatedVersionException>(() =>
				MigrationLoader.CheckForDuplicatedVersion(versions));

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
			MigrationLoader loader = new MigrationLoader("test-key111", null, assembly);

			Mock<ITransformationProvider> provider = new Moq.Mock<ITransformationProvider>();

			IMigration migration = loader.GetMigration(2, provider.Object);

			Assert.IsNotNull(migration);
			Assert.That(migration is SecondTestMigration);
			Assert.AreSame(provider.Object, migration.Database);
		}
	}
}
