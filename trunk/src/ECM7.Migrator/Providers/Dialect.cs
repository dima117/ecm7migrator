using System;
using System.Collections.Generic;
using System.Data;

using ECM7.Migrator.Framework;

namespace ECM7.Migrator.Providers
{
	/// <summary>
	/// Определяет реализацию деталей, специфических для конкретной СУБД.
	/// </summary>
	public abstract class Dialect
	{
		private readonly Dictionary<ColumnProperty, string> propertyMap = new Dictionary<ColumnProperty, string>();
		private readonly TypeNames typeNames = new TypeNames();

		protected Dialect()
		{
			RegisterProperty(ColumnProperty.Null, "NULL");
			RegisterProperty(ColumnProperty.NotNull, "NOT NULL");
			RegisterProperty(ColumnProperty.Unique, "UNIQUE");
			RegisterProperty(ColumnProperty.PrimaryKey, "PRIMARY KEY");
		}

		public abstract Type TransformationProviderType { get; }

		public TransformationProvider NewProviderForDialect(string connectionString)
		{
			// TODO: пройти по провайдерам и удалить из конструктора ключ
			return Activator.CreateInstance(TransformationProviderType, this, connectionString) as TransformationProvider;
		}

		/// <summary>
		/// Проверка, что заданный тип зарегистрирован
		/// </summary>
		/// <param name="type">Проверяемый тип</param>
		/// <returns>Возвращает true, если заданный тип зарегистрирован, иначе возвращает false.</returns>
		public bool TypeIsRegistred(DbType type)
		{
			return typeNames.HasType(type);
		}


		/// <summary>
		/// Регистрирует название типа БД, которое будет использовано для
		/// конкретного значения DbType, указанного в "миграциях".
		/// <para><c>$l</c> - будет заменено на конкретное значение длины</para>
		/// <para><c>$s</c> - будет заменено на конкретное значение, показывающее 
		/// количество знаков после запятой для вещественных чисел</para>м
		/// </summary>
		/// <param name="code">The typecode</param>
		/// <param name="capacity">Maximum length of database type</param>
		/// <param name="name">The database type name</param>
		protected void RegisterColumnType(DbType code, int capacity, string name)
		{
			typeNames.Put(code, capacity, name);
		}

		/// <summary>
		/// Регистрирует название типа БД, которое будет использовано для
		/// конкретного значения DbType, указанного в "миграциях".
		/// <para><c>$l</c> - будет заменено на конкретное значение длины</para>
		/// <para><c>$s</c> - будет заменено на конкретное значение, показывающее 
		/// количество знаков после запятой для вещественных чисел</para>
		/// </summary>
		/// <param name="code">Тип</param>
		/// <param name="capacity">Максимальная длина</param>
		/// <param name="name">Название типа БД</param>
		/// <param name="defaultScale">Значение по-умолчанию: количество знаков после запятой для вещественных чисел</param>
		protected void RegisterColumnType(DbType code, int capacity, string name, int defaultScale)
		{
			typeNames.Put(code, capacity, name, defaultScale);
		}


		/// <summary>
		/// Регистрирует название типа БД, которое будет использовано для
		/// конкретного значения DbType, указанного в "миграциях".
		/// <para><c>$l</c> - будет заменено на конкретное значение длины</para>
		/// <para><c>$s</c> - будет заменено на конкретное значение, показывающее 
		/// количество знаков после запятой для вещественных чисел</para>
		/// <param name="code">Тип</param>
		/// <param name="name">Название типа БД</param>
		protected void RegisterColumnType(DbType code, string name)
		{
			typeNames.Put(code, name);
		}

		#region GetTypeName

		/// <summary>
		/// Get the name of the database type associated with the given 
		/// </summary>
		/// <param name="type">The DbType</param>
		/// <returns>The database type name used by ddl.</returns>
		public virtual string GetTypeName(DbType type)
		{
			return typeNames.Get(type);
		}

		/// <summary>
		/// Get the name of the database type associated with the given 
		/// </summary>
		/// <param name="type">The DbType</param>
		/// <returns>The database type name used by ddl.</returns>
		public virtual string GetTypeName(ColumnType type)
		{
			return typeNames.Get(type);
		}

		/// <summary>
		/// Get the name of the database type associated with the given 
		/// </summary>
		/// <param name="type">The DbType</param>
		/// <returns>The database type name used by ddl.</returns>
		/// <param name="length"></param>
		/// <param name="scale"></param>
		public virtual string GetTypeName(DbType type, int? length, int? scale)
		{
			return typeNames.Get(type, length, scale);
		}

