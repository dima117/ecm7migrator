using System;
using System.Configuration;
using ECM7.Migrator.Framework;
using ECM7.Migrator.Providers;
using ECM7.Migrator.Providers.SqlServer;
using NUnit.Framework;

namespace ECM7.Migrator.Tests2
{
	[TestFixture]
	public class OtherTests
	{
		[Test]
		public void CanGetMigrationHumanName()
		{
			Assert.AreEqual(
			"Migration0101 add new table with primary key",
			StringUtils.ToHumanName("Migration0101_Add_NewTable_with_primary___Key"));
		}

		[Test]
		public void ProviderFinalizeTest()
		{
			string cstring = ConfigurationManager.AppSettings["SqlServerConnectionString"];
			var provider = ProviderFactory.Create(typeof (SqlServerTransformationProvider), cstring, null);
			provider.ExecuteScalar("select 1");

			provider = null;
			GC.Collect();
		}
	}
}
