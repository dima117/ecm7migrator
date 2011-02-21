namespace ECM7.Migrator
{
	using System;

	/// <summary>
	/// Exception thrown in a migration <c>Down()</c> method
	/// when changes can't be undone.
	/// </summary>
	public class IrreversibleMigrationException : Exception
	{
		/// <summary>
		/// �������������
		/// </summary>
		public IrreversibleMigrationException() : base("Irreversible migration")
		{
		}
	}
}
