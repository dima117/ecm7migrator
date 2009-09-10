using System;
using System.Data;
using System.Collections.Generic;
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

        public abstract Type TransformationProvider { get; }

        public ITransformationProvider NewProviderForDialect(string connectionString)
        {
            return (ITransformationProvider) Activator.CreateInstance(TransformationProvider, this, connectionString);
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
        /// Subclasses register a typename for the given type code and maximum
        /// column length. <c>$l</c> in the type name will be replaced by the column
        /// length (if appropriate)
        /// </summary>
        /// <param name="code">The typecode</param>
        /// <param name="capacity">Maximum length of database type</param>
        /// <param name="name">The database type name</param>
        protected void RegisterColumnType(DbType code, int capacity, string name)
        {
            typeNames.Put(code, capacity, name);
        }

        /// <summary>
        /// Suclasses register a typename for the given type code. <c>$l</c> in the 
        /// typename will be replaced by the column length (if appropriate).
        /// </summary>
        /// <param name="code">The typecode</param>
        /// <param name="name">The database type name</param>
        protected void RegisterColumnType(DbType code, string name)
        {
            typeNames.Put(code, name);
        }

        public ColumnPropertiesMapper GetColumnMapper(Column column)
        {
            string type = column.ColumnType.Length > 0 ? 
				GetTypeName(column.ColumnType) : 
				GetTypeName(column.ColumnType.DataType);
            if (! IdentityNeedsType && column.IsIdentity)
                type = String.Empty;
            
            return new ColumnPropertiesMapper(this, type);
        }
		
		#region GetTypeName

		/// <summary>
		/// Get the name of the database type associated with the given 
		/// </summary>
		/// <param name="type">The DbType</param>
		/// <returns>The database type name used by ddl.</returns>
		protected virtual string GetTypeName(DbType type)
		{
			return typeNames.Get(type);
		}

		/// <summary>
		/// Get the name of the database type associated with the given 
		/// </summary>
		/// <param name="type">The DbType</param>
		/// <returns>The database type name used by ddl.</returns>
		protected virtual string GetTypeName(ColumnType type)
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
		protected virtual string GetTypeName(DbType type, int? length, int? scale)
		{
			return typeNames.Get(type, length, scale);
		}

		#endregion        
		
        public void RegisterProperty(ColumnProperty property, string sql)
        {
            if (! propertyMap.ContainsKey(property))
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

        public virtual bool ColumnNameNeedsQuote
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
        
        public virtual string QuoteTemplate
        {
            get { return "\"{0}\""; }
        }
        
        public virtual string Default(object defaultValue)
        {
            return String.Format("DEFAULT {0}", defaultValue);
        }
        
        public ColumnPropertiesMapper GetAndMapColumnProperties(Column column)
        {
            ColumnPropertiesMapper mapper = GetColumnMapper(column);
            mapper.MapColumnProperties(column);
            if (column.DefaultValue != null)
                mapper.Default = column.DefaultValue;
            return mapper;
        }
    }
}