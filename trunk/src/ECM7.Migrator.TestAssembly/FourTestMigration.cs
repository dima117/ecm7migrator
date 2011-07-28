namespace ECM7.Migrator.TestAssembly
{
	using ECM7.Migrator.Framework;

	/// <summary>
	/// Тестовая миграция 4
	/// </summary>
	[Migration(4)]
	public class FourTestMigration : Migration
	{
		/// <summary>
		/// Defines tranformations to port the database to the current version.
		/// </summary>
		public override void Up()
		{
		}

		/// <summary>
		/// Defines transformations to revert things done in <c>Up</c>.
		/// </summary>
		public override void Down()
		{
		}
	}
}
