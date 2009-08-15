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

using System.Data;
using System;

namespace Migrator.Framework
{
	/// <summary>
	/// Represents a table column.
	/// </summary>
	public class Column : IColumn
	{
		#region constructors

		protected Column()
		{
			ColumnType = new ColumnType(default(DbType));
		}

		public Column(string name)
			: this()
		{
			Name = name;
		}

		public Column(string name, DbType type)
			: this()
		{
			Name = name;
			ColumnType.DataType = type;
		}

		public Column(string name, DbType type, int size)
			: this()
		{
			Name = name;
			ColumnType.DataType = type;
			ColumnType.Length = size;
		}

		public Column(string name, DbType type, object defaultValue)
			: this(name, type)
		{
			DefaultValue = defaultValue;
		}

		public Column(string name, DbType type, ColumnProperty property)
			: this(name, type)
		{
			ColumnProperty = property;
		}

		public Column(string name, DbType type, int size, ColumnProperty property)
			: this(name, type, size)
		{
			ColumnProperty = property;
		}

		public Column(string name, DbType type, int size, ColumnProperty property, object defaultValue)
			: this(name, type, size)
		{
			ColumnProperty = property;
			DefaultValue = defaultValue;
		}

		public Column(string name, DbType type, ColumnProperty property, object defaultValue)
			: this(name, type)
		{
			ColumnProperty = property;
			DefaultValue = defaultValue;
		}

		#region constructors with ColumnType parameter

		public Column(string name, ColumnType type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			Name = name;
			ColumnType = type;
		}

		public Column(string name, ColumnType type, ColumnProperty property)
			: this(name, type)
		{
			ColumnProperty = property;
		}

		public Column(string name, ColumnType type, object defaultValue)
			: this(name, type)
		{
			DefaultValue = defaultValue;
		}

		public Column(string name, ColumnType type, ColumnProperty property, object defaultValue)
			: this(name, type)
		{
			ColumnProperty = property;
			DefaultValue = defaultValue;
		}


		#endregion

		#endregion

		#region properties

		public string Name { get; set; }

		public void SetColumnType(ColumnType type)
		{
			if (type == null)
				throw new ArgumentNullException("type");
			ColumnType = type;
		}

		public ColumnType ColumnType { get; protected set; }

		public ColumnProperty ColumnProperty { get; set; }

		public object DefaultValue { get; set; }

		public bool IsIdentity
		{
			get { return (ColumnProperty & ColumnProperty.Identity) == ColumnProperty.Identity; }
		}

		public bool IsPrimaryKey
		{
			get { return (ColumnProperty & ColumnProperty.PrimaryKey) == ColumnProperty.PrimaryKey; }
		}

		#endregion
	}
}
