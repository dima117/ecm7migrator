using System;
using System.Configuration;
using System.Data;
using ECM7.Migrator.Framework;
using ECM7.Migrator.Providers.MySql;
using NUnit.Framework;

namespace ECM7.Migrator.Tests.TestClasses.Providers
{
	[TestFixture, Category("MySql")]
	public class MySqlTransformationProviderTest : TransformationProviderConstraintBase
	{
		[SetUp]
		public void SetUp()
		{
			string constr = ConfigurationManager.AppSettings["MySqlConnectionString"];
			if (constr == null)
				throw new ArgumentNullException("MySqlConnectionString", "No config file");
			provider = new MySqlTransformationProvider(new MySqlDialect(), constr);
			// provider.Logger = new Logger(true, new ConsoleWriter());

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
			                  new Column("name", DbType.String, 50)
				);
		}

		// [Test,Ignore("MySql doesn't support check constraints")]
		public override void CanAddCheckConstraint() {}

	}
}