		#endregion

		public void RegisterProperty(ColumnProperty property, string sql)
		{
			if (!propertyMap.ContainsKey(property))
			{
				propertyMap.Add(property, sql);
			}
			propertyMap[property] = sql;
		}

		public string SqlForProperty(ColumnProperty property)
		{
			if (propertyMap.ContainsKey(property))
			{
				return propertyMap[property];
			}
			return String.Empty;
		}

		public virtual bool NamesNeedsQuote
		{
			get { return false; }
		}

		public virtual bool TableNameNeedsQuote
		{
			get { return false; }
		}

		public virtual bool ConstraintNameNeedsQuote
		{
			get { return false; }
		}

		public virtual bool IdentityNeedsType
		{
			get { return true; }
		}

		public virtual bool NeedsNotNullForIdentity
		{
			get { return true; }
		}

		public virtual bool SupportsIndex
		{
			get { return true; }
		}

		public virtual string Quote(string value)
		{
			return String.Format(QuoteTemplate, value);
		}

		public virtual string QuoteIfNeeded(string columnName)
		{
			return NamesNeedsQuote
				? String.Format(QuoteTemplate, columnName)
				: columnName;
		}

		public virtual string QuoteTemplate
		{
			get { return "\"{0}\""; }
		}

		public virtual string Default(object defaultValue)
		{
			return String.Format("DEFAULT {0}", defaultValue);
		}

		public virtual string GetColumnSql(Column column, bool compoundPrimaryKey)
		{
			Require.IsNotNull(column, "Не задан обрабатываемый столбец");

			List<string> vals = new List<string>();
			BuildColumnSql(vals, column, compoundPrimaryKey);
			string columnSql = String.Join(" ", vals.ToArray());

			return columnSql;
		}


		#region Генерация SQL для колонок

		protected virtual void BuildColumnSql(List<string> vals, Column column, bool compoundPrimaryKey)
		{
			AddColumnName(vals, column);
			AddColumnType(vals, column);
			// identity не нуждается в типе
			AddSqlForIdentityWhichNotNeedsType(vals, column);
			AddUnsignedSql(vals, column);
			AddNotNullSql(vals, column);
			AddPrimaryKeySql(vals, column, compoundPrimaryKey);
			// identity нуждается в типе
			AddSqlForIdentityWhichNeedsType(vals, column);
			AddUniqueSql(vals, column);
			AddForeignKeySql(vals, column);
			AddDefaultValueSql(vals, column);
		}

		#region добавление элементов команды SQL для колонки

		protected void AddColumnName(List<string> vals, Column column)
		{
			vals.Add(NamesNeedsQuote ? Quote(column.Name) : column.Name);
		}

		protected void AddColumnType(List<string> vals, Column column)
		{
			string type = !IdentityNeedsType && column.IsIdentity
				? String.Empty : GetTypeName(column.ColumnType);

			if (!type.IsNullOrEmpty())
				vals.Add(type);
		}

		protected void AddSqlForIdentityWhichNotNeedsType(List<string> vals, Column column)
		{
			if (!IdentityNeedsType)// todo: исправить как унаследованные мэпперы
				AddValueIfSelected(column, ColumnProperty.Identity, vals);
		}

		protected void AddUnsignedSql(List<string> vals, Column column)
		{
			AddValueIfSelected(column, ColumnProperty.Unsigned, vals);
		}

		protected void AddNotNullSql(List<string> vals, Column column)
		{
			if (!column.ColumnProperty.HasProperty(ColumnProperty.PrimaryKey) || NeedsNotNullForIdentity)
				AddValueIfSelected(column, ColumnProperty.NotNull, vals);
		}

		protected void AddPrimaryKeySql(List<string> vals, Column column, bool compoundPrimaryKey)
		{
			if (!compoundPrimaryKey)
				AddValueIfSelected(column, ColumnProperty.PrimaryKey, vals);
		}

		protected void AddSqlForIdentityWhichNeedsType(List<string> vals, Column column)
		{
			if (IdentityNeedsType)// todo: исправить как унаследованные мэпперы
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

		protected void AddDefaultValueSql(List<string> vals, Column column)
		{
			if (column.DefaultValue != null)
				vals.Add(Default(column.DefaultValue));
		}

		#endregion

		#region Helpers

		protected void AddValueIfSelected(Column column, ColumnProperty property, ICollection<string> vals)
		{
			if (column.ColumnProperty.HasProperty(property))
				vals.Add(SqlForProperty(property));
		}

		#endregion

		#endregion

	}
}