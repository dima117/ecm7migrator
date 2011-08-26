namespace ECM7.Migrator.Providers
{
	using System.Collections.Generic;
	using ECM7.Migrator.Framework;

	public class PropertyMap : Dictionary<ColumnProperty, string>
	{
		public void RegisterProperty(ColumnProperty property, string sql)
		{
			this[property] = sql;
		}

		public string SqlForProperty(ColumnProperty property)
		{
			if (ContainsKey(property))
			{
				return this[property];
			}

			return string.Empty;
		}

		public void AddValueIfSelected(Column column, ColumnProperty property, ICollection<string> vals)
		{
			if (column.ColumnProperty.HasProperty(property))
			{
				vals.Add(SqlForProperty(property));
			}
		}
	}
}
