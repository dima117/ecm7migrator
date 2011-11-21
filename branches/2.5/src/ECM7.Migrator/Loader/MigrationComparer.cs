namespace ECM7.Migrator.Loader
{
	using System.Collections.Generic;

	/// <summary>
	/// Comparer of Migration by their version attribute.
	/// </summary>
	public class MigrationInfoComparer : IComparer<MigrationInfo>
	{
		/// <summary>
		/// �������, ��� ���������� ���������� �� �����������
		/// </summary>
		private readonly bool ascending;

		/// <summary>
		/// �������������
		/// </summary>
		/// <param name="ascending">������� ���������� (true = �� �����������, false = �� ��������)</param>
		public MigrationInfoComparer(bool ascending = true)
		{
			this.ascending = ascending;
		}

		/// <summary>
		/// ��������� ���� ��������
		/// </summary>
		public int Compare(MigrationInfo x, MigrationInfo y)
		{
			return ascending
				? x.Version.CompareTo(y.Version)
				: y.Version.CompareTo(x.Version);
		}
	}
}
