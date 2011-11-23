﻿namespace ECM7.Migrator.Framework
{
	using System;

	public class SchemaQualifiedObjectName
	{
		/// <summary>
		/// Название объекта
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Название схемы БД
		/// </summary>
		public string Schema { get; set; }

		/// <summary>
		/// Приведение типов string -> SchemaQualifiedObjectName
		/// </summary>
		public static implicit operator SchemaQualifiedObjectName(string name)
		{
			return new SchemaQualifiedObjectName { Name = name };
		}

		public override string ToString()
		{
			return Schema.IsNullOrEmpty(true) ? Name : "{0}.{1}".FormatWith(Schema, Name);
		}

		#region Equals & GetHashCode

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof(SchemaQualifiedObjectName)) return false;
			return Equals((SchemaQualifiedObjectName)obj);
		}

		public bool Equals(SchemaQualifiedObjectName other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other.Name, Name) && Equals(other.Schema, Schema);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (Schema != null ? Schema.GetHashCode() : 0);
			}
		}

		#endregion
	}
}