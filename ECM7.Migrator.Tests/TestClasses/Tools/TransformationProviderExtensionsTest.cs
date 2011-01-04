using ECM7.Migrator.Framework;
using ECM7.Migrator.Framework.Tools;
using ECM7.Migrator.Tests.Helpers;
using NUnit.Framework;

namespace ECM7.Migrator.Tests.TestClasses.Tools
{
	[TestFixture]
	public class TransformationProviderExtensionsTest
	{
		public void CreateAndDropHiLoTable(ITransformationProvider database)
		{
			database.CreateHiLoTable();
			Assert.IsTrue(database.TableExists(TransformationProviderHiLoExtensions.HI_LO_TABLE_NAME));
			database.RemoveHiLoTable();
			Assert.IsFalse(database.TableExists(TransformationProviderHiLoExtensions.HI_LO_TABLE_NAME));
		}

		[Test]
		public void CanCreateAndDropHiLoTableOnSqlServer()
		{
			CreateAndDropHiLoTable(TestProviders.SqlServer);
		}

		[Test]
		public void CanCreateAndDropHiLoTableOnSqlServer2005()
		{
			CreateAndDropHiLoTable(TestProviders.SqlServer2005);
		}

		[Test]
		public void CanCreateAndDropHiLoTableOnSqlServerCe()
		{
			CreateAndDropHiLoTable(TestProviders.SqlServerCe);
		}

		[Test]
		public void CanCreateAndDropHiLoTableOnMySql()
		{
			CreateAndDropHiLoTable(TestProviders.MySql);
		}

		[Test]
		public void CanCreateAndDropHiLoTableOnOracle()
		{
			CreateAndDropHiLoTable(TestProviders.Oracle);
		}

		[Test]
		public void CanCreateAndDropHiLoTableOnPostgreSQL()
		{
			CreateAndDropHiLoTable(TestProviders.PostgreSQL);
		}

		[Test]
		public void CanCreateAndDropHiLoTableOnSQLite()
		{
			CreateAndDropHiLoTable(TestProviders.SQLite);
		}


	}
}