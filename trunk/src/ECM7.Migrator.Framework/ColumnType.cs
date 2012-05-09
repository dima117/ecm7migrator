using System.Data;

namespace ECM7.Migrator.Framework
{
	/// <summary>
	/// Тип столбца таблицы
	/// </summary>
	public class ColumnType
	{
		public ColumnType(DbType dataType)
		{
			DataType = dataType;
		}

		public ColumnType(DbType dataType, int length)
			: this(dataType)
		{
			Length = length;
		}

		public ColumnType(DbType dataType, int length, int scale)
			: this(dataType, length)
		{
			Scale = scale;
		}

		/// <summary>
		/// Тип данных
		/// </summary>
		public DbType DataType { get; set; }

		/// <summary>
		/// Размер
		/// </summary>
		public int? Length { get; set; }

		/// <summary>
		/// Точность
		/// </summary>
		public int? Scale { get; set; }

		/// <summary>
		/// Приведение типов DbType -> ColumnType
		/// </summary>
		/// <param name="type">Тип колонки, заданный в виде DbType</param>
		/// <returns>Объект ColumnType, соответствующий заданному типу DbType</returns>
		public static implicit operator ColumnType(DbType type)
		{
			return new ColumnType(type);
		}

		public override string ToString()
		{
			string length = (Length.HasValue && Scale.HasValue) 
				? string.Format("({0}, {1})", Length, Scale) 
				: Length.HasValue 
					? string.Format("({0})", Length) 
					: string.Empty;

			return string.Format("{0}{1}", DataType, length);
		}
	}
}
