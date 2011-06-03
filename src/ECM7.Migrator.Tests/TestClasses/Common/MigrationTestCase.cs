using System.Reflection;
using ECM7.Migrator.Providers;
using NUnit.Framework;

namespace ECM7.Migrator.Tests.TestClasses.Common
{
	/// <summary>
	/// Extend this classe to test your migrations
	/// </summary>
	public abstract class MigrationsTestCase
	{
		private Migrator _migrator;

		protected abstract TransformationProvider TransformationProvider { get; }
		protected abstract string ConnectionString { get; }
		protected abstract Assembly MigrationAssembly { get; }

		[SetUp]
		public void SetUp()
		{
			_migrator = new Migrator(TransformationProvider, string.Empty, null, MigrationAssembly);

			Assert.IsTrue(_migrator.AvailableMigrations.Count > 0, "No migrations in assembly " + MigrationAssembly.Location);

			_migrator.Migrate(0);
		}

		[TearDown]
		public void TearDown()
		{
			_migrator.Migrate(0);
		}

		[Test]
		public void Up()
		{
			_migrator.Migrate();
		}

		[Test]
		public void Down()
		{
			_migrator.Migrate();
			_migrator.Migrate(0);
		}
	}
}