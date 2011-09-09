namespace ECM7.Migrator.Providers.Tests.ImplementationTest
{
	using ECM7.Migrator.Providers.SQLite;

	using NUnit.Framework;

	[TestFixture]
	public class SQLiteTransformationProviderTest
		: TransformationProviderTestBase<SQLiteTransformationProvider>
	{
		#region Overrides of TransformationProviderTestBase<SQLiteTransformationProvider>

		public override string ConnectionStrinSettingsName
		{
			get { return "SQLiteConnectionString"; }
		}

		protected override string BatchSql
		{
			get
			{
				return @"
				insert into [BatchSqlTest] ([Id], [TestId]) values (11, 111)
				GO
				insert into [BatchSqlTest] ([Id], [TestId]) values (22, 222)
				GO
				insert into [BatchSqlTest] ([Id], [TestId]) values (33, 333)
				GO
				insert into [BatchSqlTest] ([Id], [TestId]) values (44, 444)
				GO
				go
				insert into [BatchSqlTest] ([Id], [TestId]) values (55, 555)
				";
			}
		}

		#endregion
	}
}
