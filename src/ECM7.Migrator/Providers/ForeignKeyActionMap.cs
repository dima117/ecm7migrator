using System.Collections.Generic;

namespace ECM7.Migrator.Providers
{
	using System;

	using ForeignKeyConstraint = ECM7.Migrator.Framework.ForeignKeyConstraint;
	
	public class ForeignKeyActionMap : Dictionary<ForeignKeyConstraint, string>
	{
		public void RegisterSql(ForeignKeyConstraint action, string sql)
		{
			this[action] = sql;
		}

		public string GetSqlOnUpdate(ForeignKeyConstraint action)
		{
			if (ContainsKey(action) && !this[action].IsNullOrEmpty(true))
			{
				return string.Format("ON UPDATE {0}", this[action]);
			}

			return string.Empty;
		}

		public string GetSqlOnDelete(ForeignKeyConstraint action)
		{
			if (ContainsKey(action) && !this[action].IsNullOrEmpty(true))
			{
				return string.Format("ON DELETE {0}", this[action]);
			}

			return string.Empty;
		}
	}
}
