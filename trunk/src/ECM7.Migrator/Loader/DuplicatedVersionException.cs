namespace ECM7.Migrator.Loader
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Exception thrown when a migration number is not unique.
	/// </summary>
	public class DuplicatedVersionException : VersionException
	{
		/// <summary>
		/// �������������
		/// </summary>
		/// <param name="versions">������������� ������</param>
		public DuplicatedVersionException(params long[] versions) : this(versions.ToList())
		{
		}

		/// <summary>
		/// �������������
		/// </summary>
		/// <param name="versions">������������� ������</param>
		public DuplicatedVersionException(IEnumerable<long> versions)
			: base("Migration version #{0} is duplicated".FormatWith(versions.ToCommaSeparatedString()), versions)
		{
		}
	}
}
