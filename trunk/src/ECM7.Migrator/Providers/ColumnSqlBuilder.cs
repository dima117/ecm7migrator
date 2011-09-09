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

		public ColumnSqlBuilder AddColumnName(string namesQuoteTemplate)
		{
			var columnName = namesQuoteTemplate.FormatWith(column.Name);
			vals.Add(columnName);

			return this;
		}

		public ColumnSqlBuilder AddColumnType(bool identityNeedsType)
		{
			string type = column.IsIdentity && !identityNeedsType
				? string.Empty
				: typeMap.Get(column.ColumnType);

			if (!type.IsNullOrEmpty())
			{
				vals.Add(type);
			}

			return this;
		}

		public ColumnSqlBuilder AddSqlForIdentityWhichNotNeedsType(bool identityNeedsType)
		{
			if (!identityNeedsType)
			{
				propertyMap.AddValueIfSelected(column, ColumnProperty.Identity, vals);
			}

			return this;
		}

		public ColumnSqlBuilder AddUnsignedSql()
		{
			propertyMap.AddValueIfSelected(column, ColumnProperty.Unsigned, vals);

			return this;
		}

		public ColumnSqlBuilder AddNotNullSql(bool needsNotNullForIdentity)
		{
			if (!column.ColumnProperty.HasProperty(ColumnProperty.PrimaryKey) || needsNotNullForIdentity)
			{
				propertyMap.AddValueIfSelected(column, ColumnProperty.NotNull, vals);
			}

			return this;
		}

		public ColumnSqlBuilder AddPrimaryKeySql(bool compoundPrimaryKey)
		{
			if (!compoundPrimaryKey)
			{
				propertyMap.AddValueIfSelected(column, ColumnProperty.PrimaryKey, vals);
			}

			return this;
		}

		public ColumnSqlBuilder AddSqlForIdentityWhichNeedsType(bool identityNeedsType)
		{
			if (identityNeedsType)
			{
				propertyMap.AddValueIfSelected(column, ColumnProperty.Identity, vals);
			}

			return this;
		}

		public ColumnSqlBuilder AddUniqueSql()
		{
			propertyMap.AddValueIfSelected(column, ColumnProperty.Unique, vals);

			return this;
		}

		public ColumnSqlBuilder AddDefaultValueSql(Func<object, string> defaultValueMapper)
		{
			if (column.DefaultValue != null)
			{
				string defaultValueSql = defaultValueMapper(this.column.DefaultValue);

				vals.Add(defaultValueSql);
			}

			return this;
		}

		public ColumnSqlBuilder AddRawSql(string sql)
		{
			vals.Add(sql);

			return this;
		}

		#endregion

		public ColumnSqlBuilder Clear()
		{
			vals.Clear();

			return this;
		}

		public override string ToString()
		{
			string columnSql = string.Join(" ", vals.ToArray());

			return columnSql;
		}
	}
}
