namespace ECM7.Migrator.Providers.Tests
{
	using System;
	using System.Configuration;
	using System.Data;

	using ECM7.Migrator.Framework;
	using ECM7.Migrator.Providers.MySql;

	using NUnit.Framework;

	[TestFixture, Category("MySql")]
	public class MySqlTransformationProviderTest : TransformationProviderConstraintBase
	{
		protected override string BatchSql
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

		[SetUp]
		public void SetUp()
		{
			string constr = ConfigurationManager.AppSettings["MySqlConnectionString"];
			Require.IsNotNullOrEmpty(constr, "Connection string \"MySqlConnectionString\" is not exist");

			provider = ProviderFactoryBuilder
				.CreateProviderFactory<MySqlTransformationProviderFactory>()
				.CreateProvider(constr);

			AddDefaultTable();
		}

		[TearDown]
		public override void TearDown()
		{
			DropTestTables();
		}

		[Test]
		public void AddTableWithMyISAMEngine()
		{
			provider.AddTable("Test", "MyISAM",
			                  new Column("Id", DbType.Int32, ColumnProperty.NotNull),
			                  new Column("name", DbType.String, 50));
		}

		// [Test,Ignore("MySql doesn't support check constraints")]
		public override void CanAddCheckConstraint() {}

	}
}