namespace ECM7.Migrator.Framework
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
	}
}
