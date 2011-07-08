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
		private Migrator migrator;

		protected abstract TransformationProvider TransformationProvider { get; }
		protected abstract string ConnectionString { get; }
		protected abstract Assembly MigrationAssembly { get; }

		[SetUp]
		public void SetUp()
		{
			this.migrator = new Migrator(TransformationProvider, MigrationAssembly, null);

			Assert.IsTrue(this.migrator.AvailableMigrations.Count > 0, "No migrations in assembly " + MigrationAssembly.Location);

			this.migrator.Migrate(0);
		}

		[TearDown]
		public void TearDown()
		{
			this.migrator.Migrate(0);
		}

		[Test]
		public void Up()
		{
			this.migrator.Migrate();
		}

		[Test]
		public void Down()
		{
			this.migrator.Migrate();
			this.migrator.Migrate(0);
		}
	}
}