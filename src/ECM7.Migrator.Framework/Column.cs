using System;

namespace ECM7.Migrator.Framework
{
	/// <summary>
	/// Represents a table column.
	/// </summary>
	public class Column : IColumn
	{
		#region constructors

		public Column(string name, ColumnType type, ColumnProperty property = ColumnProperty.None, object defaultValue = null)
		{
			
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentNullException("name");
			}

			if (type == null)
			{
				throw new ArgumentNullException("type");
			}

			Name = name;
			ColumnType = type;
			ColumnProperty = property;
			DefaultValue = defaultValue;
		}

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
