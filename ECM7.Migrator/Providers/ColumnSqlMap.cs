namespace ECM7.Migrator.Providers
{
	/// <summary>
	/// Отображение объекта Column в команды языка SQL
	/// </summary>
	public class ColumnSqlMap
	{
		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="columnSql">SQL для колонки</param>
		/// <param name="indexSql">SQL для индекса</param>
		public ColumnSqlMap(string columnSql, string indexSql)
		{
			ColumnSql = columnSql;
			IndexSql = indexSql;
		}

		/// <summary>
		/// SQL для индекса
		/// </summary>
		public string IndexSql { get; protected set; }

		/// <summary>
		/// SQL для колонки
		/// </summary>
		public string ColumnSql { get; protected set; }
	}
}
