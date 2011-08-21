namespace ECM7.Migrator.Providers.Tests
{
	using ECM7.Migrator.Providers.SQLite;

	using NUnit.Framework;

	[TestFixture, Category("SQLite")]
	public class SQLiteTransformationProviderTest : TransformationProviderBase<SQLiteTransformationProvider>
	{
		public override string ConnectionStrinSettingsName
		{
			get { return "SQLiteConnectionString"; }
		}

		public override bool UseTransaction
		{
			get { return true; }
		}

		protected override string BatchSql
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