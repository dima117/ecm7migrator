using System.Data;

namespace ECM7.Migrator.Framework
{
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
		
		public ColumnType(DbType dataType, int length, int precision)
			: this(dataType, length)
		{
			Precision = precision;
		}

		public DbType DataType { get; set; }

		public int Length { get; set; }

		public int Precision { get; set; }

		public int Scale { get; set; }

	}
}
