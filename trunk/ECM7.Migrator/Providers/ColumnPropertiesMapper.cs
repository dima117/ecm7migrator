using System;
using System.Collections.Generic;
using ECM7.Migrator.Framework;

namespace ECM7.Migrator.Providers
{
	/// <summary>
	/// This is basically a just a helper base class
	/// per-database implementors may want to override ColumnSql
	/// </summary>
	/// todo: вынести методы мэппера колонок в диалект?
	/// todo: переопределить мэппинг колонок для оракла и поменять местами not null и default
	/// todo: разобраться с IdentityNeedsType
	public class ColumnPropertiesMapper
	{
		protected Dialect dialect;

		public ColumnPropertiesMapper(Dialect dialect)
		{
			this.dialect = dialect;
		}

		public ColumnSqlMap MapColumnProperties(Column column)
		{
			Require.IsNotNull(column, "Не задан обрабатываемый столбец");

			string indexSql = GetIndexSql(column);


			List<string> vals = new List<string>();
			BuildColumnSql(vals, column);
			string columnSql = String.Join(" ", vals.ToArray());

			return new ColumnSqlMap(columnSql, indexSql);
		}

		#region Генерация SQL

		protected virtual void BuildColumnSql(List<string> vals, Column column)
		{
			AddColumnName(vals, column);
			AddColumnType(vals, column);
			// identity не нуждается в типе
			AddSqlForIdentityWhichNotNeedsType(vals, column);
			AddUnsignedSql(vals, column);
			AddNotNullSql(vals, column);
			AddPrimaryKeySql(vals, column);
			// identity нуждается в типе
			AddSqlForIdentityWhichNeedsType(vals, column);
			AddUniqueSql(vals, column);
			AddForeignKeySql(vals, column);
			AddDefaultValueSql(vals, column);
		}

		protected string GetIndexSql(Column column)
		{
			bool indexed = column.ColumnProperty.HasProperty(ColumnProperty.Indexed);
			if (dialect.SupportsIndex && indexed)
				return String.Format("INDEX({0})", dialect.Quote(column.Name));
			return null;
		}


		#region добавление элементов команды SQL для колонки

		protected void AddColumnName(List<string> vals, Column column)
		{
			vals.Add(dialect.ColumnNameNeedsQuote ? dialect.Quote(column.Name) : column.Name);
		}

		protected void AddColumnType(List<string> vals, Column column)
		{
			string type = !dialect.IdentityNeedsType && column.IsIdentity
				? String.Empty : dialect.GetTypeName(column.ColumnType);

			if (!type.IsNullOrEmpty())
				vals.Add(type);
		}

		protected void AddSqlForIdentityWhichNotNeedsType(List<string> vals, Column column)
		{
			if (!dialect.IdentityNeedsType)// todo: исправить как унаследованные мэпперы
				AddValueIfSelected(column, ColumnProperty.Identity, vals);
		}

		protected void AddUnsignedSql(List<string> vals, Column column)
		{
			AddValueIfSelected(column, ColumnProperty.Unsigned, vals);
		}

		protected void AddNotNullSql(List<string> vals, Column column)
		{
			if (!column.ColumnProperty.HasProperty(ColumnProperty.PrimaryKey) || dialect.NeedsNotNullForIdentity)
				AddValueIfSelected(column, ColumnProperty.NotNull, vals);
		}

		protected void AddPrimaryKeySql(List<string> vals, Column column)
		{
			AddValueIfSelected(column, ColumnProperty.PrimaryKey, vals);
		}

		protected void AddSqlForIdentityWhichNeedsType(List<string> vals, Column column)
		{
			if (dialect.IdentityNeedsType)// todo: исправить как унаследованные мэпперы
				AddValueIfSelected(column, ColumnProperty.Identity, vals);
		}

		protected void AddForeignKeySql(List<string> vals, Column column)
		{
			AddValueIfSelected(column, ColumnProperty.ForeignKey, vals);
		}

		protected void AddUniqueSql(List<string> vals, Column column)
		{
			AddValueIfSelected(column, ColumnProperty.Unique, vals);
		}

		private void AddDefaultValueSql(List<string> vals, Column column)
		{
			if (column.DefaultValue != null)
				vals.Add(dialect.Default(column.DefaultValue));
		}

		#endregion

		#endregion

		#region Helpers

		protected void AddValueIfSelected(Column column, ColumnProperty property, ICollection<string> vals)
		{
			if (column.ColumnProperty.HasProperty(property))
				vals.Add(dialect.SqlForProperty(property));
		}

		#endregion
	}
}
