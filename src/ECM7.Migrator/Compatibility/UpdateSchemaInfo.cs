using System.Data;
using ECM7.Migrator.Framework;

namespace ECM7.Migrator.Compatibility
{
	public class UpdateSchemaInfo
	{
		/// <summary>
		/// Расширяет таблицу SchemaInfo полем Key.
		/// Использутеся при переходе от старой версии
		///
		/// Должно быть удалено в будущем
		/// </summary>
		/// <param name="provider"></param>
		public static void Update(ITransformationProvider provider)
		{
			provider.AddTable(
				"SchemaTmp",
				new Column("Version", DbType.Int64, ColumnProperty.NotNull),
				new Column("[Key]", DbType.String.WithSize(200), ColumnProperty.NotNull, "''"));
			provider.ExecuteNonQuery("INSERT INTO SchemaTmp (Version) SELECT Version FROM SchemaInfo");
			provider.RemoveTable("SchemaInfo");
			provider.RenameTable("SchemaTmp", "SchemaInfo");
			provider.AddPrimaryKey("PK_SchemaInfo", "SchemaInfo", "Version", "[Key]");
		}
	}
}
