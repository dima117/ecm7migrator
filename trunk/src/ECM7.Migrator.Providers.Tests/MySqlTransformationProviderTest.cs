namespace ECM7.Migrator.Providers.Tests
{
	using System;
	using System.Data;

	using ECM7.Migrator.Framework;
	using ECM7.Migrator.Providers.MySql;

	using NUnit.Framework;

	[TestFixture, Category("MySql")]
	public class MySqlTransformationProviderTest : TransformationProviderConstraintBase<MySqlTransformationProvider>
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

		public override string ConnectionStrinSettingsName
		{
			get { return "MySqlConnectionString"; }
		}

		public override bool UseTransaction
		{
			get { return false; }
		}

		[Test]
		public void AddTableWithMyISAMEngine()
		{
			provider.AddTable("Test", "MyISAM",
			                  new Column("Id", DbType.Int32, ColumnProperty.NotNull),
			                  new Column("name", DbType.String, 50));
		}

		[Test]
		public override void CanAddCheckConstraint()
		{
			Assert.Throws<NotSupportedException>(AddCheckConstraint);
		}

	}
}