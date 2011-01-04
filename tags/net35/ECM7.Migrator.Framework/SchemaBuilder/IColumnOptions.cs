using System.Data;

namespace ECM7.Migrator.Framework.SchemaBuilder
{
	public interface IColumnOptions
	{
		SchemaBuilder OfType(DbType dbType);

		SchemaBuilder WithSize(int size);

		IForeignKeyOptions AsForeignKey();
	}
}