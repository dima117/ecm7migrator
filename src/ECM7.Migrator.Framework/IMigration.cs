namespace ECM7.Migrator.Framework
{
	/// <summary>
	/// Интерфейс миграции
	/// </summary>
	public interface IMigration
	{
		/// <summary>
		/// Название
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Represents the database.
		/// <see cref="ITransformationProvider"></see>.
		/// </summary>
		/// <seealso cref="ITransformationProvider">Migration.Framework.ITransformationProvider</seealso>
		ITransformationProvider Database { get; set; }

		/// <summary>
		/// Defines tranformations to port the database to the current version.
		/// </summary>
		void Up();

		/// <summary>
		/// Defines transformations to revert things done in <c>Up</c>.
		/// </summary>
		void Down();
	}
}
