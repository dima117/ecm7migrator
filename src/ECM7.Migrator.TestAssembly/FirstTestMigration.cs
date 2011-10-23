namespace ECM7.Migrator.TestAssembly
{
	using Framework;

	/// <summary>
	/// Тестовая миграция 1
	/// </summary>
	[Migration(1)]
	public class FirstTestMigration : Migration
	{
		/// <summary>
		/// Defines tranformations to port the database to the current version.
		/// </summary>
		public override void Apply()
		{
		}

		/// <summary>
		/// Defines transformations to revert things done in <c>Apply</c>.
		/// </summary>
		public override void Revert()
		{
		}
	}
}
