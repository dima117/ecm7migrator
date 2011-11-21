using System;

namespace ECM7.Migrator.Exceptions
{
	public class SQLException : Exception
	{
		public SQLException(Exception innerException)
			: base(innerException.Message, innerException)
		{
		}
	}
}
