using System.Collections.Generic;
using ECM7.Migrator.Utils;

namespace ECM7.Migrator.Exceptions
{
	/// <summary>
	/// Exception thrown when a migration number is not unique.
	/// </summary>
	public class DuplicatedVersionException : VersionException
	{
		/// <summary>
		/// �������������
		/// </summary>
		/// <param name="versions">������������� ������</param>
		public DuplicatedVersionException(params long[] versions)
			: base(string.Format("Migration version #{0} is duplicated", versions.ToCommaSeparatedString()), versions)
		{
		}
	}
}
