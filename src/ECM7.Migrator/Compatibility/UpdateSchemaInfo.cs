using System.Data;
using ECM7.Migrator.Framework;

namespace ECM7.Migrator.Compatibility
{
	using System;

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
				new Column("Key", DbType.String.WithSize(200), ColumnProperty.NotNull, "''"));

			string sql = "INSERT INTO {0} ({1}) SELECT {1} FROM {2}".FormatWith(
				provider.QuoteName("SchemaTmp"),
				provider.QuoteName("Version"),
				provider.QuoteName("SchemaInfo"));
			provider.ExecuteNonQuery(sql);
			
			provider.RemoveTable("SchemaInfo");
			provider.RenameTable("SchemaTmp", "SchemaInfo");
			provider.AddPrimaryKey("PK_SchemaInfo", "SchemaInfo", "Version", "Key");
		}
	}
}
