using System.Data;
using ECM7.Migrator.Framework;

namespace ECM7.Migrator.Compatibility
{
	public class UpdateSchemaInfo
	{
		/// <summary>
		/// Расширяет таблицу SchemaInfo полем AssemblyKey.
		/// Использутеся при переходе от старой версии
		///
		/// Должно быть удалено в будущем
		/// </summary>
		/// <param name="provider"></param>
		public static void Update1To3(ITransformationProvider provider)
		{
			provider.AddTable(
				"SchemaTmp",
				new Column("Version", DbType.Int64, ColumnProperty.PrimaryKey),
				new Column("AssemblyKey", DbType.String.WithSize(200), ColumnProperty.PrimaryKey, "''"));

			string sql = provider.FormatSql(
				"INSERT INTO {0:NAME} ({1:NAME}) SELECT {1:NAME} FROM {2:NAME}",
					"SchemaTmp", "Version", "SchemaInfo");

			provider.ExecuteNonQuery(sql);
			
			provider.RemoveTable("SchemaInfo");
			provider.RenameTable("SchemaTmp", "SchemaInfo");
		}

		public static void Update2To3(ITransformationProvider provider)
		{
			provider.RenameColumn("SchemaInfo", "Key", "AssemblyKey");
		}
	}
}
