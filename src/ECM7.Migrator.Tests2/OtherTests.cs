using ECM7.Migrator.Framework;
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
	}
}
