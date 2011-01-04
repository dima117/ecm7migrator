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
			_migrator = new Migrator(TransformationProvider, true, MigrationAssembly);
			
			Assert.IsTrue(_migrator.MigrationsTypes.Count > 0, "No migrations in assembly " + MigrationAssembly.Location);
			
			_migrator.MigrateTo(0);
		}
		
		[TearDown]
		public void TearDown()
		{
			_migrator.MigrateTo(0);
		}
		
		[Test]
		public void Up()
		{
			_migrator.MigrateToLastVersion();
		}
		
		[Test]
		public void Down()
		{
			_migrator.MigrateToLastVersion();
			_migrator.MigrateTo(0);
		}
	}
}