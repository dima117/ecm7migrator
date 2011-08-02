using System;
using System.Collections.Generic;

namespace ECM7.Migrator.Providers
{
	using System.Data;

	using ECM7.Migrator.Framework;

	using ForeignKeyConstraint = ECM7.Migrator.Framework.ForeignKeyConstraint;

	public abstract class SqlGenerator
	{
		protected readonly IFormatProvider sqlFormatProvider;
		private readonly Dictionary<ColumnProperty, string> propertyMap = new Dictionary<ColumnProperty, string>();
		private readonly TypeNames typeNames = new TypeNames();

		protected SqlGenerator()
		{
			sqlFormatProvider = new SqlFormatter(obj => QuoteName(obj.ToString()));
		}


		#region Экранирование зарезервированных слов в идентификаторах


		/// <summary>
		/// Обертывание идентификаторов в кавычки
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public virtual string QuoteName(string name)
		{
			return String.Format(NamesQuoteTemplate, name);
		}

		public string FormatSql(string format, params object[] args)
		{
			return string.Format(sqlFormatProvider, format, args);
		}

		#endregion

		public virtual string Default(object defaultValue)
		{
			return String.Format("DEFAULT {0}", defaultValue);
		}

		public string SqlForConstraint(ForeignKeyConstraint constraint)
		{
			switch (constraint)
			{
				case ForeignKeyConstraint.Cascade:
					return "CASCADE";
				case ForeignKeyConstraint.Restrict:
					return "RESTRICT";
				case ForeignKeyConstraint.SetDefault:
					return "SET DEFAULT";
				case ForeignKeyConstraint.SetNull:
					return "SET NULL";
				default:
					return "NO ACTION";
			}
		}


		#region мэппинг типов

		/// <summary>
		/// Проверка, что заданный тип зарегистрирован
		/// </summary>
		/// <param name="type">Проверяемый тип</param>
		/// <returns>Возвращает true, если заданный тип зарегистрирован, иначе возвращает false.</returns>
		public bool TypeIsSupported(DbType type)
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
		/// </summary>
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

		#endregion

		#region Свойства колонок

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

		protected void AddValueIfSelected(Column column, ColumnProperty property, ICollection<string> vals)
		{
			if (column.ColumnProperty.HasProperty(property))
			{
				vals.Add(SqlForProperty(property));
			}
		}

		#endregion

		#region Генерация SQL для колонок

		public virtual string GetColumnSql(Column column, bool compoundPrimaryKey)
		{
			Require.IsNotNull(column, "Не задан обрабатываемый столбец");

			List<string> vals = new List<string>();
			BuildColumnSql(vals, column, compoundPrimaryKey);
			string columnSql = String.Join(" ", vals.ToArray());

			return columnSql;
		}

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

		// добавление элементов команды SQL для колонки

		protected void AddColumnName(List<string> vals, Column column)
		{
			var columnName = QuoteName(column.Name);
			vals.Add(columnName);
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
			if (!IdentityNeedsType)
			{
				AddValueIfSelected(column, ColumnProperty.Identity, vals);
			}
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
			if (IdentityNeedsType)
			{
				AddValueIfSelected(column, ColumnProperty.Identity, vals);
			}
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

		#region Особенности СУБД

		public abstract bool IdentityNeedsType { get; }

		public abstract bool NeedsNotNullForIdentity { get; }

		public abstract bool SupportsIndex { get; }

		/// <summary>
		/// Шаблон кавычек для идентификаторов
		/// </summary>
		public abstract string NamesQuoteTemplate { get; }

		/// <summary>
		/// Разделитель для пакетов запросов
		/// </summary>
		public abstract string BatchSeparator { get; }

		#endregion
	}
}
