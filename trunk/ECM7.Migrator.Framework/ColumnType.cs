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

	}
}
