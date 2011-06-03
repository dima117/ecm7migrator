#region License

//The contents of this file are subject to the Mozilla Public License
//Version 1.1 (the "License"); you may not use this file except in
//compliance with the License. You may obtain a copy of the License at
//http://www.mozilla.org/MPL/
//Software distributed under the License is distributed on an "AS IS"
//basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//License for the specific language governing rights and limitations
//under the License.

#endregion

using System;
using System.Collections.Generic;
using System.Data;

namespace ECM7.Migrator.Framework.SchemaBuilder
{
	[Obsolete]
	public class SchemaBuilder : IColumnOptions, IForeignKeyOptions, IDeleteTableOptions
	{
		private string currentTable;
		private IFluentColumn currentColumn;
		private readonly IList<ISchemaBuilderExpression> exprs;

		public SchemaBuilder()
		{
			exprs = new List<ISchemaBuilderExpression>();
		}

		public IEnumerable<ISchemaBuilderExpression> Expressions
		{
			get { return exprs; }
		}

		/// <summary>
		/// Adds a Table to be created to the Schema
		/// </summary>
		/// <param name="name">Table name to be created</param>
		/// <returns>SchemaBuilder for chaining</returns>
		public SchemaBuilder AddTable(string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			exprs.Add(new AddTableExpression(name));
			currentTable = name;

			return this;
		}

		public IDeleteTableOptions DeleteTable(string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");
			currentTable = "";
			currentColumn = null;

			exprs.Add(new DeleteTableExpression(name));

			return this;
		}

		/// <summary>
		/// Reference an existing table.
		/// </summary>
		/// <param name="newName">Table to reference</param>
		/// <returns>SchemaBuilder for chaining</returns>
		public SchemaBuilder RenameTable(string newName)
		{
			if (string.IsNullOrEmpty(newName))
				throw new ArgumentNullException("newName");

			exprs.Add(new RenameTableExpression(currentTable, newName));
			currentTable = newName;

			return this;
		}

		/// <summary>
		/// Reference an existing table.
		/// </summary>
		/// <param name="name">Table to reference</param>
		/// <returns>SchemaBuilder for chaining</returns>
		public SchemaBuilder WithTable(string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			currentTable = name;

			return this;
		}

		/// <summary>
		/// Adds a Column to be created
		/// </summary>
		/// <param name="name">Column name to be added</param>
		/// <returns>IColumnOptions to restrict chaining</returns>
		public IColumnOptions AddColumn(string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");
			if (string.IsNullOrEmpty(currentTable))
				throw new ArgumentException("missing referenced table");

			IFluentColumn column = new FluentColumn(name);
			currentColumn = column;

			exprs.Add(new AddColumnExpression(currentTable, column));
			return this;
		}

		public SchemaBuilder OfType(DbType columnType)
		{
			currentColumn.ColumnType.DataType = columnType;

			return this;
		}

		public SchemaBuilder WithProperty(ColumnProperty columnProperty)
		{
			currentColumn.ColumnProperty = columnProperty;

			return this;
		}

		public SchemaBuilder WithSize(int size)
		{
			if (size == 0)
				throw new ArgumentNullException("size", "Size must be greater than zero");

			currentColumn.ColumnType.Length = size;

			return this;
		}

		public SchemaBuilder WithPrecision(int precision)
		{
			if (precision < 0)
				throw new ArgumentNullException("precision", "Size must be greater or equal than zero");

			currentColumn.ColumnType.Scale = precision;

			return this;
		}

		public SchemaBuilder WithDefaultValue(object defaultValue)
		{
			if (defaultValue == null)
				throw new ArgumentNullException("defaultValue", "DefaultValue cannot be null or empty");

			currentColumn.DefaultValue = defaultValue;

			return this;
		}

		public IForeignKeyOptions AsForeignKey()
		{
			currentColumn.ColumnProperty = ColumnProperty.ForeignKey;

			return this;
		}

		public SchemaBuilder ReferencedTo(string primaryKeyTable, string primaryKeyColumn)
		{
			currentColumn.Constraint = ForeignKeyConstraint.NoAction;
			currentColumn.ForeignKey = new ForeignKey(primaryKeyTable, primaryKeyColumn);
			return this;
		}

		public SchemaBuilder WithConstraint(ForeignKeyConstraint action)
		{
			currentColumn.Constraint = action;

			return this;
		}
	}
}