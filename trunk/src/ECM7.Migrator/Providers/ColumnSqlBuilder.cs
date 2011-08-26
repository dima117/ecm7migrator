namespace ECM7.Migrator.Providers
{
	using System;
	using System.Collections.Generic;

	using ECM7.Migrator.Framework;

	public class ColumnSqlBuilder
	{
		protected readonly List<string> vals = new List<string>();

		protected readonly TypeMap typeMap;
		protected readonly PropertyMap propertyMap;
		protected readonly Column column;

		public ColumnSqlBuilder(Column column, TypeMap typeMap, PropertyMap propertyMap)
		{
			Require.IsNotNull(column, "Не задан столбец таблицы для построения SQL-выражения");
			Require.IsNotNull(typeMap, "Не задан мэппинг типов данных");
			Require.IsNotNull(propertyMap, "Не задан мэппинг свойств столбца таблицы");

			this.column = column;
			this.typeMap = typeMap;
			this.propertyMap = propertyMap;
		}

		#region добавление элементов SQL-выражения для колонки

		public void AddColumnName(string namesQuoteTemplate)
		{
			var columnName = namesQuoteTemplate.FormatWith(column.Name);
			vals.Add(columnName);
		}

		public void AddColumnType(bool identityNeedsType)
		{
			string type = column.IsIdentity && !identityNeedsType
				? string.Empty
				: typeMap.Get(column.ColumnType);

			if (!type.IsNullOrEmpty())
				vals.Add(type);
		}

		public void AddSqlForIdentityWhichNotNeedsType(bool identityNeedsType)
		{
			if (!identityNeedsType)
			{
				propertyMap.AddValueIfSelected(column, ColumnProperty.Identity, vals);
			}
		}

		public void AddUnsignedSql()
		{
			propertyMap.AddValueIfSelected(column, ColumnProperty.Unsigned, vals);
		}

		public void AddNotNullSql(bool needsNotNullForIdentity)
		{
			if (!column.ColumnProperty.HasProperty(ColumnProperty.PrimaryKey) || needsNotNullForIdentity)
			{
				propertyMap.AddValueIfSelected(column, ColumnProperty.NotNull, vals);
			}
		}

		public void AddPrimaryKeySql(bool compoundPrimaryKey)
		{
			if (!compoundPrimaryKey)
			{
				propertyMap.AddValueIfSelected(column, ColumnProperty.PrimaryKey, vals);
			}
		}

		public void AddSqlForIdentityWhichNeedsType(bool identityNeedsType)
		{
			if (identityNeedsType)
			{
				propertyMap.AddValueIfSelected(column, ColumnProperty.Identity, vals);
			}
		}

		public void AddUniqueSql()
		{
			propertyMap.AddValueIfSelected(column, ColumnProperty.Unique, vals);
		}

		public void AddDefaultValueSql(Func<object, string> defaultValueMapper)
		{
			if (column.DefaultValue != null)
			{
				string defaultValueSql = defaultValueMapper(this.column.DefaultValue);

				vals.Add(defaultValueSql);
			}
		}

		#endregion

		public void Clear()
		{
			vals.Clear();
		}

		public override string ToString()
		{
			string columnSql = string.Join(" ", vals.ToArray());

			return columnSql;
		}
	}
}
