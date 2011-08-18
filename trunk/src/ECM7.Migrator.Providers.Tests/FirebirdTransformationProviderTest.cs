using System.Data;

namespace ECM7.Migrator.Providers.Tests
{
	using ECM7.Migrator.Providers.Firebird;

    using NUnit.Framework;

    [TestFixture, Category("Firebird")]
    public class FirebirdTransformationProviderTest 
		: TransformationProviderConstraintBase<FirebirdTransformationProvider>
    {
    	public override string ConnectionStrinSettingsName
    	{
    		get { return "FirebirdConnectionString"; }
    	}

    	public override bool UseTransaction
    	{
			get { return false; }
    	}


		[Test]
		public override void AddDecimalColumn()
		{
			provider.AddColumn("TestTwo", "TestDecimal", DbType.Decimal, 18);
			Assert.IsTrue(provider.ColumnExists("TestTwo", "TestDecimal"));
		}
    }
}