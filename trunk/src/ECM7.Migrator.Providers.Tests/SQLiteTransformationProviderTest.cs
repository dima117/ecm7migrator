namespace ECM7.Migrator.Providers.Tests
{
	public class SQLiteTransformationProviderTest
	{
		public string ConnectionStrinSettingsName
		{
			get { return "SQLiteConnectionString"; }
		}

		protected string BatchSql
		{
			get
			{
				return @"
				insert into [TestTwo] ([Id], [TestId]) values (11, 111)
				GO
				insert into [TestTwo] ([Id], [TestId]) values (22, 222)
				GO
				insert into [TestTwo] ([Id], [TestId]) values (33, 333)
				GO
				insert into [TestTwo] ([Id], [TestId]) values (44, 444)
				GO
				go
				insert into [TestTwo] ([Id], [TestId]) values (55, 555)
				";
			}
		}
	}
}