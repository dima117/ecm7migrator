namespace ECM7.Migrator.Providers.Tests
{
	public class MySqlTransformationProviderTest
	{
		protected string BatchSql
		{
			get
			{
				return @"
				insert into `TestTwo` (`Id`, `TestId`) values (11, 111);
				insert into `TestTwo` (`Id`, `TestId`) values (22, 222);
				insert into `TestTwo` (`Id`, `TestId`) values (33, 333);
				insert into `TestTwo` (`Id`, `TestId`) values (44, 444);
				insert into `TestTwo` (`Id`, `TestId`) values (55, 555);
				";
			}
		}

		public void AddTableWithMyISAMEngine()
		{
			//provider.AddTable("Test", "MyISAM",
			//                  new Column("Id", DbType.Int32, ColumnProperty.NotNull),
			//                  new Column("name", DbType.String, 50));
		}
	}
}