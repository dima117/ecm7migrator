using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ECM7.Migrator.Framework;
using System.Data;

namespace ECM7.Migrator.Compatibility
{
	class UpdateSchemaInfo
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
			provider.AddUniqueConstraint("UC_SchemaInfo", "SchemaInfo", "Version", "[Key]");
		}
	}
}
