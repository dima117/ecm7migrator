namespace ECM7.Migrator.Loader
{
	using System;

	/// <summary>
	/// Exception thrown when a migration number is not unique.
	/// </summary>
	public class DuplicatedVersionException : Exception
	{
		/// <summary>
		/// �������������
		/// </summary>
		/// <param name="version">������ ��, ��� ������� ������� ������������� ��������</param>
		public DuplicatedVersionException(long version)
			: base(String.Format("Migration version #{0} is duplicated", version))
		{
		}
	}
}
