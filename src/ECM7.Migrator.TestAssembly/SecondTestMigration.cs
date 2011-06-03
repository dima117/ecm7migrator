using ECM7.Migrator.Framework;

namespace ECM7.Migrator.TestAssembly
{
	/// <summary>
	/// Тестовая миграция 2
	/// </summary>
	[Migration(2)]
	public class SecondTestMigration : IMigration
	{
		/// <summary>
		/// Название
		/// </summary>
		public string Name
		{
			get { return StringUtils.ToHumanName(GetType().Name); }
		}

		/// <summary>
		/// Defines tranformations to port the database to the current version.
		/// </summary>
		public void Up()
		{
		}

		/// <summary>
		/// Defines transformations to revert things done in <c>Up</c>.
		/// </summary>
		public void Down()
		{
		}

		/// <summary>
		/// Represents the database.
		/// <see cref="ITransformationProvider"></see>.
		/// </summary>
		/// <seealso cref="ITransformationProvider">Migration.Framework.ITransformationProvider</seealso>
		public ITransformationProvider Database { get; set; }
	}
}
