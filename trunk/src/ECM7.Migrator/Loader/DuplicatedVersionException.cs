namespace ECM7.Migrator.Loader
{
	using System;

	/// <summary>
	/// Exception thrown when a migration number is not unique.
	/// </summary>
	public class DuplicatedVersionException : VersionException
	{
		/// <summary>
		/// Инициализация
		/// </summary>
		/// <param name="versions">Версия БД, для которой найдена дублирующаяся миграция</param>
		public DuplicatedVersionException(params long[] versions)
			: base("Migration version #{0} is duplicated".FormatWith(versions.ToCommaSeparatedString()), versions)
		{
		}
	}
}
