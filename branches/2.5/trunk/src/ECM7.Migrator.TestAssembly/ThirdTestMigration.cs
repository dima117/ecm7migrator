using ECM7.Migrator.Framework;

namespace ECM7.Migrator.TestAssembly
{
	/// <summary>
	/// Тестовая миграция
	/// </summary>
	[Migration(3, Ignore = true)]
	public class ThirdTestMigration : Migration
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
