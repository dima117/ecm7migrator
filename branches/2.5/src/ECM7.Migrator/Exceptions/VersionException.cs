using System;
using System.Collections.Generic;
using ECM7.Common.Utils.Exceptions;

namespace ECM7.Migrator.Exceptions
{
	/// <summary>
	/// Исключение, генерируемое при наличии некорректных версий
	/// </summary>
	public class VersionException : RequirementNotCompliedException
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
			get
			{
				return versions;
			}
		}

		/// <summary>
		/// Инициализация
		/// </summary>
		/// <param name="message">Сообщение об ошибке</param>
		/// <param name="versionses">Список некорректных версий</param>
		public VersionException(string message = null, IEnumerable<long> versionses = null)
			: base(message)
		{
			if (versionses != null && !versionses.IsEmpty())
			{
				versions.AddRange(versionses);
			}
		}
	}
}
