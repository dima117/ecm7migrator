using NUnit.Framework;

namespace ECM7.Migrator.Providers.Tests.ImplementationTest.NoQuotesWithSchema
{
	[TestFixture, Category("MySql")]
    public class MySqlTransformationProviderSchemaTest : WithSchema.MySqlTransformationProviderSchemaTest
	{
	    protected override bool AddQuotes
	    {
	        get { return false; }
	    }
	}
}
