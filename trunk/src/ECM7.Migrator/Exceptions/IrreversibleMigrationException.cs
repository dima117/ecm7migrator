using System;

namespace ECM7.Migrator.Exceptions
{
	/// <summary>
	/// Exception thrown in a migration <c>Revert()</c> method
	/// when changes can't be undone.
	/// </summary>
	public class IrreversibleMigrationException : Exception
	{
		/// <summary>
		/// Инициализация
		/// </summary>
		public IrreversibleMigrationException() : base("Irreversible migration")
		{
		}
	}
}
