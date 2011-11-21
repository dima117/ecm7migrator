namespace ECM7.Migrator.Loader
{
	using System.Collections.Generic;

	/// <summary>
	/// Comparer of Migration by their version attribute.
	/// </summary>
	public class MigrationInfoComparer : IComparer<MigrationInfo>
	{
		/// <summary>
		/// Признак, что проводится сортировка по возрастанию
		/// </summary>
		private readonly bool ascending;

		/// <summary>
		/// Инициализация
		/// </summary>
		/// <param name="ascending">Порядок сортировки (true = по возрастанию, false = по убыванию)</param>
		public MigrationInfoComparer(bool ascending = true)
		{
			this.ascending = ascending;
		}

		/// <summary>
		/// Сравнение двух миграций
		/// </summary>
		public int Compare(MigrationInfo x, MigrationInfo y)
		{
			return ascending
				? x.Version.CompareTo(y.Version)
				: y.Version.CompareTo(x.Version);
		}
	}
}
