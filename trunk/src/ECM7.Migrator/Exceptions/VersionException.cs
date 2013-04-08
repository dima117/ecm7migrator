using System;
using System.Collections.Generic;
using System.Linq;

namespace ECM7.Migrator.Exceptions
{
	/// <summary>
	/// Исключение, генерируемое при наличии некорректных версий
	/// </summary>
	public class VersionException : Exception
	{
		/// <summary>
		/// Список некорректных версий
		/// </summary>
		private readonly List<long> versions = new List<long>();

		/// <summary>
		/// Список некорректных версий
		/// </summary>
		public IList<long> Versions
		{
			get { return versions; }
		}

		/// <summary>
		/// Инициализация
		/// </summary>
		/// <param name="message">Сообщение об ошибке</param>
		/// <param name="invalidVersions">Список некорректных версий</param>
		public VersionException(string message = null, params long[] invalidVersions)
			: base(message)
		{
			var list = invalidVersions == null ? new long[0] : invalidVersions.ToArray();

			if (list.Any())
			{
				versions.AddRange(list);
			}
		}
	}
}
