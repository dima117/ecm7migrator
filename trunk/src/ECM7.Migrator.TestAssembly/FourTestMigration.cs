namespace ECM7.Migrator.TestAssembly
{
	using ECM7.Migrator.Framework;

	/// <summary>
	/// Тестовая миграция 4 (выполняется без транзакции)
	/// </summary>
	[Migration(4, WithoutTransaction = true)]
	public class FourTestMigration : Migration
	{
		/// <summary>
		/// Defines tranformations to port the database to the current version.
		/// </summary>
		public override void Apply()
		{
			Database.ExecuteNonQuery("up4");
		}

		/// <summary>
		/// Defines transformations to revert things done in <c>Apply</c>.
		/// </summary>
		public override void Revert()
		{
			Database.ExecuteNonQuery("down4");
		}
	}
}
